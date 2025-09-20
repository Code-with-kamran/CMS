using CMS.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CMS.Data
{
    public static class DbInitializer
    {
        public static async Task SeedAdminUser(ApplicationDbContext context)
        {
            if (!context.Users.Any())
            {
                var adminUser = new User
                {
                    Username = "admin",
                    Email = "admin@admin.com",
                    FullName = "Admin User",
                    Role = "Admin",  // Directly assign the role
                    IsActive = true,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now,
                    PasswordHash = new PasswordHasher<User>().HashPassword(null, "Admin@123")
                };

                await context.Users.AddAsync(adminUser);
                await context.SaveChangesAsync();
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
            Id = 1,
            Password = "12345",
            ConfirmPassword = "12345",
            FullName = "Dr. Sarah Miller",
            Specialty = "Cardiology",
            Degrees = "MD, PhD",
            About = "Dr. Miller is a board-certified cardiologist with over 15 years of experience specializing in preventive heart care and non-invasive procedures.",
            Email = "sarah.miller@example.com",
            Phone = "555-123-4567",
            Address = "123 Heartbeat Lane, Anytown, USA",
            ProfileImageUrl = "https://example.com/images/sarah-miller.jpg",
            ConsultationCharge = 150,
            ConsultationDurationInMinutes = 30,
            MedicalLicenseNumber = "CM123456",
            BloodGroup = "A+",
            YearOfExperience =" 15",
            AvailabilityStatus = "Available",
            
           
        },
       };

                context.Doctors.AddRange(doctors);
                await context.SaveChangesAsync();
            }
        }
        
    }
}
