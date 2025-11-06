//using CMS.Data;
//using CMS.Models;
//using CMS.Services;
//using CMS.ViewModels;
//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.AspNetCore.Mvc.Rendering;
//using Microsoft.EntityFrameworkCore;
//using System;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

////[Authorize(Roles = "Admin")] // Protect the entire controller
//public class UserManagementController : Controller
//{
//    private readonly ApplicationDbContext _context;
//    private readonly IEmailSender _emailSender;

//    public UserManagementController(ApplicationDbContext context, IEmailSender emailSender)
//    {
//        _context = context;
//        _emailSender = emailSender;
//    }

//    public IActionResult Index()
//    {
//        return View();
//    }

//    // In Controllers/UserManagementController.cs

//    [HttpGet]
//    public async Task<IActionResult> GetUserList()
//    {
//        try
//        {
//            // Get request parameters for pagination, sorting, and search
//            var draw = Request.Query["draw"].FirstOrDefault();
//            var start = Request.Query["start"].FirstOrDefault();
//            var length = Request.Query["length"].FirstOrDefault();
//            var sortColumn = Request.Query["columns[" + Request.Query["order[0][column]"].FirstOrDefault() + "][name]"].FirstOrDefault();
//            var sortColumnDirection = Request.Query["order[0][dir]"].FirstOrDefault();
//            var searchValue = Request.Query["search[value]"].FirstOrDefault();

//            // Set default page size and skip value
//            int pageSize = length != null ? Convert.ToInt32(length) : 10;
//            int skip = start != null ? Convert.ToInt32(start) : 0;

//            // Start with the base query to get user data
//            var userData = _context.Users.AsQueryable();

//            // --- Step 1: Filtering based on search value ---
//            if (!string.IsNullOrEmpty(searchValue))
//            {
//                userData = userData.Where(u => u.FullName.Contains(searchValue) || u.Email.Contains(searchValue));
//            }

//            // --- Step 2: Sorting ---
//            if (!string.IsNullOrEmpty(sortColumn) && !string.IsNullOrEmpty(sortColumnDirection))
//            {
//                switch (sortColumn.ToLower())
//                {
//                    case "username":
//                        userData = sortColumnDirection.ToLower() == "asc"
//                            ? userData.OrderBy(u => u.Username)
//                            : userData.OrderByDescending(u => u.Username);
//                        break;
//                    case "email":
//                        userData = sortColumnDirection.ToLower() == "asc"
//                            ? userData.OrderBy(u => u.Email)
//                            : userData.OrderByDescending(u => u.Email);
//                        break;
//                    case "fullname":
//                        userData = sortColumnDirection.ToLower() == "asc"
//                            ? userData.OrderBy(u => u.FullName)
//                            : userData.OrderByDescending(u => u.FullName);
//                        break;
//                    case "createdat":
//                        userData = sortColumnDirection.ToLower() == "asc"
//                            ? userData.OrderBy(u => u.CreatedAt)
//                            : userData.OrderByDescending(u => u.CreatedAt);
//                        break;
//                    default:
//                        userData = userData.OrderBy(u => u.Username); // Default sorting
//                        break;
//                }
//            }

//            // --- Step 3: Get total records before filtering ---
//            int recordsTotal = await _context.Users.CountAsync();

//            // --- Step 4: Apply pagination ---
//            var data = await userData.Skip(skip).Take(pageSize)
//                .Select(u => new UserViewModel
//                {
//                    Id = u.Id,
//                    FullName = u.FullName,
//                    Email = u.Email,
//                    Role = u.Role,
//                    IsActive = u.IsActive,
//                    CreatedAt = u.CreatedAt
//                })
//                .ToListAsync();

//            // --- Step 5: Get filtered records count ---
//            int recordsFiltered = await userData.CountAsync();

//            // --- Step 6: Return the correct data in the JSON response ---
//            var jsonData = new
//            {
//                draw = draw,
//                recordsFiltered = recordsFiltered,
//                recordsTotal = recordsTotal,
//                data = data
//            };

//            return Ok(jsonData);
//        }
//        catch (Exception ex)
//        {
//            Response.StatusCode = StatusCodes.Status500InternalServerError;
//            return Json(new { error = "Server error: " + ex.Message });
//        }
//    }

//    [HttpGet]
//    public async Task<IActionResult> Upsert(int id = 0)
//    {
//        var roles = new List<string> { "Admin", "Doctor", "Receptionist", "Accountant" };
//        var model = new UpsertUserViewModel
//        {
//            RoleList = roles.Select(r => new SelectListItem { Text = r, Value = r })
//        };

//        if (id == 0) // Create
//        {
//            return PartialView("_UpsertUserModal", model);
//        }
//        else // Edit
//        {
//            var user = await _context.Users.FindAsync(id);
//            if (user == null) return NotFound();

//            model.Id = user.Id;
//            model.FullName = user.FullName;
//            model.Email = user.Email;
//            model.Role = user.Role;
//            model.IsActive = user.IsActive;

