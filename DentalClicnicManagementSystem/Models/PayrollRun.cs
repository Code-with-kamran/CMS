// File: Models/PayrollRun.cs
using CMS.Models;
using System.ComponentModel.DataAnnotations;

namespace CMS.Models
{
    public class PayrollRun
    {
        public int Id { get; set; }
        public int Year { get; set; }
        public int Month { get; set; } // 1-12
        public DateTimeOffset RunAt { get; set; }
        [StringLength(500)] public string? Notes { get; set; }
        public ICollection<PayrollItem> Items { get; set; } = new List<PayrollItem>();
    }
}
