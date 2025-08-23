using CMS.Data;
using CMS.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CMS.Controllers
{
    public class DoctorController : Controller
    {
       


         private readonly ApplicationDbContext _context;

        private readonly IWebHostEnvironment _webHostEnvironment;

        public DoctorController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        // GET: Doctor
        public async Task<IActionResult> Index()
            {
                var doctors = await _context.Doctors.ToListAsync();
                return View(doctors);
            }

            // GET: Doctor/Details/5
            public async Task<IActionResult> Details(int? id)
            {
                if (id == null)
                    return NotFound();

                var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.Id == id);
                if (doctor == null)
                    return NotFound();

                return View(doctor);
            }

        // GET: Doctor/Create
        public IActionResult Create()
        {
            ViewBag.IsEdit = false;
            return View("Upsert", new Doctor());
        }
        // GET: Doctor/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var doctor = await _context.Doctors.FindAsync(id);
            if (doctor == null)
                return NotFound();

            ViewBag.IsEdit = true;
            return View("Upsert", doctor);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upsert(Doctor doctor, IFormFile? ProfileImage)
        {
            if (ModelState.IsValid)
            {
                string wwwRootPath = _webHostEnvironment.WebRootPath;
                string defaultImagePath = "/uploads/doctors/default.jpg";

                if (ProfileImage != null)
                {
                    // Generate a unique file name
                    string fileName = Path.GetFileNameWithoutExtension(ProfileImage.FileName);
                    string extension = Path.GetExtension(ProfileImage.FileName);
                    string newFileName = fileName + "_" + Guid.NewGuid() + extension;
                    string uploadPath = Path.Combine(wwwRootPath + "/uploads/doctors/", newFileName);

                    // Delete existing image if it’s not default
                    if (!string.IsNullOrEmpty(doctor.ProfileImageUrl) && doctor.ProfileImageUrl != defaultImagePath)
                    {
                        string oldImagePath = Path.Combine(wwwRootPath, doctor.ProfileImageUrl.TrimStart('/'));
                        if (System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }
                    }

                    // Save new image
                    using (var fileStream = new FileStream(uploadPath, FileMode.Create))
                    {
                        await ProfileImage.CopyToAsync(fileStream);
                    }

                    doctor.ProfileImageUrl = "/uploads/doctors/" + newFileName;
                }
                else if (doctor.Id == 0)
                {
                    // Set default image when creating and no image uploaded
                    doctor.ProfileImageUrl = defaultImagePath;
                }

                if (doctor.Id == 0)
                {
                    // Create
                    _context.Doctors.Add(doctor);
                    TempData["success"] = "Doctor Created Successfully!";
                }
                else
                {
                    // Update — preserve old image if none is uploaded
                    var existingDoctor = await _context.Doctors.AsNoTracking().FirstOrDefaultAsync(d => d.Id == doctor.Id);
                    if (existingDoctor == null)
                        return NotFound();

                    doctor.ProfileImageUrl ??= existingDoctor.ProfileImageUrl;

                    _context.Doctors.Update(doctor);
                    TempData["success"] = "Doctor Updated Successfully!";
                }

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View("Upsert", doctor);
        }





        // POST: Doctor/Edit/5

       

        // POST: Doctor/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try { 
            var doctor = await _context.Doctors.FindAsync(id);
            if (doctor == null)
            {
                return NotFound();
            }

            // Optional: Delete associated image
            if (!string.IsNullOrEmpty(doctor.ProfileImageUrl))
            {
                var imagePath = Path.Combine(_webHostEnvironment.WebRootPath, doctor.ProfileImageUrl.TrimStart('/'));
                if (System.IO.File.Exists(imagePath))
                {
                    System.IO.File.Delete(imagePath);
                }
            }

            _context.Doctors.Remove(doctor);
            await _context.SaveChangesAsync();

            TempData["success"] = "Doctor deleted successfully!";
                return Json(new { status = true, message = "Deleted successfully." });

                //return RedirectToAction(nameof(Index));
            }
            catch { 
                return Json(new { status = false, message = "Error deleting doctor." });
            }
        }


        private bool DoctorExists(int id)
            {
                return _context.Doctors.Any(e => e.Id == id);
            }
        }
    }


