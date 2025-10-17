using CMS.Data;
using CMS.Helpers;
using CMS.Models;
using CMS.ViewModels;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using static CMS.Models.ViewModels;

namespace CMS.Controllers
{
    public class DoctorController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly FileHelper _fileHelper;
        public DoctorController(ApplicationDbContext context, FileHelper fileHelper)
        {
            _context = context;
            _fileHelper = fileHelper;
        }

        // ----------------------GET: Doctor
        public async Task<IActionResult> Index()
        {
            var doctors = await _context.Doctors.ToListAsync();
            return View(doctors);
        }

        [HttpGet("Doctor/Profile/{id}")]
        public IActionResult Profile(int id)
        {
            ViewData["DoctorId"] = id;
            return View();
        }


        // AJAX endpoint to get doctor data as JSON
        [HttpGet]
        public async Task<IActionResult> GetDoctorProfile(int id)
        {
            var doctor = await _context.Doctors
                .Include(d => d.Educations)
                .Include(d => d.Awards)
                .Include(d => d.Certifications)
                .Include(d => d.WeeklyAvailabilities)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (doctor == null)
                return NotFound();

            return Json(doctor);
        }
        //[HttpGet]
        //public async Task<JsonResult> GetDoctorData(int id)
        //{
        //    var doctor = await _context.Doctors
        //                               .Where(d => d.Id == id)
        //                               .Select(d => new
        //                               {
        //                                   d.Id,
        //                                   d.FullName,
        //                                   d.Specialty,
        //                                   d.Degrees,
        //                                   d.About,
        //                                   d.Email,
        //                                   d.Phone,
        //                                   d.Address,
        //                                   d.ProfileImageUrl,
        //                                   d.ConsultationCharge,
        //                                   d.ConsultationDurationInMinutes,
        //                                   d.MedicalLicenseNumber,
        //                                   d.BloodGroup,
        //                                   d.YearOfExperience,
        //                                   d.AvailabilityStatus,
        //                                   //// Select related data if it's needed for the details page
        //                                   //EducationInformation = d.EducationId.Select(e => new { e.Institution, e.Degree, e.Duration }),
        //                                   //AwardsAndRecognition = d.AwardsAndRecognition.Select(a => new { a.Title, a.Description }),
        //                                   //Certifications = d.Certifications.Select(c => new { c.Title, c.Description })
        //                               })
        //                               .FirstOrDefaultAsync(); // Use FirstOrDefaultAsync for async calls

        //    if (doctor == null)
        //    {
        //        Response.StatusCode = 404;
        //        return Json(new { success = false, message = "Doctor not found." });
        //    }

        //    return Json(new { success = true, data = doctor });
        //}

        // GET: /Doctor/GetDoctorsData

        [HttpGet]
        public async Task<IActionResult> GetDetail(int id)
        {
            var doctor = await _context.Doctors
                .Include(d => d.WeeklyAvailabilities)
                .Include(d => d.DateAvailabilities)
                .Include(d => d.Educations)
                .Include(d => d.Awards)
                .Include(d => d.Certifications)
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.Id == id);

            if (doctor == null)
                return NotFound();

            var vm = new DoctorUpsertVM
            {
                Doctor = doctor,
                WeeklyAvailabilities = doctor.WeeklyAvailabilities?.ToList() ?? new(),
                DateAvailabilities = doctor.DateAvailabilities?.ToList() ?? new(),
                Educations = doctor.Educations?.ToList() ?? new(),
                Awards = doctor.Awards?.ToList() ?? new(),
                Certifications = doctor.Certifications?.ToList() ?? new()
            };

