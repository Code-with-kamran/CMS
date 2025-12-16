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




   public class PublicAppointmentViewModel
        {
            [Required(ErrorMessage = "Full name is required")]
            [Display(Name = "Full Name")]
            public string FullName { get; set; }

            [Required(ErrorMessage = "Phone number is required")]
            [Phone(ErrorMessage = "Invalid phone number format")]
            [Display(Name = "Phone")]
            public string Phone { get; set; }

            [EmailAddress(ErrorMessage = "Invalid email format")]
            [Display(Name = "Email Address")]
            public string? Email { get; set; }

            [Required(ErrorMessage = "Date of birth is required")]
            [Display(Name = "Date of Birth")]
            [DataType(DataType.Date)]
            public DateTime DateOfBirth { get; set; }

            [Required(ErrorMessage = "Gender is required")]
            [Display(Name = "Gender")]
            public string Gender { get; set; } // "Male" or "Female"

            [Required(ErrorMessage = "Please specify if this is your first appointment")]
            [Display(Name = "Is this your first appointment?")]
            public bool IsFirstAppointment { get; set; }

            [Display(Name = "Client ID")]
            public string? ClientId { get; set; } // Only required if IsFirstAppointment = false

            [Required(ErrorMessage = "Appointment date is required")]
            [Display(Name = "Appointment Date")]
            [DataType(DataType.Date)]
            public DateTime AppointmentDate { get; set; }

            [Required(ErrorMessage = "Appointment time is required")]
            [Display(Name = "Appointment Time")]
            [DataType(DataType.Time)]
            public TimeSpan AppointmentTime { get; set; }

            [Required(ErrorMessage = "Appointment type is required")]
            [Display(Name = "Appointment Type")]
            public string AppointmentType { get; set; } // Check-up, Prophylaxis, Specialized Care, Extraction, Other

            [Display(Name = "Other Appointment Type")]
            public string? OtherAppointmentType { get; set; } // Required if AppointmentType = "Other"

            [Display(Name = "Problem Description")]
            [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
            public string? ProblemDescription { get; set; }
        }
    



    public class AppointmentApiRequest
        {
            [Required]
            [StringLength(100)]
            public string FullName { get; set; }

            [Required]
            [Phone]
            public string Phone { get; set; }

            [EmailAddress]
            public string? Email { get; set; }

            [Required]
            public DateTime DateOfBirth { get; set; }

            [Required]
            public string Gender { get; set; } // "Male" or "Female"

            [Required]
            public string Clinic { get; set; } // "Port-au-Prince", "Cap-Haïtien", "Gonaïves"

            [Required]
            public bool IsFirstAppointment { get; set; }

            public string? ClientId { get; set; } // Required if IsFirstAppointment = false

            [Required]
            public DateTime AppointmentDate { get; set; }

            [Required]
            public string AppointmentTime { get; set; } // "10:30 AM"

            [Required]
            public string AppointmentType { get; set; } // "Check-up", "Prophylaxis", etc.

            [StringLength(500)]
            public string? ProblemDescription { get; set; }
        }

        public class AppointmentApiResponse
        {
            public bool Success { get; set; }
            public string Message { get; set; }
            public string? AppointmentNumber { get; set; }
            public DateTime? ScheduledDateTime { get; set; }
            public List<string>? Errors { get; set; }
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


    /// <summary>
    /// Request model for booking an appointment
    /// </summary>
    public class PublicAppointmentApiRequest
    {
        [Required(ErrorMessage = "Le nom complet est requis")]
        [StringLength(100, ErrorMessage = "Le nom ne peut pas dépasser 100 caractères")]
        [Display(Name = "Nom Complet")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "L'email est requis")]
        [EmailAddress(ErrorMessage = "Veuillez fournir une adresse email valide")]
        public string Email { get; set; } = string.Empty;

        [Phone(ErrorMessage = "Veuillez fournir un numéro de téléphone valide")]
        [Display(Name = "Numéro de Téléphone")]
        public string? PhoneNumber { get; set; }

        [Required(ErrorMessage = "Le genre est requis")]
        [RegularExpression("^(Male|Female|Other)$", ErrorMessage = "Le genre doit être Male, Female, ou Other")]
        public string Gender { get; set; } = string.Empty;

        [Display(Name = "Date de Naissance")]
        [DataType(DataType.Date)]
        public DateTime? DateOfBirth { get; set; }

        [StringLength(500, ErrorMessage = "L'adresse ne peut pas dépasser 500 caractères")]
        public string? Address { get; set; }

        [Required(ErrorMessage = "La date préférée est requise")]
        [Display(Name = "Date Préférée")]
        [DataType(DataType.Date)]
        public DateTime PreferredDate { get; set; }

        [Required(ErrorMessage = "L'heure préférée est requise")]
        [Display(Name = "Heure Préférée")]
        public string PreferredTime { get; set; } = string.Empty;

        [Display(Name = "Spécialisation Préférée")]
        [StringLength(100, ErrorMessage = "La spécialisation ne peut pas dépasser 100 caractères")]
        public string? PreferredSpecialization { get; set; }

        [StringLength(1000, ErrorMessage = "Les notes ne peuvent pas dépasser 1000 caractères")]
        public string? Notes { get; set; }
    }

    /// <summary>
    /// Response model for successful appointment booking
    /// </summary>
    public class AppointmentBookingResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = null!;
        public int AppointmentId { get; set; }
        public string TrackingId { get; set; } = null!;
        public string DoctorName { get; set; } = null!;
        public DateTime AppointmentDate { get; set; }
        public string AppointmentTime { get; set; } = null!;
        public decimal Fee { get; set; }
        public DateTime Timestamp { get; set; }
    }



    public class AvailableSlotsResponse
    {
        public bool Success { get; set; }
        public DateTime Date { get; set; }
        public string? Specialization { get; set; }
        public List<DoctorSlotInfo> AvailableDoctors { get; set; } = new();
        public DateTime Timestamp { get; set; }
    }

    public class DoctorSlotInfo
    {
        public int DoctorId { get; set; }
        public string DoctorName { get; set; } = null!;
        public string Specialization { get; set; } = null!;
        public decimal Fee { get; set; }
        public List<string> AvailableSlots { get; set; } = new();
    }

    public class AppointmentStatusResponse
    {
        public bool Success { get; set; }
        public string TrackingId { get; set; } = null!;
        public string Status { get; set; } = null!;
        public string Message { get; set; } = null!;
        public DateTime Timestamp { get; set; }
    }
    public class ApiErrorResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = null!;
        public string? ErrorCode { get; set; }
        public Dictionary<string, string[]>? Errors { get; set; }
        public DateTime Timestamp { get; set; }
    }
}

