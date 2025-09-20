using System.Globalization;
using System.Text.Json;
using CMS.Data;
using CMS.Models;
using CMS.Services;
using CMS.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace CMS.Controllers
{
    [AutoValidateAntiforgeryToken]
    public class PublicAppointmentsController : Controller
    {
        private const string PatientDataKey = "PatientData";
        private const string PatientTempIdKey = "PatientTempId";

        private readonly ApplicationDbContext _context;
        private readonly SlotBuilder _slotBuilder;
        private readonly Services.IEmailSender _email;

        public PublicAppointmentsController(ApplicationDbContext context, SlotBuilder slotBuilder, Services.IEmailSender email)
        {
            _context = context;
            _slotBuilder = slotBuilder;
            _email = email;
        }

        // GET: /PublicAppointments/Index
        public async Task<IActionResult> Index()
        {
            var vm = new PublicAppointmentVM
            {
                PreferredDate = DateTime.Today,
                SpecializationOptions = await GetSpecializationOptionsAsync()
            };
            return View(vm);
        }

        // POST: /PublicAppointments/Index
        [HttpPost]
        public async Task<IActionResult> Index(PublicAppointmentVM model)
        {
            if (!ModelState.IsValid)
            {
                model.SpecializationOptions = await GetSpecializationOptionsAsync();
                return View(model);
            }

            // Keep patient payload for fallback Select step
            TempData[PatientDataKey] = JsonSerializer.Serialize(model);
            TempData[PatientTempIdKey] = Guid.NewGuid().ToString();

            // Try auto-assign for preferred specialization
            var avail = await GetAvailableDoctorsAsync(DateOnly.FromDateTime(model.PreferredDate), model.PreferredSpecialization);
            var hasAnySlot = avail.Any() && avail.SelectMany(d => d.Slots).Any();

            if (hasAnySlot)
            {
                // pick earliest slot from the cheapest doctor (adjust rule if needed)
                var chosenDoctor = avail.OrderBy(d => d.Fee).First(d => d.Slots.Any());
                var chosenSlotText = EarliestSlotString(chosenDoctor.Slots);

                var confirmVm = new PublicAppointmenConfirmfVM
                {
                    DoctorId = chosenDoctor.Id,
                    SelectedSlot = chosenSlotText,
                    AppointmentDate = model.PreferredDate.Date
                };

                var appointmentId = await BookAsync(confirmVm, model, autoAssign: true);
                if (appointmentId > 0)
                {
                    // AJAX callers: return JSON; Non-AJAX: show popup on Index
                    if (appointmentId > 0)
                    {
                        if (IsAjax())
                            return Ok(new { success = true, redirectUrl = Url.Action(nameof(ThankYou), new { id = appointmentId }) });

                        return RedirectToAction(nameof(ThankYou), new { id = appointmentId });
                    }


                    TempData["SweetAlert"] = JsonSerializer.Serialize(new
                    {
                        title = "Appointment Confirmed",
                        text = $"Your appointment has been scheduled with {chosenDoctor.Name} on {model.PreferredDate:ddd, MMM d} at {chosenSlotText}.",
                        icon = "success"
                    });
                    return RedirectToAction(nameof(Index));
                }
                // else: race -> fallthrough to Select
            }

            // No slots (or failed auto-book due to race) → go to Select page (browse all)
            return RedirectToAction(nameof(Select));
        }

        // AJAX: Availability (returns FREE ∪ BOOKED)
        [HttpPost]
        public async Task<IActionResult> Availability([FromBody] AvailabilityRequestDto request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var doctors = await GetAvailableDoctorsAsync(request.PreferredDate, request.PreferredSpecialization);
            var response = new AvailabilityResponseDto
            {
                Date = request.PreferredDate.ToString("yyyy-MM-dd"),
                Doctors = doctors
            };
            return Ok(response);
        }

        // GET: /PublicAppointments/Select
        [HttpGet]
        public async Task<IActionResult> Select()
        {
            var patientDataJson = TempData[PatientDataKey]?.ToString();
            var patientTempId = TempData[PatientTempIdKey]?.ToString();
            if (string.IsNullOrEmpty(patientDataJson) || string.IsNullOrEmpty(patientTempId))
                return RedirectToAction(nameof(Index));

            var patient = JsonSerializer.Deserialize<PublicAppointmentVM>(patientDataJson)!;

            var availableDoctors = await GetAvailableDoctorsAsync(DateOnly.FromDateTime(patient.PreferredDate), null);

            var model = new PublicAppointmenConfirmfVM
            {
                PatientTempId = patientTempId!,
                AppointmentDate = patient.PreferredDate.Date,
                PatientName = patient.FullName,
                Email = patient.Email,
                PhoneNumber = patient.PhoneNumber,
                Gender = patient.Gender,
                DateOfBirth = patient.DateOfBirth,
                Address = patient.Address,
                Notes = patient.Notes,
                AvailableDoctors = availableDoctors
            };

            TempData.Keep(PatientDataKey);
            TempData.Keep(PatientTempIdKey);

            return View(model);
        }

        // Non-JS fallback only (AJAX flow uses BookSlot)
        [HttpPost]
        public async Task<IActionResult> Select(PublicAppointmenConfirmfVM model)
        {
            var patientDataJson = TempData[PatientDataKey]?.ToString();
            if (string.IsNullOrEmpty(patientDataJson))
                return RedirectToAction(nameof(Index));

            if (!ModelState.IsValid)
            {
                model.AvailableDoctors = await GetAvailableDoctorsAsync(DateOnly.FromDateTime(model.AppointmentDate), null);
                return View(model);
            }

            var patient = JsonSerializer.Deserialize<PublicAppointmentVM>(patientDataJson)!;

            var appointmentId = await BookAsync(model, patient, autoAssign: false);
            if (appointmentId > 0)
            {
                TempData.Remove("PatientData");
                TempData.Remove("PatientTempId");
                return RedirectToAction(nameof(ThankYou), new { id = appointmentId }); // <-- redirect
            }


            // booking failed (e.g., race condition) → reload selections
            ModelState.AddModelError("", "Selected slot was just booked. Please choose another.");
            model.AvailableDoctors = await GetAvailableDoctorsAsync(DateOnly.FromDateTime(model.AppointmentDate), null);
            return View(model);
        }

        // AJAX: Book a slot (no redirect; popup in UI)
        [HttpPost]
        public async Task<IActionResult> BookSlot([FromBody] BookSlotRequestDto req)
        {
            var patientJson = TempData[PatientDataKey] as string;
            if (string.IsNullOrWhiteSpace(patientJson))
                return BadRequest(new { success = false, message = "Session expired. Please start again." });

            TempData.Keep(PatientDataKey);
            TempData.Keep(PatientTempIdKey);

            var patient = JsonSerializer.Deserialize<PublicAppointmentVM>(patientJson)!;

            var confirm = new PublicAppointmenConfirmfVM
            {
                DoctorId = req.DoctorId,
                SelectedSlot = req.SelectedSlot,
                AppointmentDate = req.AppointmentDate
            };

            var appointmentId = await BookAsync(confirm, patient, autoAssign: false);
            if (appointmentId <= 0)
                return Ok(new { success = false, message = "That slot was just booked. Please choose another." });

            var doctor = await _context.Doctors.AsNoTracking().FirstOrDefaultAsync(d => d.Id == req.DoctorId);
            var appt = await _context.Appointments.AsNoTracking().FirstOrDefaultAsync(a => a.AppointmentId == appointmentId);

            return Ok(new
            {
                success = true,
                appointmentId,
                appointmentNo = appt?.AppointmentNo,
                doctorName = doctor?.FullName,
                doctorSpecialty = doctor?.Specialty,
                time = req.SelectedSlot,
                date = req.AppointmentDate.ToString("dddd, MMM d, yyyy"),
                redirectUrl = Url.Action(nameof(ThankYou), new { id = appointmentId })
            });
        }

        // ================== helpers ==================
        private async Task<int> BookAsync(PublicAppointmenConfirmfVM confirm, PublicAppointmentVM patientData, bool autoAssign)
        {
            if (!TimeOnly.TryParse(confirm.SelectedSlot, out var selectedTime))
                return 0;

            var appointmentDateTime = confirm.AppointmentDate.Date.Add(selectedTime.ToTimeSpan());

            int appointmentId = 0;
            var patientFullName = patientData.FullName?.Trim();
            var patientEmail = string.IsNullOrWhiteSpace(patientData.Email) ? null : patientData.Email!.Trim();

            string doctorNameForEmail = "";
            string doctorSpecForEmail = "";
            decimal doctorFeeForEmail = 0m;
            DateTime appointmentDateTimeForEmail = appointmentDateTime;
            string appointmentNoForEmail = "";

            var committed = false;

            await using var trx = await _context.Database.BeginTransactionAsync();
            try
            {
                // final conflict check
                var exists = await _context.Appointments.AsNoTracking()
                    .AnyAsync(a => a.DoctorId == confirm.DoctorId && a.AppointmentDate == appointmentDateTime);
                if (exists) return 0;

                // Upsert patient by email
                var patient = await _context.Patients.FirstOrDefaultAsync(p => p.Email == patientEmail);
                if (patient == null)
                {
                    var parts = (patientFullName ?? "").Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
                    var first = parts.ElementAtOrDefault(0) ?? "";
                    var last = parts.ElementAtOrDefault(1) ?? "";

                    // SAFE DOB handling
                    // If your Patient.DateOfBirth column is NULLABLE: assign patientData.DateOfBirth directly.
                    // If it's NOT nullable: coalesce to a safe sentinel supported by SQL 'datetime'.
                    // Choose ONE of the following lines:

                    // (A) Column is nullable (recommended)
                    var safeDob = patientData.DateOfBirth;                    // DateTime?

                    // (B) Column is NOT nullable (use a sentinel)
                    // var safeDob = patientData.DateOfBirth ?? new DateTime(1900, 1, 1);

                    patient = new Patient
                    {
                        FirstName = first,
                        LastName = last,
                        Email = patientEmail!,         // you likely have [Required] on Email
                        PhoneNumber = patientData.PhoneNumber,
                        Gender = patientData.Gender,
                        DateOfBirth = safeDob,               // ✅ no invalid cast
                        Address = patientData.Address,
                        CreatedDate = DateTime.Now
                    };
                    _context.Patients.Add(patient);
                    await _context.SaveChangesAsync();
                }

                var doctor = await _context.Doctors.FindAsync(confirm.DoctorId);
                if (doctor == null) return 0;

                var appt = new Appointment
                {
                    PatientId = patient.PatientId,
                    DoctorId = confirm.DoctorId,
                    AppointmentDate = appointmentDateTime,
                    CreatedOn = DateTime.Now,
                    Fee = doctor.ConsultationCharge,
                    Status = "Scheduled",
                    AppointmentType = "General",
                    Notes = patientData.Notes,
                    AppointmentNo = GenerateAppointmentNumber()
                };

                _context.Appointments.Add(appt);
                await _context.SaveChangesAsync();

                // capture info for email
                doctorNameForEmail = doctor.FullName;
                doctorSpecForEmail = doctor.Specialty;
                doctorFeeForEmail = doctor.ConsultationCharge;
                appointmentNoForEmail = appt.AppointmentNo;
                appointmentId = appt.AppointmentId;

                await trx.CommitAsync();
                committed = true;
            }
             catch
            {
                if (!committed)
                {
                    try { await trx.RollbackAsync(); } catch { /* ignore */ }
                }
                throw;
            }

            // send email outside transaction; skip if email is missing
            if (!string.IsNullOrWhiteSpace(patientEmail))
            {
                try
                {
                    var subject = "Your Appointment Confirmation";
                    var body = $@"
                <p>Dear {System.Net.WebUtility.HtmlEncode(patientFullName)},</p>
                <p>Your appointment has been scheduled.</p>
                <ul>
                  <li><b>Doctor:</b> {System.Net.WebUtility.HtmlEncode(doctorNameForEmail)} ({System.Net.WebUtility.HtmlEncode(doctorSpecForEmail)})</li>
                  <li><b>Date:</b> {appointmentDateTimeForEmail:dddd, MMM d, yyyy}</li>
                  <li><b>Time:</b> {appointmentDateTimeForEmail:hh:mm tt}</li>
                  <li><b>Fee:</b> {doctorFeeForEmail:C}</li>
                  <li><b>Appointment No:</b> {System.Net.WebUtility.HtmlEncode(appointmentNoForEmail)}</li>
                </ul>
                <p>Thank you.</p>";
                    await _email.SendAsync(patientEmail, subject, body);
                }
                catch { /* log & continue */ }
            }

            return appointmentId;
        }

        private async Task<List<DoctorAvailabilityDto>> GetAvailableDoctorsAsync(DateOnly date, string? specialty)
        {
            var dayOfWeek = date.DayOfWeek;

            var query = _context.Doctors
                .AsNoTracking()
                .Include(d => d.WeeklyAvailabilities)
                .Where(d => d.WeeklyAvailabilities.Any(wa => wa.DayOfWeek == dayOfWeek && wa.IsWorkingDay));

            if (!string.IsNullOrWhiteSpace(specialty))
                query = query.Where(d => d.Specialty.Contains(specialty));

            var doctors = await query.ToListAsync();

            var start = date.ToDateTime(TimeOnly.MinValue);
            var end = start.AddDays(1);

            var booked = await _context.Appointments.AsNoTracking()
                .Where(a => a.AppointmentDate >= start && a.AppointmentDate < end)
                .Select(a => new { a.DoctorId, a.AppointmentDate })
                .ToListAsync();

            // Pre-group booked times per doctor for faster lookup
            var bookedMap = booked
                .GroupBy(b => b.DoctorId)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(x => x.AppointmentDate.ToString("HH:mm"))
                          .ToHashSet(StringComparer.Ordinal));

            var result = new List<DoctorAvailabilityDto>();

            foreach (var doctor in doctors)
            {
                var freeTimes = await _slotBuilder.BuildSlotsAsync(doctor.Id, date); // likely only FREE
                var freeKeys = freeTimes.Select(t => t.ToString("HH:mm")).ToHashSet(StringComparer.Ordinal);
                var bookedKeys = bookedMap.TryGetValue(doctor.Id, out var set) ? set : new HashSet<string>(StringComparer.Ordinal);

                // ALL = FREE ∪ BOOKED (display format)
                var allSlots = freeKeys.Union(bookedKeys)
                    .Select(k => TimeOnly.ParseExact(k, "HH:mm"))
                    .OrderBy(t => t)
                    .Select(t => t.ToString("hh:mm tt"))
                    .ToList();

                var bookedDisplay = bookedKeys
                    .Select(k => TimeOnly.ParseExact(k, "HH:mm"))
                    .OrderBy(t => t)
                    .Select(t => t.ToString("hh:mm tt"))
                    .ToList();

                if (allSlots.Any())
                {
                    result.Add(new DoctorAvailabilityDto
                    {
                        Id = doctor.Id,
                        Name = doctor.FullName,
                        Specialization = doctor.Specialty,
                        Fee = doctor.ConsultationCharge,
                        Slots = allSlots,
                        BookedSlots = bookedDisplay
                    });
                }
            }

            return result;
        }

        private async Task<IEnumerable<SelectListItem>> GetSpecializationOptionsAsync()
        {
            var specs = await _context.Doctors.AsNoTracking()
                .Select(d => d.Specialty)
                .Distinct()
                .OrderBy(s => s)
                .ToListAsync();

            return specs.Select(s => new SelectListItem(s, s));
        }

        private static string EarliestSlotString(IEnumerable<string> slots)
        {
            // Parse "hh:mm tt" safely and choose the earliest
            var times = new List<DateTime>();
            foreach (var s in slots)
            {
                if (DateTime.TryParseExact(s, "hh:mm tt", CultureInfo.InvariantCulture,
                    DateTimeStyles.None, out var t))
                {
                    times.Add(t);
                }
            }
            return times.OrderBy(t => t).First().ToString("hh:mm tt");
        }

        [HttpGet]
        public async Task<IActionResult> ThankYou(int id)
        {
            var appt = await _context.Appointments
                .AsNoTracking()
                .Include(a => a.Doctor)
                .Include(a => a.Patient)
                .FirstOrDefaultAsync(a => a.AppointmentId == id);

            if (appt == null) return NotFound();

            var end = appt.AppointmentDate.AddMinutes(appt.Doctor.ConsultationDurationInMinutes);
            var vm = new CMS.ViewModels.ThankYouViewModel
            {
                AppointmentId = appt.AppointmentId,
                AppointmentNo = appt.AppointmentNo,
                PatientName = $"{appt.Patient.FirstName} {appt.Patient.LastName}".Trim(),
                DoctorName = appt.Doctor.FullName,
                DoctorSpecialty = appt.Doctor.Specialty,
                AppointmentDate = appt.AppointmentDate,
                TimeRange = $"{appt.AppointmentDate:hh:mm tt} - {end:hh:mm tt}",
                Fee = appt.Fee
            };

            return View(vm); // Views/PublicAppointments/ThankYou.cshtml
        }

        private string GenerateAppointmentNumber() => $"AP{DateTime.UtcNow:yyyyMMddHHmmssfff}";



        private bool IsAjax()
        {
            var accept = Request.Headers["Accept"].ToString();
            var xrw = Request.Headers["X-Requested-With"].ToString();
            return accept.Contains("application/json", StringComparison.OrdinalIgnoreCase)
                || string.Equals(xrw, "XMLHttpRequest", StringComparison.OrdinalIgnoreCase);
        }
    }

    // Request DTO for AJAX booking
    public class BookSlotRequestDto
    {
        public int DoctorId { get; set; }
        public string SelectedSlot { get; set; } = "";
        public DateTime AppointmentDate { get; set; }
    }
}



