namespace CMS.Models
{
    public class PaymentRequest
    {
        public int Id { get; set; }
        public int InvoiceId { get; set; }
        public decimal Amount { get; set; }
        public int PaymentMethodId { get; set; }
        
        public string? TransactionReference { get; set; }
        public string? Notes { get; set; }


    }
}