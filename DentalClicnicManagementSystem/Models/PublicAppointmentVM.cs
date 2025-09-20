//// ViewModels/Step1ViewModel.cs
//using System.ComponentModel.DataAnnotations;

//namespace CMS.ViewModels
//{
//    public class PublicAppointmentVM
//    {
//        [Required]
//        [StringLength(100)]
//        [Display(Name = "First Name")]
//        public string FirstName { get; set; }

//        [Required]
//        [StringLength(100)]
//        [Display(Name = "Last Name")]
//        public string LastName { get; set; }

//        [Required]
//        [EmailAddress]
//        public string Email { get; set; }

//        [Required]
//        [Phone]
//        [Display(Name = "Phone Number")]
//        public string PhoneNumber { get; set; }

//        [Required]
//        public string Gender { get; set; }

//        [Display(Name = "Date of Birth")]
//        public DateTime? DateOfBirth { get; set; }

//        public string? Address { get; set; }

//        [Required]
//        [Display(Name = "Preferred Date")]
//        public DateOnly PreferredDate { get; set; } = DateOnly.FromDateTime(DateTime.Today.AddDays(1));

//        [Display(Name = "Preferred Specialization")]
//        public string? PreferredSpecialization { get; set; }

//        public string? Reason { get; set; }
//    }

//    public class AvailabilityRequestDto
//    {
//        [Required]
//        public DateOnly PreferredDate { get; set; }
//        public string? PreferredSpecialization { get; set; }
//    }

//    public class AvailabilityResponseDto
//    {
//        public string Date { get; set; }
//        public List<DoctorAvailabilityDto> Doctors { get; set; } = new();
//    }

//    public class DoctorAvailabilityDto
//    {
//        public int Id { get; set; }
//        public string Name { get; set; }
//        public string Specialization { get; set; }
//        public decimal Fee { get; set; }
//        public List<string> Slots { get; set; } = new();
//    }

//    public class PublicAppointmenConfirmfVM
//    {
//        public string PatientTempId { get; set; }

//        [Required]
//        public int DoctorId { get; set; }

//        [Required]
//        public DateTime AppointmentDate { get; set; }

//        [Required]
//        public string SelectedSlot { get; set; }

//        // Patient info for review
//        public string PatientName { get; set; }
//        public string Email { get; set; }
//        public string PhoneNumber { get; set; }
//        public string Gender { get; set; }
//        public DateTime? DateOfBirth { get; set; }
//        public string? Address { get; set; }
//        public string? Reason { get; set; }

//        // Doctor info
//        public string DoctorName { get; set; }
//        public string DoctorSpecialty { get; set; }
//        public decimal Fee { get; set; }

//        public List<DoctorAvailabilityDto> AvailableDoctors { get; set; } = new();
//    }

//    public class SuccessViewModel
//    {
//        public int AppointmentId { get; set; }
//        public string PatientName { get; set; }
//        public string DoctorName { get; set; }
//        public DateTime AppointmentDate { get; set; }
//        public string TimeRange { get; set; }
//        public decimal Fee { get; set; }
//    }
//}



using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace CMS.ViewModels
{
    public class PublicAppointmentVM
    {
        [Required, StringLength(100)]
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = null!;

        [Required, EmailAddress]
        public string Email { get; set; } = null!;

        [Phone, Display(Name = "Phone Number")]
        public string? PhoneNumber { get; set; }

        [Required]
        public string Gender { get; set; } = null!;

        [Display(Name = "Date of Birth")]
        public DateTime? DateOfBirth { get; set; }

        public string? Address { get; set; }

        [Required]
        [Display(Name = "Preferred Date")]
        [DataType(DataType.Date)]
        public DateTime PreferredDate { get; set; }

        [Required, Display(Name = "Preferred Time")]
        public string PreferredTime { get; set; }


        [Display(Name = "Preferred Specialization")]
        public string? PreferredSpecialization { get; set; }

        public string? Notes { get; set; }

        // Typed dropdown (no ViewBag)
        public IEnumerable<SelectListItem> SpecializationOptions { get; set; } = Enumerable.Empty<SelectListItem>();
        public IEnumerable<SelectListItem> GenderOptions { get; set; } = new[]
        {
            new SelectListItem("Male", "Male"),
            new SelectListItem("Female", "Female"),
            new SelectListItem("Other", "Other")
        };
    }

    public class AvailabilityRequestDto
    {
        [Required] public DateOnly PreferredDate { get; set; }
        public string? PreferredSpecialization { get; set; }
    }

    public class AvailabilityResponseDto
    {
        public string Date { get; set; } = "";
        public List<DoctorAvailabilityDto> Doctors { get; set; } = new();
    }

    public class DoctorAvailabilityDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Specialization { get; set; } = "";
        public decimal Fee { get; set; }
        public List<string> Slots { get; set; } = new();
        // Optional: already booked slot list for UI
        public List<string> BookedSlots { get; set; } = new();
    }

    public class PublicAppointmenConfirmfVM
    {
        public string PatientTempId { get; set; } = "";

        [Required] public int DoctorId { get; set; }
        [Required] public DateTime AppointmentDate { get; set; } // date only part used
        [Required] public string SelectedSlot { get; set; } = "";

        // Patient info (review)
        public string PatientName { get; set; } = "";
        public string Email { get; set; } = "";
        public string? PhoneNumber { get; set; }
        public string Gender { get; set; } = "";
        public DateTime? DateOfBirth { get; set; }
        public string? Address { get; set; }
        public string? Notes { get; set; }

        // Doctor info
        public string DoctorName { get; set; } = "";
        public string DoctorSpecialty { get; set; } = "";
        public decimal Fee { get; set; }

        public List<DoctorAvailabilityDto> AvailableDoctors { get; set; } = new();
    }


    public class BookSlotRequestDto
    {
        public int DoctorId { get; set; }
        public string SelectedSlot { get; set; } = "";
        public DateTime AppointmentDate { get; set; } // yyyy-MM-dd from the page
}



}