//using CMS.Data;
//using CMS.Models;
//using CMS.Services;
//using CMS.ViewModels;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.AspNetCore.Mvc.Rendering;
//using Microsoft.EntityFrameworkCore;
//using System.Text.Json;

//namespace CMS.Controllers
//{
//    public interface IEmailSender
//    {
//        Task SendAsync(string to, string subject, string htmlBody);
//    }

//    public class PublicAppointmentsController : Controller
//    {
//        private readonly ApplicationDbContext _context;
//        private readonly SlotBuilder _slotBuilder;
//        private readonly IEmailSender _email;

//        public PublicAppointmentsController(
//            ApplicationDbContext context,
//            SlotBuilder slotBuilder,
//            IEmailSender email)
//        {
//            _context = context;
//            _slotBuilder = slotBuilder;
//            _email = email;
//        }

//        // GET: /PublicAppointments/Index
//        public async Task<IActionResult> Index()
//        {
//            var vm = new PublicAppointmentVM
//            {
//                PreferredDate = DateTime.Today,
//                SpecializationOptions = await GetSpecializationOptionsAsync()
//            };
//            return View(vm);
//        }

//        // POST: /PublicAppointments/Index
//        [HttpPost, ValidateAntiForgeryToken]
//        public async Task<IActionResult> Index(PublicAppointmentVM model)
//        {
//            if (!ModelState.IsValid)
//            {
//                model.SpecializationOptions = await GetSpecializationOptionsAsync();
//                return View(model);
//            }

