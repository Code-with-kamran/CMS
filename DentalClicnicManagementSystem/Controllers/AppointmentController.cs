// ==============================
// File: Controllers/AppointmentController.cs
// ==============================



using CMS.Data;
using CMS.Models;
using CMS.Services; 
using CMS.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Linq.Expressions;

namespace CMS.Controllers
{
    public class AppointmentController : Controller
    {
        private readonly ApplicationDbContext _context;

        private readonly IEmailSender _email;                

        public AppointmentController(ApplicationDbContext context, IEmailSender email) 
        {
            _context = context;
            _email = email;                    
        }

        // ===== Availability helpers (weekly schedule repeats forever, with optional date overrides) =====
        private readonly TimeSpan DefaultSlot = TimeSpan.FromMinutes(30);
        private record AvailabilityWindow(TimeSpan Start, TimeSpan End);

        // 1) Take DoctorDateAvailability if present for that date; else fall back to DoctorWeeklyAvailability for that DayOfWeek.
        private async Task<List<AvailabilityWindow>> GetEffectiveAvailabilityWindowsAsync(int doctorId, DateTime date)
        {
            // Date-specific overrides
            var dateAvail = await _context.DoctorDateAvailabilities
                .Where(x => x.DoctorId == doctorId && x.Date.HasValue && x.Date.Value.Date == date.Date)
                .ToListAsync();

            if (dateAvail.Count > 0)
            {
                bool hardBlock = dateAvail.Any(x => !x.IsAvailable && !x.StartTime.HasValue && !x.EndTime.HasValue);
                var positive = dateAvail
                    .Where(x => x.IsAvailable && x.StartTime.HasValue && x.EndTime.HasValue && x.EndTime > x.StartTime)
                    .Select(x => new AvailabilityWindow(x.StartTime!.Value, x.EndTime!.Value))
                    .ToList();

                return (hardBlock && positive.Count == 0) ? new() : positive;
            }

            // Weekly template (repeats by DayOfWeek)
            var dow = date.DayOfWeek;
            var weekly = await _context.DoctorWeeklyAvailabilities
                .Where(a => a.DoctorId == doctorId
                            && a.IsWorkingDay
                            && a.DayOfWeek.HasValue
                            && a.DayOfWeek == dow)
                .ToListAsync();

            return weekly
                .Where(a => a.EndTime > a.StartTime)
                .Select(a => new AvailabilityWindow(a.StartTime, a.EndTime))
                .ToList();
        }

        private static bool FitsInWindows(TimeSpan start, TimeSpan duration, IEnumerable<AvailabilityWindow> windows)
        {
            var end = start + duration;
            return windows.Any(w => start >= w.Start && end <= w.End);
        }

        // 2) Server-side validator (doctor exists, works that date/time, and no overlap)
        private async Task<(bool ok, string? message)> ValidateAppointmentAsync(Appointment appt)
        {
            if (appt == null) return (false, "Invalid appointment.");
            if (appt.DoctorId <= 0) return (false, "Please select a doctor.");
            if (appt.PatientId <= 0) return (false, "Please select a patient.");
            if (appt.AppointmentDate == default) return (false, "Please select a valid date/time.");

            var doctor = await _context.Doctors.AsNoTracking().FirstOrDefaultAsync(d => d.Id == appt.DoctorId);
            if (doctor == null) return (false, "Doctor not found.");

            var duration = doctor.ConsultationDurationInMinutes > 0
                ? TimeSpan.FromMinutes(doctor.ConsultationDurationInMinutes)
                : DefaultSlot;

            var windows = await GetEffectiveAvailabilityWindowsAsync(appt.DoctorId, appt.AppointmentDate);
            if (windows.Count == 0) return (false, "Doctor is not available on the selected date.");

            var start = appt.AppointmentDate.TimeOfDay;
            if (!FitsInWindows(start, duration, windows))
                return (false, "Selected time is outside doctor's working hours.");

            var sameDayStarts = await _context.Appointments
                .Where(a => a.DoctorId == appt.DoctorId
                         && a.AppointmentDate.Date == appt.AppointmentDate.Date
                         && a.AppointmentId != appt.AppointmentId)
                .Select(a => a.AppointmentDate.TimeOfDay)
                .ToListAsync();

            var end = start + duration;
            bool overlaps = sameDayStarts.Any(existingStart =>
            {
                var existingEnd = existingStart + duration;
                return existingStart < end && start < existingEnd;
            });

            if (overlaps) return (false, "This time overlaps with another appointment.");
            return (true, null);
        }

