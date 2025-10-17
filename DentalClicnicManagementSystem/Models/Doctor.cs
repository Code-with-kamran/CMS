using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CMS.Models
{
    public class Doctor
    {
        [Key] public int Id { get; set; }

        // Core
        [Required, StringLength(100)]
        public string FullName { get; set; }

        [Required]
        [StringLength(50, ErrorMessage = "Specialty may not exceed 50 characters.")]
        public string Specialty { get; set; }

        [Required]
        public string Degrees { get; set; }

        [StringLength(500)]
        public string About { get; set; }

        // Contact
        [StringLength(100), DataType(DataType.EmailAddress)]
        public string Email { get; set; }

        [StringLength(15), DataType(DataType.PhoneNumber)]
        public string Phone { get; set; }

        [StringLength(100)]
        public string Address { get; set; }

        // adding new types
        public int? DepartmentId { get; set; }
        [ValidateNever]
        public virtual Department Department { get; set; }


        [ValidateNever]
        public string? ProfileImageUrl { get; set; }

        // Admin
        public decimal ConsultationCharge { get; set; }
        public int ConsultationDurationInMinutes { get; set; }
        public string MedicalLicenseNumber { get; set; }
        public string Clinic { get; set; }

        // General
        public string BloodGroup { get; set; }
        public string YearOfExperience { get; set; }
        public string AvailabilityStatus { get; set; } = "Available";

        // Auth (⚠️ hash in real apps)
        [Required] public string Password { get; set; }
        public string ConfirmPassword { get; set; }

        public DateTime CreatedOn { get; set; } = DateTime.Now;
        public DateTime? UpdatedOn { get; set; }

        public int? UserId { get; set; }
        [ValidateNever]
        public User User { get; set; }

        // Child collections (no dropdowns; created inline)
        public List<DoctorWeeklyAvailability> WeeklyAvailabilities { get; set; } = new();
        public List<DoctorDateAvailability> DateAvailabilities { get; set; } = new();
        public List<DoctorEducation> Educations { get; set; } = new();
        public List<DoctorAward> Awards { get; set; } = new();
        public List<DoctorCertification> Certifications { get; set; } = new();
    }

    public class DoctorWeeklyAvailability
    {
        public int Id { get; set; }
        public int DoctorId { get; set; }

        public DayOfWeek? DayOfWeek { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public bool IsWorkingDay { get; set; } = true;
        public TimeSpan SlotDuration { get; set; } = TimeSpan.FromMinutes(30);

    }

    public class DoctorDateAvailability
    {
        public int Id { get; set; }
        public int DoctorId { get; set; }

        public DateTime? Date { get; set; }
        public TimeSpan? StartTime { get; set; }
        public TimeSpan? EndTime { get; set; }
        public bool IsAvailable { get; set; } = true;
    }

    public class DoctorEducation
    {
        public int Id { get; set; }
        public int DoctorId { get; set; }

        public string? Degree { get; set; }
        public string? Institution { get; set; }
        public int? Year { get; set; }
        public string? Notes { get; set; }
    }

    public class DoctorAward
    {
        public int Id { get; set; }
        public int DoctorId { get; set; }

        public string? Title { get; set; }
        public string? Issuer { get; set; }
        public int? Year { get; set; }
    }

    public class DoctorCertification
    {
        public int Id { get; set; }
        public int DoctorId { get; set; }

        public string? Title { get; set; }
        public string? Authority { get; set; }
        public string? LicenseNumber { get; set; }
        public string? Department { get; set; } = string.Empty;
        public DateTime? IssuedOn { get; set; }
        public DateTime? ExpiresOn { get; set; }
    }

}