            return Json(vm);
        }

        //[HttpGet]
        //public async Task<IActionResult> GetDoctor(int id)
        //{
        //    var doctor = await _context.Doctors
        //        .Include(d => d.WeeklyAvailabilities)
        //        .Include(d => d.DateAvailabilities)
        //        .Include(d => d.Educations)
        //        .Include(d => d.Awards)
        //        .Include(d => d.Certifications)
        //        .FirstOrDefaultAsync(d => d.Id == id);

        //    if (doctor == null) return NotFound();

        //    var vm = new ViewModels.AppointmentUpsertVM
        //    {
        //        Doctor = doctor,
        //        WeeklyAvailabilities = doctor.WeeklyAvailabilities.ToList(),
        //        DateAvailabilities = doctor.DateAvailabilities.ToList(),
        //        Educations = doctor.Educations.ToList(),
        //        Awards = doctor.Awards.ToList(),
        //        Certifications = doctor.Certifications.ToList()
        //    };

        //    return Json(vm);
        //}
        [HttpGet]
        public async Task<IActionResult> GetDoctorsData()
        {
            try
            {
                // DataTables params
                int draw = int.TryParse(Request.Query["draw"], out var d) ? d : 1;
                int start = int.TryParse(Request.Query["start"], out var s) ? s : 0;
                int length = int.TryParse(Request.Query["length"], out var l) ? l : 10;
                string searchValue = (Request.Query["search[value]"].FirstOrDefault() ?? string.Empty).Trim();

                int orderColIndex = int.TryParse(Request.Query["order[0][column]"], out var oc) ? oc : 0;
                string orderDir = Request.Query["order[0][dir]"].FirstOrDefault() ?? "asc";

                if (length <= 0) length = 10;
                if (start < 0) start = 0;

                IQueryable<Doctor> q = _context.Doctors.AsNoTracking();

                int recordsTotal = await q.CountAsync();

                // Search
                if (!string.IsNullOrWhiteSpace(searchValue))
                {
                    string term = searchValue.ToLower();
                    q = q.Where(d =>
                        (d.FullName ?? "").ToLower().Contains(term) ||
                        (d.Specialty ?? "").ToLower().Contains(term) ||
                        (d.Degrees ?? "").ToLower().Contains(term) ||
                        (d.Email ?? "").ToLower().Contains(term) ||
                        (d.Phone ?? "").ToLower().Contains(term)
                    );
                }

                int recordsFiltered = await q.CountAsync();

                // Sort map (index aligns with columns in view)
                // 0 Name | 1 Specialty | 2 Phone | 3 Email | 4 CreatedOn | 5 Actions
                Expression<Func<Doctor, object>> sortSelector = orderColIndex switch
                {
                    0 => d => d.FullName,
                    1 => d => d.Specialty,
                    2 => d => d.Phone,
                    3 => d => d.Email,
                    4 => d => d.CreatedOn,
                    _ => d => d.Id   // fallback
                };
                bool desc = orderDir.Equals("desc", StringComparison.OrdinalIgnoreCase);
                q = desc ? q.OrderByDescending(sortSelector) : q.OrderBy(sortSelector);

                // Page + shape
                var data = await q
                    .Skip(start)
                    .Take(length)
                    .Select(d => new
                    {
                        id = d.Id,
                        fullName = d.FullName,
                        designation = d.Degrees,        // shows under the name
                        specialty = d.Specialty,
                        phone = d.Phone,
                        email = d.Email,
                        addedDateTime = d.CreatedOn,    // DateTime
                        profileImageUrl = d.ProfileImageUrl
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

        // POST: /Doctor/Delete/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var doctor = await _context.Doctors.FindAsync(id);
                if (doctor == null)
                    return Json(new { status = false, message = "Doctor not found." });
                bool hasAppointments = _context.Appointments.Any(a => a.DoctorId == id);
                if (hasAppointments)
                {
                    return Json(new { status = false, message = "Doctor has appointments. Delete them first." });
                }
                _fileHelper.DeleteIfNotDefault(doctor.ProfileImageUrl);

                _context.Doctors.Remove(doctor);
                await _context.SaveChangesAsync();

                return Json(new { status = true, message = "Deleted successfully." });
            }
            catch(Exception ex)
            {
                return Json(new { status = false, message = "Error deleting doctor." });
            }
        }





        [HttpGet]
        public async Task<IActionResult> Upsert(int? id, bool prefill5 = false, bool prefill7 = false)
        {
            var vm = new DoctorUpsertVM
            {
                // Load departments from database
                Departments = await _context.Departments
                    .OrderBy(d => d.DepartmentName)
                    .Select(d => new SelectListItem
                    {
                        Text = d.DepartmentName,
                        Value = d.DepartmentId.ToString()
                    })
                    .ToListAsync()
            };

            if (id == null || id == 0)
            {
                // Initialize a new Doctor object if no ID is passed (Create)
                vm.Doctor = new Doctor
                {
                    AvailabilityStatus = "Available",
                    ConsultationDurationInMinutes = 15
                };

                // Pre-fill weekly availability if needed
                if (prefill5) PrefillWeekly(vm, days: 5);
                if (prefill7) PrefillWeekly(vm, days: 7);
            }
            else
            {
                // Retrieve existing doctor details from the database (Edit)
                vm.Doctor = await _context.Doctors
                    .Include(d => d.WeeklyAvailabilities)
                    .Include(d => d.DateAvailabilities)
                    .Include(d => d.Educations)
                    .Include(d => d.Awards)
                    .Include(d => d.Certifications)
                    .FirstOrDefaultAsync(d => d.Id == id.Value);

                if (vm.Doctor == null)
                {
                    // If no doctor is found, return a 404 error
                    return NotFound();
                }

                // Load additional data for existing doctor
                vm.WeeklyAvailabilities = vm.Doctor.WeeklyAvailabilities.OrderBy(a => a.DayOfWeek).ToList();
                vm.DateAvailabilities = vm.Doctor.DateAvailabilities.OrderBy(a => a.Date).ToList();
                vm.Educations = vm.Doctor.Educations.OrderByDescending(e => e.Year).ToList();
                vm.Awards = vm.Doctor.Awards.OrderByDescending(a => a.Year).ToList();
                vm.Certifications = vm.Doctor.Certifications.OrderByDescending(c => c.IssuedOn).ToList();
            }

            // Return the correct view with the DoctorUpsertVM
            return View(vm);
        }

        // ---------------------- POST: Doctor/Upsert
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upsert(DoctorUpsertVM vm, IFormFile? ProfileImage)
        {
            // Load departments for the view (in case of validation errors)
            vm.Departments = await _context.Departments
                .OrderBy(d => d.DepartmentName)
                .Select(d => new SelectListItem
                {
                    Text = d.DepartmentName,
                    Value = d.DepartmentId.ToString()
                })
                .ToListAsync();

            if (ProfileImage is { Length: > 1 * 1024 * 1024 })
            {
                ModelState.AddModelError(nameof(vm.Doctor.ProfileImageUrl), "Profile photo must not exceed 1 MB.");
                return View(vm);
            }

            var isCreate = vm.Doctor.Id == 0;

            // Trim empty child rows
            TrimEmptyChildren(vm);


            if (vm.Doctor.Id == 0) // create
            {
                if (string.IsNullOrWhiteSpace(vm.Doctor.Password))
                    ModelState.AddModelError("Doctor.Password", "Password is required.");
                if (vm.Doctor.Password != vm.Doctor.ConfirmPassword)
                    ModelState.AddModelError("Doctor.ConfirmPassword", "Passwords do not match.");
            }
            else // edit
            {
                if (string.IsNullOrWhiteSpace(vm.Doctor.Password) &&
                    string.IsNullOrWhiteSpace(vm.Doctor.ConfirmPassword))
                {
                    ModelState.Remove("Doctor.Password");
                    ModelState.Remove("Doctor.ConfirmPassword");
                }
                else if (vm.Doctor.Password != vm.Doctor.ConfirmPassword)
                {
                    ModelState.AddModelError("Doctor.ConfirmPassword", "Passwords do not match.");
                }
            }
            // Validate Weekly Availability
            for (int i = 0; i < (vm.WeeklyAvailabilities?.Count ?? 0); i++)
            {
                var w = vm.WeeklyAvailabilities[i];

                if (w.DayOfWeek is null)
                    ModelState.AddModelError($"WeeklyAvailabilities[{i}].DayOfWeek", "Day is required.");
                if (w.StartTime == default)
                    ModelState.AddModelError($"WeeklyAvailabilities[{i}].StartTime", "Start time is required.");
                if (w.EndTime == default)
                    ModelState.AddModelError($"WeeklyAvailabilities[{i}].EndTime", "End time is required.");
                if (w.StartTime != default && w.EndTime != default && w.StartTime >= w.EndTime)
                    ModelState.AddModelError($"WeeklyAvailabilities[{i}].EndTime", "End must be after Start.");
                if (w.SlotDuration <= TimeSpan.Zero)
                    ModelState.AddModelError($"WeeklyAvailabilities[{i}].SlotDuration", "Slot duration must be > 0.");

                if (w.StartTime != default && w.EndTime != default &&
                    (w.EndTime - w.StartTime).TotalMinutes < 30)
                    ModelState.AddModelError($"WeeklyAvailabilities[{i}].EndTime", "Each availability block must be at least 30 minutes long.");
            }

            // Validate ModelState
            if (!ModelState.IsValid)
            {
                return View(vm);
            }

            // Handle Profile Image
            if (ProfileImage != null)
            {
                _fileHelper.DeleteIfNotDefault(vm.Doctor.ProfileImageUrl);
                vm.Doctor.ProfileImageUrl = await _fileHelper.SaveAsync(ProfileImage, "uploads/doctors");
            }

            // Default image on create only
            if (isCreate && string.IsNullOrEmpty(vm.Doctor.ProfileImageUrl))
                vm.Doctor.ProfileImageUrl = "/uploads/doctors/default.jpg";

            try
            {
                if (isCreate)
                {
                    // Create user account
                    var user = new User
                    {
                        Username = vm.Doctor.Email,
                        Email = vm.Doctor.Email,
                        PasswordHash = vm.Doctor.Password, // Make sure to hash this in production
                        FullName = vm.Doctor.FullName,
                        Role = "Doctor",
                        IsActive = true,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    };
                    _context.Users.Add(user);
                    await _context.SaveChangesAsync();

                    vm.Doctor.UserId = user.Id;
                    vm.Doctor.CreatedOn = DateTime.Now;

                    _context.Doctors.Add(vm.Doctor);
                    await _context.SaveChangesAsync();

                    await ReplaceChildren(vm.Doctor.Id, vm);

                    TempData["success"] = "Doctor created successfully.";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    // Update existing doctor
                    var dbDoctor = await _context.Doctors
                        .Include(d => d.WeeklyAvailabilities)
                        .Include(d => d.DateAvailabilities)
                        .Include(d => d.Educations)
                        .Include(d => d.Awards)
                        .Include(d => d.Certifications)
                        .FirstOrDefaultAsync(x => x.Id == vm.Doctor.Id);

                    if (dbDoctor == null)
                    {
                        TempData["error"] = "Doctor not found.";
                        return RedirectToAction(nameof(Index));
                    }

                    // Update doctor properties
                    dbDoctor.FullName = vm.Doctor.FullName;
                    dbDoctor.Specialty = vm.Doctor.Specialty;
                    dbDoctor.DepartmentId = vm.Doctor.DepartmentId;
                    dbDoctor.Degrees = vm.Doctor.Degrees;
                    dbDoctor.About = vm.Doctor.About;
                    dbDoctor.Email = vm.Doctor.Email;
                    dbDoctor.Phone = vm.Doctor.Phone;
                    dbDoctor.Address = vm.Doctor.Address;
                    dbDoctor.ConsultationCharge = vm.Doctor.ConsultationCharge;
                    dbDoctor.ConsultationDurationInMinutes = vm.Doctor.ConsultationDurationInMinutes;
                    dbDoctor.MedicalLicenseNumber = vm.Doctor.MedicalLicenseNumber;
                    dbDoctor.Clinic = vm.Doctor.Clinic;
                    dbDoctor.BloodGroup = vm.Doctor.BloodGroup;
                    dbDoctor.YearOfExperience = vm.Doctor.YearOfExperience;
                    dbDoctor.AvailabilityStatus = vm.Doctor.AvailabilityStatus;
                    dbDoctor.UpdatedOn = DateTime.Now;

                    // Only update password if provided
                    if (!string.IsNullOrEmpty(vm.Doctor.Password))
                    {
                        dbDoctor.Password = vm.Doctor.Password; // Hash this in production
                    }

                    // Update profile image if changed
                    if (ProfileImage != null)
                    {
                        dbDoctor.ProfileImageUrl = vm.Doctor.ProfileImageUrl;
                    }

                    await _context.SaveChangesAsync();
                    await ReplaceChildren(dbDoctor.Id, vm);

                    TempData["success"] = "Doctor updated successfully.";
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception ex)
            {
                TempData["error"] = $"An error occurred: {ex.Message}";
                return View(vm);
            }
        }

       
        private static void PrefillWeekly(DoctorUpsertVM vm, int days)
        {
            vm.WeeklyAvailabilities.Clear();
            var start = new TimeSpan(9, 0, 0);
            var end = new TimeSpan(17, 0, 0);

            if (days == 5)
            {
                foreach (var dow in new[] { DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday })
                    vm.WeeklyAvailabilities.Add(new DoctorWeeklyAvailability { DayOfWeek = dow, StartTime = start, EndTime = end, IsWorkingDay = true });
            }
            else
            {
                foreach (DayOfWeek dow in Enum.GetValues(typeof(DayOfWeek)))
                    vm.WeeklyAvailabilities.Add(new DoctorWeeklyAvailability { DayOfWeek = dow, StartTime = start, EndTime = end, IsWorkingDay = true });
            }
        }

        // inside DoctorController
        private static void TrimEmptyChildren(DoctorUpsertVM vm)
        {
            bool IsEmptyWeekly(DoctorWeeklyAvailability w) =>
                w is null
                || (!w.DayOfWeek.HasValue
                    && w.StartTime == default   // 00:00:00 means empty
                    && w.EndTime == default
                    // optional: also treat zero/negative slot as empty meta
                    && w.SlotDuration <= TimeSpan.Zero);

            bool IsEmptyDate(DoctorDateAvailability d) =>
                d is null
                || (!d.Date.HasValue
                    && d.StartTime == null
                    && d.EndTime == null);

            bool IsEmptyEdu(DoctorEducation e) =>
                e is null || (string.IsNullOrWhiteSpace(e.Degree)
                              && string.IsNullOrWhiteSpace(e.Institution)
                              && !e.Year.HasValue);

            bool IsEmptyAward(DoctorAward a) =>
                a is null || (string.IsNullOrWhiteSpace(a.Title)
                              && string.IsNullOrWhiteSpace(a.Issuer)
                              && !a.Year.HasValue);

            bool IsEmptyCert(DoctorCertification c) =>
                c is null || (string.IsNullOrWhiteSpace(c.Title)
                              && string.IsNullOrWhiteSpace(c.Authority)
                              && string.IsNullOrWhiteSpace(c.LicenseNumber)
                              && !c.IssuedOn.HasValue
                              && !c.ExpiresOn.HasValue);

            vm.WeeklyAvailabilities = (vm.WeeklyAvailabilities ?? new()).Where(w => !IsEmptyWeekly(w)).ToList();
            vm.DateAvailabilities = (vm.DateAvailabilities ?? new()).Where(d => !IsEmptyDate(d)).ToList();
            vm.Educations = (vm.Educations ?? new()).Where(e => !IsEmptyEdu(e)).ToList();
            vm.Awards = (vm.Awards ?? new()).Where(a => !IsEmptyAward(a)).ToList();
            vm.Certifications = (vm.Certifications ?? new()).Where(c => !IsEmptyCert(c)).ToList();
        }



        private async Task ReplaceChildren(int doctorId, DoctorUpsertVM vm)
        {
            var oldW = _context.DoctorWeeklyAvailabilities.Where(x => x.DoctorId == doctorId);
            var oldD = _context.DoctorDateAvailabilities.Where(x => x.DoctorId == doctorId);
            var oldE = _context.DoctorEducations.Where(x => x.DoctorId == doctorId);
            var oldA = _context.DoctorAwards.Where(x => x.DoctorId == doctorId);
            var oldC = _context.DoctorCertifications.Where(x => x.DoctorId == doctorId);

            _context.DoctorWeeklyAvailabilities.RemoveRange(oldW);
            _context.DoctorDateAvailabilities.RemoveRange(oldD);
            _context.DoctorEducations.RemoveRange(oldE);
            _context.DoctorAwards.RemoveRange(oldA);
            _context.DoctorCertifications.RemoveRange(oldC);
            await _context.SaveChangesAsync();

            foreach (var w in vm.WeeklyAvailabilities)
            {
                w.Id = 0; w.DoctorId = doctorId;
                _context.DoctorWeeklyAvailabilities.Add(w);
            }

            // ONLY ADD EXPLICIT DATE AVAILABILITIES (not converted from weekly)
            foreach (var d in vm.DateAvailabilities)
            {
                d.Id = 0; d.DoctorId = doctorId;
                _context.DoctorDateAvailabilities.Add(d);
            }

            foreach (var e in vm.Educations)
            {
                e.Id = 0; e.DoctorId = doctorId;
                _context.DoctorEducations.Add(e);
            }
            foreach (var a in vm.Awards)
            {
                a.Id = 0; a.DoctorId = doctorId;
                _context.DoctorAwards.Add(a);
            }
            foreach (var c in vm.Certifications)
            {
                c.Id = 0; c.DoctorId = doctorId;
                _context.DoctorCertifications.Add(c);
            }

            await _context.SaveChangesAsync();
        }

    }
}


