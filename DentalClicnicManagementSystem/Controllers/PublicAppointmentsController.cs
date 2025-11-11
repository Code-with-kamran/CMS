//using CMS.Data; // Assuming you have a DbContext
//using CMS.Models;
//using CMS.ViewModels;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.EntityFrameworkCore;

//namespace CMS.Controllers.Api           
//{
//    [ApiController]
//    [Route("api/[controller]")]
//    public class PublicAppointmentController : ControllerBase
//    {
//        private readonly ApplicationDbContext _context; // Replace with your DbContext name

//        public PublicAppointmentController(ApplicationDbContext context)
//        {
//            _context = context;
//        }

//        [HttpPost("book")]
//        public async Task<IActionResult> BookAppointment([FromBody] PublicAppointmentViewModel model)
//        {
//            try
//            {
//                // **Validate the model** [[1]]
//                if (!ModelState.IsValid)
//                {
//                    var errors = ModelState
//                        .Where(x => x.Value.Errors.Count > 0)
//                        .Select(x => new {
//                            Field = x.Key,
//                            Errors = x.Value.Errors.Select(e => e.ErrorMessage)
//                        });

//                    return BadRequest(new
//                    {
//                        success = false,
//                        message = "Validation failed",
//                        errors = errors
//                    });
//                }

//                // **Custom validation for business rules**
//                var validationResult = await ValidateAppointmentRequest(model);
//                if (!validationResult.IsValid)
//                {
//                    return BadRequest(new
//                    {
//                        success = false,
//                        message = validationResult.ErrorMessage
//                    });
//                }

//                // **Find or create patient**
//                Patient patient = null;

//                if (!model.IsFirstAppointment && !string.IsNullOrEmpty(model.ClientId))
//                {
//                    // Try to find existing patient by ClientId
//                    patient = await _context.Patients
//                        .FirstOrDefaultAsync(p => p.PatientIdNumber == model.ClientId && !p.IsDeleted);

//                    if (patient == null)
//                    {
//                        return BadRequest(new
//                        {
//                            success = false,
//                            message = "Client ID not found. Please check your client ID or select 'Yes' for first appointment."
//                        });
//                    }
//                }
//                else
//                {
//                    // Create new patient
//                    patient = new Patient
//                    {
//                        FirstName = model.FullName,
//                        PhoneNumber = model.Phone,
//                        Email = model.Email,
//                        DateOfBirth = model.DateOfBirth,
//                        Gender = model.Gender,
//                        PatientIdNumber = GeneratePatientNumber(),
//                        CreatedDate = DateTimeOffset.UtcNow,
//                        IsDeleted = false
//                    };

//                    _context.Patients.Add(patient);
//                    await _context.SaveChangesAsync(); // Save to get PatientId
//                }

//                // **Create appointment**
//                var appointmentDateTime = model.AppointmentDate.Add(model.AppointmentTime);
//                var appointmentType = model.AppointmentType == "Other" ?
//                    model.OtherAppointmentType : model.AppointmentType;

//                var appointment = new Appointment
//                {
//                    AppointmentNo = GenerateAppointmentNumber(),
//                    PatientId = patient.PatientId,
//                    DoctorId = await GetDefaultDoctorId(), // You'll need to implement this
//                    AppointmentDate = new DateTimeOffset(appointmentDateTime),
//                    CreatedOn = DateTimeOffset.UtcNow,
//                    Status = "Scheduled",
//                    AppointmentType = appointmentType ?? "General",
//                    Mode = "In-Person",
//                    Notes = model.ProblemDescription,
//                    Fee = 0, // Set default fee or calculate based on type
//                    IsDeleted = false
//                };

//                _context.Appointments.Add(appointment);
//                await _context.SaveChangesAsync();

//                return Ok(new
//                {
//                    success = true,
//                    message = "Appointment booked successfully!",
//                    appointmentId = appointment.AppointmentId,
//                    appointmentNo = appointment.AppointmentNo,
//                    patientId = patient.PatientIdNumber
//                });
//            }
//            catch (Exception ex)
//            {
//                // Log the exception
//                return StatusCode(500, new
//                {
//                    success = false,
//                    message = "An error occurred while booking the appointment. Please try again."
//                });
//            }
//        }

