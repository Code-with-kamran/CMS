using CMS.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CMS.Data
{
    public static class DbInitializer
    {
        public static async Task SeedRolesAndAdminAsync(RoleManager<IdentityRole> roleManager, UserManager<ApplicationUser> userManager)
        {
            // Default roles
            string[] roles = { "Admin", "Dentist", "Receptionist", "HR" };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            // Create a default Admin user
            string adminEmail = "admin@clinic.com";
            string adminPassword = "Admin@123";

            var existingAdmin = await userManager.FindByEmailAsync(adminEmail);
            if (existingAdmin == null)
            {
                var adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    Name = "System Admin",
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(adminUser, adminPassword);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }
        }
        public static async Task SeedDoctorsAsync(ApplicationDbContext context)
        {
            // Apply any pending migrations
            await context.Database.MigrateAsync();

            // Seed sample doctors if none exist
            if (!await context.Doctors.AnyAsync())
            {
                var doctors = new List<Doctor>
                {
                    new Doctor
                    {
                        FullName = "Dr. Sarah Johnson",
                        Email = "sarah.johnson@example.com",
                        Phone = "03001234567",
                        Specialty = "Cardiology",
                        Address = "Lahore",
                        ProfileImageUrl = "/images/doctors/doc1.jpg",
                        About = "Expert in cardiac surgery.",
                        Passward="12345",
                        ConfirmPassward="12345",
                    },
                    new Doctor
                    {
                        FullName = "Dr. Usman Ali",
                        Email = "usman.ali@example.com",
                        Phone = "03211234567",
                        Specialty = "Neurology",
                        Address = "Karachi",
                        ProfileImageUrl = "/images/doctors/doc2.jpg",
                        About = "Brain and nervous system specialist.",
                        Passward="12345",
                        ConfirmPassward="12345",
                    }
                };

                context.Doctors.AddRange(doctors);
                await context.SaveChangesAsync();
            }
        }
    }
}
