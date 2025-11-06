using CMS.Data;
using CMS.Models;
using CMS.Services; 
using CMS.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Linq.Expressions;
using System.Security.Claims;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace CMS.Controllers
{
    [Authorize(Roles = "Admin,Doctor,Nurse")]
    public class AppointmentController : Controller
    {
        private readonly ApplicationDbContext _context;

        private readonly IEmailSender _email;
        private readonly ILogger<AppointmentController> _logger;
        private readonly IWebHostEnvironment _env;


        public AppointmentController(ApplicationDbContext context, IEmailSender email, ILogger<AppointmentController> logger, IWebHostEnvironment env)
        {
            _context = context;
            _email = email;
            _logger = logger;
            _env = env;
        }

        // ===== Availability helpers (weekly schedule repeats forever, with optional date overrides) =====
        private readonly TimeSpan DefaultSlot = TimeSpan.FromMinutes(30);
        private record AvailabilityWindow(TimeSpan Start, TimeSpan End);

        private async Task<List<AvailabilityWindow>> GetEffectiveAvailabilityWindowsAsync(int doctorId, DateTime date)
        {
            var localDate = TimeZoneInfo.ConvertTimeFromUtc(date, _clinicTimeZone);

            // 1. Date-specific overrides
            var dateAvail = await _context.DoctorDateAvailabilities
                .Where(x => x.DoctorId == doctorId &&
                            x.Date.HasValue &&
                            x.Date.Value.Date == localDate.Date)
                .ToListAsync();

            if (dateAvail.Count > 0)
            {
                bool hardBlock = dateAvail.Any(x => !x.IsAvailable && !x.StartTime.HasValue && !x.EndTime.HasValue);
                var positive = dateAvail
                    .Where(x => x.IsAvailable && x.StartTime.HasValue && x.EndTime.HasValue && x.EndTime > x.StartTime)
                    .Select(x => new AvailabilityWindow(x.StartTime!.Value, x.EndTime!.Value))
                    .Distinct() // <-- remove duplicates
                    .ToList();

                return (hardBlock && positive.Count == 0) ? new() : positive;
            }

            // 2. Weekly template (only once per day)
            var dow = localDate.DayOfWeek;
            var weekly = await _context.DoctorWeeklyAvailabilities
                .Where(a => a.DoctorId == doctorId &&
                            a.IsWorkingDay &&
                            a.DayOfWeek == dow)
                .ToListAsync();

            return weekly
                .Where(a => a.EndTime > a.StartTime)
                .Select(a => new AvailabilityWindow(a.StartTime, a.EndTime))
                .Distinct() // <-- remove duplicates
                .ToList();
        }
        private static bool FitsInWindows(TimeSpan start, TimeSpan duration, IEnumerable<AvailabilityWindow> windows)
        {
            var end = start + duration;
            return windows.Any(w => start >= w.Start && end <= w.End);
        }

        // 2) Server-side validator (doctor exists, works that date/time, and no overlap)
        private readonly TimeZoneInfo _clinicTimeZone =
     TimeZoneInfo.FindSystemTimeZoneById("Pakistan Standard Time");
        private async Task<(bool ok, string? message)> ValidateAppointmentAsync(Appointment appt)
        {
            if (appt == null) return (false, "Invalid appointment.");
            if (appt.DoctorId <= 0) return (false, "Please select a doctor.");
            if (appt.PatientId <= 0) return (false, "Please select a patient.");
            if (appt.AppointmentDate == default) return (false, "Please select a valid date/time.");

            var doctor = await _context.Doctors.AsNoTracking()
                                               .FirstOrDefaultAsync(d => d.Id == appt.DoctorId);
            if (doctor == null) return (false, "Doctor not found.");

            var duration = doctor.ConsultationDurationInMinutes > 0
                ? TimeSpan.FromMinutes(doctor.ConsultationDurationInMinutes)
                : DefaultSlot;

            /* ---------- 1.  convert NEW appointment to clinic local time ---------- */
            var newStartLocal = TimeZoneInfo.ConvertTime(appt.AppointmentDate, _clinicTimeZone); // Convert using DateTimeOffset
            var newEndLocal = newStartLocal.Add(duration);

            /* ---------- 2.  availability windows (already stored in local time) ---------- */
            var localDate = newStartLocal.Date;          // use the already-converted value
            var windows = await GetEffectiveAvailabilityWindowsAsync(appt.DoctorId, localDate);
            if (windows.Count == 0)
                return (false, "Doctor is not available on the selected date.");

            if (!FitsInWindows(newStartLocal.TimeOfDay, duration, windows))
                return (false, "Selected time is outside doctor's working hours.");

            /* ---------- 3.  load booked appointments for this local day ---------- */
            var booked = await _context.Appointments
                .Where(a => a.DoctorId == appt.DoctorId &&
                            a.AppointmentDate.Date == localDate)
                .Select(a => a.AppointmentDate)   // local PK time
                .ToListAsync();

            /* ---------- 4.  overlap check ---------- */
            bool overlaps = booked.Any(b =>
            {
                var bEnd = b.Add(duration);
                return b < newEndLocal && newStartLocal < bEnd;
            });

            if (overlaps) return (false, "This time overlaps with another appointment.");
            return (true, null);
        }// ===== Dropdown population (Department removed) =====
        private async Task PopulateDropdowns(AppointmentUpsertVM vm)
        {
            vm.Patients = await _context.Patients
                .Select(p => new SelectListItem
                {
                    Value = p.PatientId.ToString(),
                    Text = p.FirstName + " " + p.LastName
                }).ToListAsync();

            vm.Doctors = await _context.Doctors
                .Select(d => new SelectListItem
                {
                    Value = d.Id.ToString(),
                    Text = d.FullName
                }).ToListAsync();

            vm.AppointmentTypes = new List<SelectListItem>
            {
                new() { Value = "Checkup", Text = "Checkup" },
                new() { Value = "Consultation", Text = "Consultation" },
                new() { Value = "FollowUp", Text = "Follow Up" }
            };

            vm.Statuses = new List<SelectListItem>
            {
                new() { Value = "Scheduled", Text = "Scheduled" },
                new() { Value = "Completed", Text = "Completed" },
                new() { Value = "Cancelled", Text = "Cancelled" },
                new() { Value = "Rescheduled", Text = "Rescheduled" }
            };
        }

        // ===== Patient modal (partial) =====
        [HttpGet]
        public async Task<IActionResult> PatientModel(int? id)
        {
            Patient patient = new Patient();

            if (id.HasValue && id > 0)
            {
                patient = await _context.Patients.FindAsync(id) ?? new Patient();
            }

            return PartialView("_Upsert", patient);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PatientModel(Patient patient)
        {
            if (!ModelState.IsValid)
                return PartialView("_Upsert", patient);

            if (patient.PatientId == 0)
                _context.Patients.Add(patient);
            else
                _context.Patients.Update(patient);

            await _context.SaveChangesAsync();

            return Json(new
            {
                success = true,
                message = "Patient saved successfully!",
                patientId = patient.PatientId,
                patientName = patient.FirstName + " " + patient.LastName
            });
        }

        // ===== Helper =====
        private async Task SendAppointmentCreatedEmailAsync(int appointmentId)
        {
            var appt = await _context.Appointments
                .AsNoTracking()
                .Include(a => a.Patient)
                .Include(a => a.Doctor)
                .FirstOrDefaultAsync(a => a.AppointmentId == appointmentId);

            if (appt == null) return;

            var patientName = $"{appt.Patient?.FirstName} {appt.Patient?.LastName}".Trim();
            var to = appt.Patient?.Email;
            if (string.IsNullOrWhiteSpace(to)) return; // nothing to send

            var subject = $"Appointment Confirmed (#{appt.AppointmentNo})";

            var body = $@"
        <p>Dear {System.Net.WebUtility.HtmlEncode(patientName)},</p>
        <p>Your appointment has been scheduled.</p>
        <ul>
          <li><b>Doctor:</b> {System.Net.WebUtility.HtmlEncode(appt.Doctor?.FullName)} ({System.Net.WebUtility.HtmlEncode(appt.Doctor?.Specialty)})</li>
          <li><b>Date:</b> {appt.AppointmentDate:dddd, MMM d, yyyy}</li>
          <li><b>Time:</b> {appt.AppointmentDate:hh:mm tt}</li>
          <li><b>Fee:</b> {appt.Fee:C}</li>
          <li><b>Appointment No:</b> {System.Net.WebUtility.HtmlEncode(appt.AppointmentNo)}</li>
        </ul>
        <p>Thank you.</p>";

            await _email.SendAsync(to, subject, body);
        }

        // ===== Upsert =====

        // GET: /Invoice/Upsert or /Invoice/Upsert/5

        // Add action for thermal receipt printing

        [HttpGet]
        public async Task<IActionResult> Upsert(int? id)
        {
            var vm = new AppointmentUpsertVM
            {
                Appointment = id == null
                    ? new Appointment()
                    : await _context.Appointments.FindAsync(id),

                AppointmentNo = id == null || id == 0
            ? await GenerateNextAppointmentNumberAsync()
            : string.Empty
            };

            await PopulateDropdowns(vm);
            return View(vm);
        }

        private async Task<string> GenerateNextAppointmentNumberAsync()
        {
            string num;
            do
            {
                num = $"AP{Random.Shared.Next(100_000, 999_999)}";
            }
            while (await _context.Appointments.AnyAsync(a => a.AppointmentNo == num));

            return num;
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upsert(AppointmentUpsertVM vm)
        {
            // Check if it's an AJAX request
            bool isAjaxRequest = Request.Headers["X-Requested-With"] == "XMLHttpRequest";

            try
            {
                if (!ModelState.IsValid)
                {
                    if (isAjaxRequest)
                    {
                        var errors = ModelState.Values
                            .SelectMany(v => v.Errors)
                            .Select(e => e.ErrorMessage)
                            .ToList();
                        return Json(new { success = false, errors });
                    }

                    await PopulateDropdowns(vm);
                    return View(vm);
                }

                // Handle slot change logic
                if (!vm.ChangeSlot && vm.Appointment.AppointmentId > 0)
                {
                    var original = await _context.Appointments
                                              .AsNoTracking()
                                              .FirstOrDefaultAsync(a => a.AppointmentId == vm.Appointment.AppointmentId);
                    if (original != null)
                        vm.Appointment.AppointmentDate = original.AppointmentDate;
                }
                else
                {
                    var (ok, message) = await ValidateAppointmentAsync(vm.Appointment);
                    if (!ok)
                    {
                        if (isAjaxRequest)
                        {
                            return Json(new { success = false, errors = new List<string> { message } });
                        }

                        ModelState.AddModelError("Appointment.AppointmentDate", message ?? "Selected time is not available.");
                        await PopulateDropdowns(vm);
                        return View(vm);
                    }
                }

                bool isNew = vm.Appointment.AppointmentId == 0;

                try
                {
                    if (isNew)
                    {
                        _context.Appointments.Add(vm.Appointment);
                        await _context.SaveChangesAsync();
                        await AddAppointmentToInvoice(vm.Appointment);

                        // Set IsAppointmentInvoice to true when creating a new appointment with an associated invoice
                        var invoice = await _context.Invoices.FirstOrDefaultAsync(i => i.AppointmentId == vm.Appointment.AppointmentId);
                        if (invoice != null)
                        {
                            invoice.IsAppointmentInvoice = true;
                            _context.Invoices.Update(invoice);
                            await _context.SaveChangesAsync();
                        }

                        try
                        {
                            await SendAppointmentCreatedEmailAsync(vm.Appointment.AppointmentId);
                        }
                        catch (Exception ex)
                        {
                            // Log and continue; do not break receptionist flow if SMTP fails
                            _logger?.LogError(ex, "Failed to send appointment confirmation email.");
                        }
                    }
                    else
                    {
                        _context.Appointments.Update(vm.Appointment);
                        await _context.SaveChangesAsync();
                    }
                }
                catch (DbUpdateException)
                {
                    if (isAjaxRequest)
                    {
                        return Json(new { success = false, errors = new List<string> { "That time has just been booked by someone else. Please pick another slot." } });
                    }

                    ModelState.AddModelError("Appointment.AppointmentDate", "That time has just been booked by someone else. Please pick another slot.");
                    await PopulateDropdowns(vm);
                    return View(vm);
                }

                // Return JSON for AJAX, redirect for normal requests
                if (isAjaxRequest)
                {
                    TempData["success"] = isNew ? "Appointment created successfully." : "Appointment updated successfully.";

                    return Json(new
                    {
                        success = true,
                        message = isNew ? "Appointment created successfully!" : "Appointment updated successfully!"
                    });
                }
                TempData["success"] = isNew ? "Appointment created successfully." : "Appointment updated successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                if (isAjaxRequest)
                {
                    return Json(new { success = false, message = ex.Message });
                }
                return BadRequest(ex.Message);
            }
        }
        [HttpPost]
        public async Task<JsonResult> UnbookSlot(int appointmentId)
        {
            try
            {
                var appointment = await _context.Appointments.FindAsync(appointmentId);
                if (appointment != null)
                {
                   

                    // Option 2: Clear the appointment date so it doesn't block the slot
                    appointment.AppointmentDate = DateTimeOffset.MinValue; // or some other "empty" value
                    appointment.Status = "Schedule";

                    await _context.SaveChangesAsync();

                    return Json(new { success = true, message = "Slot freed successfully" });
                }

                return Json(new { success = false, message = "Appointment not found" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        } //}

        // Your main method that is calling AddAppointmentToInvoice
        private string GetCurrentUser()
        {
            return User.FindFirst(ClaimTypes.Name)?.Value ?? "system";
        }

        private async Task AddAppointmentToInvoice(Appointment appointment)
        {
            if (appointment == null || appointment.DoctorId == 0 || appointment.PatientId == 0)
                return;

            try
            {
                var patient = await _context.Patients.FindAsync(appointment.PatientId);
                var doctor = await _context.Doctors.FindAsync(appointment.DoctorId);

                if (patient == null || doctor == null)
                {
                    _logger.LogWarning("Patient or Doctor not found for appointment {AppointmentId}", appointment.AppointmentId);
                    return;
                }

                // Get the doctor's consultation charge with fallback
                decimal consultationCharge = doctor.ConsultationCharge > 0 ? doctor.ConsultationCharge : 1000m; // Default fallback

                // Calculate totals
                decimal subtotal = consultationCharge;
                decimal tax = 0;
                decimal discount = 0;
                decimal total = subtotal + tax - discount;

                var currentUser = User.FindFirst(ClaimTypes.Name)?.Value ?? "system";

                // Create the invoice with all required fields
                var invoice = new Invoice
                {
                    AppointmentId = appointment.AppointmentId,
                    PatientId = patient.PatientId,
                    DoctorId = doctor.Id,
                    CustomerName = $"{patient.FirstName} {patient.LastName}".Trim(),
                    CustomerEmail = patient.Email ?? "no-email@example.com",
                    CustomerAddress = patient.Address ?? "Not specified",
                    SubTotal = subtotal,
                    Tax = tax,
                    Discount = discount,
                    Total = total,
                    AmountDue = total, // Initially, amount due equals total
                    AmountPaid = 0,
                    CurrencyCode = "PKR",
                    IssueDate = DateTime.Now,
                    DueDate = DateTime.Now.AddDays(7),
                    status = "Pending",
                    IsAppointmentInvoice = true,
                    PaymentStatus = PaymentStatus.Pending,
                    InvoiceType = InvoiceType.Appointment,
                    CreatedBy = currentUser,
                    UpdatedBy = currentUser,
                    CreatedDate = DateTime.Now,
                    UpdatedDate = DateTime.Now,
                    Items = new List<InvoiceItem>
            {
                new InvoiceItem
                {
                    AppointmentWIth = $"Consultation with Dr. {doctor.FullName}",
                    Quantity = 1,
                    UnitPrice = consultationCharge,
                    Total = consultationCharge,
                    ItemType = "Appointment",
                    AppointmentId = appointment.AppointmentId,
                    //CreatedBy = currentUser
                    //UpdatedBy = currentUser
                }
            }
                };

                // Generate invoice number
                invoice.InvoiceNumber = GenerateAppointmentInvoiceNumber();

                // Add the invoice to the context
                _context.Invoices.Add(invoice);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Invoice created successfully for appointment {AppointmentId}", appointment.AppointmentId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating invoice for appointment {AppointmentId}", appointment.AppointmentId);
                // Don't throw - we don't want to break the appointment creation if invoice fails
            }
        }

        // Helper method to generate invoice number
        private string GenerateAppointmentInvoiceNumber()
        {
            return $"APT-{DateTime.Now:yyyyMMddHHmmss}";
        }


        public async Task<IActionResult> Index()
        {
            var appointments = await _context.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Doctor)
                .ToListAsync();

            return View(appointments);
        }

        // ===== Doctor details for UI =====
        [HttpGet]
        public async Task<IActionResult> GetDoctor(int id)
        {
            var d = await _context.Doctors
                .AsNoTracking()
                .Select(x => new
                {
                    x.Id,
                    FullName = x.FullName,
                    Fee = x.ConsultationCharge,
                    Licence = x.MedicalLicenseNumber,
                    Phone = x.Phone,
                    Email = x.Email,
                    Address = x.Address,
                    BloodGroup = x.BloodGroup,
                    YearsExp = x.YearOfExperience,
                    ProfileUrl = x.ProfileImageUrl ?? "/assets/img/doctors/default-doctor.jpg"
                })
                .FirstOrDefaultAsync(x => x.Id == id);

            return d == null ? NotFound() : Json(d);
        }
        [HttpGet]
        public async Task<IActionResult> GetWeekSlots(int doctorId, DateTimeOffset start)
        {
            var doctor = await _context.Doctors.AsNoTracking()
                .FirstOrDefaultAsync(d => d.Id == doctorId);

            if (doctor == null)
                return BadRequest("Doctor not found.");

            var clinicZone = TimeZoneInfo.FindSystemTimeZoneById("Pakistan Standard Time");

            // 1. Get the *local* day (DateTimeOffset) that corresponds to the incoming start
            var localDate = TimeZoneInfo.ConvertTime(start, clinicZone).Date; // DateTimeOffset

            // 2. Availability windows (already in local clock time)
            var rawWindows = await GetEffectiveAvailabilityWindowsAsync(doctorId, localDate.Date);
            var windows = rawWindows.Where(w => w.End > w.Start).ToList();

            if (!windows.Any())
            {
                return Json(new[]
                {
            new
            {
                date = localDate.ToString("yyyy-MM-dd"),
                day  = localDate.ToString("ddd, dd MMMM yyyy"),
                full = localDate.ToString("dd MMM yyyy"),
                isFullyBooked = true,
                slots = Array.Empty<object>()
            }
        });
            }

            var duration = TimeSpan.FromMinutes(
                doctor.ConsultationDurationInMinutes > 0
                    ? doctor.ConsultationDurationInMinutes
                    : 30);

            // 3. Booked appointments – keep as DateTimeOffset
            var booked = await _context.Appointments
                .Where(a => a.DoctorId == doctorId &&
                            a.AppointmentDate.Date == localDate.Date)
                .Select(a => a.AppointmentDate)   // DateTimeOffset from DB
                .ToListAsync();

            // Convert to clinic zone once
            var bookedLocal = booked
                .Select(dt => TimeZoneInfo.ConvertTime(dt, clinicZone))
                .ToList();

            // 4. Generate slots
            var slots = windows
                .SelectMany(w =>
                {
                    var list = new List<TimeSpan>();
                    for (var t = w.Start; t + duration <= w.End; t = t.Add(duration))
                        list.Add(t);
                    return list;
                })
                .Select(offset =>
                {
                    var slotStartLocal = localDate.Add(offset);               // DateTimeOffset
                    var slotEndLocal = slotStartLocal.Add(duration);

                    bool isBooked = bookedLocal.Any(b =>
                    {
                        var bEnd = b.Add(duration);
                        return b < slotEndLocal && slotStartLocal < bEnd;
                    });

                    return new
                    {
                        iso = slotStartLocal.ToString("yyyy-MM-ddTHH:mm"),
                        label = $"{slotStartLocal:h:mm tt} – {slotEndLocal:h:mm tt}",
                        isBooked
                    };
                })
                .OrderBy(x => x.iso)
                .ToList();

            // 5. Return result
            var result = new[]
            {
        new
        {
            date = localDate.ToString("yyyy-MM-dd"),
            day  = localDate.ToString("ddd, dd MMMM yyyy"),
            full = localDate.ToString("dd MMM yyyy"),
            isFullyBooked = slots.All(s => s.isBooked),
            slots
        }
    };

            return Json(result);
        }
        [HttpGet]
        public async Task<IActionResult> GetAll(string status) // This method handles all GET requests to /Appointment/GetAll
        {
            try
            {
                int draw = int.TryParse(Request.Query["draw"], out var d) ? d : 1;
                int start = int.TryParse(Request.Query["start"], out var s) ? s : 0;
                int length = int.TryParse(Request.Query["length"], out var l) ? l : 10;
                string searchValue = (Request.Query["search[value]"].FirstOrDefault() ?? string.Empty).Trim();
                int orderColIndex = int.TryParse(Request.Query["order[0][column]"], out var oc) ? oc : 0;
                string orderDir = Request.Query["order[0][dir]"].FirstOrDefault() ?? "asc";

                if (length <= 0) length = 10;
                if (start < 0) start = 0;

                IQueryable<Appointment> q = _context.Appointments
                    .Include(a => a.Patient)
                    .Include(a => a.Doctor)
                    .AsNoTracking();

                int recordsTotal = await q.CountAsync();

                // Global search filter
                if (!string.IsNullOrWhiteSpace(searchValue))
                {
                    string term = searchValue.ToLower();
                    q = q.Where(a =>
                        (a.Patient.FirstName + " " + a.Patient.LastName).ToLower().Contains(term) ||
                        (a.Doctor.FullName ?? "").ToLower().Contains(term) ||
                        (a.AppointmentType.ToString() ?? "").ToLower().Contains(term) ||
                        (a.Status.ToString() ?? "").ToLower().Contains(term) ||
                        (a.Mode ?? "").ToLower().Contains(term)
                    );
                }

                // Status filter logic
                // IMPORTANT: Replace 'AppointmentStatus' with the actual name of your enum
                if (!string.IsNullOrEmpty(status))
                {
                    q = q.Where(a => a.Status == status);
                }

                int recordsFiltered = await q.CountAsync();

                System.Linq.Expressions.Expression<Func<Appointment, object>> sortSelector = orderColIndex switch
                {
                    0 => a => a.Patient.FirstName,
                    1 => a => a.Doctor.FullName,
                    2 => a => a.AppointmentDate,
                    3 => a => a.CreatedOn,
                    4 => a => a.AppointmentType,
                    5 => a => a.Status,
                    _ => a => a.AppointmentId
                };

                bool desc = orderDir.Equals("desc", StringComparison.OrdinalIgnoreCase);
                q = desc ? q.OrderByDescending(sortSelector) : q.OrderBy(sortSelector);

                var data = await q
                    .Skip(start)
                    .Take(length)
                    .Select(a => new
                    {
                        id = a.AppointmentId,
                        patientName = a.Patient.FirstName + " " + a.Patient.LastName,
                        patientGender = a.Patient.Gender,
                        patientImg = a.Patient.ProfileImageUrl,
                        doctorName = a.Doctor.FullName,
                        doctorSpecialty = a.Doctor.Specialty,
                        doctorImg = a.Doctor.ProfileImageUrl,
                        appointmentDate = a.AppointmentDate,
                        appCreatedOn = a.CreatedOn,
                        type = a.AppointmentType,
                        status = a.Status
                    })
                    .ToListAsync();

                return Json(new
                {
                    draw,
                    recordsTotal,
                    recordsFiltered,
                    data
                });
            }
            catch (Exception ex)
            {
                Response.StatusCode = StatusCodes.Status500InternalServerError;
                return Json(new { error = "Server error: " + ex.Message });
            }
        }


        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public IActionResult Delete(int id)
        //{
        //    try {
        //        var appointment = _context.Appointments.FirstOrDefault(a => a.AppointmentId == id);
        //        if (appointment == null)
        //        {
        //            return Json(new { success = false, message = "Error while deleting" });
        //        }

        //        _context.Appointments.Remove(appointment);
        //        _context.SaveChanges();
        //        return Json(new { success = true, message = "Delete successful" });
        //    }
        //    catch (DbUpdateException ex)
        //    {
        //        // Check inner exception for FK violation
        //        return Json(new { success = false, message = "Cannot delete this appointment because invoices are linked to it.", ex });
               
        //    }
        //}
        [HttpPost]
[ValidateAntiForgeryToken]
public IActionResult Delete(int id)
{
    try
    {
        var appointment = _context.Appointments.FirstOrDefault(a => a.AppointmentId == id);
        if (appointment == null)
        {
            return Json(new { success = false, message = "Error while deleting. Appointment not found." });
        }

        // Soft delete by setting IsDeleted flag
        appointment.IsDeleted = true;

        _context.Update(appointment);
        _context.SaveChanges();

        return Json(new { success = true, message = "Delete successful (soft delete)" });
    }
    catch (DbUpdateException ex)
    {
        return Json(new { success = false, message = "Cannot delete this appointment because invoices are linked to it.", ex });
    }
}

      

        [HttpPost]
        public IActionResult CompleteAppointment(int appointmentId)
        {
            var appointment = _context.Appointments
                                      .Include(a => a.Patient)
                                      .FirstOrDefault(a => a.AppointmentId == appointmentId);

            if (appointment == null) return NotFound();

            appointment.Status = "Completed";
            _context.SaveChanges();

            return RedirectToAction("CreateFromAppointment", "Invoice", new { appointmentId });
        }


        [HttpGet("Appointment/Details/{id}")]
        public IActionResult Details(int id)
        {
            return View(id);
        }

        // AJAX endpoint to get all appointment data

        [HttpGet]
        public async Task<IActionResult> GetAppointmentDetails(int id)
        {
            try
            {
                var appointment = await _context.Appointments
                    .Include(a => a.Patient)
                    .FirstOrDefaultAsync(a => a.AppointmentId == id);

                if (appointment?.Patient == null)
                    return NotFound(new { message = "Appointment not found." });

                // 1. Vitals
                var vitals = await _context.PatientVitals
                    .FirstOrDefaultAsync(v => v.AppointmentId == id);

                // 2. Treatments already added
                var appointmentTreatments = await _context.PatientTreatments
                    .Include(at => at.Treatment)
                    .Where(at => at.AppointmentId == id)
                    .Select(at => new PatientTreatmentViewModel
                    {
                        Id = at.Treatment.TreatmentId,
                        Name = at.Treatment.Name,
                        UnitPrice = at.Treatment.UnitPrice
                    })
                    .ToListAsync();

                // 3. Medications already prescribed (Inventory OR Custom)
                var appointmentMedications = await _context.InvoiceItems
                    .Where(ii => ii.AppointmentId == id &&
                                (ii.InventoryItemId != null || ii.MedicationsId != null))
                    .Include(ii => ii.InventoryItem)
                    .Include(ii => ii.Medications)
                    .Select(ii => new MedicationViewModel
                    {
                        Id = ii.InventoryItemId ?? ii.MedicationsId ?? 0,
                        Name = ii.InventoryItem != null ? ii.InventoryItem.Name
                             : ii.Medications != null ? ii.Medications.Name
                             : "Unknown",
                        UnitPrice = ii.UnitPrice
                    })
                    .ToListAsync();

                // 4. Dropdown sources
                var treatmentOptions = await _context.Treatments
                    .Select(t => new SelectListItem
                    {
                        Value = t.TreatmentId.ToString(),
                        Text = $"{t.Name} - ${t.UnitPrice:F2}"
                    })
                    .ToListAsync();

                var medicationOptions = await _context.InventoryItems
                    .Select(i => new SelectListItem
                    {
                        Value = i.Id.ToString(),
                        Text = $"{i.Name} - ${i.UnitPrice:F2} (Stock: {i.Quantity})"
                    })
                    .ToListAsync();

                // 5. Compose ViewModel
                var vm = new AppointmentDetailsViewModel
                {
                    AppointmentId = appointment.AppointmentId,
                    CurrentStatus = appointment.Status,
                    Patient = new PatientInfoViewModel
                    {
                        PatientId = appointment.Patient.PatientId,
                        ProfileImageUrl = appointment.Patient.ProfileImageUrl,
                        FullName = $"{appointment.Patient.FirstName} {appointment.Patient.LastName}",
                        Gender = appointment.Patient.Gender ?? "N/A",

                        // DateTimeOffset.Now instead of DateTime.Now
                        Age = appointment.Patient.DateOfBirth.HasValue
                              ? DateTimeOffset.Now.Year - appointment.Patient.DateOfBirth.Value.Year
                              : 0,

                        VisitDate = appointment.AppointmentDate,   // already DateTimeOffset
                        Email = appointment.Patient.Email,
                        PhoneNumber = appointment.Patient.PhoneNumber,
                        VisitId = appointment.AppointmentNo ?? $"V{appointment.AppointmentId}",
                        ChiefComplaint = appointment.Patient.Allergies ?? "No Allergies recorded"
                    },
                    Vitals = vitals == null
                        ? new UpdatePatientVitalsViewModel()
                        : new UpdatePatientVitalsViewModel
                        {
                            Id = vitals.Id,
                            BloodPressure = vitals.BloodPressure,
                            HeartRate = vitals.HeartRate,
                            Spo2 = vitals.Spo2,
                            Temperature = vitals.Temperature,
                            RespiratoryRate = vitals.RespiratoryRate,
                            Weight = vitals.Weight,
                            RecordedAt = DateTimeOffset.Now       // <-- DateTimeOffset
                        },
                    Treatments = appointmentTreatments,
                    Medications = appointmentMedications,
                    TreatmentOptions = treatmentOptions,
                    MedicationOptions = medicationOptions
                };

                return Json(vm);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching appointment details for ID {AppointmentId}", id);
                return StatusCode(500, new { message = "An error occurred while fetching appointment details." });
            }
        }


        // Update appointment status
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, string status)
        {
            try
            {
                var appointment = await _context.Appointments.FindAsync(id);
                if (appointment == null) return NotFound();

                appointment.Status = status;
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = $"Status updated to {status}." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating appointment status");
                return Json(new { success = false, message = "Failed to update status." });
            }
        }

        // In AppointmentController.cs

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddTreatment(int appointmentId, int treatmentId)
        {
            try
            {
                // 1️⃣  Avoid duplicate treatment entries
                bool exists = await _context.PatientTreatments
                    .AnyAsync(pt => pt.AppointmentId == appointmentId && pt.TreatmentId == treatmentId);

                if (!exists)
                {
                    _context.PatientTreatments.Add(new PatientTreatments
                    {
                        AppointmentId = appointmentId,
                        TreatmentId = treatmentId
                    });
                    await _context.SaveChangesAsync();
                }

                // 2️⃣  Ensure an invoice exists for this appointment
                var invoice = await _context.Invoices
                    .FirstOrDefaultAsync(i => i.AppointmentId == appointmentId);

                if (invoice == null)
                {
                    var appt = await _context.Appointments
                        .Include(a => a.Patient)
                        .Include(a => a.Doctor)
                        .FirstOrDefaultAsync(a => a.AppointmentId == appointmentId);

                    if (appt == null)
                        return Json(new { success = false, message = "Appointment not found." });

                    invoice = new Invoice
                    {
                        AppointmentId = appointmentId,
                        InvoiceNumber = $"MED-{appt.AppointmentNo}",
                        PatientId = appt.PatientId,
                        DoctorId = appt.DoctorId,
                        CustomerName = $"{appt.Patient.FirstName} {appt.Patient.LastName}",
                        CustomerEmail = appt.Patient.Email,
                        IssueDate = DateTime.Now,
                        InvoiceType = InvoiceType.Combined,
                        DueDate = DateTime.Now.AddDays(7),
                        CurrencyCode = "PKR",
                        status = "Pending",
                        SubTotal = 0,
                        Tax = 0,
                        Discount = 0,
                        Total = 0,
                        CreatedBy = User.FindFirst("UserId")?.Value ?? "system",
                        UpdatedBy = User.FindFirst("UserId")?.Value ?? "system"
                    };

                    _context.Invoices.Add(invoice);
                    await _context.SaveChangesAsync();
                }

                // 3️⃣  Add a line item for the treatment and update totals
                var treatment = await _context.Treatments.FindAsync(treatmentId);
                if (treatment != null)
                {
                    var line = new InvoiceItem
                    {
                        InvoiceId = invoice.Id,
                        AppointmentId = appointmentId,
                        //MedicationsId = treatmentId,          // FK to Treatment if you have it
                        TreatmentName = treatment.Name,
                        Quantity = 1,
                        UnitPrice = treatment.UnitPrice,
                        Total = treatment.UnitPrice,
                        ItemType = "Combined"
                    };

                    _context.InvoiceItems.Add(line);
                    invoice.SubTotal += line.Total;
                    invoice.Total += line.Total;

                    // 4️⃣  Mark invoice as combined (new field/property)
                    invoice.InvoiceType = InvoiceType.Combined;

                    // 2.  set the bool flags
                    invoice.IsAppointmentInvoice = false;
                    invoice.IsCombinedInvoice = true;
                    

                   
                    _context.Invoices.Update(invoice);
                    await _context.SaveChangesAsync();
                }

                // 5️⃣  Return updated treatments list
                var updated = await _context.PatientTreatments
                    .Include(pt => pt.Treatment)
                    .Where(pt => pt.AppointmentId == appointmentId)
                    .Select(pt => new PatientTreatmentViewModel
                    {
                        Id = pt.Treatment.TreatmentId,
                        Name = pt.Treatment.Name,
                        UnitPrice = pt.Treatment.UnitPrice
                    })
                    .ToListAsync();

                return Json(new { success = true, treatments = updated });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet("Appointment/NurseView")]
        public IActionResult NurseView()
        {
            return View();
        }

        // AJAX endpoint for nurse appointment list
        [HttpGet]
        public async Task<IActionResult> GetNurseAppointmentList()
        {
            try
            {
                int draw = int.TryParse(Request.Query["draw"], out var d) ? d : 1;
                int start = int.TryParse(Request.Query["start"], out var s) ? s : 0;
                int length = int.TryParse(Request.Query["length"], out var l) ? l : 10;
                string searchValue = (Request.Query["search[value]"].FirstOrDefault() ?? string.Empty).Trim();

                // Get today's appointments that are scheduled or in-progress
                var todayStart = DateTime.Today;
                var todayEnd = DateTime.Today.AddDays(1);

                IQueryable<Appointment> q = _context.Appointments
                    .Include(a => a.Patient)
                    .Where(a => a.AppointmentDate >= todayStart &&
                               a.AppointmentDate < todayEnd &&
                               (a.Status == "Scheduled" || a.Status == "InProgress"))
                    .AsNoTracking();

                int recordsTotal = await q.CountAsync();

                // Search functionality
                if (!string.IsNullOrWhiteSpace(searchValue))
                {
                    string term = searchValue.ToLower();
                    q = q.Where(a =>
                        (a.Patient.FirstName ?? "").ToLower().Contains(term) ||
                        (a.Patient.LastName ?? "").ToLower().Contains(term) ||
                        (a.AppointmentNo ?? "").ToLower().Contains(term)
                    );
                }

                int recordsFiltered = await q.CountAsync();

                // Get the data with vital status
                var appointments = await q
                    .Skip(start)
                    .Take(length)
                    .Select(a => new
                    {
                        appointmentId = a.AppointmentId,
                        appointmentNo = a.AppointmentNo,
                        patientName = $"{a.Patient.FirstName} {a.Patient.LastName}",
                        appointmentTime = a.AppointmentDate,
                        status = a.Status,
                        appointmentType = a.AppointmentType,
                        // Check if vitals already exist
                        hasVitals = _context.PatientVitals.Any(v => v.AppointmentId == a.AppointmentId)
                    })
                    .ToListAsync();

                return Json(new
                {
                    draw,
                    recordsTotal,
                    recordsFiltered,
                    data = appointments
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching nurse appointment list");
                return StatusCode(500, new { message = "Server error occurred." });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAppointmentVitals(int appointmentId)
        {
            try
            {
                var appointment = await _context.Appointments
                    .Include(a => a.Patient)
                    .Include(a => a.Treatments)
                    .FirstOrDefaultAsync(a => a.AppointmentId == appointmentId);

                if (appointment == null)
                    return NotFound();

                var vitals = await _context.PatientVitals
                    .FirstOrDefaultAsync(v => v.AppointmentId == appointmentId);

                // FIX: Use your existing UpdatePatientVitalsViewModel instead of raw entity
                var vitalsViewModel = vitals != null ? new UpdatePatientVitalsViewModel
                {
                    Id = vitals.Id,
                    BloodPressure = vitals.BloodPressure,
                    HeartRate = vitals.HeartRate,
                    Spo2 = vitals.Spo2,
                    Temperature = vitals.Temperature,
                    RespiratoryRate = vitals.RespiratoryRate,
                    Weight = vitals.Weight,
                    RecordedAt = DateTime.Now
                } : new UpdatePatientVitalsViewModel
                {
                    Id = 0,
                    BloodPressure = null,
                    HeartRate = null,
                    Spo2 = null,
                    Temperature = null,
                    RespiratoryRate = null,
                    Weight = null,
                    RecordedAt = DateTime.Now
                };

                var result = new
                {
                    appointmentId = appointment.AppointmentId,
                    patientName = $"{appointment.Patient.FirstName} {appointment.Patient.LastName}",
                    appointmentTime = appointment.AppointmentDate.ToString("MMM dd, yyyy hh:mm tt"),
                    vitals = vitalsViewModel // <-- Now using safe ViewModel, no circular reference
                };

                return Json(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching appointment vitals for appointment {AppointmentId}", appointmentId);
                return StatusCode(500, new { message = "Server error occurred." });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateVitals([FromBody] UpdatePatientVitalsViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var existingVitals = await _context.PatientVitals
                    .FirstOrDefaultAsync(v => v.AppointmentId == model.AppointmentId);

                if (existingVitals == null)
                {
                    // Create new vitals record
                    var newVitals = new PatientVitals
                    {
                        AppointmentId = model.AppointmentId,
                        BloodPressure = model.BloodPressure,
                        HeartRate = model.HeartRate,
                        Spo2 = model.Spo2,
                        Temperature = model.Temperature,
                        RespiratoryRate = model.RespiratoryRate,
                        Weight = model.Weight
                    };
                    _context.PatientVitals.Add(newVitals);
                }
                else
                {
                    // Update existing vitals
                    existingVitals.BloodPressure = model.BloodPressure;
                    existingVitals.HeartRate = model.HeartRate;
                    existingVitals.Spo2 = model.Spo2;
                    existingVitals.Temperature = model.Temperature;
                    existingVitals.RespiratoryRate = model.RespiratoryRate;
                    existingVitals.Weight = model.Weight;
                }

                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Vitals updated successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating vitals for appointment {AppointmentId}", model.AppointmentId);
                return Json(new { success = false, message = "Failed to update vitals." });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveTreatment(int appointmentId, int treatmentId)
        {
            try
            {
                var treatmentRecord = await _context.PatientTreatments
                    .FirstOrDefaultAsync(at => at.AppointmentId == appointmentId && at.TreatmentId == treatmentId);

                if (treatmentRecord == null)
                {
                    return Json(new { success = false, message = "Treatment not found for this appointment." });
                }

                _context.PatientTreatments.Remove(treatmentRecord);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Treatment removed successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing treatment");
                return Json(new { success = false, message = "Failed to remove treatment." });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveMedication(int appointmentId, int medicationId) // medicationId refers to Medications.Id
        {
            try
            {
                var invoiceItems = await _context.InvoiceItems
                    .Where(ii => ii.AppointmentId == appointmentId && ii.MedicationsId == medicationId)
                    .ToListAsync();

                if (!invoiceItems.Any())
                {
                    return Json(new { success = false, message = "Medication not found for this appointment." });
                }

                _context.InvoiceItems.RemoveRange(invoiceItems);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Medication removed successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing medication");
                return Json(new { success = false, message = "Failed to remove medication." });
            }
        }



        [HttpGet]
        public async Task<IActionResult> GetAttachments(int patientId)
        {
            var list = await _context.Document
                .Where(d => d.PatientId == patientId)
                .OrderByDescending(d => d.CreateAt)
                .Select(d => new
                {
                    id = d.Id,
                    fileName = d.FileName,
                    fileUrl = d.FileUrl,
                    fileSize = d.FileSize,
                    description = d.Description
                })
                .ToListAsync();

            return Json(list);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadAttachment(int patientId, IFormFile file, string description)
        {
            if (file == null || file.Length == 0)
                return Json(new { success = false, message = "No file received." });

            try
            {
                // physical save (keep it simple)
                var uploads = Path.Combine(_env.WebRootPath, "uploads", "attachments");
                Directory.CreateDirectory(uploads);
                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
                var filePath = Path.Combine(uploads, fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                    await file.CopyToAsync(stream);

                // db record
                var doc = new Document
                {
                    PatientId = patientId,
                    FileName = file.FileName,
                    FileUrl = $"/uploads/attachments/{fileName}",
                    FileSize = $"{file.Length / 1024.0:0.##} KB",
                    Description = description,
                    CreateAt = DateTime.Now
                };
                _context.Document.Add(doc);
                await _context.SaveChangesAsync();

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "UploadAttachment failed");
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAttachment(int id)
        {
            var doc = await _context.Document.FindAsync(id);
            if (doc == null) return Json(new { success = false, message = "File not found." });

            // delete physical file
            var physical = Path.Combine(_env.WebRootPath, doc.FileUrl.TrimStart('/'));
            if (System.IO.File.Exists(physical))
                System.IO.File.Delete(physical);

            _context.Document.Remove(doc);
            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }





        [HttpGet]
        public async Task<IActionResult> GetNotes(int appointmentId)
        {
            var list = await _context.NotesList
                .Where(n => n.AppointmentId == appointmentId)
                .OrderByDescending(n => n.CreatedAt)   // newest first
                .Select(n => new
                {
                    id = n.Id,
                    text = n.Text,
                    createdAt = n.CreatedAt
                })
                .ToListAsync();

            return Json(list);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddNote(int appointmentId, string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return Json(new { success = false, message = "Empty note." });

            var note = new Note
            {
                AppointmentId = appointmentId,
                Text = text,
                CreatedAt = DateTime.Now
            };
            _context.NotesList.Add(note);
            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteNote(int id)
        {
            var note = await _context.NotesList.FindAsync(id);
            if (note == null) return Json(new { success = false, message = "Note not found." });

            _context.NotesList.Remove(note);
            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }



        [HttpGet]
        public async Task<IActionResult> GetFollowUps(int appointmentId)
        {
            var list = await _context.FollowUps
                .Where(f => f.AppointmentId == appointmentId)   // NEW filter
                .OrderByDescending(f => f.FollowUpDate)
                .Select(f => new
                {
                    id = f.Id,
                    date = f.FollowUpDate,
                    notes = f.Notes,
                    status = f.Status
                })
                .ToListAsync();
            return Json(list);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddFollowUp(int appointmentId, int patientId, DateTime date, string notes)
        {
            if (date < DateTime.Today)
                return Json(new { success = false, message = "Date cannot be in the past." });

            var fu = new FollowUp
            {
                AppointmentId = appointmentId,   // NEW
                PatientId = patientId,
                FollowUpDate = date,
                Notes = notes,
                Status = "Scheduled"
            };
            _context.FollowUps.Add(fu);
            await _context.SaveChangesAsync();
            return Json(new { success = true, id = fu.Id });
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteFollowUp(int id)
        {
            var fu = await _context.FollowUps.FindAsync(id);
            if (fu == null) return Json(new { success = false, message = "Not found" });
            _context.FollowUps.Remove(fu);
            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
      public async Task<IActionResult> AddMedicationStandAloneInvoice(
      int appointmentId,
      int? inventoryId,
      string customName,
      decimal? customCost)
        {
            try
            {
                // 1. Resolve item & price
                int itemId;
                string itemName;
                decimal itemPrice;

                if (inventoryId.HasValue)
                {
                    var inv = await _context.InventoryItems.FindAsync(inventoryId.Value);
                    if (inv == null)
                        return NotFound("Inventory item not found.");
                    itemId = inv.Id;
                    itemName = inv.Name;
                    itemPrice = inv.UnitPrice;
                }
                else if (!string.IsNullOrWhiteSpace(customName))
                {
                    var med = new Medications { Name = customName, UnitPrice = customCost ?? 0 };
                    _context.Medications.Add(med);
                    await _context.SaveChangesAsync();
                    itemId = med.Id;
                    itemName = med.Name;
                    itemPrice = (decimal)med.UnitPrice;
                }
                else
                {
                    return Json(new { success = false, message = "No medication selected or entered." });
                }

                // 2. Find the appointment and patient
                var appt = await _context.Appointments
                    .Include(a => a.Patient)
                    .FirstOrDefaultAsync(a => a.AppointmentId == appointmentId);
                if (appt == null)
                    return NotFound("Appointment not found.");

                // 3. Find existing medication invoice for this appointment, or create new
                var invoice = await _context.Invoices
     .FirstOrDefaultAsync(i => i.AppointmentId == appointmentId &&
                               i.IsMedicationInvoice);
                if (invoice == null)
                {
                    invoice = new Invoice
                    {
                        PatientId = appt.PatientId,
                        AppointmentId = appointmentId,
                        InvoiceNumber = $"MED-{appt.AppointmentNo}",
                        CustomerName = $"{appt.Patient.FirstName} {appt.Patient.LastName}",
                        CustomerEmail = appt.Patient.Email,
                        IssueDate = DateTime.Now,
                        DueDate = DateTime.Now.AddDays(7),
                        CurrencyCode = "PKR",
                        status = "Pending",
                        InvoiceType = InvoiceType.Medication,
                        IsMedicationInvoice = true,
                        CreatedBy = User.FindFirst("UserId")?.Value ?? "system",
                        UpdatedBy = User.FindFirst("UserId")?.Value ?? "system",
                        SubTotal = 0,
                        Total = 0
                    };
                    _context.Invoices.Add(invoice);
                    await _context.SaveChangesAsync();
                }

                // 4. Add medication line
                var line = new InvoiceItem
                {
                    InvoiceId = invoice.Id,
                    AppointmentId = appointmentId,
                    InventoryItemId = inventoryId.HasValue ? itemId : (int?)null,
                    MedicationsId = inventoryId.HasValue ? (int?)null : itemId,
                    Description = itemName,
                    Quantity = 1,
                    UnitPrice = itemPrice,
                    Total = itemPrice,
                    ItemType = "Medication"
                };
                _context.InvoiceItems.Add(line);

                // 5. Update invoice totals
                invoice.SubTotal += line.Total;
                invoice.Total += line.Total;
                _context.Invoices.Update(invoice);
                await _context.SaveChangesAsync();

                // 6. Return refreshed appointment medication list
                var updated = await _context.InvoiceItems
                .Where(ii => ii.AppointmentId == appointmentId && (ii.InventoryItemId != null || ii.MedicationsId != null))
                .Select(ii => new MedicationViewModel
                {
                    Id = ii.InventoryItemId ?? ii.MedicationsId ?? 0,
                    Name = ii.InventoryItem != null ? ii.InventoryItem.Name
                                                   : ii.Medications != null ? ii.Medications.Name
                                                                           : "Unknown",
                    UnitPrice = ii.UnitPrice
                })
                .ToListAsync();

                return Json(new { success = true, medications = updated });
            }
            catch (Exception ex)
            {
                // Log the exception as needed
                return Json(new { success = false, message = ex.Message });
            }
        }






        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> AddTestStandAloneInvoice(
        //      int appointmentId,
        //      string testName,
        //      decimal testCost)
        //{

        //    try
        //    {
        //        if (string.IsNullOrWhiteSpace(testName))
        //            return Json(new { success = false, message = "Test name is required." });

        //        var appt = await _context.Appointments
        //                                 .Include(a => a.Patient)
        //                                 .FirstOrDefaultAsync(a => a.AppointmentId == appointmentId);
        //        if (appt == null) return NotFound("Appointment not found.");

        //        /* find or create TEST invoice for this appointment */
        //        var inv = await _context.Invoices
        //                                .FirstOrDefaultAsync(i => i.AppointmentId == appointmentId
        //                                                       && i.IsLaboratoryInvoice);
        //        if (inv == null)
        //        {
        //            inv = new Invoice
        //            {
        //                PatientId = appt.PatientId,
        //                AppointmentId = appointmentId,
        //                CustomerName = $"{appt.Patient.FirstName} {appt.Patient.LastName}",
        //                CustomerEmail = appt.Patient.Email,
        //                IssueDate = DateTime.Now,
        //                DueDate = DateTime.Now.AddDays(7),
        //                CurrencyCode = "PKR",
        //                status = "Pending",
        //                IsLaboratoryInvoice = true,
        //                CreatedBy = User.FindFirst("UserId")?.Value ?? "system",
        //                UpdatedBy = User.FindFirst("UserId")?.Value ?? "system",
        //                SubTotal = 0,
        //                Total = 0
        //            };
        //            _context.Invoices.Add(inv);
        //            await _context.SaveChangesAsync();
        //        }

        //        /* add line */
        //        var line = new InvoiceItem
        //        {
        //            InvoiceId = inv.Id,
        //            AppointmentId = appointmentId,
        //            Description = testName,
        //            Quantity = 1,
        //            UnitPrice = testCost,
        //            Total = testCost
        //        };
        //        _context.InvoiceItems.Add(line);
        //        inv.SubTotal += line.Total;
        //        inv.Total += line.Total;
        //        await _context.SaveChangesAsync();

        //        /* return refreshed list */
        //        var updated = await _context.InvoiceItems
        //                                    .Where(ii => ii.AppointmentId == appointmentId
        //                                              && ii.Invoice.IsLaboratoryInvoice)
        //                                    .Select(ii => new TestVm { Name = ii.Description, Cost = ii.UnitPrice, Id = ii.Id })
        //                                    .ToListAsync();
        //        return Json(new { success = true, tests = updated });
        //    }
        //    catch (Exception ex)
        //    {
        //        return Json(new { success = false, message = ex.Message });
        //    }
        //}

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddTestStandAloneInvoice(
      int appointmentId,
      string testName,
      decimal testCost)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(testName))
                    return Json(new { success = false, message = "Test name is required." });

                var appt = await _context.Appointments
                                        .Include(a => a.Patient)
                                        .FirstOrDefaultAsync(a => a.AppointmentId == appointmentId);

                if (appt == null)
                    return NotFound("Appointment not found.");

                // Find existing TEST invoice for this appointment
                var inv = await _context.Invoices
                                       .FirstOrDefaultAsync(i => i.AppointmentId == appointmentId && i.IsLaboratoryInvoice);

                if (inv == null)
                {
                    // Create a new invoice if none exists for this appointment
                    inv = new Invoice
                    {
                        PatientId = appt.PatientId,
                        AppointmentId = appointmentId,
                        InvoiceNumber = $"Lab-{appt.AppointmentNo}",
                        CustomerName = $"{appt.Patient.FirstName} {appt.Patient.LastName}",
                        CustomerEmail = appt.Patient.Email,
                        IssueDate = DateTime.Now,
                        DueDate = DateTime.Now.AddDays(7),
                        CurrencyCode = "PKR",
                        status = "Pending",
                        InvoiceType = InvoiceType.Laboratory,
                        IsLaboratoryInvoice = true,
                        CreatedBy = User.FindFirst("UserId")?.Value ?? "system",
                        UpdatedBy = User.FindFirst("UserId")?.Value ?? "system",
                        SubTotal = 0,
                        Total = 0
                    };

                    _context.Invoices.Add(inv);
                    
                    await _context.SaveChangesAsync();
                }
                else if (!inv.IsLaboratoryInvoice)
                {
                    // If the existing invoice is not a laboratory invoice, create a new one
                    inv = new Invoice
                    {
                        PatientId = appt.PatientId,
                        AppointmentId = appointmentId,
                        CustomerName = $"{appt.Patient.FirstName} {appt.Patient.LastName}",
                        CustomerEmail = appt.Patient.Email,
                        IssueDate = DateTime.Now,
                        DueDate = DateTime.Now.AddDays(7),
                        CurrencyCode = "PKR",
                        status = "Pending",
                        IsLaboratoryInvoice = true,
                        CreatedBy = User.FindFirst("UserId")?.Value ?? "system",
                        UpdatedBy = User.FindFirst("UserId")?.Value ?? "system",
                        SubTotal = 0,
                        Total = 0
                    };

                    _context.Invoices.Add(inv);
                    await _context.SaveChangesAsync();
                }

                // Add invoice item
                var line = new InvoiceItem
                {
                    InvoiceId = inv.Id,
                    AppointmentId = appointmentId,
                    TestName = testName,
                    Quantity = 1,
                    UnitPrice = testCost,
                    Total = testCost,
                    ItemType = "Test"
                };

                _context.InvoiceItems.Add(line);

                // Update invoice totals
                inv.SubTotal += line.Total;
                inv.Total += line.Total;
                _context.Update(inv);

                // Save the changes
                await _context.SaveChangesAsync();

                // Return updated list of items for the invoice
                var updated = await _context.InvoiceItems
                                            .Where(ii => ii.AppointmentId == appointmentId
                                                      && ii.Invoice.IsLaboratoryInvoice)
                                            .Select(ii => new TestVm { Name = ii.Description, UnitPrice = ii.UnitPrice, Id = ii.Id })
                                            .ToListAsync();

                return Json(new { success = true, tests = updated });
            }
            catch (Exception ex)
            {
                // Handle any exception that occurs and provide a meaningful message
                return Json(new { success = false, message = $"An error occurred: {ex.Message}" });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveTest(int id)   // id = InvoiceItem.Id
        {
            try
            {
                var line = await _context.InvoiceItems.FindAsync(id);
                if (line == null) return Json(new { success = false });

                var inv = await _context.Invoices.FindAsync(line.InvoiceId);
                if (inv != null)
                {
                    inv.SubTotal -= line.Total;
                    inv.Total -= line.Total;
                }
                _context.InvoiceItems.Remove(line);
                await _context.SaveChangesAsync();

                var updated = await _context.InvoiceItems
                                           .Where(ii => ii.AppointmentId == line.AppointmentId
                                                     && ii.Invoice.IsLaboratoryInvoice)
                                           .Select(ii => new TestVm { Name = ii.Description, UnitPrice = ii.UnitPrice, Id = ii.Id })
                                           .ToListAsync();
                return Json(new { success = true, tests = updated });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });

            }
        }


        [HttpGet]
        public async Task<IActionResult> GetTests(int appointmentId)
        {
            var tests = await _context.InvoiceItems
                .Include(ii => ii.Invoice)                    // 🔑 ensure join
                .Where(ii => ii.Invoice.AppointmentId == appointmentId
                         && ii.Invoice.IsLaboratoryInvoice)    // optional lab filter
                .Select(ii => new TestVm
                {
                    Id = ii.Id,
                    Name = ii.Description,
                    UnitPrice = ii.UnitPrice
                })
                .ToListAsync();

            return Json(new { tests });
        }



[HttpGet]
public async Task<IActionResult> GetTreatments(int appointmentId)
        {
            try
            {
                var treatments = await _context.Treatments
                   .Select(t => new SelectListItem
                   {
                       Value = t.TreatmentId.ToString(),
                       Text = $"{t.Name} - ${t.UnitPrice:F2}"
                   })
                   .ToListAsync();
                

                return Json(new { treatments });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching treatments for appointment {AppointmentId}", appointmentId);
                return StatusCode(500, new { message = "Failed to load treatments." });
            }
        }

        // GET: Get medications for an appointment
        [HttpGet]
        public async Task<IActionResult> GetMedications(int appointmentId)
        {
            try
            {
                var medications = await _context.InvoiceItems
                    .Where(ii => ii.AppointmentId == appointmentId &&
                                (ii.InventoryItemId != null || ii.MedicationsId != null))
                    .Include(ii => ii.InventoryItem)
                    
                    .Select(ii => new MedicationViewModel
                    {
                        Id = ii.InventoryItemId ?? ii.MedicationsId ?? 0,
                        Name = ii.InventoryItem != null ? ii.InventoryItem.Name
                                 : ii.Medications != null ? ii.Medications.Name
                                 : "Unknown",
                        UnitPrice = ii.UnitPrice
                    })
                    .ToListAsync();

                return Json(new { medications });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching medications for appointment {AppointmentId}", appointmentId);
                return StatusCode(500, new { message = "Failed to load medications." });
            }
        }


    }
}

