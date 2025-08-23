using System.ComponentModel.DataAnnotations;

namespace CMS.Models
{
    public class Doctor
    {
        public int Id { get; set; }

        [Required, StringLength(100)]
        public string FullName { get; set; } = string.Empty;

        [StringLength(100)]
        public string Email { get; set; }= string.Empty;

        [StringLength(15)]
        public string Phone { get; set; } = string.Empty;

        [StringLength(100)]
        public string Specialty { get; set; } = string.Empty;

        [StringLength(100)]
        public string Address { get; set; } = string.Empty;

        [StringLength(200)]
        public string? ProfileImageUrl { get; set; } = string.Empty;

        [StringLength(500)]
        public string About { get; set; } = string.Empty;
        [Required]
        public string Passward { get; set; } = string.Empty;
        [Required]
        public string ConfirmPassward { get; set; } = string.Empty;
    }
}
