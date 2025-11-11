using CMS.Data;
using CMS.ViewModels;
using CMS.Models;
using CMS.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace CMS.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class AppointmentApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AppointmentApiController> _logger;

        public AppointmentApiController(ApplicationDbContext context, ILogger<AppointmentApiController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpPost("submit")]
        public async Task<ActionResult<AppointmentApiResponse>> SubmitAppointment([FromBody] AppointmentApiRequest request)
        {
            try
            {
                // Validate the request
                var validationResult = await ValidateAppointmentRequest(request);
                if (!validationResult.IsValid)
                {
                    return BadRequest(new AppointmentApiResponse
                    {
                        Success = false,
                        Message = "Validation failed",
                        Errors = validationResult.Errors
                    });
                }

                // Check if it's a business day
                if (!IsBusinessDay(request.AppointmentDate))
                {
                    return BadRequest(new AppointmentApiResponse
                    {
                        Success = false,
                        Message = "Appointments can only be scheduled on business days (Monday-Friday)"
                    });
                }

                // Parse and validate appointment time
                if (!TryParseAppointmentTime(request.AppointmentTime, request.AppointmentDate, out DateTime appointmentDateTime))
                {
                    return BadRequest(new AppointmentApiResponse
                    {
                        Success = false,
                        Message = "Invalid appointment time. Please use format like '10:30 AM' between 9:00 AM and 4:00 PM"
                    });
                }

                // Create or find patient
                var patient = await CreateOrFindPatient(request);

                // Find available doctor (you might want to implement doctor selection logic)
                var doctor = await GetAvailableDoctor(request.Clinic, appointmentDateTime);
                if (doctor == null)
                {
                    return BadRequest(new AppointmentApiResponse
                    {
                        Success = false,
                        Message = "No available doctor found for the selected time and clinic"
                    });
                }

                // Create appointment
                var appointment = new Appointment
                {
                    AppointmentNo = GenerateAppointmentNumber(),
                    PatientId = patient.PatientId,
                    DoctorId = doctor.Id,
                    AppointmentDate = appointmentDateTime,
                    AppointmentType = request.AppointmentType,
                    Notes = request.ProblemDescription,
                    Status = "Scheduled",
                    Mode = "In-Person",
                    Fee = GetAppointmentFee(request.AppointmentType),
                    CreatedOn = DateTimeOffset.UtcNow
                };

                _context.Appointments.Add(appointment);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Appointment created successfully: {appointment.AppointmentNo}");

                return Ok(new AppointmentApiResponse
                {
                    Success = true,
                    Message = "Appointment scheduled successfully",
                    AppointmentNumber = appointment.AppointmentNo,
                    ScheduledDateTime = appointmentDateTime
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating appointment");
                return StatusCode(500, new AppointmentApiResponse
                {
                    Success = false,
                    Message = "An error occurred while processing your appointment. Please try again."
                });
            }
        }

        private async Task<ValidationResult> ValidateAppointmentRequest(AppointmentApiRequest request)
        {
            var errors = new List<string>();

            // Validate gender
            if (!new[] { "Male", "Female" }.Contains(request.Gender))
            {
                errors.Add("Gender must be either 'Male' or 'Female'");
            }

            // Validate clinic
            if (!new[] { "Port-au-Prince", "Cap-Haïtien", "Gonaïves" }.Contains(request.Clinic))
            {
                errors.Add("Invalid clinic selection");
            }

            // Validate appointment type
            var validTypes = new[] { "Check-up", "Prophylaxis", "Specialized Care", "Extraction", "Other" };
            if (!validTypes.Contains(request.AppointmentType))
            {
                errors.Add("Invalid appointment type");
            }

            // Validate client ID for returning patients
            if (!request.IsFirstAppointment && string.IsNullOrEmpty(request.ClientId))
            {
                errors.Add("Client ID is required for returning patients");
            }

            // Validate appointment date (not in the past)
            if (request.AppointmentDate.Date < DateTime.Today)
            {
                errors.Add("Appointment date cannot be in the past");
            }

            return new ValidationResult
            {
                IsValid = errors.Count == 0,
                Errors = errors
            };
        }

        private bool IsBusinessDay(DateTime date)
        {
            return date.DayOfWeek != DayOfWeek.Saturday && date.DayOfWeek != DayOfWeek.Sunday;
        }

        private bool TryParseAppointmentTime(string timeString, DateTime appointmentDate, out DateTime appointmentDateTime)
        {
            appointmentDateTime = default;

            if (DateTime.TryParse($"{appointmentDate:yyyy-MM-dd} {timeString}", out appointmentDateTime))
            {
                var hour = appointmentDateTime.Hour;
                return hour >= 9 && hour < 16; // 9 AM to 4 PM
            }

            return false;
        }

        private async Task<Patient> CreateOrFindPatient(AppointmentApiRequest request)
        {
            Patient patient;

            if (!request.IsFirstAppointment && !string.IsNullOrEmpty(request.ClientId))
            {
                // Try to find existing patient by client ID
                patient = await _context.Patients.FirstOrDefaultAsync(p => p.PatientIdNumber == request.ClientId);
                if (patient != null)
                {
                    return patient;
                }
            }

            // Create new patient
            patient = new Patient
            {
                PatientIdNumber = GeneratePatientNumber(),
                FirstName = request.FullName.Split(' ').FirstOrDefault() ?? "",
                LastName = string.Join(" ", request.FullName.Split(' ').Skip(1)),
                PhoneNumber = request.Phone,
                Email = request.Email,
                DateOfBirth = request.DateOfBirth,
                Gender = request.Gender,
                CreatedDate = DateTimeOffset.UtcNow
            };

            _context.Patients.Add(patient);
            await _context.SaveChangesAsync();

            return patient;
        }

        private async Task<Doctor> GetAvailableDoctor(string clinic, DateTime appointmentDateTime)
        {
            // Simple logic - you might want to implement more sophisticated doctor selection
            return await _context.Doctors.FirstOrDefaultAsync(d => !d.IsDeleted);
        }

        private string GenerateAppointmentNumber()
        {
            return $"APT{DateTime.Now:yyyyMMdd}{DateTime.Now.Ticks.ToString().Substring(10)}";
        }

        private string GeneratePatientNumber()
        {
            return $"PAT{DateTime.Now:yyyyMMdd}{DateTime.Now.Ticks.ToString().Substring(10)}";
        }

        private decimal GetAppointmentFee(string appointmentType)
        {
            return appointmentType switch
            {
                "Check-up" => 50.00m,
                "Prophylaxis" => 75.00m,
                "Specialized Care" => 150.00m,
                "Extraction" => 100.00m,
                _ => 50.00m
            };
        }
    }

    public class ValidationResult
    {
        public bool IsValid { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
    }
}
