namespace CMS.Models
{
   
       
    public enum AppointmentType
    {
        Inperson = 0,
        online = 1

    }
    public enum AppointmentStatus
    {
        Scheduled = 0,
        Completed = 1,
        Cancelled = 2,
        CheckedIn = 3,
        CheckedOut = 4,
        Confirmed = 5,

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



