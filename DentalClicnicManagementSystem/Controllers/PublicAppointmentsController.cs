using CMS.Data;
using CMS.Models;
using CMS.Services;
using CMS.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Text;
using System.Text.Json;

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
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;

        public PublicAppointmentsController(ApplicationDbContext context, SlotBuilder slotBuilder, Services.IEmailSender email, IConfiguration configuration, IHttpClientFactory httpClientFactory)
        {
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
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

        // POST: /PublicAppointments/SubmitAppointment
        [HttpPost]
        public async Task<IActionResult> SubmitAppointmentAjax([FromForm] PublicAppointmentVM model)
        {
            ModelState.Remove("Fee");
            ModelState.Remove("Mode");
            if (!ModelState.IsValid)
                return Json(new { success = false, message = "Veuillez remplir tous les champs obligatoires." });

            var selectedDomain = model.SelectedDomain;
            var baseDomain = _configuration[$"AppointmentDomains:{selectedDomain}"];

            if (string.IsNullOrEmpty(baseDomain))
                return Json(new { success = false, message = "Domaine invalide sélectionné." });

            var apiEndpoint = $"{baseDomain.TrimEnd('/')}/api/createappointment";

            var client = _httpClientFactory.CreateClient();
            var content = new StringContent(JsonSerializer.Serialize(model), Encoding.UTF8, "application/json");

            try
            {
                var response = await client.PostAsync(apiEndpoint, content);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    return Json(new { success = true, message = "Rendez-vous créé avec succès !" });
                }
                else
                {
                    var err = await response.Content.ReadAsStringAsync();
                    return Json(new { success = false, message = "Échec de l'envoi: " + err });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Erreur: " + ex.Message });
            }
        }



        [HttpPost]
        public async Task<IActionResult> SubmitAppointment(PublicAppointmentVM model)
        {
            ModelState.Remove("Fee");
            ModelState.Remove("Mode");
            if (!ModelState.IsValid)
            {
                return RedirectToAction(nameof(Index));
            }

            // Build the full API endpoint URL
            var selectedDomain = model.SelectedDomain;
            var baseDomain = _configuration[$"AppointmentDomains:{selectedDomain}"];

            if (string.IsNullOrEmpty(baseDomain))
            {
                ModelState.AddModelError("", "Invalid domain selected.");
                return RedirectToAction(nameof(Index));
            }

            // ✅ Append the API path
            var apiEndpoint = $"{baseDomain.TrimEnd('/')}/api/appointments";
            Console.WriteLine(apiEndpoint);
            // Create a new HttpClient instance
            var client = _httpClientFactory.CreateClient();

            // Convert the model data to JSON
            var content = new StringContent(
                JsonSerializer.Serialize(model),
                Encoding.UTF8,
                "application/json"
            );

            try
            {
                // Post data to the API endpoint
                var response = await client.PostAsync(baseDomain, content);
                Console.WriteLine(response);
                if (response.IsSuccessStatusCode)
                {
                    // Appointment successfully booked
                    return RedirectToAction(nameof(ThankYou));
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    ModelState.AddModelError("", $"Failed to submit the appointment. Server response: {error}");
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"An error occurred: {ex.Message}");
                return RedirectToAction(nameof(Index));
            }
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

