using CMS.Models;
namespace CMS.ViewModels
{
    public class PayrollRunDetailsViewModel
    {
        public PayrollRun PayrollRun { get; set; }
        public PayrollRunCardSummary CardSummary { get; set; }
        // Add any other data as needed
    }
    public class PayrollRunCardSummary
    {
        public int Year { get; set; }         // Add this line!
        public int Month { get; set; }
        public int TotalEmployees { get; set; }
        public decimal TotalEmployeesChangePercent { get; set; }
        public decimal TotalNetPay { get; set; }
        public decimal TotalNetPayChangePercent { get; set; }
        public decimal TotalAllowances { get; set; }
        public decimal TotalAllowancesChangePercent { get; set; }
        public decimal TotalDeductions { get; set; }
        public decimal TotalDeductionsChangePercent { get; set; }
    }

    public class PayrollSummaryViewModel
    {
        public PayrollRun PayrollRun { get; set; }
        public int TotalEmployees { get; set; }
        public decimal TotalBaseSalary { get; set; }
        public decimal TotalAllowances { get; set; }
        public decimal TotalDeductions { get; set; }
        public decimal TotalNetPay { get; set; }
        public decimal AverageNetPay { get; set; }
    }
}