//            // keep patient payload for fallback Select step
//            TempData["PatientData"] = JsonSerializer.Serialize(model);
//            TempData["PatientTempId"] = Guid.NewGuid().ToString();

//            // AUTO-ASSIGN if any slot exists for preferred specialization on preferred date
//            var avail = await GetAvailableDoctorsAsync(DateOnly.FromDateTime(model.PreferredDate), model.PreferredSpecialization);

//            if (avail.Any() && avail.SelectMany(d => d.Slots).Any())
//            {
//                // pick earliest slot of the first doctor (or pick by your own business rule)
//                var chosenDoctor = avail
//                    .OrderBy(d => d.Fee) // or .OrderBy(d => d.Name) / any rule
//                    .First(d => d.Slots.Any());

//                var chosenSlotText = chosenDoctor.Slots
//                    .Select(s => DateTime.Parse(s))
//                    .OrderBy(t => t)
//                    .First()
//                    .ToString("hh:mm tt");

//                // Build booking view model and commit
//                var confirmVm = new PublicAppointmenConfirmfVM
//                {
//                    DoctorId = chosenDoctor.Id,
//                    SelectedSlot = chosenSlotText,
//                    AppointmentDate = model.PreferredDate.Date,

//                    // for success page
//                    PatientName = model.FullName,
//                    Email = model.Email,
//                    PhoneNumber = model.PhoneNumber,
//                    Gender = model.Gender,
//                    DateOfBirth = model.DateOfBirth,
//                    Address = model.Address,
//                    Reason = model.Reason
//                };