//        private async Task<(bool IsValid, string ErrorMessage)> ValidateAppointmentRequest(PublicAppointmentViewModel model)
//        {
//            // **Validate appointment date is not in the past**
//            if (model.AppointmentDate.Date < DateTime.Today)
//            {
//                return (false, "Appointment date cannot be in the past.");
//            }

//            // **Validate appointment time is within business hours (9 AM - 4 PM)**
//            if (model.AppointmentTime < TimeSpan.FromHours(9) || model.AppointmentTime > TimeSpan.FromHours(16))
//            {
//                return (false, "Appointment time must be between 9:00 AM and 4:00 PM.");
//            }

//            // **Validate appointment date is a weekday**
//            if (model.AppointmentDate.DayOfWeek == DayOfWeek.Saturday ||
//                model.AppointmentDate.DayOfWeek == DayOfWeek.Sunday)
//            {
//                return (false, "Appointments can only be scheduled on weekdays.");
//            }

//            // **Validate ClientId is provided for returning patients**
//            if (!model.IsFirstAppointment && string.IsNullOrEmpty(model.ClientId))
//            {
//                return (false, "Client ID is required for returning patients.");
//            }

//            // **Validate Other appointment type is specified**
//            if (model.AppointmentType == "Other" && string.IsNullOrEmpty(model.OtherAppointmentType))
//            {
//                return (false, "Please specify the appointment type when selecting 'Other'.");
//            }

//            return (true, string.Empty);
//        }

//        private string GeneratePatientNumber()
//        {
//            return $"PAT{DateTime.Now:yyyyMMdd}{new Random().Next(1000, 9999)}";
//        }

//        private string GenerateAppointmentNumber()
//        {
//            return $"APT{DateTime.Now:yyyyMMdd}{new Random().Next(1000, 9999)}";
//        }

