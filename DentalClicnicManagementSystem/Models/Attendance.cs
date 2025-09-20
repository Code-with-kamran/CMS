using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace CMS.Models
{
    public class Attendance
    {
        public int Id { get; set; }
        public string UserId { get; set; } = "";
        public DateTime? CheckIn { get; set; }
        public DateTime? CheckOut { get; set; }
    }
}
