using CMS.Models;

namespace CMS.ViewModels
{
    public class ThermalReceiptViewModel
    {
        public string InvoiceNumber { get; set; }
        public DateTime IssueDate { get; set; }
        public string CustomerName { get; set; }
        public string CustomerAddress { get; set; }
        public string Status { get; set; }
        public decimal SubTotal { get; set; }
        public decimal Tax { get; set; }
        public decimal Discount { get; set; }
        public decimal Total { get; set; }
        public decimal AmountPaid { get; set; }
        public decimal AmountDue { get; set; }
        public string Notes { get; set; }
        public string DoctorName { get; set; }
        public string CompanyName { get; set; }
        public string CompanyAddress { get; set; }
        public string CompanyPhone { get; set; }
        public List<ReceiptItem> Items { get; set; }
    }

    public class ReceiptItem
    {
        public string Description { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal LineTotal { get; set; }
        public string ItemType { get; set; }
        public string TestName { get; set; }
        public string MedicationsName { get; set; }
        public string TreatmentName { get; set; }
        public string AppointmentWith { get; set; }
        public string InventoryItemName { get; set; }
    }
}
