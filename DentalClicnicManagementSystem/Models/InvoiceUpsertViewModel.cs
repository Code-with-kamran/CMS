using CMS.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Permissions;

namespace CMS.ViewModels
{
    public class InvoiceUpsertViewModel
    {
        public Invoice Invoice { get; set; } = new();
        public bool IsAppointmentInvoice { get; set; }
        public bool IsCombinedInvoice { get; set; }
        public bool IsLaboratoryInvoice { get; set; }
        public bool IsMedicationInvoice { get; set; }

        // Add invoice type selection
        public List<SelectListItem> InvoiceTypes { get; set; } = new();

        public List<SelectListItem> Doctors { get; set; } = new();
        public List<SelectListItem> Treatments { get; set; } = new();
        public List<SelectListItem> LaboratoryTests { get; set; } = new();
        public List<InventoryItem> InventoryItems { get; set; } = new();
        public Dictionary<int, Dictionary<string, decimal>> ConsultationCharge { get; set; } = new();
        public List<SelectListItem> Appointments { get; set; } = new();
        public List<SelectListItem> PaymentMethods { get; set; } = new();

        // Helper method to determine invoice type flags
        // Helper property to get invoice type for routingdisabled
        public InvoiceType InvoiceType => Invoice?.InvoiceType ?? InvoiceType.Medication;

        // Helper method to set invoice type flags
        public void SetInvoiceTypeFlags()
        {
            IsAppointmentInvoice = Invoice.InvoiceType == InvoiceType.Appointment;
            IsCombinedInvoice = Invoice.InvoiceType == InvoiceType.Combined;
            IsLaboratoryInvoice = Invoice.InvoiceType == InvoiceType.Laboratory;
            IsMedicationInvoice = Invoice.InvoiceType == InvoiceType.Medication;
        }
       
    }

    public class EmployeeUpsertViewModel
    {
        public Employee Employee { get; set; }
        [ValidateNever]
        public List<PaymentMethod>? PaymentMethods { get; set; }
        public IEnumerable<SelectListItem> PaymentMethodList
        {
            get
            {
                return PaymentMethods?.Select(pm => new SelectListItem
                {
                    Text = pm.Name,
                    Value = pm.Id.ToString()
                }) ?? Enumerable.Empty<SelectListItem>();
            }
        }

    }

}