//                var appointmentId = await BookAsync(confirmVm, model, autoAssign: true);
//                if (appointmentId > 0)
//                {
//                    // SweetAlert + Success page will use TempData
//                    TempData["SweetAlert"] = JsonSerializer.Serialize(new
//                    {
//                        title = "Appointment Confirmed",
//                        text = $"Your appointment has been scheduled with {chosenDoctor.Name} on {model.PreferredDate:ddd, MMM d} at {chosenSlotText}.",
//                        icon = "success"
//                    });

//                    return RedirectToAction(nameof(Success), new { id = appointmentId });
//                }

//                // If booking failed (race condition), fall back to select page
//            }

//            // No slots for preferred specialization → go to Select page (browse all doctors)
//            return RedirectToAction(nameof(Select));
//        }

//        // AJAX availability (kept for your UI if needed)
//        [HttpPost("appointments/availability"), ValidateAntiForgeryToken]
//        public async Task<IActionResult> GetAvailability([FromBody] AvailabilityRequestDto request)
//        {
//            if (!ModelState.IsValid) return BadRequest(ModelState);

//            var doctors = await GetAvailableDoctorsAsync(request.PreferredDate, request.PreferredSpecialization);
//            var response = new AvailabilityResponseDto
//            {
//                Date = request.PreferredDate.ToString("yyyy-MM-dd"),
//                Doctors = doctors
//            };
//            return Json(response);
//        }