//            return PartialView("_UpsertUserModal", model);
//        }
//    }

//    [HttpPost]
//    [ValidateAntiForgeryToken]
//    public async Task<IActionResult> Upsert(UpsertUserViewModel model)
//    {
//        if (!ModelState.IsValid) return Json(new { success = false, message = "Invalid data submitted." });

//        if (model.Id == 0) // Create
//        {
//            if (string.IsNullOrEmpty(model.Password))
//            {
//                return Json(new { success = false, message = "Password is required." });
//            }

//            var user = new User
//            {
//                Username = model.Email,
//                Email = model.Email,
//                FullName = model.FullName,
//                Role = model.Role,
//                IsActive = model.IsActive,
//                CreatedAt = DateTime.UtcNow,
//                PasswordHash = model.Password  // Directly store the password
//            };

//            _context.Users.Add(user);
//            await _context.SaveChangesAsync();

//            await _emailSender.SendAsync(user.Email, "Your New Account",
//                $"Welcome! Your temporary password is: <strong>{model.Password}</strong><br/>Please change it after your first login.");

//            return Json(new { success = true, message = "User created successfully! Password sent." });
//        }
//        else // Edit
//        {
//            var user = await _context.Users.FindAsync(model.Id);
//            if (user == null) return NotFound();

//            user.FullName = model.FullName;
//            user.Email = model.Email;
//            user.Username = model.Email;
//            user.IsActive = model.IsActive;
//            user.Role = model.Role;
//            user.UpdatedAt = DateTime.UtcNow;

//            if (!string.IsNullOrEmpty(model.Password))
//            {
//                user.PasswordHash = model.Password;  // Update password if provided
//            }

//            _context.Users.Update(user);
//            await _context.SaveChangesAsync();
//            return Json(new { success = true, message = "User updated successfully!" });
//        }
//    }

//    [HttpPost]
//    [ValidateAntiForgeryToken]
//    public async Task<IActionResult> ToggleStatus(int id)
//    {
//        var user = await _context.Users.FindAsync(id);
//        if (user == null) return NotFound();

//        user.IsActive = !user.IsActive;
//        await _context.SaveChangesAsync();
//        return Json(new { success = true, message = $"User has been {(user.IsActive ? "activated" : "deactivated")}." });
//    }

//    [HttpPost]
//    [ValidateAntiForgeryToken]
//    public async Task<IActionResult> Delete(int id)
//    {
//        var user = await _context.Users.FindAsync(id);
//        if (user == null) return Json(new { success = false, message = "User not found." });

//        _context.Users.Remove(user);
//        await _context.SaveChangesAsync();
//        return Json(new { success = true, message = "User deleted successfully." });
//    }

//    [AcceptVerbs("GET", "POST")]
//    public async Task<IActionResult> CheckEmail(string email, int id = 0)
//    {
//        var userExists = await _context.Users.AnyAsync(u => u.Email == email && u.Id != id);
//        if (userExists)
//        {
//            return Json($"Email '{email}' is already in use.");
//        }
//        return Json(true);
//    }


//}


