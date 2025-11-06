using CMS.Data;
using CMS.Models;
using CMS.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

public class ProfileController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IWebHostEnvironment _environment;

    public ProfileController(ApplicationDbContext context, IWebHostEnvironment environment)
    {
        _context = context;
        _environment = environment;
    }

    // GET: /Profile
    public async Task<IActionResult> Index()
    {
        var userIdClaim = User.FindFirstValue("UserId");
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
        {
            TempData["Error"] = "User not authenticated properly.";
            return RedirectToAction("Login", "Auth");
        }

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null)
        {
            TempData["Error"] = "User not found.";
            return RedirectToAction("Login", "Auth");
        }

        return View(user);
    }

    // returns the edit FORM ONLY (for modal body)
    [HttpGet]
    public async Task<IActionResult> Edit()
    {
        var userIdClaim = User.FindFirstValue("UserId");
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            return BadRequest("Not authenticated");

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null) return NotFound();

        var vm = new ProfileViewModel
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            ProfileImageUrl = user.ProfileImageUrl
        };

        return PartialView("_EditProfileForm", vm); // <<< return partial for modal
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(ProfileViewModel model, IFormFile? ProfileImage)
    {
        // Make sure password fields NEVER act as required
        ModelState.Remove(nameof(model.NewPassword));
        ModelState.Remove(nameof(model.ConfirmPassword));

        if (!ModelState.IsValid)
        {
            var errs = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
            return Json(new { success = false, message = string.Join(", ", errs) });
        }

        var userIdClaim = User.FindFirstValue("UserId");
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            return Json(new { success = false, message = "User not authenticated properly." });

        var user = await _context.Users.FindAsync(userId);
        if (user == null) return Json(new { success = false, message = "User not found." });

        // If user typed anything, THEN validate password
        if (!string.IsNullOrWhiteSpace(model.NewPassword) || !string.IsNullOrWhiteSpace(model.ConfirmPassword))
        {
            if (model.NewPassword != model.ConfirmPassword)
                return Json(new { success = false, message = "New password and confirmation do not match." });

            if (string.IsNullOrWhiteSpace(model.NewPassword) || model.NewPassword.Length < 6)
                return Json(new { success = false, message = "Password must be at least 6 characters long." });

            user.PasswordHash = HashPassword(model.NewPassword);
        }

        // Update basics
        user.FullName = model.FullName;
        user.Email = model.Email;
        user.PhoneNumber = model.PhoneNumber;
        user.UpdatedAt = DateTime.Now;
        user.Address = model.Address;

        // Image (max 1 MB)
        if (ProfileImage is { Length: > 1 * 1024 * 1024 })
            return Json(new { success = false, message = "Profile photo must not exceed 1 MB." });

        if (ProfileImage != null && ProfileImage.Length > 0)
        {
            if (!string.IsNullOrEmpty(user.ProfileImageUrl) && !user.ProfileImageUrl.Contains("patient_default.jpg"))
            {
                var oldPath = Path.Combine(_environment.WebRootPath, user.ProfileImageUrl.TrimStart('/'));
                if (System.IO.File.Exists(oldPath)) System.IO.File.Delete(oldPath);
            }

            var uploads = Path.Combine(_environment.WebRootPath, "uploads", "doctors");
            if (!Directory.Exists(uploads)) Directory.CreateDirectory(uploads);

            var fileName = $"profile_{userId}_{DateTime.Now:yyyyMMddHHmmss}{Path.GetExtension(ProfileImage.FileName)}";
            var filePath = Path.Combine(uploads, fileName);
            using var fs = new FileStream(filePath, FileMode.Create);
            await ProfileImage.CopyToAsync(fs);

            user.ProfileImageUrl = $"/uploads/doctors/{fileName}";
        }

        _context.Users.Update(user);
        await _context.SaveChangesAsync();

        return Json(new { success = true, message = "Profile updated successfully!" });
    }

    private string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(password);
        var hash = sha256.ComputeHash(bytes);
        return Convert.ToBase64String(hash);
    }
}