//        // GET: /PublicAppointments/Select
//        [HttpGet]
//        public async Task<IActionResult> Select()
//        {
//            var patientDataJson = TempData["PatientData"]?.ToString();
//            var patientTempId = TempData["PatientTempId"]?.ToString();
//            if (string.IsNullOrEmpty(patientDataJson) || string.IsNullOrEmpty(patientTempId))
//                return RedirectToAction(nameof(Index));

//            var patient = JsonSerializer.Deserialize<PublicAppointmentVM>(patientDataJson)!;

//            var availableDoctors = await GetAvailableDoctorsAsync(
//                DateOnly.FromDateTime(patient.PreferredDate),
//                null /* show all */);

//            var model = new PublicAppointmenConfirmfVM
//            {
//                PatientTempId = patientTempId,
//                AppointmentDate = patient.PreferredDate.Date,
//                PatientName = patient.FullName,
//                Email = patient.Email,
//                PhoneNumber = patient.PhoneNumber,
//                Gender = patient.Gender,
//                DateOfBirth = patient.DateOfBirth,
//                Address = patient.Address,
//                Reason = patient.Reason,
//                AvailableDoctors = availableDoctors
//            };

//            TempData.Keep("PatientData");
//            TempData.Keep("PatientTempId");

//            return View(model);
//        }

//        // POST: /PublicAppointments/Select
//        [HttpPost, ValidateAntiForgeryToken]
//        public async Task<IActionResult> Select(PublicAppointmenConfirmfVM model)
//        {
//            var patientDataJson = TempData["PatientData"]?.ToString();
//            if (string.IsNullOrEmpty(patientDataJson))
//                return RedirectToAction(nameof(Index));

//            if (!ModelState.IsValid)
//            {
//                model.AvailableDoctors = await GetAvailableDoctorsAsync(DateOnly.FromDateTime(model.AppointmentDate), null);
//                return View(model);
//            }

//            var patient = JsonSerializer.Deserialize<PublicAppointmentVM>(patientDataJson)!;

//            var appointmentId = await BookAsync(model, patient, autoAssign: false);
//            if (appointmentId > 0)
//            {
//                TempData.Remove("PatientData");
//                TempData.Remove("PatientTempId");

//                TempData["SweetAlert"] = JsonSerializer.Serialize(new
//                {
//                    title = "Appointment Confirmed",
//                    text = $"Your appointment is booked for {model.AppointmentDate:ddd, MMM d} at {model.SelectedSlot}.",
//                    icon = "success"
//                });

//                return RedirectToAction(nameof(Success), new { id = appointmentId });
//            }

//            // booking failed (e.g., race condition) → reload selections
//            ModelState.AddModelError("", "Selected slot was just booked. Please choose another.");
//            model.AvailableDoctors = await GetAvailableDoctorsAsync(DateOnly.FromDateTime(model.AppointmentDate), null);
//            return View(model);
//        }

//        // GET: /PublicAppointments/Success/{id}
//        [HttpGet]
//        public async Task<IActionResult> Success(int id)
//        {
//            var appointment = await _context.Appointments
//                .Include(a => a.Patient)
//                .Include(a => a.Doctor)
//                .FirstOrDefaultAsync(a => a.AppointmentId == id);

//            if (appointment == null) return NotFound();

//            var endTime = appointment.AppointmentDate.AddMinutes(appointment.Doctor.ConsultationDurationInMinutes);

//            var model = new SuccessViewModel
//            {
//                AppointmentId = appointment.AppointmentId,
//                AppointmentNo = appointment.AppointmentNo,
//                PatientName = $"{appointment.Patient.FirstName} {appointment.Patient.LastName}".Trim(),
//                DoctorName = appointment.Doctor.FullName,
//                AppointmentDate = appointment.AppointmentDate,
//                TimeRange = $"{appointment.AppointmentDate:hh:mm tt} - {endTime:hh:mm tt}",
//                Fee = appointment.Fee
//            };

//            return View(model);
//        }

//        // ================== helpers ==================
//        private async Task<int> BookAsync(PublicAppointmenConfirmfVM confirm, PublicAppointmentVM patientData, bool autoAssign)
//        {
//            if (!TimeOnly.TryParse(confirm.SelectedSlot, out var selectedTime))
//                return 0;

//            var appointmentDateTime = confirm.AppointmentDate.Date.Add(selectedTime.ToTimeSpan());

//            int appointmentId = 0;
//            string? patientFullName = patientData.FullName;
//            string? patientEmail = patientData.Email;
//            string doctorNameForEmail = "";
//            string doctorSpecForEmail = "";
//            decimal doctorFeeForEmail = 0m;
//            DateTime appointmentDateTimeForEmail = appointmentDateTime;
//            string appointmentNoForEmail = "";

//            var committed = false;

//            await using var trx = await _context.Database.BeginTransactionAsync();
//            try
//            {
//                // Last-second conflict check (race condition)
//                var already = await _context.Appointments
//                    .FirstOrDefaultAsync(a => a.DoctorId == confirm.DoctorId && a.AppointmentDate == appointmentDateTime);
//                if (already != null)
//                {
//                    // no exception -> no rollback necessary; just return
//                    return 0;
//                }

