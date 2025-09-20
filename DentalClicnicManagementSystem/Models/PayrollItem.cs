using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CMS.Models
{
    public class PayrollItem
    {
        public int Id { get; set; }

        [Required]
        public int PayrollRunId { get; set; }
        [ValidateNever]
        public PayrollRun PayrollRun { get; set; }

        [Required]
        public int EmployeeId { get; set; }
        [ValidateNever]       
        public Employee Employee { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        [Range(0, 9999999999999999.99)]
        public decimal BaseSalary { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        [Range(0, 9999999999999999.99)]
        public decimal Allowances { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        [Range(0, 9999999999999999.99)]
        public decimal Deductions { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        [Range(0, 9999999999999999.99)]
        public decimal NetPay { get; set; }
    }
}
