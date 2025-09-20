using CMS.Data;
using CMS.Helpers;
using CMS.Models;
using CMS.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using static Azure.Core.HttpHeader;

namespace CMS.Controllers
{

    //[Authorize(Roles = "Admin,Receptionist,Dentist,HR")]
    public class PatientController : Controller
    {



        private readonly ApplicationDbContext _context;
        private readonly ILogger<PatientController> _logger;

        private readonly FileHelper _fileHelper;
        public PatientController(ApplicationDbContext context, FileHelper? fileHelper, ILogger<PatientController> logger)
        {
            _context = context;
            _fileHelper = fileHelper;
            _logger = logger;
        }

        // ----------------------GET: Doctor
        public async Task<IActionResult> Index()
        {
            var patient = await _context.Patients.ToListAsync();
            return View(patient);
        }
        
        // GET: Doctor/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var patient = await _context.Patients.FirstOrDefaultAsync(d => d.PatientId == id);
            if (patient == null)
                return NotFound();

            return View(patient);
        }

            [Route("Patient/Profile/{id}")]
        public IActionResult Profile(int id)
        {
            return View(id);
        }
        // In PatientController.cs
        [HttpGet]
        public async Task<IActionResult> GetPatientProfile(int id)
        {
            try
            {
                // This part for fetching patientInfo is correct
                var patientInfo = await _context.Patients
                    .Where(p => p.PatientId == id)
                    .Select(p => new PatientInfoViewModel
                    {
                        PatientId = p.PatientId,
                        FullName = (p.FirstName ?? "") + " " + (p.LastName ?? ""),
                        Email = p.Email,
                        Gender = p.Gender,
                        DateOfBirth = p.DateOfBirth ?? DateTime.MinValue,
                        PhoneNumber = p.PhoneNumber,
                        Address = p.Address,
                        CreatedDate = p.CreatedDate,
                        ProfileImageUrl = !string.IsNullOrEmpty(p.ProfileImageUrl)
                                          ? p.ProfileImageUrl.Replace('\\', '/')
                                          : "/uploads/doctors/patient_default.jpg"
                    })
                    .FirstOrDefaultAsync();

                if (patientInfo == null)
                {
                    return NotFound(new { message = "Patient not found." });
                }

                // --- FIX STARTS HERE ---

                // 1. Fetch upcoming appointments correctly into the ViewModel
                var upcomingAppointments = await _context.Appointments
                    .Include(a => a.Doctor) // Eagerly load the related Doctor entity
                    .Where(a => a.PatientId == id && a.AppointmentDate >= DateTime.Now)
                    .OrderBy(a => a.AppointmentDate)
                    .Select(a => new AppointmentViewModel // Project into the ViewModel
                    {
                        AppointmentId = a.AppointmentId,
                        AppointmentDate = a.AppointmentDate,
                        AppointmentType = a.AppointmentType,
                        DoctorName = a.Doctor != null ? a.Doctor.FullName : "N/A"
                    })
                    .ToListAsync();

                // 2. FIX: Fetch past appointments with the same correct logic
                var pastAppointments = await _context.Appointments
                    .Include(a => a.Doctor) // Eagerly load the related Doctor entity
                    .Where(a => a.PatientId == id && a.AppointmentDate < DateTime.Now)
                    .OrderByDescending(a => a.AppointmentDate)
                    .Select(a => new AppointmentViewModel // Project into the ViewModel
                    {
                        AppointmentId = a.AppointmentId,
                        AppointmentDate = a.AppointmentDate,
                        AppointmentType = a.AppointmentType,
                        DoctorName = a.Doctor != null ? a.Doctor.FullName : "N/A"
                    })
                    .ToListAsync();

                // The rest of your logic for documents, payments, etc. is correct
                var documents = await _context.Document.Where(d => d.PatientId == id).ToListAsync();
                var payments = await _context.Payments.Where(p => p.PatientId == id).ToListAsync();
                var followUps = await _context.FollowUps.Where(f => f.PatientId == id).OrderByDescending(f => f.FollowUpDate).ToListAsync();

                // Combine notes logic
                var patientRecord = await _context.Patients.AsNoTracking().FirstOrDefaultAsync(p => p.PatientId == id);
                var combinedNotes = new List<DisplayNote>();
                if (patientRecord != null && !string.IsNullOrWhiteSpace(patientRecord.Notes))
                {
                    // Use CreatedDate as per your model, or UpdatedAt if more appropriate
                    combinedNotes.Add(new DisplayNote { Content = patientRecord.Notes, CreatedAt = patientRecord.CreatedDate, Source = "Patient Record" });
                }
                foreach (var followUp in followUps)
                {
                    if (!string.IsNullOrWhiteSpace(followUp.Notes))
                    {
                        combinedNotes.Add(new DisplayNote { Content = followUp.Notes, CreatedAt = followUp.FollowUpDate, Source = "Follow-up" });
                    }
                }

                // 3. FIX: REMOVE the incorrect conversion block. It is not needed.

                // 4. FIX: Build the final ViewModel with the CORRECT lists
                var viewModel = new PatientProfileViewModel
                {
                    Patient = patientInfo,
                    UpcomingAppointments = upcomingAppointments, // This is now the correct List<AppointmentViewModel>
                    PastAppointments = pastAppointments,       // This is also the correct List<AppointmentViewModel>
                    PastAppointmentsCount = pastAppointments.Count,
                    UpcomingAppointmentsCount = upcomingAppointments.Count,
                    Documents = documents,
                    Payments = payments,
                    FollowUps = followUps,
                    CombinedNotes = combinedNotes.OrderByDescending(n => n.CreatedAt).ToList()
                };

                return Json(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching patient profile for ID {PatientId}", id);
                return StatusCode(500, new { message = "An error occurred while fetching the patient profile." });
            }
        }

        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddDocument(int patientId, string description, IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return Json(new { success = false, message = "No file was selected." });
            }

            var patient = await _context.Patients.FindAsync(patientId);
            if (patient == null)
            {
                return NotFound(new { success = false, message = "Patient not found." });
            }

            // Use your existing FileHelper to save the file
            string fileUrl = await _fileHelper.SaveAsync(file, $"uploads/documents/{patientId}");

            // Create a new Document record in the database
            var document = new Document
            {
                PatientId = patientId,
                FileName = file.FileName,
                FileUrl = fileUrl,
                Description = description,
                FileSize = $"{file.Length / 1024} KB", // Calculate file size
                CreateAt = DateTime.UtcNow
            };

            _context.Document.Add(document);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Document uploaded successfully!" });
        }




