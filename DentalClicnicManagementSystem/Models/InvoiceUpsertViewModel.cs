using CMS.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Permissions;
namespace CMS.ViewModels
{
    public class InvoiceUpsertViewModel
    {
        public Invoice Invoice { get; set; }

        public List<InventoryItem> InventoryItems { get; set; } = new();

        
        public bool IsAppointmentInvoice { get; set; }
        public  decimal ConsultationCharge { get; set; } = new();

        public List<SelectListItem> Doctors { get; set; }

        public List<SelectListItem> Appointments { get; set; }
    }
}
