
// File: Models/PatientViewModels/PatientProfileViewModel.cs
using CMS.Models;
using System;
using System.Collections.Generic;

namespace CMS.ViewModels

{
   
    public class PatientInfoViewModel
    {
        public int PatientId { get; set; }
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? Gender { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }
        public DateTime CreatedDate { get; set; }
        public string? ProfileImageUrl { get; set; }
    }

    // This is the main ViewModel for the entire page
    public class PatientProfileViewModel
    {
        // UPDATE: Use the new flat ViewModel instead of the raw Patient model
        public PatientInfoViewModel Patient { get; set; } = new();
        //public List<MedicalHistory> MedicalHistories { get; set; }

        // Ensure these are using AppointmentViewModel
        public List<AppointmentViewModel> UpcomingAppointments { get; set; } = new();
        public List<AppointmentViewModel> PastAppointments { get; set; } = new();
        public int PastAppointmentsCount { get; set; }
        public int UpcomingAppointmentsCount { get; set; }
        public List<Document> Documents { get; set; } = new();
        public List<Payment> Payments { get; set; } = new();
        public List<FollowUp> FollowUps { get; set; } = new();
        public List<DisplayNote> CombinedNotes { get; set; } = new();
    }

    // This class represents a single note in the combined list
    public class DisplayNote
    {
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public string Source { get; set; } = string.Empty;
    }

    public class PatientHistoryViewModel
    {
        public List<MedicalHistory> MedicalHistory { get; set; }
        public List<FollowUp> FollowUps { get; set; }
    }

   

   

   

    

    

    
}
