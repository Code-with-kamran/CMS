using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CMS.Models
{


    [Table("Department")]
    public class Department
    {
        [Key]
        public int DepartmentId { get; set; }

        [Required, StringLength(100)]
        public string DepartmentName { get; set; }   // e.g., "Cardiology", "Dental", "Neurology"

        [StringLength(250)]
        public string? Description { get; set; }           // optional details about department

        public bool IsActive { get; set; } = true;
        // to soft-disable a department if needed
        public ICollection<Doctor>? Doctors { get; set; }
    }
}
