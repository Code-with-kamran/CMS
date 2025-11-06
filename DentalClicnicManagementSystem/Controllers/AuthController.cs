//using CMS.Data;
//using CMS.Models;
//using CMS.ViewModels;
//using Microsoft.AspNetCore.Authentication;
//using Microsoft.AspNetCore.Authentication.Cookies;
//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Identity;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.EntityFrameworkCore;
//using System.Numerics;
//using System.Security.Claims;

//namespace CMS.Controllers
//{
//    public class AuthController : Controller
//    {
//        private readonly ApplicationDbContext _context;
//        private readonly IConfiguration _config;
//        private readonly IPasswordHasher<User> _passwordHasher;

//        public AuthController(ApplicationDbContext context, IConfiguration config, IPasswordHasher<User> passwordHasher)
//        {
//            _context = context;
//            _config = config;
//            _passwordHasher = passwordHasher;
//        }

//        [HttpGet]
//        public IActionResult Login()
//        {
//            return View();
//        }


//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> Login(LoginViewModel model)
//        {
//            if (!ModelState.IsValid)
//                return View(model);

//            // 1. Find the user by email
//            var user = await _context.Users
//                                     .FirstOrDefaultAsync(u => u.Email == model.Email);

//            // 2. Validate user & password  (NO hashing for now)
//            if (user == null || user.PasswordHash != model.Password)
//            {
//                ModelState.AddModelError(string.Empty, "Invalid email or password.");
//                return View(model);
//            }

//            // 3. Sign-in and create claims
//            await SignInUserAsync(user, model.RememberMe);

//            // 4. Redirect based on role
//            switch (user.Role)
//            {
//                case "Admin":
//                    return RedirectToAction("Index", "Admin");
//                case "Doctor":
//                    return RedirectToAction("Index", "DoctorDashboard");
//                case "Receptionist":
//                    return RedirectToAction("Index", "ReceptionistDashboard");
//                default:
//                    return RedirectToAction("Index", "Home");
//            }
//        }


//        private async Task SignInUserAsync(User user, bool isPersistent)
//        {
//            // Ensure FullName and other user data are populated correctly.
//    var claims = new List<Claim>
//         {
//        new Claim("UserId", user.Id.ToString()),  // Add UserId for session tracking
//        new Claim(ClaimTypes.Name, user.FullName ?? "Unknown User"), // Default if FullName is null
//        new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
//        new Claim(ClaimTypes.Role, user.Role ?? string.Empty) // Role-based claim
//          };

//            // Additional claims for specific roles
//            if (user.Role == "Doctor")
//            {
//                var doctor = await _context.Doctors
//                                           .AsNoTracking()
//                                           .FirstOrDefaultAsync(d => d.UserId == user.Id);
//                if (doctor != null)
//                {
//                    claims.Add(new Claim("ProfileImageUrl", doctor.ProfileImageUrl ?? "~/uploads/doctors/patient_default.jpg"));
//                    claims.Add(new Claim("DoctorId", doctor.Id.ToString())); // Add Doctor-specific claim if needed
//                }
//            }

//            // If the user is a Receptionist, make sure you are handling their claims properly.
//            if (user.Role == "Receptionist")
//            {
//                // Optionally add more claims for Receptionists, like hospital/clinic-related data.
//                claims.Add(new Claim("ReceptionistId", user.Id.ToString()));  // Example: Adding a ReceptionistId claim
//            }

//            // Create identity and principal objects for the user.
//            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
//            var principal = new ClaimsPrincipal(identity);
//            var authProps = new AuthenticationProperties { IsPersistent = isPersistent };

//            // Sign-in the user with authentication scheme and properties.
//            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, authProps);
//        }


//        //    private async Task SignInUserAsync(User user, bool isPersistent)
//        //    {
//        //        var claims = new List<Claim>
//        //{
//        //    new Claim("UserId", user.Id.ToString()),
//        //    new Claim(ClaimTypes.Name, user.FullName ?? string.Empty),
//        //    new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
//        //    new Claim(ClaimTypes.Role, user.Role ?? string.Empty)
//        //};

//        //        // If this user is a doctor, store the DoctorId claim as well
//        //        if (user.Role == "Doctor")
//        //        {
//        //            var doctor = await _context.Doctors
//        //                                       .AsNoTracking()
//        //                                       .FirstOrDefaultAsync(d => d.UserId == user.Id);
//        //            if (doctor != null)
//        //                claims.Add(new Claim("DoctorId", doctor.Id.ToString()));
//        //        }

//        //        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
//        //        var principal = new ClaimsPrincipal(identity);
//        //        var authProps = new AuthenticationProperties { IsPersistent = isPersistent };

//        //        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, authProps);
//        //    }

