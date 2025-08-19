using DentalClicnicManagementSystem.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DentalClicnicManagementSystem.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<Patient> Patients  {get; set; }
        public DbSet<Dentist> Dentists { get; set; }
        public DbSet<Appointment> Appointments { get; set; }
        public DbSet<Treatment> Treatments { get; set; }
        public DbSet<Invoice> Invoices { get; set; }
        public DbSet<InvoiceItem> InvoiceItems { get; set; }
        public DbSet<Xray> Xrays { get; set; }
        public DbSet<InventoryItem> InventoryItems { get; set; }
        public DbSet<StockMovement> StockMovements { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Appointment: required relations
            builder.Entity<Appointment>()
                .HasOne(a => a.Patient)
                .WithMany(p => p.Appointments)
                .HasForeignKey(a => a.PatientId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Appointment>()
                .HasOne(a => a.Dentist)
                .WithMany(d => d.Appointments)
                .HasForeignKey(a => a.DentistId)
                .OnDelete(DeleteBehavior.Restrict);

            // Treatment -> Appointment (many-to-one)
            builder.Entity<Treatment>()
                .HasOne(t => t.Appointment)
                .WithMany(a => a.Treatments)
                .HasForeignKey(t => t.AppointmentId)
                .OnDelete(DeleteBehavior.Cascade);

            // InvoiceItem one-to-one with Treatment; required Treatment, required Invoice
            builder.Entity<InvoiceItem>()
                .HasOne(ii => ii.Treatment)
                .WithOne(t => t.InvoiceItem)
                .HasForeignKey<InvoiceItem>(ii => ii.TreatmentId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<InvoiceItem>()
                .HasOne(ii => ii.Invoice)
                .WithMany(i => i.Items)
                .HasForeignKey(ii => ii.InvoiceId)
                .OnDelete(DeleteBehavior.Cascade);

            // Xray -> Patient
            builder.Entity<Xray>()
                .HasOne(x => x.Patient)
                .WithMany(p => p.Xrays)
                .HasForeignKey(x => x.PatientId)
                .OnDelete(DeleteBehavior.Cascade);

            // InventoryItem -> StockMovement
            builder.Entity<StockMovement>()
                .HasOne(sm => sm.InventoryItem)
                .WithMany(ii => ii.StockMovements)
                .HasForeignKey(sm => sm.InventoryItemId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<ApplicationUser>()
           .Property(u => u.Salary)
           .HasPrecision(18, 2);

            // Precision for money fields (if you prefer Fluent over [Precision])
            builder.Entity<Treatment>().Property(p => p.Cost).HasPrecision(18, 2);
            builder.Entity<Invoice>().Property(p => p.Total).HasPrecision(18, 2);
            builder.Entity<Invoice>().Property(p => p.Paid).HasPrecision(18, 2);
            builder.Entity<InvoiceItem>().Property(p => p.UnitPrice).HasPrecision(18, 2);

            // Defaults
            builder.Entity<Appointment>().Property(a => a.Status).HasDefaultValue(AppointmentStatus.Scheduled);
            builder.Entity<Invoice>().Property(i => i.Status).HasDefaultValue(InvoiceStatus.Draft);
            builder.Entity<StockMovement>().Property(s => s.CreatedOn).HasDefaultValueSql("GETUTCDATE()");
        }
    }
}
