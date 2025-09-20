using CMS.Data;
using CMS.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace CMS.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Dashboard()
        {
            var userId = int.Parse(User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value);
            var user = _context.Users.FirstOrDefault(u => u.Id == userId);

            if (user != null && user.Role == "Admin")
            {
                return View("Patient");  // Only Admins should access this page
            }

            return Unauthorized();  // For other users
        }
    }
}