//        [HttpPost]
//        public async Task<IActionResult> Logout()
//        {
//            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
//            return RedirectToAction("Login", "Auth");
//        }


//        [HttpGet]
//        public IActionResult Register()
//        {
//            return View();
//        }

//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> Register(RegisterViewModel model)
//        {
//            if (ModelState.IsValid)
//            {
//                // Check if the email already exists
//                var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email);
//                if (existingUser != null)
//                {
//                    ModelState.AddModelError(string.Empty, "User with this email already exists.");
//                    return View(model);
//                }

//                // Create a new user from the RegisterViewModel
//                var newUser = new User
//                {
//                    Username = model.Username,  // Set the Username
//                    FullName = model.FullName,
//                    Email = model.Email,
//                    IsActive = true,
//                    CreatedAt = DateTime.Now,
//                    UpdatedAt = DateTime.Now,
//                    Role = "User",  // Default role for new users
//                    PasswordHash = new PasswordHasher<User>().HashPassword(null, model.Password)
//                };

//                // Save the new user to the database
//                _context.Users.Add(newUser);
//                await _context.SaveChangesAsync();

//                // Redirect to login page after successful registration
//                return RedirectToAction("Login", "Auth");
//            }

//            // If we reach here, something went wrong with model binding or validation
//            return View(model);
//        }

//    }

//}



using CMS.Data;
using CMS.Models;
using CMS.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace CMS.Controllers
{
    public class AuthController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _config;

        public AuthController(ApplicationDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            // 1. Find the user by email
            var user = await _context.Users
                                     .FirstOrDefaultAsync(u => u.Email == model.Email);

            // 2. Validate user & password WITH hashing
            if (user == null || !VerifyPassword(model.Password, user.PasswordHash))
            {
                ModelState.AddModelError(string.Empty, "Invalid email or password.");
                return View(model);
            }

            // 3. Sign-in and create claims
            await SignInUserAsync(user, model.RememberMe);

            // 4. Redirect based on role
            switch (user.Role)
            {
                case "Admin":
                    return RedirectToAction("Index", "Admin");
                case "Doctor":
                    return RedirectToAction("Index", "DoctorDashboard");
                case "Receptionist":
                    return RedirectToAction("Index", "ReceptionistDashboard");
                default:
                    return RedirectToAction("Index", "Home");
            }
        }

        private async Task SignInUserAsync(User user, bool isPersistent)
        {
            var claims = new List<Claim>
    {
        new Claim("UserId", user.Id.ToString()),
        new Claim(ClaimTypes.Name, user.FullName ?? "Unknown User"),
        new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
        new Claim(ClaimTypes.Role, user.Role ?? string.Empty)
    };

            // Additional claims for specific roles
            if (user.Role == "Doctor")
            {
                var doctor = await _context.Doctors
                                            .AsNoTracking()
                                            .FirstOrDefaultAsync(d => d.UserId == user.Id);
                if (doctor != null)
                {
                    claims.Add(new Claim("ProfileImageUrl", doctor.ProfileImageUrl ?? "~/uploads/doctors/patient_default.jpg"));
                    claims.Add(new Claim("DoctorId", doctor.Id.ToString()));
                }
            }

            // Add claims for Admin role
            if (user.Role == "Admin")
            {
                var adminUser = await _context.Users
                                              .AsNoTracking()
                                              .FirstOrDefaultAsync(u => u.Id == user.Id && u.Role == "Admin");
                if (adminUser != null)
                {
                    claims.Add(new Claim("ProfileImageUrl", adminUser.ProfileImageUrl ?? "~/uploads/doctors/default.jpg"));
                    claims.Add(new Claim("AdminId", adminUser.Id.ToString()));
                }
            }


            if (user.Role == "Receptionist")
            {
                claims.Add(new Claim("ReceptionistId", user.Id.ToString()));
            }

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);
            var authProps = new AuthenticationProperties { IsPersistent = isPersistent };

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, authProps);
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Auth");
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Check if the email already exists
                var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email);
                if (existingUser != null)
                {
                    ModelState.AddModelError(string.Empty, "User with this email already exists.");
                    return View(model);
                }

                // Create a new user from the RegisterViewModel
                var newUser = new User
                {
                    Username = model.Username,
                    FullName = model.FullName,
                    Email = model.Email,
                    IsActive = true,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now,
                    Role = "User",
                    PasswordHash = HashPassword(model.Password) // Use the same hashing method
                };

                // Save the new user to the database
                _context.Users.Add(newUser);
                await _context.SaveChangesAsync();

                return RedirectToAction("Login", "Auth");
            }

            return View(model);
        }

        // Password Hashing Methods
        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(password);
            var hash = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }

        private bool VerifyPassword(string inputPassword, string storedHash)
        {
            var inputHash = HashPassword(inputPassword);
            return inputHash == storedHash;
        }
    }
}