//                // Upsert patient by email
//                var patient = await _context.Patients.FirstOrDefaultAsync(p => p.Email == patientData.Email);
//                if (patient == null)
//                {
//                    var split = (patientData.FullName ?? "").Trim().Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
//                    var first = split.ElementAtOrDefault(0) ?? "";
//                    var last = split.ElementAtOrDefault(1) ?? "";

//                    patient = new Patient
//                    {
//                        FirstName = first,
//                        LastName = last,
//                        Email = patientData.Email,
//                        PhoneNumber = patientData.PhoneNumber,
//                        Gender = patientData.Gender,
//                        DateOfBirth = patientData.DateOfBirth ?? DateTime.MinValue,
//                        Address = patientData.Address,
//                        CreatedDate = DateTime.Now
//                    };
//                    _context.Patients.Add(patient);
//                    await _context.SaveChangesAsync();
//                }

//                var doctor = await _context.Doctors.FindAsync(confirm.DoctorId);
//                if (doctor == null) return 0;

//                var appt = new Appointment
//                {
//                    PatientId = patient.PatientId,
//                    DoctorId = confirm.DoctorId,
//                    AppointmentDate = appointmentDateTime,
//                    CreatedOn = DateTime.Now,
//                    Fee = doctor.ConsultationCharge,
//                    Status = "Scheduled",
//                    AppointmentType = "General",
//                    Reason = patientData.Reason,
//                    AppointmentNo = GenerateAppointmentNumber()
//                };

//                _context.Appointments.Add(appt);
//                await _context.SaveChangesAsync();

//                // capture info for email before disposing context/txn
//                doctorNameForEmail = doctor.FullName;
//                doctorSpecForEmail = doctor.Specialty;
//                doctorFeeForEmail = doctor.ConsultationCharge;
//                appointmentNoForEmail = appt.AppointmentNo;
//                appointmentId = appt.AppointmentId;

//                await trx.CommitAsync();
//                committed = true;
//            }
//            catch
//            {
//                if (!committed)
//                {
//                    // Only rollback if we have NOT committed yet
//                    try { await trx.RollbackAsync(); } catch { /* ignore rollback error */ }
//                }
//                throw; // let caller handle (or convert to a friendly message)
//            }

//            // ------------------------------
//            // OUTSIDE the transaction: send email (don’t risk DB consistency)
//            // ------------------------------
//            try
//            {
//                var subject = "Your Appointment Confirmation";
//                var body = $@"
//            <p>Dear {System.Net.WebUtility.HtmlEncode(patientFullName)},</p>
//            <p>Your appointment has been scheduled.</p>
//            <ul>
//              <li><b>Doctor:</b> {System.Net.WebUtility.HtmlEncode(doctorNameForEmail)} ({System.Net.WebUtility.HtmlEncode(doctorSpecForEmail)})</li>
//              <li><b>Date:</b> {appointmentDateTimeForEmail:dddd, MMM d, yyyy}</li>
//              <li><b>Time:</b> {appointmentDateTimeForEmail:hh:mm tt}</li>
//              <li><b>Fee:</b> {doctorFeeForEmail:C}</li>
//              <li><b>Appointment No:</b> {System.Net.WebUtility.HtmlEncode(appointmentNoForEmail)}</li>
//            </ul>
//            <p>Thank you.</p>";

//                await _email.SendAsync(patientEmail!, subject, body);
//            }
//            catch
//            {
//                // Log and continue; do not fail the booking because SMTP failed
//                // _logger.LogError(ex, "Email send failed for appointment {AppointmentId}", appointmentId);
//            }

//            return appointmentId;
//        }


//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> Availability([FromBody] AvailabilityRequestDto request)
//        {
//            if (!ModelState.IsValid) return BadRequest(ModelState);

//            var doctors = await GetAvailableDoctorsAsync(request.PreferredDate, request.PreferredSpecialization);
//            var response = new AvailabilityResponseDto
//            {
//                Date = request.PreferredDate.ToString("yyyy-MM-dd"),
//                Doctors = doctors
//            };
//            return Ok(response);
//        }

//        private async Task<List<DoctorAvailabilityDto>> GetAvailableDoctorsAsync(DateOnly date, string? specialty)
//        {
//            var dayOfWeek = date.DayOfWeek;

//            var query = _context.Doctors
//                .Include(d => d.WeeklyAvailabilities)
//                .Where(d => d.WeeklyAvailabilities.Any(wa => wa.DayOfWeek == dayOfWeek && wa.IsWorkingDay));

//            if (!string.IsNullOrWhiteSpace(specialty))
//                query = query.Where(d => d.Specialty.Contains(specialty));

//            var doctors = await query.ToListAsync();

//            var start = date.ToDateTime(TimeOnly.MinValue);
//            var end = start.AddDays(1);

//            var booked = await _context.Appointments
//                .Where(a => a.AppointmentDate >= start && a.AppointmentDate < end)
//                .ToListAsync();

//            var result = new List<DoctorAvailabilityDto>();

//            foreach (var doctor in doctors)
//            {
//                var freeTimes = await _slotBuilder.BuildSlotsAsync(doctor.Id, date); // usually only FREE
//                var freeKeys = freeTimes.Select(t => t.ToString("HH:mm")).ToHashSet(StringComparer.Ordinal);

//                var bookedKeys = booked
//                    .Where(b => b.DoctorId == doctor.Id)
//                    .Select(b => b.AppointmentDate.ToString("HH:mm"))
//                    .ToHashSet(StringComparer.Ordinal);

