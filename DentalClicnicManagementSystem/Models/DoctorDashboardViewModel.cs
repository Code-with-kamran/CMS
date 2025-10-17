using CMS.Models;
using System;
using System.Collections.Generic;

namespace CMS.ViewModels
{
    public class DoctorDashboardViewModel
    {
        // Doctor Information
        public Doctor Doctor { get; set; }
        public int TotalAppointmentsToday { get; set; }
        public int OnlineConsultationsToday { get; set; }
        public int CancelledAppointmentsToday { get; set; }

        // Percentage changes
        public decimal TotalAppointmentsChange { get; set; }
        public decimal CompletedConsultaion { get; set; }
        public decimal CancelledAppointmentsChange { get; set; }

        // Today's Appointments List
        public List<TodayAppointmentViewModel> TodayAppointments { get; set; }

        // Next Patient Details
        public PatientDetailsViewModel NextPatient { get; set; }

        // Appointment Requests
        public List<AppointmentRequestViewModel> AppointmentRequests { get; set; }

        // Chart Data
        public List<int> WeeklyAppointmentData { get; set; }
        public List<int> WeeklyOnlineConsultationData { get; set; }
        public List<int> WeeklyCancelledData { get; set; }
    }

    public class TodayAppointmentViewModel
    {
        public int AppointmentId { get; set; }
        public string PatientName { get; set; }
        public string PatientImage { get; set; }
        public string Status { get; set; }
        public string StatusClass { get; set; }
        public DateTimeOffset AppointmentTime { get; set; }
        public string Reason { get; set; }
    }

    public class PatientDetailsViewModel
    {
        public int PatientId { get; set; }
        public string FullName { get; set; }
        public string ProfileImage { get; set; }
        public string Address { get; set; }
        public DateTimeOffset DateOfBirth { get; set; }
        public string Gender { get; set; }
        public decimal Weight { get; set; }
        public decimal Height { get; set; }
        public DateTimeOffset? LastAppointmentDate { get; set; }
        public DateTimeOffset RegisterDate { get; set; }
        public List<string> Conditions { get; set; }
        public string PhoneNumber { get; set; }
    }

    public class AppointmentRequestViewModel
    {
        public int RequestId { get; set; }
        public string PatientName { get; set; }
        public string PatientImage { get; set; }
        public string RequestType { get; set; }
        public DateTimeOffset RequestDate { get; set; }
    }
}