//        private async Task<int> GetDefaultDoctorId()
//        {
//            // Return the first available doctor or a default doctor
//            var doctor = await _context.Doctors.FirstOrDefaultAsync(d => !d.IsDeleted);
//            return doctor?.Id ?? 1; // Fallback to ID 1 if no doctor found
//        }
//    }
//}
using CMS.Data;
using CMS.Models;
using CMS.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CMS.Controllers.Api
{
    [ApiController]
    [Route("api/[controller]")]
    [IgnoreAntiforgeryToken]
    public class PublicAppointmentController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public PublicAppointmentController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost("book")]
        public async Task<IActionResult> BookAppointment([FromBody] PublicAppointmentViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState
                        .Where(x => x.Value.Errors.Count > 0)
                        .Select(x => new {
                            Field = x.Key,
                            Errors = x.Value.Errors.Select(e => e.ErrorMessage)
                        });

                    return BadRequest(new
                    {
                        success = false,
                        message = "Validation failed",
                        errors = errors
                    });
                }

                var validationResult = await ValidateAppointmentRequest(model);
                if (!validationResult.IsValid)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = validationResult.ErrorMessage
                    });
                }

                Patient patient = null;

                if (!model.IsFirstAppointment && !string.IsNullOrEmpty(model.ClientId))
                {
                    patient = await _context.Patients
                        .FirstOrDefaultAsync(p => p.PatientIdNumber == model.ClientId && !p.IsDeleted);

                    if (patient == null)
                    {
                        return BadRequest(new
                        {
                            success = false,
                            message = "Client ID not found. Please check your client ID or select 'Yes' for first appointment."
                        });
                    }
                }
                else
                {
                    // Split full name into first and last name
                    var nameParts = SplitFullName(model.FullName);

                    patient = new Patient
                    {
                        FirstName = nameParts.FirstName,
                        LastName = nameParts.LastName,  // Now includes LastName
                        PhoneNumber = model.Phone,
                        Email = model.Email,
                        DateOfBirth = model.DateOfBirth,
                        Gender = model.Gender,
                        PatientIdNumber = GeneratePatientNumber(),
                        CreatedDate = DateTimeOffset.UtcNow,
                        IsDeleted = false
                    };

                    _context.Patients.Add(patient);
                    await _context.SaveChangesAsync();
                }

                var appointmentDateTime = model.AppointmentDate.Add(model.AppointmentTime);
                var appointmentType = model.AppointmentType == "Other" ?
                    model.OtherAppointmentType : model.AppointmentType;

                var appointment = new Appointment
                {
                    AppointmentNo = GenerateAppointmentNumber(),
                    PatientId = patient.PatientId,
                    DoctorId = await GetDefaultDoctorId(),
                    AppointmentDate = new DateTimeOffset(appointmentDateTime),
                    CreatedOn = DateTimeOffset.UtcNow,
                    Status = "Pending",
                    AppointmentType = appointmentType ?? "General",
                    Mode = "In-Person",
                    Notes = model.ProblemDescription,
                    Fee = 0,
                    IsDeleted = false
                };

                _context.Appointments.Add(appointment);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    success = true,
                    message = "Thank you! Your appointment request has been received. You will get an email confirmation for your appointment shortly.",
                    appointmentNo = appointment.AppointmentNo,
                    patientId = patient.PatientIdNumber
                });
            }
            catch (Exception ex)
            {
                // Log the exception here
                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred while processing your appointment request. Please try again later."
                });
            }
        }

        // Add this helper method
        private (string FirstName, string LastName) SplitFullName(string fullName)
        {
            if (string.IsNullOrWhiteSpace(fullName))
            {
                return ("Unknown", "Unknown");
            }

            var parts = fullName.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length == 0)
            {
                return ("Unknown", "Unknown");
            }
            else if (parts.Length == 1)
            {
                // Only one name provided, use it as first name and set last name as empty string or same
                return (parts[0], parts[0]);
            }
            else if (parts.Length == 2)
            {
                // First and last name
                return (parts[0], parts[1]);
            }
            else
            {
                // More than 2 parts: first word is FirstName, rest is LastName
                return (parts[0], string.Join(" ", parts.Skip(1)));
            }
        }

        // Keep all your other methods the same
        private async Task<(bool IsValid, string ErrorMessage)> ValidateAppointmentRequest(PublicAppointmentViewModel model)
        {
            if (model.AppointmentDate.Date < DateTime.Today)
            {
                return (false, "Appointment date cannot be in the past.");
            }

            if (model.AppointmentTime < TimeSpan.FromHours(9) || model.AppointmentTime > TimeSpan.FromHours(16))
            {
                return (false, "Appointment time must be between 9:00 AM and 4:00 PM.");
            }

            if (model.AppointmentDate.DayOfWeek == DayOfWeek.Saturday ||
                model.AppointmentDate.DayOfWeek == DayOfWeek.Sunday)
            {
                return (false, "Appointments can only be scheduled on weekdays.");
            }

            if (!model.IsFirstAppointment && string.IsNullOrEmpty(model.ClientId))
            {
                return (false, "Client ID is required for returning patients.");
            }

            if (model.AppointmentType == "Other" && string.IsNullOrEmpty(model.OtherAppointmentType))
            {
                return (false, "Please specify the appointment type when selecting 'Other'.");
            }

            return (true, string.Empty);
        }

        private string GeneratePatientNumber()
        {
            return $"PAT{DateTime.Now:yyyyMMdd}{new Random().Next(1000, 9999)}";
        }

        private string GenerateAppointmentNumber()
        {
            return $"APT{DateTime.Now:yyyyMMdd}{new Random().Next(1000, 9999)}";
        }

        private async Task<int> GetDefaultDoctorId()
        {
            var doctor = await _context.Doctors.FirstOrDefaultAsync(d => !d.IsDeleted);
            return doctor?.Id ?? 1;
        }

    }
}