using CMS.Data;
using CMS.Models;
using CMS.Services;
using CMS.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using System;
using System.Linq;
using System.Threading.Tasks;
[Authorize(Roles = "Admin, HR")]
public class UserManagementController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IEmailSender _emailSender;

    public UserManagementController(ApplicationDbContext context, IEmailSender emailSender)
    {
        _context = context;
        _emailSender = emailSender;
    }

    public IActionResult Index()
    {
        return View();
    }

    [HttpGet]
    public async Task<IActionResult> GetUserList()
    {
        try
        {
            var draw = Request.Query["draw"].FirstOrDefault();
            var start = Request.Query["start"].FirstOrDefault();
            var length = Request.Query["length"].FirstOrDefault();
            var sortColumn = Request.Query["columns[" + Request.Query["order[0][column]"].FirstOrDefault() + "][name]"].FirstOrDefault();
            var sortColumnDirection = Request.Query["order[0][dir]"].FirstOrDefault();
            var searchValue = Request.Query["search[value]"].FirstOrDefault();

            int pageSize = length != null ? Convert.ToInt32(length) : 10;
            int skip = start != null ? Convert.ToInt32(start) : 0;

            var userData = _context.Users.AsQueryable();

            if (!string.IsNullOrEmpty(searchValue))
            {
                userData = userData.Where(u => u.FullName.Contains(searchValue) || u.Email.Contains(searchValue));
            }

            if (!string.IsNullOrEmpty(sortColumn) && !string.IsNullOrEmpty(sortColumnDirection))
            {
                switch (sortColumn.ToLower())
                {
                    case "username":
                        userData = sortColumnDirection.ToLower() == "asc"
                            ? userData.OrderBy(u => u.Username)
                            : userData.OrderByDescending(u => u.Username);
                        break;
                    case "email":
                        userData = sortColumnDirection.ToLower() == "asc"
                            ? userData.OrderBy(u => u.Email)
                            : userData.OrderByDescending(u => u.Email);
                        break;
                    case "fullname":
                        userData = sortColumnDirection.ToLower() == "asc"
                            ? userData.OrderBy(u => u.FullName)
                            : userData.OrderByDescending(u => u.FullName);
                        break;
                    case "createdat":
                        userData = sortColumnDirection.ToLower() == "asc"
                            ? userData.OrderBy(u => u.CreatedAt)
                            : userData.OrderByDescending(u => u.CreatedAt);
                        break;
                    default:
                        userData = userData.OrderBy(u => u.Username);
                        break;
                }
            }

            int recordsTotal = await _context.Users.CountAsync();

            var data = await userData.Skip(skip).Take(pageSize)
                .Select(u => new UserViewModel
                {
                    Id = u.Id,
                    FullName = u.FullName,
                    Email = u.Email,
                    Role = u.Role,
                    IsActive = u.IsActive,
                    CreatedAt = u.CreatedAt
                })
                .ToListAsync();

            int recordsFiltered = await userData.CountAsync();

            var jsonData = new
            {
                draw = draw,
                recordsFiltered = recordsFiltered,
                recordsTotal = recordsTotal,
                data = data
            };

            return Ok(jsonData);
        }
        catch (Exception ex)
        {
            Response.StatusCode = StatusCodes.Status500InternalServerError;
            return Json(new { error = "Server error: " + ex.Message });
        }
    }

    [HttpGet]
    public async Task<IActionResult> Upsert(int id = 0)
    {
        var roles = new List<string> { "Admin", "Doctor", "Receptionist", "HR","Nurse" };
        var model = new UpsertUserViewModel
        {
            RoleList = roles.Select(r => new SelectListItem { Text = r, Value = r })
        };

        if (id == 0) // Create
        {
            return PartialView("_UpsertUserModal", model);
        }
        else // Edit
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();

            model.Id = user.Id;
            model.FullName = user.FullName;
            model.Email = user.Email;
            model.Role = user.Role;
            model.IsActive = user.IsActive;

            return PartialView("_UpsertUserModal", model);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Upsert(UpsertUserViewModel model)
    {
        if (!ModelState.IsValid) return Json(new { success = false, message = "Invalid data submitted." });

        if (model.Id == 0) // Create
        {
            if (string.IsNullOrEmpty(model.Password))
            {
                return Json(new { success = false, message = "Password is required." });
            }

            // Check if email already exists
            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email);
            if (existingUser != null)
            {
                return Json(new { success = false, message = "Email already exists." });
            }

            var user = new User
            {
                Username = model.Email,
                Email = model.Email,
                FullName = model.FullName,
                Role = model.Role,
                IsActive = model.IsActive,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                PasswordHash = HashPassword(model.Password)  // Hash the password
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Send email with plain text password (for initial setup)
            await _emailSender.SendAsync(user.Email, "Your New Account",
                $"Welcome! Your temporary password is: <strong>{model.Password}</strong><br/>Please change it after your first login.");

            return Json(new { success = true, message = "User created successfully! Password sent." });
        }
        else // Edit
        {
            var user = await _context.Users.FindAsync(model.Id);
            if (user == null) return Json(new { success = false, message = "User not found." });

            // Check if email already exists (excluding current user)
            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email && u.Id != model.Id);
            if (existingUser != null)
            {
                return Json(new { success = false, message = "Email already exists." });
            }

            user.FullName = model.FullName;
            user.Email = model.Email;
            user.Username = model.Email;
            user.IsActive = model.IsActive;
            user.Role = model.Role;
            user.UpdatedAt = DateTime.UtcNow;

            // Only update password if provided
            if (!string.IsNullOrEmpty(model.Password))
            {
                user.PasswordHash = HashPassword(model.Password);  // Hash the new password
            }

            _context.Users.Update(user);
            await _context.SaveChangesAsync();
            return Json(new { success = true, message = "User updated successfully!" });
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleStatus(int id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null) return Json(new { success = false, message = "User not found." });

        user.IsActive = !user.IsActive;
        user.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return Json(new { success = true, message = $"User has been {(user.IsActive ? "activated" : "deactivated")}." });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null) return Json(new { success = false, message = "User not found." });

        _context.Users.Remove(user);
        await _context.SaveChangesAsync();
        return Json(new { success = true, message = "User deleted successfully." });
    }

    [AcceptVerbs("GET", "POST")]
    public async Task<IActionResult> CheckEmail(string email, int id = 0)
    {
        var userExists = await _context.Users.AnyAsync(u => u.Email == email && u.Id != id);
        if (userExists)
        {
            return Json($"Email '{email}' is already in use.");
        }
        return Json(true);
    }

    // Password Hashing Method (Same as AuthController)
    private string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(password);
        var hash = sha256.ComputeHash(bytes);
        return Convert.ToBase64String(hash);
    }
}