        [HttpGet]
        public async Task<IActionResult> GetPatientHistory(int id)
        {
            // Fetch medical history, including related document info
            var history = await _context.MedicalHistories
                .Include(h => h.Document) // Eager load the document
                .Where(h => h.PatientId == id)
                .OrderByDescending(h => h.DateOfVisit)
                .Select(h => new MedicalHistory
                {
                    Id = h.Id,
                    DateOfVisit = h.DateOfVisit,
                    Diagnosis = h.Diagnosis,
                    Severity = h.Severity,
                    TotalVisits = h.TotalVisits,
                    Status = h.Status,
                    Document = h.Document != null ? new Document
                    {
                        FileName = h.Document.FileName,
                        FileUrl = h.Document.FileUrl
                    } : null
                })
                .ToListAsync();

            // Fetch follow-ups
            var followUps = await _context.FollowUps
                .Where(f => f.PatientId == id)
                .OrderByDescending(f => f.FollowUpDate)
                .ToListAsync();

            var viewModel = new PatientHistoryViewModel
            {
                MedicalHistory = history,
                FollowUps = followUps
            };

            return Json(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddFollowUp([FromBody] FollowUp model)
        {
            if (ModelState.IsValid)
            {
                // Set the status and add the new follow-up to the database
                model.Status = "Scheduled";
                _context.FollowUps.Add(model);
                await _context.SaveChangesAsync();

                // After saving, you can trigger a notification or just return success
                return Json(new { success = true, message = "Follow-up scheduled successfully!" });
            }

            // Create a string of validation errors to send back
            var errorList = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            return BadRequest(new { success = false, message = "Invalid data submitted.", errors = errorList });
        }


        // ------------------Upsert -----------

        // GET: /Patient/Upsert or /Patient/Upsert/5
        [HttpGet]
        public async Task<IActionResult> Upsert(int? id)
        {
            Patient model;

            if (id == null || id == 0)
            {
                // New patient
                model = new Patient();
            }
            else
            {
                model = await _context.Patients
                    .FirstOrDefaultAsync(x => x.PatientId == id.Value);

                if (model == null)
                    return NotFound();
            }


            // Full page
            return View(model);
        }


        // POST: /Patient/Upsert
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upsert(Patient patient, IFormFile? ProfileImage)
        {
            ModelState.Remove(nameof(Patient.FollowUpInterval));
            ModelState.Remove(nameof(Patient.PatientIdNumber));
            var isCreate = patient.PatientId == 0;
            if (ProfileImage is { Length: > 1 * 1024 * 1024 })
            {
                ModelState.AddModelError(
                    nameof(Patient.ProfileImageUrl),
                    "Profile photo must not exceed 1 MB.");
                return View(patient);
            }
           
            if (!ModelState.IsValid)
                return View(patient);
            if (ProfileImage != null)
            {
                _fileHelper.DeleteIfNotDefault(patient.ProfileImageUrl);  // remove old
                patient.ProfileImageUrl = await _fileHelper.SaveAsync(ProfileImage, "uploads/doctors");
            }
             //2.Default image on create only
                if (patient.PatientId == 0 && string.IsNullOrEmpty(patient.ProfileImageUrl))
                    patient.ProfileImageUrl = "/uploads/doctors/patient_default.jpg";

            if (isCreate)
            {
                patient.PatientIdNumber = $"PT{DateTime.Now.Ticks.ToString().Substring(10)}";
                patient.CreatedDate = DateTime.Now;
                _context.Patients.Add(patient);
                
                await _context.SaveChangesAsync();

                TempData["success"] = "Patient created successfully.";
            }
            else
            {
                // Preserve existing image if none was uploaded
                if (ProfileImage == null)
                    _context.Entry(patient).Property(d => d.ProfileImageUrl).IsModified = false;

                var dbPatient = await _context.Patients
                    .FirstOrDefaultAsync(x => x.PatientId == patient.PatientId);

                if (dbPatient == null) return NotFound();

                // map scalars
                dbPatient.FirstName = patient.FirstName;
                dbPatient.LastName = patient.LastName;
                dbPatient.Email = patient.Email;
                dbPatient.PhoneNumber = patient.PhoneNumber;
                dbPatient.DateOfBirth = patient.DateOfBirth;
                dbPatient.Address = patient.Address;
                dbPatient.InsuranceProvider = patient.InsuranceProvider;
                dbPatient.InsuranceNumber = patient.InsuranceNumber;
                dbPatient.Allergies = patient.Allergies;
                dbPatient.DentalHistory = patient.DentalHistory;
                dbPatient.Notes = patient.Notes;
                dbPatient.LastVisited = patient.LastVisited;
                dbPatient.Gender = patient.Gender;



                await _context.SaveChangesAsync();

                TempData["success"] = "Patient updated successfully.";
            }

            return RedirectToAction("Index");
        }


        // GET: /Patient/GetPatientsData
        [HttpGet]
        public async Task<IActionResult> GetPatientList()
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

                IQueryable<Patient> q = _context.Patients.AsNoTracking();

                int recordsTotal = await q.CountAsync();

                // Search
                if (!string.IsNullOrWhiteSpace(searchValue))
                {
                    string term = searchValue.ToLower();
                    q = q.Where(p =>
                        (p.FirstName ?? "").ToLower().Contains(term) ||
                        (p.LastName ?? "").ToLower().Contains(term) ||
                        (p.Email ?? "").ToLower().Contains(term) ||
                        (p.PhoneNumber ?? "").ToLower().Contains(term) ||
                        (p.Address ?? "").ToLower().Contains(term) ||
                        (p.InsuranceProvider ?? "").ToLower().Contains(term) ||
                        (p.Allergies ?? "").ToLower().Contains(term)
                    );
                }

                int recordsFiltered = await q.CountAsync();

                // Sort map (index aligns with your table columns in view)
                // Example: 0 Name | 1 Email | 2 Phone | 3 DOB | 4 CreatedDate | 5 LastVisit | 6 Actions
                Expression<Func<Patient, object>> sortSelector = orderColIndex switch
                {
                    0 => p => p.FirstName,
                    1 => p => p.Email,
                    2 => p => p.PhoneNumber,
                    3 => p => p.DateOfBirth,
                    4 => p => p.CreatedDate,
                    5 => p => p.LastVisited,

                };

                bool desc = orderDir.Equals("desc", StringComparison.OrdinalIgnoreCase);
                q = desc ? q.OrderByDescending(sortSelector) : q.OrderBy(sortSelector);

                // Page + shape
                var data = await q
                    .Skip(start)
                    .Take(length)
                    .Select(p => new
                    {
                        id = p.PatientId,
                        firstName = p.FirstName,
                        lastName = p.LastName,
                        email = p.Email,
                        phoneNumber = p.PhoneNumber,
                        dateOfBirth = p.DateOfBirth,
                        age = EF.Functions.DateDiffYear(p.DateOfBirth, DateTime.UtcNow),
                        gender = p.Gender,
                        createdDate = p.CreatedDate,
                        imageUrl = p.ProfileImageUrl ?? "/uploads/patient/patient_default.jpg",
                        status = "Available"   // or p.Status
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
        


        // POST: Doctor/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var patient = await _context.Patients.FindAsync(id);
                if (patient == null)
                {
                    return NotFound();
                }
                bool hasAppointments = _context.Appointments.Any(a => a.PatientId == id);
                if (hasAppointments)
                {
                    return Json(new { status = false, message = "Patient has appointments. Delete them first." });
                }

                // Optional: Delete associated image
                _fileHelper.DeleteIfNotDefault(patient.ProfileImageUrl);


                _context.Patients.Remove(patient);
                await _context.SaveChangesAsync();

                TempData["success"] = "Doctor deleted successfully!";
                return Json(new { status = true, message = "Deleted successfully." });

                //return RedirectToAction(nameof(Index));
            }
            catch 
            {
                return Json(new { status = false, message = "Error deleting patient." });
            }
        }


        private bool DoctorExists(int id)
        {
            return _context.Patients.Any(e => e.PatientId == id);
        }
    }
}