//                var allKeys = freeKeys
//                    .Union(bookedKeys)
//                    .Select(k => TimeOnly.ParseExact(k, "HH:mm"))
//                    .OrderBy(t => t)
//                    .Select(t => t.ToString("hh:mm tt"))
//                    .ToList();

//                var bookedDisplay = bookedKeys
//                    .Select(k => TimeOnly.ParseExact(k, "HH:mm"))
//                    .OrderBy(t => t)
//                    .Select(t => t.ToString("hh:mm tt"))
//                    .ToList();

//                if (allKeys.Any())
//                {
//                    result.Add(new DoctorAvailabilityDto
//                    {
//                        Id = doctor.Id,
//                        Name = doctor.FullName,
//                        Specialization = doctor.Specialty,
//                        Fee = doctor.ConsultationCharge,
//                        Slots = allKeys,            // FREE + BOOKED
//                        BookedSlots = bookedDisplay // mark in UI
//                    });
//                }
//            }

//            return result;
//        }

//        private async Task<IEnumerable<SelectListItem>> GetSpecializationOptionsAsync()
//        {
//            var specs = await _context.Doctors
//                .Select(d => d.Specialty)
//                .Distinct()
//                .OrderBy(s => s)
//                .ToListAsync();

//            return specs.Select(s => new SelectListItem(s, s));
//        }

//        private string GenerateAppointmentNumber() => $"AP{DateTime.UtcNow:yyyyMMddHHmmssfff}";
//    }
//}






//using CMS.Data;
//using CMS.Models;
//using CMS.ViewModels;
//using CMS.Services;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.EntityFrameworkCore;
//using System.Text.Json;

//namespace CMS.Controllers
//{
//    public class PublicAppointmentsController : Controller
//    {
//        private readonly ApplicationDbContext _context;
//        private readonly SlotBuilder _slotBuilder;

//        public PublicAppointmentsController(ApplicationDbContext context, SlotBuilder slotBuilder)
//        {
//            _context = context;
//            _slotBuilder = slotBuilder;
//        }


//        public IActionResult Index()
//        {
//            var model = new PublicAppointmentVM();
//            return View(model);
//        }

//        // POST: /appointments/new
//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public IActionResult Index(PublicAppointmentVM model)
//        {
//            if (!ModelState.IsValid)
//                return View(model);

//            // Store patient data in TempData for Step 2
//            TempData["PatientData"] = JsonSerializer.Serialize(model);
//            TempData["PatientTempId"] = Guid.NewGuid().ToString();

//            return RedirectToAction("Select");
//        }

//        // POST: /appointments/availability (AJAX)
//        [HttpPost("appointments/availability")]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> GetAvailability([FromBody] AvailabilityRequestDto request)
//        {
//            if (!ModelState.IsValid)
//                return BadRequest(ModelState);

//            var doctors = await GetAvailableDoctorsAsync(request.PreferredDate, request.PreferredSpecialization);

//            var response = new AvailabilityResponseDto
//            {
//                Date = request.PreferredDate.ToString("yyyy-MM-dd"),
//                Doctors = doctors
//            };

//            return Json(response);
//        }

//        // GET: /appointments/select
//        [HttpGet]
//        public async Task<IActionResult> Select()
//        {
//            var patientDataJson = TempData["PatientData"]?.ToString();
//            var patientTempId = TempData["PatientTempId"]?.ToString();

//            if (string.IsNullOrEmpty(patientDataJson) || string.IsNullOrEmpty(patientTempId))
//                return RedirectToAction("Index");

//            var patientData = JsonSerializer.Deserialize<PublicAppointmentVM>(patientDataJson);

//            // Get available doctors for the preferred date
//            var availableDoctors = await GetAvailableDoctorsAsync(
//                patientData.PreferredDate,
//                patientData.PreferredSpecialization);

//            var model = new PublicAppointmenConfirmfVM
//            {
//                PatientTempId = patientTempId,
//                AppointmentDate = patientData.PreferredDate.ToDateTime(TimeOnly.MinValue),

//                // Patient info for display
//                PatientName = $"{patientData.FirstName} {patientData.LastName}",
//                Email = patientData.Email,
//                PhoneNumber = patientData.PhoneNumber,
//                Gender = patientData.Gender,
//                DateOfBirth = patientData.DateOfBirth,
//                Address = patientData.Address,
//                Reason = patientData.Reason,

//                // Available doctors
//                AvailableDoctors = availableDoctors
//            };

//            // Keep data for potential resubmission
//            TempData.Keep("PatientData");
//            TempData.Keep("PatientTempId");

//            return View(model);
//        }

//        // POST: /appointments/select
//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> Select(PublicAppointmenConfirmfVM model)
//        {
//            var patientDataJson = TempData["PatientData"]?.ToString();

//            if (string.IsNullOrEmpty(patientDataJson))
//                return RedirectToAction("Index");

//            if (!ModelState.IsValid)
//            {
//                // Reload available doctors if validation fails
//                model.AvailableDoctors = await GetAvailableDoctorsAsync(
//                    DateOnly.FromDateTime(model.AppointmentDate), null);
//                return View(model);
//            }

//            var patientData = JsonSerializer.Deserialize<PublicAppointmentVM>(patientDataJson);

//            using var transaction = await _context.Database.BeginTransactionAsync();
//            try
//            {
//                // Parse selected time slot
//                if (!TimeOnly.TryParse(model.SelectedSlot, out var selectedTime))
//                {
//                    ModelState.AddModelError("SelectedSlot", "Invalid time slot selected.");
//                    model.AvailableDoctors = await GetAvailableDoctorsAsync(
//                        DateOnly.FromDateTime(model.AppointmentDate), null);
//                    return View(model);
//                }

