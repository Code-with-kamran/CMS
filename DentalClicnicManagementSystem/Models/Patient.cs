using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CMS.Models
{
    public class Patient
    {
        public int PatientId { get; set; }
        public string? PatientIdNumber { get; set; } // e.g., #PT0025
        [Required]
        [StringLength(100)]
        public string FirstName { get; set; }

        [Required]
        [StringLength(100)]
        public string LastName { get; set; }
        public bool IsDeleted { get; set; } = false;

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [Phone]
        public string PhoneNumber { get; set; }

        public string? FollowUpInterval { get; set; } // e.g., "1_week", "1_month"

        public DateTimeOffset? FollowUpScheduledAt { get; set; } // The exact UTC date the follow-up is due

      
        public DateTimeOffset? FollowUpSentAt { get; set; }

        public DateTimeOffset? DateOfBirth { get; set; }
        public string? BloodGroup { get; set; }
       
        public DateTimeOffset? RegistrationDate { get; set; } // Added from UI
        public string Gender { get; set; }
        public string? Address { get; set; }
        public string? InsuranceProvider { get; set; }
        public string? Profession { get; set; }
        public string? InsuranceNumber { get; set; }
        public string? Allergies { get; set; }
        public string? DentalHistory { get; set; }
        public string? Notes { get; set; }
        public string? ProfileImageUrl { get; set; } 
        public DateTimeOffset CreatedDate { get; set; } = DateTimeOffset.Now;
        public DateTimeOffset? LastVisited { get; set; }



        // Navigation properties
        public virtual ICollection<Document>? Documents { get; set; } // Assuming the Patient has many Documents
        public virtual ICollection<FollowUp>? FollowUps { get; set; } // Assuming the Patient has many FollowUps

        public virtual ICollection<Treatment>? Treatments { get; set; } = new List<Treatment>();
        public virtual ICollection<Appointment>? Appointments { get; set; } = new List<Appointment>();
        public virtual ICollection<InvoiceItem> InvoiceItems { get; set; } = new List<InvoiceItem>();
    }
}