        // ===== Dropdown population (Department removed) =====
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
        [HttpGet]
        public async Task<IActionResult> Upsert(int? id)
        {
            var vm = new AppointmentUpsertVM
            {
                Appointment = id == null
                    ? new Appointment()
                    : await _context.Appointments.FindAsync(id)
            };

            await PopulateDropdowns(vm);
            return View(vm);
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upsert(AppointmentUpsertVM vm)
        {
            if (!ModelState.IsValid)
            {
                await PopulateDropdowns(vm);
                return View(vm);
            }

            var (ok, message) = await ValidateAppointmentAsync(vm.Appointment);
            if (!ok)
            {
                ModelState.AddModelError("Appointment.AppointmentDate", message ?? "Selected time is not available.");
                await PopulateDropdowns(vm);
                return View(vm);
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
                    catch
                    {
                        // log and continue; do not break receptionist flow if SMTP fails
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
                ModelState.AddModelError("Appointment.AppointmentDate", "That time has just been booked by someone else. Please pick another slot.");
                await PopulateDropdowns(vm);
                return View(vm);
            }

            return RedirectToAction(nameof(Index));
        }

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Upsert(AppointmentUpsertVM vm)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        await PopulateDropdowns(vm);
        //        return View(vm);
        //    }

        //    var (ok, message) = await ValidateAppointmentAsync(vm.Appointment);
        //    if (!ok)
        //    {
        //        ModelState.AddModelError("Appointment.AppointmentDate", message ?? "Selected time is not available.");
        //        await PopulateDropdowns(vm);
        //        return View(vm);
        //    }

        //    bool isNew = vm.Appointment.AppointmentId == 0;

        //    try
        //    {
        //        if (isNew)
        //        {
        //            _context.Appointments.Add(vm.Appointment);
        //            await _context.SaveChangesAsync();
        //            await AddAppointmentToInvoice(vm.Appointment);
        //            try
        //            {
        //                await SendAppointmentCreatedEmailAsync(vm.Appointment.AppointmentId);
        //            }
        //            catch
        //            {
        //                // log and continue; do not break receptionist flow if SMTP fails
        //            }
        //        }
        //        else
        //        {
        //            _context.Appointments.Update(vm.Appointment);
        //            await _context.SaveChangesAsync();
        //        }
        //    }
        //    catch (DbUpdateException)
        //    {
        //        ModelState.AddModelError("Appointment.AppointmentDate", "That time has just been booked by someone else. Please pick another slot.");
        //        await PopulateDropdowns(vm);
        //        return View(vm);
        //    }

        //    return RedirectToAction(nameof(Index));
        //}

        // Your main method that is calling AddAppointmentToInvoice
        private async Task AddAppointmentToInvoice(Appointment appointment)
        {
            if (appointment == null || appointment.DoctorId == 0) return;

            var patient = await _context.Patients.FindAsync(appointment.PatientId);
            var doctor = await _context.Doctors.FindAsync(appointment.DoctorId);
            if (patient == null || doctor == null) return;

            // Get the doctor's consultation charge
            decimal consultationCharge = doctor.ConsultationCharge;

            // Initialize charges for the appointment (you can add more charge types if needed)
            decimal additionalCharge = 0;  // Example: additional charges from the appointment if applicable

            decimal subtotal = consultationCharge + additionalCharge;  // Sum of all charges for the appointment

            decimal tax = 0; // Add your tax logic here if applicable
            decimal discount = 0; // Add your discount logic here if applicable

            // Calculate total: subtotal + tax - discount
            decimal total = subtotal + tax - discount;

            // Create the invoice
            var invoice = new Invoice
            {
                AppointmentId = appointment.AppointmentId,
                PatientId = patient.PatientId,
                DoctorId = doctor.Id,
                Patient = patient,
                Doctor = doctor,
                CustomerName = patient.FirstName + " " + patient.LastName,
                CustomerEmail = patient.Email,
                CustomerAddress = patient.Address,
                SubTotal = subtotal, // Set the subtotal to the sum of all charges
                Tax = tax,
                Discount = discount,
                Total = total, // Final total after tax and discount
                CurrencyCode = "PKR", // Set the currency code to PKR (or as applicable)
                IssueDate = DateTime.Now, // Set the issue date
                DueDate = DateTime.Now.AddDays(7), // Set the due date (7 days from now)
                Status = "Pending", // Default status
                Items = new List<InvoiceItem>
        {
            // Create an invoice item for the doctor's consultation
            new InvoiceItem
            {
                Description = "Consultation with Dr. " + doctor.FullName,
                Quantity = 1, // Assuming 1 unit of the service
                UnitPrice = consultationCharge // Unit price is the consultation charge
            },
            // You can add more items related to the appointment here if necessary
            new InvoiceItem
            {
                Description = "Additional charges for appointment", // Example additional charge description
                Quantity = 1, // Example quantity
                UnitPrice = additionalCharge // Additional charge amount
            }
        }
            };

            // Add the invoice to the context and save changes
            _context.Invoices.Add(invoice);
            await _context.SaveChangesAsync();
        }

        //private async Task AddAppointmentToInvoice(Appointment appointment)
        //{
        //    if (appointment == null || appointment.DoctorId == 0) return;

        //    var patient = await _context.Patients.FindAsync(appointment.PatientId);
        //    var doctor = await _context.Doctors.FindAsync(appointment.DoctorId);
        //    if (patient == null || doctor == null) return;

        //    Dictionary<string,decimal> subTotal = doctor.ConsultationCharge;
        //    decimal tax = 0; // Add tax logic if applicable
        //    decimal discount = 0; // Add discount logic if applicable
        //    decimal total = subTotal + tax - discount;

        //    var invoice = new Invoice
        //    {
        //        AppointmentId = appointment.AppointmentId,
        //        PatientId = patient.PatientId,
        //        DoctorId = doctor.Id,
        //        Patient = patient,
        //        Doctor = doctor,
        //        CustomerName = patient.FirstName + " " + patient.LastName,
        //        CustomerEmail = patient.Email,
        //        CustomerAddress = patient.Address,
        //        SubTotal = subTotal,
        //        Tax = tax,
        //        Discount = discount,
        //        Total = total,
        //        CurrencyCode = "PKR",
        //        IssueDate = DateTime.Now,
        //        DueDate = DateTime.Now.AddDays(7),
        //        Status = "Pending",
        //        Items = new List<InvoiceItem>
        //{
        //    new InvoiceItem
        //    {
        //        Description = "Appointment with Dr. " + doctor.FullName,
        //        Quantity = 1,
        //        UnitPrice = doctor.ConsultationCharge
        //    }
        //}
        //    };

        //    _context.Invoices.Add(invoice);
        //    await _context.SaveChangesAsync();
        //}

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

        // ===== Week grid API: returns 7 days with free slots; marks fully booked days
        // 
        [HttpGet]
        public async Task<IActionResult> GetWeekSlots(int doctorId, DateTime start)
        {
            // 1. Basic doctor check
            var doctor = await _context.Doctors
                                       .AsNoTracking()
                                       .FirstOrDefaultAsync(d => d.Id == doctorId);
            if (doctor == null)
                return BadRequest("Doctor not found.");

            var duration = doctor.ConsultationDurationInMinutes > 0
                ? TimeSpan.FromMinutes(doctor.ConsultationDurationInMinutes)
                : TimeSpan.FromMinutes(30);

            // 2. Work only with the single date that was asked for
            var date = start.Date;

            // 3. Availability windows for that date
            var windows = await GetEffectiveAvailabilityWindowsAsync(doctorId, date);

            // 4. Already booked times on that date
            var booked = await _context.Appointments
                .Where(a => a.DoctorId == doctorId &&
                            a.AppointmentDate.Date == date.Date)
                .Select(a => a.AppointmentDate.TimeOfDay)
                .ToListAsync();

            // 5. Build free slots  ->  List<{ iso, label }>
            var slots = windows
                .SelectMany(w =>
                {
                    var list = new List<(TimeSpan start, TimeSpan end)>();
                    for (var t = w.Start; t + duration <= w.End; t = t.Add(duration))
                        list.Add((t, t.Add(duration)));
                    return list;
                })
                .Where(s => !booked.Any(b => b < s.end && s.start < b.Add(duration))) // no overlap
                .Distinct()
                .OrderBy(s => s.start)
                .Select(s => new
                {
                    iso = (date + s.start).ToString("yyyy-MM-ddTHH:mm"),
                    label = $"{(date + s.start):hh:mm tt} – {(date + s.end):hh:mm tt}"
                })
                .ToList();

            // 6. Single-day payload
            var result = new[]
            {
        new
        {
            date = date.ToString("yyyy-MM-dd"),
            day  = date.ToString("ddd, dd MMMM yyyy", CultureInfo.InvariantCulture),
            full = date.ToString("dd MMM yyyy"),
            isFullyBooked = slots.Count == 0,
            slots          // now contains { iso: "...", label: "..." }
        }
    };

            return Json(result);
        }
        //[HttpGet]
        //public async Task<IActionResult> GetWeekSlots(int doctorId, DateTime start)
        //{
        //    var doctor = await _context.Doctors.AsNoTracking().FirstOrDefaultAsync(d => d.Id == doctorId);
        //    if (doctor == null) return BadRequest("Doctor not found.");

        //    var duration = doctor.ConsultationDurationInMinutes > 0
        //        ? TimeSpan.FromMinutes(doctor.ConsultationDurationInMinutes)
        //        : TimeSpan.FromMinutes(30);

        //    var s = start.Date;
        //    var results = new List<object>();

        //    for (int i = 0; i < 7; i++)
        //    {
        //        var date = s.AddDays(i);
        //        var windows = await GetEffectiveAvailabilityWindowsAsync(doctorId, date);

        //        var booked = await _context.Appointments
        //            .Where(a => a.DoctorId == doctorId && a.AppointmentDate.Date == date.Date)
        //            .Select(a => a.AppointmentDate.TimeOfDay)
        //            .ToListAsync();

        //        var allSlots = windows.SelectMany(w =>
        //        {
        //            var list = new List<TimeSpan>();
        //            for (var t = w.Start; t + duration <= w.End; t = t.Add(duration))
        //                list.Add(t);
        //            return list;
        //        });

        //        var free = allSlots
        //            .Where(s0 => !booked.Any(b => b < s0 + duration && s0 < b + duration))
        //            .Distinct()
        //            .OrderBy(s0 => s0)
        //            .Select(ts => (date.Date + ts).ToString("yyyy-MM-ddTHH:mm"))
        //            .ToArray();

        //        results.Add(new
        //        {
        //            date = date.ToString("yyyy-MM-dd"),
        //            day = date.ToString("ddd").ToUpperInvariant(),
        //            full = date.ToString("dd MMM yyyy"),
        //            isFullyBooked = free.Length == 0,
        //            slots = free
        //        });
        //    }

        //    return Json(results);
        //}

        // ===== DataTable + Delete (unchanged) =====
        [HttpGet]
        public async Task<IActionResult> GetAll()
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

                int recordsFiltered = await q.CountAsync();

                Expression<Func<Appointment, object>> sortSelector = orderColIndex switch
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            var appointment = _context.Appointments.FirstOrDefault(a => a.AppointmentId == id);
            if (appointment == null)
            {
                return Json(new { success = false, message = "Error while deleting" });
            }

            _context.Appointments.Remove(appointment);
            _context.SaveChanges();
            return Json(new { success = true, message = "Delete successful" });
        }

        private string NextAppointmentNo()
        {
            return $"AP{DateTime.UtcNow:yyyyMMddHHmmssfff}";
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
    }
}

