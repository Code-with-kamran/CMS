using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CMS.Models
{
    public class LaboratoryOrder
    {
        public int Id { get; set; }

        [Required]
        [StringLength(150)]
        [Display(Name = "Test Name")]
        public string TestName { get; set; }

        [StringLength(50)]
        [Display(Name = "Test Code")]
        public string? TestCode { get; set; }

        [Required]
        [Display(Name = "Patient")]
        public int PatientId { get; set; }

        

        [Required]
        [Column(TypeName = "decimal(18, 2)")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than zero.")]
        public decimal TestPrice { get; set; }

        [Required]
        public OrderStatus Status { get; set; } = OrderStatus.Ordered;

        [Display(Name = "Order Date")]
        public DateTime OrderDate { get; set; } = DateTime.Now;

        [Display(Name = "Sample Collection Date")]
        public DateTime? CollectionDate { get; set; }

        [Display(Name = "Result Date")]
        public DateTime? ResultDate { get; set; }

        [StringLength(4000)]
        public string? Result { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }

        // Navigation Properties
        [ForeignKey("PatientId")]
        [ValidateNever]
        public virtual Patient? Patient { get; set; }

     

        // Audit Fields
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }
    }

    public enum OrderStatus
    {
        Ordered,
        [Display(Name = "Sample Collected")]
        SampleCollected,
        [Display(Name = "In Progress")]
        InProgress,
        Completed,
        Cancelled
    }
}