//                var appointmentDateTime = model.AppointmentDate.Date.Add(selectedTime.ToTimeSpan());

//                // Check if slot is still available (race condition protection)
//                var existingAppointment = await _context.Appointments
//                    .FirstOrDefaultAsync(a => a.DoctorId == model.DoctorId
//                                           && a.AppointmentDate == appointmentDateTime);

//                if (existingAppointment != null)
//                {
//                    ModelState.AddModelError("", "This time slot was just booked by another patient. Please select a different time.");
//                    model.AvailableDoctors = await GetAvailableDoctorsAsync(
//                        DateOnly.FromDateTime(model.AppointmentDate), null);
//                    return View(model);
//                }

//                // Create or find patient
//                var patient = await _context.Patients
//                    .FirstOrDefaultAsync(p => p.Email == patientData.Email);

//                if (patient == null)
//                {
//                    patient = new Patient
//                    {
//                        FirstName = patientData.FirstName,
//                        LastName = patientData.LastName,
//                        Email = patientData.Email,
//                        PhoneNumber = patientData.PhoneNumber,
//                        Gender = patientData.Gender,
//                        DateOfBirth = patientData.DateOfBirth ?? DateTime.MinValue,
//                        Address = patientData.Address,
//                        CreatedDate = DateTime.Now
//                    };
//                    _context.Patients.Add(patient);
//                    await _context.SaveChangesAsync();
//                }

//                // Get doctor for fee calculation
//                var doctor = await _context.Doctors.FindAsync(model.DoctorId);
//                if (doctor == null)
//                {
//                    ModelState.AddModelError("", "Selected doctor not found.");
//                    model.AvailableDoctors = await GetAvailableDoctorsAsync(
//                        DateOnly.FromDateTime(model.AppointmentDate), null);
//                    return View(model);
//                }

//                // Create appointment
//                var appointment = new Appointment
//                {
//                    PatientId = patient.PatientId,
//                    DoctorId = model.DoctorId,
//                    AppointmentDate = appointmentDateTime,
//                    CreatedOn = DateTime.Now,
//                    Fee = doctor.ConsultationCharge,
//                    Status = "Scheduled",
//                    AppointmentType = "General",
//                    Reason = patientData.Reason,
//                    AppointmentNo = GenerateAppointmentNumber()
//                };

//                _context.Appointments.Add(appointment);
//                await _context.SaveChangesAsync();

//                await transaction.CommitAsync();

//                // Clear TempData after successful booking
//                TempData.Remove("PatientData");
//                TempData.Remove("PatientTempId");

//                return RedirectToAction("Success", new { id = appointment.AppointmentId });
//            }
//            catch (Exception ex)
//            {
//                await transaction.RollbackAsync();
//                ModelState.AddModelError("", "An error occurred while booking your appointment. Please try again.");
//                model.AvailableDoctors = await GetAvailableDoctorsAsync(
//                    DateOnly.FromDateTime(model.AppointmentDate), null);
//                return View(model);
//            }
//        }

//        // GET: /appointments/success/{id}
//        [HttpGet]
//        public async Task<IActionResult> Success(int id)
//        {
//            var appointment = await _context.Appointments
//                .Include(a => a.Patient)
//                .Include(a => a.Doctor)
//                .FirstOrDefaultAsync(a => a.AppointmentId == id);

//            if (appointment == null)
//                return NotFound();

//            var endTime = appointment.AppointmentDate.AddMinutes(appointment.Doctor.ConsultationDurationInMinutes);

//            var model = new SuccessViewModel
//            {
//                AppointmentId = appointment.AppointmentId,
//                PatientName = $"{appointment.Patient.FirstName} {appointment.Patient.LastName}",
//                DoctorName = appointment.Doctor.FullName,
//                AppointmentDate = appointment.AppointmentDate,
//                TimeRange = $"{appointment.AppointmentDate:hh:mm tt} - {endTime:hh:mm tt}",
//                Fee = appointment.Fee,
//            };

//            return View(model);
//        }

//        private async Task<List<DoctorAvailabilityDto>> GetAvailableDoctorsAsync(DateOnly date, string? specialty)
//        {
//            var dayOfWeek = date.DayOfWeek;

//            var query = _context.Doctors
//                .Include(d => d.WeeklyAvailabilities)
//                .Where(d => d.WeeklyAvailabilities.Any(wa =>
//                    wa.DayOfWeek == dayOfWeek && wa.IsWorkingDay));

//            if (!string.IsNullOrEmpty(specialty))
//            {
//                query = query.Where(d => d.Specialty.Contains(specialty));
//            }

//            var doctors = await query.ToListAsync();
//            var result = new List<DoctorAvailabilityDto>();

//            foreach (var doctor in doctors)
//            {
//                var slots = await _slotBuilder.BuildSlotsAsync(doctor.Id, date);
//                var slotStrings = slots.Select(s => s.ToString("hh:mm tt")).ToList();

//                if (slotStrings.Any())
//                {
//                    result.Add(new DoctorAvailabilityDto
//                    {
//                        Id = doctor.Id,
//                        Name = doctor.FullName,
//                        Specialization = doctor.Specialty,
//                        Fee = doctor.ConsultationCharge,
//                        Slots = slotStrings
//                    });
//                }
//            }

//            return result;
//        }

//        private string GenerateAppointmentNumber()
//        {
//            return $"AP{DateTime.UtcNow:yyyyMMddHHmmssfff}";
//        }
//    }
//}
