using CMS.Data;
using CMS.Helpers;
using CMS.Models;
using CMS.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis.Elfie.Model.Tree;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using static Azure.Core.HttpHeader;

namespace CMS.Controllers
{

    [Authorize(Roles = "Admin,Receptionist,Doctor,Acountant")]
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
                        DateOfBirth = p.DateOfBirth ?? DateTimeOffset.MinValue,   // ►◄ offset
                        PhoneNumber = p.PhoneNumber,
                        Address = p.Address,
                        CreatedDate = p.CreatedDate,         // already DateTimeOffset in DB
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

                var now = DateTimeOffset.Now;   // ►◄ single offset-based clock

                // 1. Fetch upcoming appointments correctly into the ViewModel
                var upcomingAppointments = await _context.Appointments
                    .Include(a => a.Doctor) // Eagerly load the related Doctor entity
                    .Where(a => a.PatientId == id && a.AppointmentDate >= now) // ►◄ offset compare
                    .OrderBy(a => a.AppointmentDate)
                    .Select(a => new AppointmentViewModel // Project into the ViewModel
                    {
                        AppointmentId = a.AppointmentId,
                        AppointmentDate = a.AppointmentDate, // already DateTimeOffset
                        AppointmentType = a.AppointmentType,
                        DoctorName = a.Doctor != null ? a.Doctor.FullName : "N/A"
                    })
                    .ToListAsync();

                // 2. FIX: Fetch past appointments with the same correct logic
                var pastAppointments = await _context.Appointments
                    .Include(a => a.Doctor) // Eagerly load the related Doctor entity
                    .Where(a => a.PatientId == id && a.AppointmentDate < now) // ►◄ offset compare
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
                    combinedNotes.Add(new DisplayNote
                    {
                        Content = patientRecord.Notes,
                        CreatedAt = patientRecord.CreatedDate, // already DateTimeOffset
                        Source = "Patient Record"
                    });
                }
                foreach (var followUp in followUps)
                {
                    if (!string.IsNullOrWhiteSpace(followUp.Notes))
                    {
                        combinedNotes.Add(new DisplayNote
                        {
                            Content = followUp.Notes,
                            CreatedAt = followUp.FollowUpDate, // already DateTimeOffset
                            Source = "Follow-up"
                        });
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
            var viewModel = new PatientUpsertViewModel
            {
                AvailableTreatments = await _context.Treatments
                    .Where(t => t.IsActive)
                    .Select(t => new SelectListItem { Value = t.TreatmentId.ToString(), Text = t.Name })
                    .ToListAsync(),

                AvailableMedications = await _context.InventoryItems
                    .Where(i => i.IsActive)
                    .Select(i => new SelectListItem { Value = i.Id.ToString(), Text = $"{i.Name} (Stock: {i.Stock})" })
                    .ToListAsync(),

                // Fetch the patient data if id is not null
                Patient = id == null || id == 0 ? new Patient() : await _context.Patients
                    .FirstOrDefaultAsync(x => x.PatientId == id.Value)
            };

            // If the patient doesn't exist, return NotFound
            if (viewModel.Patient == null && id != null)
                return NotFound();

            // Return the View with the view model
            return View(viewModel);
        }

        // POST: /Patient/Upsert
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upsert(PatientUpsertViewModel viewModel, IFormFile? ProfileImage)
        {
            // Remove properties that you don't want to bind or process in this POST action
            ModelState.Remove(nameof(Patient.FollowUpInterval));
            ModelState.Remove(nameof(Patient.PatientIdNumber));

            var isCreate = viewModel.Patient.PatientId == 0;

            // Validate profile image size if exists
            if (ProfileImage is { Length: > 1 * 1024 * 1024 })
            {
                ModelState.AddModelError(
                    nameof(Patient.ProfileImageUrl),
                    "Profile photo must not exceed 1 MB.");
                return View(viewModel); // Return the view with the model so validation errors are shown
            }

            if (!ModelState.IsValid)
                return View(viewModel); // Return the view with the model if validation fails

            // Handle the profile image if it's provided
            if (ProfileImage != null)
            {
                _fileHelper.DeleteIfNotDefault(viewModel.Patient.ProfileImageUrl); // Remove old profile image if a new one is uploaded
                viewModel.Patient.ProfileImageUrl = await _fileHelper.SaveAsync(ProfileImage, "uploads/doctors");
            }

            // Set a default image if creating a new patient and none is uploaded
            if (viewModel.Patient.PatientId == 0 && string.IsNullOrEmpty(viewModel.Patient.ProfileImageUrl))
                viewModel.Patient.ProfileImageUrl = "/uploads/doctors/patient_default.jpg";

            if (isCreate)
            {
                // Generate a unique patient ID and set the creation date
                viewModel.Patient.PatientIdNumber = $"PT{DateTime.Now.Ticks.ToString().Substring(10)}";
                viewModel.Patient.CreatedDate = DateTime.Now;
                _context.Patients.Add(viewModel.Patient);
                await _context.SaveChangesAsync();

                TempData["success"] = "Patient created successfully.";
                return RedirectToAction("Index");
            }
            else
            {
                // Preserve existing image if none was uploaded during update
                if (ProfileImage == null)
                    _context.Entry(viewModel.Patient).Property(d => d.ProfileImageUrl).IsModified = false;

                var dbPatient = await _context.Patients
                    .FirstOrDefaultAsync(x => x.PatientId == viewModel.Patient.PatientId);

                if (dbPatient == null)
                    return NotFound();

                // Update patient data in the database
                dbPatient.FirstName = viewModel.Patient.FirstName;
                dbPatient.LastName = viewModel.Patient.LastName;
                dbPatient.Email = viewModel.Patient.Email;
                dbPatient.PhoneNumber = viewModel.Patient.PhoneNumber;
                dbPatient.DateOfBirth = viewModel.Patient.DateOfBirth;
                dbPatient.Address = viewModel.Patient.Address;
                dbPatient.InsuranceProvider = viewModel.Patient.InsuranceProvider;
                dbPatient.InsuranceNumber = viewModel.Patient.InsuranceNumber;
                dbPatient.Allergies = viewModel.Patient.Allergies;
                dbPatient.DentalHistory = viewModel.Patient.DentalHistory;
                dbPatient.Notes = viewModel.Patient.Notes;
                dbPatient.LastVisited = viewModel.Patient.LastVisited;
                dbPatient.Gender = viewModel.Patient.Gender;

                await _context.SaveChangesAsync();

                TempData["success"] = "Patient updated successfully.";
                return RedirectToAction("Index");
            }

            // After saving the patient, reload available treatments and medications into the viewModel
            viewModel.AvailableTreatments = await _context.Treatments
                .Where(t => t.IsActive)
                .Select(t => new SelectListItem { Value = t.TreatmentId.ToString(), Text = t.Name })
                .ToListAsync();

            viewModel.AvailableMedications = await _context.InventoryItems
                .Where(i => i.IsActive)
                .Select(i => new SelectListItem { Value = i.Id.ToString(), Text = $"{i.Name} (Stock: {i.Stock})" })
                .ToListAsync();

            // Return the updated viewModel to the view
            return View(viewModel);
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










