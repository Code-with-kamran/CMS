namespace CMS.Models
{
   
        public enum AppointmentStatus
        {
            Scheduled = 0,
            Completed = 1,
            Cancelled = 2,
            NoShow = 3
        }

        public enum InvoiceStatus
        {
            Draft = 0,
            Unpaid = 1,
            PartiallyPaid = 2,
            Paid = 3,
            Overdue = 4
        }

        public enum StockReason
        {
            Purchase = 0,
            Use = 1,
            Adjustment = 2,
            Expired = 3,
            Return = 4
        }
    }



