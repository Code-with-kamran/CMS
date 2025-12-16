using CMS.Data;
using CMS.Models;
using CMS.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace CMS.Controllers
{
    public class ApiController : Controller
    {
        private readonly ApplicationDbContext _dbContext;

        public ApiController(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpPost]
        public async Task<IActionResult> CreateAppointment([FromBody] PublicAppointmentVM model)
        {
            if (model == null)
            {
                return BadRequest("Invalid appointment data.");
            }

            // Create an Appointment entity
            var appointment = new Appointment
            {
                // Populate the necessary fields from the model
                AppointmentNo = GenerateAppointmentNumber(), // Assuming you have a method to generate a unique number
                PatientId = model.PatientId, // Make sure PatientId is included in the view model
                DoctorId = model.DoctorId, // Assuming you have DoctorId in the view model
                AppointmentDate = model.PreferredDate, // The Date/Time the appointment is scheduled for
                DepartmentId = model.DepartmentId, // If provided
               
                Status = "Pending", // Set initial status to Pending
 
                Mode = model.Mode, // In-person/Virtual/Other
                Notes = model.Notes, // Optional notes
                IsDeleted = false // By default, it's not deleted
            };

            try
            {
                // Save appointment to the database
                await _dbContext.Appointments.AddAsync(appointment);
                await _dbContext.SaveChangesAsync();

                return Ok(new { Message = "Appointment successfully booked!" });
            }
            catch (Exception ex)
            {
                // Log the exception (consider using logging here)
                return StatusCode(500, new { Message = "An error occurred while saving the appointment.", Error = ex.Message });
            }
        }


        private string GenerateAppointmentNumber()
        {
            // Example: Generate unique appointment number (You can customize this)
            return "APP" + DateTime.UtcNow.ToString("yyyyMMddHHmmss");
        }
    }

}