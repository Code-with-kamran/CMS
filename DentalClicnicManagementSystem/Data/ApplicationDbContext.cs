using CMS.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using System.Reflection.Emit;

namespace CMS.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<Patient> Patients { get; set; }
       
        public DbSet<Appointment> Appointments { get; set; }
        public DbSet<Treatment> Treatments { get; set; }
        public DbSet<Invoice> Invoices { get; set; }
        public DbSet<InvoiceItem> InvoiceItems { get; set; }
        public DbSet<Xray> Xrays { get; set; }
        public DbSet<InventoryItem> InventoryItems { get; set; }
        public DbSet<StockMovement> StockMovements { get; set; }
        public DbSet<Attendance> Attendances { get; set; }
        public DbSet<Currency> Currencies { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<MedicalHistory> MedicalHistories { get; set; }
        public DbSet<FollowUp> FollowUps { get; set; }
        public DbSet<Document> Document { get; set; }
        public DbSet<PatientVitals> PatientVitals { get; set; }
        public DbSet<Payment> Payments{ get; set; }
        public DbSet<BillReceipt> BillReceipts { get; set; }

        // Add this line inside your ApplicationDbContext class
        public DbSet<LaboratoryOrder> LaboratoryOrders { get; set; }

        public DbSet<Doctor> Doctors { get; set; }
        public DbSet<DoctorWeeklyAvailability> DoctorWeeklyAvailabilities { get; set; }
        public DbSet<DoctorDateAvailability> DoctorDateAvailabilities { get; set; }
        public DbSet<DoctorEducation> DoctorEducations { get; set; }
        public DbSet<DoctorAward> DoctorAwards { get; set; }
        public DbSet<DoctorCertification> DoctorCertifications { get; set; }
       
        public DbSet<DefaultSettings> DefaultSettings { get; set; }
        public DbSet<PaymentMethod> PaymentMethods { get; set; }
        public DbSet<InvoicePayment> InvoicePayments { get; set; }
        public DbSet<PaymentRequest> PaymentRequests { get; set; }
        public DbSet<PurchaseItem> PurchaseItems { get; set; }
        public DbSet<ServiceItem> ServiceItems { get; set; }
        public DbSet<PatientTreatments> PatientTreatments { get; set; }
        public DbSet<Medications> Medications { get; set; }






      

        public DbSet<Employee> Employees { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<AttendanceRecord> AttendanceRecords { get; set; }
        public DbSet<LeaveRequest> LeaveRequests { get; set; }
        public DbSet<PayrollRun> PayrollRuns { get; set; }
        public DbSet<PayrollItem> PayrollItems { get; set; }
        public DbSet<PerformanceReview> PerformanceReviews { get; set; }
        public DbSet<LabTestOrder> LabTestOrders { get; set; }
        public DbSet<Note> NotesList { get; set; }
        protected override void OnModelCreating(ModelBuilder b)
        {
            base.OnModelCreating(b);

            // --- Global precision for all decimal fields (money, cost, etc.) ---
            foreach (var p in b.Model.GetEntityTypes()
                                    .SelectMany(t => t.GetProperties())
                                    .Where(p => p.ClrType == typeof(decimal)))
            {
                p.SetPrecision(18); // Total number of digits
                p.SetScale(2);      // Digits after decimal
            }




            b.Entity<Invoice>()
        .HasIndex(i => i.AppointmentId)
        .HasFilter($"{nameof(Invoice.IsMedicationInvoice)} = 1")
        .IsUnique();

            b.Entity<Invoice>()
                .HasIndex(i => i.AppointmentId)
                .HasFilter($"{nameof(Invoice.IsAppointmentInvoice)} = 1")
                .IsUnique();

            b.Entity<Invoice>()
                .HasIndex(i => i.AppointmentId)
                .HasFilter($"{nameof(Invoice.IsCombinedInvoice)} = 1")
                .IsUnique();

            // check constraint
            b.Entity<Invoice>()
                .ToTable(t => t.HasCheckConstraint(
                    "CK_Invoices_OneTypeOnly",
                    "(CASE WHEN IsMedicationInvoice = 1 THEN 1 ELSE 0 END) + " +
                    "(CASE WHEN IsAppointmentInvoice = 1 THEN 1 ELSE 0 END) + " +
                    "(CASE WHEN IsCombinedInvoice = 1 THEN 1 ELSE 0 END) <= 1"));



            b.Entity<FollowUp>()
          .HasOne(f => f.Patient)
          .WithMany(p => p.FollowUps)
          .HasForeignKey(f => f.PatientId)
          .OnDelete(DeleteBehavior.Restrict);

            b.Entity<FollowUp>()
            .HasOne(f => f.Appointment)
            .WithMany(a => a.FollowUps)
            .HasForeignKey(f => f.AppointmentId)
            .OnDelete(DeleteBehavior.Cascade);

          
            //b.Entity<InvoiceItem>()
            //            .HasOne(ii => ii.Invoice)
            //            .WithMany(i => i.Items)
            //            .HasForeignKey(ii => ii.InvoiceId)   // real column
            //            .OnDelete(DeleteBehavior.Restrict);  // optional

    //        b.Entity<Invoice>()
    //.HasOne(i => i.PaymentMethod)
    //.WithMany()
    //.HasForeignKey(i => i.PaymentMethodId) // ✅ specify FK explicitly
    //.OnDelete(DeleteBehavior.Restrict);



            b.Entity<Document>()
         .HasOne(d => d.Patient)
         .WithMany(p => p.Documents)
         .HasForeignKey(d => d.PatientId)
         .OnDelete(DeleteBehavior.Restrict);
            // --- Employee Entity --------------------------------------------
            b.Entity<Employee>()
                .HasIndex(e => e.Code)
                .IsUnique(); // Employee Code must be unique



            b.Entity<Employee>()
                .HasIndex(e => e.Email)
                .IsUnique(); // Email must be unique

            // --- AttendanceRecord Entity -----------------------------------
            b.Entity<AttendanceRecord>()
                .HasOne(ar => ar.Employee)
                .WithMany(e => e.AttendanceRecords)
                .HasForeignKey(ar => ar.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict); // Prevent cascading deletes

            // --- LeaveRequest Entity ---------------------------------------
            //b.Entity<LeaveRequest>()
            //    .HasOne(lr => lr.Employee)
            //    .WithMany(e => e.LeaveRequests)
            //    .HasForeignKey(lr => lr.EmployeeId)
            //    .OnDelete(DeleteBehavior.Restrict); // Prevent cascading deletes

            // --- PayrollRun & PayrollItem Entities -------------------------
            b.Entity<PayrollRun>()
                .HasMany(pr => pr.Items)
                .WithOne(pi => pi.PayrollRun)
                .HasForeignKey(pi => pi.PayrollRunId)
                .OnDelete(DeleteBehavior.Cascade); // Delete items if run is deleted

            //b.Entity<PayrollItem>()
            //    .HasOne(pi => pi.Employee)
            //    .WithMany(e => e.PayrollItems)
            //    .HasForeignKey(pi => pi.EmployeeId)
            //    .OnDelete(DeleteBehavior.Restrict); // Prevent cascade delete

            // --- PerformanceReview Entity ---------------------------------
            //b.Entity<PerformanceReview>()
            //    .HasOne(pr => pr.Employee)
            //    .WithMany(e => e.PerformanceReviews)
            //    .HasForeignKey(pr => pr.EmployeeId)
            //    .OnDelete(DeleteBehavior.Restrict); // Prevent cascade delete

            // --- Invoice & InvoiceItem -------------------------------------
            b.Entity<Invoice>(e =>
            {
                // Money-related precision
                e.Property(x => x.SubTotal).HasPrecision(18, 2);
                e.Property(x => x.Tax).HasPrecision(18, 2);
                e.Property(x => x.Discount).HasPrecision(18, 2);
                e.Property(x => x.Total).HasPrecision(18, 2);
  
            });

            // 1. sequence (already done)
            b.HasSequence<int>("EmployeeCodeSeq", schema: "dbo")
             .StartsAt(10000)
             .IncrementsBy(1);

            // 2. configure Code – generated on insert, never touched afterwards
            b.Entity<Employee>()
             .Property(e => e.Code)
             .HasDefaultValueSql("('EMP' + FORMAT(NEXT VALUE FOR EmployeeCodeSeq, '00000'))")
             .ValueGeneratedOnAdd()          // ← only on INSERT
             .Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);   // ✅ correct API

            // 3. enforce uniqueness
            b.Entity<Employee>()
                   .HasIndex(e => e.Code)
                   .IsUnique();


            b.Entity<Invoice>()
    .HasOne(i => i.Doctor)
    .WithMany()
    .HasForeignKey(i => i.DoctorId)
    .IsRequired(false);

            // --- InventoryItem Entity -------------------------------------
            b.Entity<InventoryItem>()
                .Property(i => i.UnitPrice)
                .HasColumnType("decimal(18,2)");

            b.Entity<InventoryItem>()
                .HasIndex(i => i.SKU)
                .IsUnique(); // SKU must be unique

            // --- Seed Inventory Items -------------------------------------
            b.Entity<InventoryItem>().HasData(
                new InventoryItem { Id = 1, Name = "Bandages (Sterile)", SKU = "MED-001", Stock = 100, UnitPrice = 5.50m, Category = "Medical Supplies" },
                new InventoryItem { Id = 2, Name = "Pain Reliever (Tablet)", SKU = "PHARM-002", Stock = 250, UnitPrice = 12.75m, Category = "Pharmaceuticals" },
                new InventoryItem { Id = 3, Name = "Syringe (Disposable)", SKU = "MED-003", Stock = 500, UnitPrice = 2.00m, Category = "Medical Devices" },
                new InventoryItem { Id = 4, Name = "Antiseptic Solution", SKU = "CHEM-004", Stock = 75, UnitPrice = 25.00m, Category = "Chemicals" },
                new InventoryItem { Id = 5, Name = "Surgical Gloves (Box)", SKU = "MED-005", Stock = 120, UnitPrice = 30.00m, Category = "Medical Supplies" }
            );

            b.Entity<InvoiceItem>(e =>
            {
                e.Property(x => x.UnitPrice).HasPrecision(18, 2); // Precision for each item
            });

            b.Entity<InventoryItem>(e =>
            {
                e.Property(x => x.UnitPrice).HasPrecision(18, 2); // Precision for inventory
            });

            // --- Invoice ID & Status defaults -----------------------------
            //b.Entity<Invoice>(i =>
            //{
            //    i.Property(e => e.Id)
            //     .UseIdentityColumn(1000, 1); // Start invoice IDs from 1000

            //    i.Property(e => e.status)
            //     .HasConversion<string>() // Store enum as string
            //     .HasDefaultValue(InvoiceStatus.Draft);

            //    i.HasMany(e => e.Items)
            //     .WithOne()
            //     .HasForeignKey(e => e.InvoiceId)
            //     .OnDelete(DeleteBehavior.Cascade);
            //});

            b.Entity<Note>()
            .HasOne(n => n.Appointment)
            .WithMany(a => a.NotesList)
            .HasForeignKey(n => n.AppointmentId)
            .OnDelete(DeleteBehavior.Cascade);

            // --- Appointment & Treatments ---------------------------------
  //          b.Entity<Appointment>(a =>
  //          {
  //              a.Property(e => e.Status)
  //               .HasConversion<string>()
  //               .HasDefaultValue(AppointmentStatus.Scheduled);

  //              a.Property(e => e.AppointmentType)
  //               .HasConversion<string>();

  //              a.HasOne(e => e.Patient)
  //               .WithMany()
  //               .HasForeignKey(e => e.PatientId)
  //               .IsRequired()
  //               .OnDelete(DeleteBehavior.Restrict);

  //              a.HasOne(e => e.Doctor)
  //               .WithMany()
  //               .HasForeignKey(e => e.DoctorId)
  //               .IsRequired()
  //               .OnDelete(DeleteBehavior.Restrict);

  //              a.HasOne(e => e.Treatments)
  //.WithOne(t => t.Appointment)
  //.HasForeignKey<Treatment>(t => t.AppointmentId)
  //.OnDelete(DeleteBehavior.Restrict);

  //          });

            // --- Treatment Cost Precision ---------------------------------
            b.Entity<Treatment>().Property(t => t.UnitPrice).HasPrecision(18, 2);

            // --- Enum conversions for readability ------------------------
            b.Entity<LeaveRequest>().Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
            b.Entity<LeaveRequest>().Property(x => x.Type).HasConversion<string>().HasMaxLength(20);
            b.Entity<AttendanceRecord>().Property(x => x.Status).HasConversion<string>().HasMaxLength(20);

            // --- Seed all remaining data ---------------------------------
            SeedData(b);
        }

        // --- SeedData Method ---------------------------------------------
       
        private void SeedData(ModelBuilder b)
        {
            // Seed Departments
            var departments = new Department[]
            {
                new Department { DepartmentId = 11, DepartmentName = "Administration", IsActive = true, Description = "Manages clinic operations." },
                new Department { DepartmentId = 21, DepartmentName = "Dental Assistants", IsActive = true, Description = "Assists dentists during procedures." },
                new Department { DepartmentId = 31, DepartmentName = "Hygienists", IsActive = true, Description = "Performs dental cleanings and preventive care." },
                new Department { DepartmentId = 41, DepartmentName = "Front Desk", IsActive = true, Description = "Handles patient scheduling and billing." },
                new Department { DepartmentId = 51, DepartmentName = "HR", IsActive = true, Description = "Manages human resources." }
            };
            b.Entity<Department>().HasData(departments);

            // Seed Employees
            var employees = new Employee[]
            {
                new Employee { Id = 1, Code = "EMP001", FirstName = "Alice", LastName = "Smith", Email = "alice.smith@example.com", Phone = "111-222-3333", Designation = "Dental Assistant", HireDate = new DateTime(2020, 1, 15), BaseSalary = 45000.00m, IsActive = true, LeaveBalance = 18 },
                new Employee { Id = 2, Code = "EMP002", FirstName = "Bob", LastName = "Johnson", Email = "bob.johnson@example.com", Phone = "444-555-6666", Designation = "Hygienist", HireDate = new DateTime(2019, 3, 10), BaseSalary = 60000.00m, IsActive = true, LeaveBalance = 20 },
                new Employee { Id = 3, Code = "EMP003", FirstName = "Charlie", LastName = "Brown", Email = "charlie.brown@example.com", Phone = "777-888-9999", Designation = "Front Desk Staff", HireDate = new DateTime(2021, 7, 1), BaseSalary = 35000.00m, IsActive = true, LeaveBalance = 15 },
                new Employee { Id = 4, Code = "EMP004", FirstName = "Diana", LastName = "Prince", Email = "diana.prince@example.com", Phone = "123-456-7890", Designation = "HR Manager", HireDate = new DateTime(2018, 5, 20), BaseSalary = 70000.00m, IsActive = true, LeaveBalance = 25 },
                new Employee { Id = 5, Code = "EMP005", FirstName = "Eve", LastName = "Adams", Email = "eve.adams@example.com", Phone = "987-654-3210", Designation = "Dental Assistant", HireDate = new DateTime(2022, 2, 1), BaseSalary = 42000.00m, IsActive = true, LeaveBalance = 17 }
            };
            b.Entity<Employee>().HasData(employees);

            // Seed Attendance Records (last 30 days for EMP001, EMP002)
            var attendanceRecords = new List<AttendanceRecord>();
            var today = DateTime.Today;
            for (int i = 0; i < 30; i++)
            {
                var date = today.AddDays(-i);
                attendanceRecords.Add(new AttendanceRecord { Id = attendanceRecords.Count + 1, EmployeeId = 1, Date = date, CheckIn = new TimeOnly(9, 0, 0), CheckOut = new TimeOnly(17, 0, 0), Status = AttendanceStatus.Present, Note = "Regular day" });
                attendanceRecords.Add(new AttendanceRecord { Id = attendanceRecords.Count + 1, EmployeeId = 2, Date = date, CheckIn = new TimeOnly(9, 15, 0), CheckOut = new TimeOnly(17, 0, 0), Status = AttendanceStatus.Late, Note = "Traffic delay" });
                if (i % 5 == 0) // Every 5th day, one is absent
                {
                    attendanceRecords.Add(new AttendanceRecord { Id = attendanceRecords.Count + 1, EmployeeId = 3, Date = date, Status = AttendanceStatus.Absent, Note = "Sick leave" });
                }
                else
                {
                    attendanceRecords.Add(new AttendanceRecord { Id = attendanceRecords.Count + 1, EmployeeId = 3, Date = date, CheckIn = new TimeOnly(8, 55, 0), CheckOut = new TimeOnly(16, 50, 0), Status = AttendanceStatus.Present, Note = "Regular day" });
                }
            }
            b.Entity<AttendanceRecord>().HasData(attendanceRecords);

            // Seed Leave Requests
            var leaveRequests = new LeaveRequest[]
            {
                new LeaveRequest { Id = 1, EmployeeId = 1, Type = LeaveTypeEnum.Annual, StartDate = new DateTime(2023, 10, 23), EndDate = new DateTime(2023, 10, 27), Days = 5, Reason = "Family vacation", Status = LeaveStatus.Approved, DecisionBy = "HR Manager", DecisionAt = new DateTime(2023, 10, 10) },
                new LeaveRequest { Id = 2, EmployeeId = 2, Type = LeaveTypeEnum.Sick, StartDate = new DateTime(2023, 11, 1), EndDate = new DateTime(2023, 11, 1), Days = 1, Reason = "Flu", Status = LeaveStatus.Pending },
                new LeaveRequest { Id = 3, EmployeeId = 3, Type = LeaveTypeEnum.Casual, StartDate = new DateTime(2023, 11, 15), EndDate = new DateTime(2023, 11, 16), Days = 2, Reason = "Personal errands", Status = LeaveStatus.Rejected, DecisionBy = "HR Manager", DecisionAt = new DateTime(2023, 11, 1) },
                new LeaveRequest { Id = 4, EmployeeId = 4, Type = LeaveTypeEnum.Unpaid, StartDate = new DateTime(2023, 12, 1), EndDate = new DateTime(2023, 12, 5), Days = 5, Reason = "Extended travel", Status = LeaveStatus.Pending }
            };
            b.Entity<LeaveRequest>().HasData(leaveRequests);

            // Seed one PayrollRun with items
            var payrollRun = new PayrollRun { Id = 1, Year = 2023, Month = 10, RunAt = new DateTime(2023, 10, 30), Notes = "October 2023 Payroll" };
            b.Entity<PayrollRun>().HasData(payrollRun);

            var payrollItems = new PayrollItem[]
            {
                new PayrollItem { Id = 1, PayrollRunId = 1, EmployeeId = 1, BaseSalary = 45000.00m / 12, Allowances = 500.00m, Deductions = 200.00m, NetPay = (45000.00m / 12) + 500.00m - 200.00m, AttendanceSummary = "Present: 23, Absent: 0, Half-days: 0"},
                new PayrollItem { Id = 2, PayrollRunId = 1, EmployeeId = 2, BaseSalary = 60000.00m / 12, Allowances = 750.00m, Deductions = 300.00m, NetPay = (60000.00m / 12) + 750.00m - 300.00m, AttendanceSummary = "Present: 12, Absent: 10, Half-days: 0"  },
                new PayrollItem { Id = 3, PayrollRunId = 1, EmployeeId = 3, BaseSalary = 35000.00m / 12, Allowances = 300.00m, Deductions = 150.00m, NetPay = (35000.00m / 12) + 300.00m - 150.00m, AttendanceSummary = "Present: 52, Absent: 0, Half-days: 0" },
                new PayrollItem { Id = 4, PayrollRunId = 1, EmployeeId = 4, BaseSalary = 70000.00m / 12, Allowances = 1000.00m, Deductions = 400.00m, NetPay = (70000.00m / 12) + 1000.00m - 400.00m, AttendanceSummary = "Present: 2, Absent: 0, Half-days: 5" }
            };
            b.Entity<PayrollItem>().HasData(payrollItems);

            // Seed Performance Reviews
            var performanceReviews = new PerformanceReview[]
            {
                new PerformanceReview { Id = 1, EmployeeId = 1, ReviewDate = new DateTime(2023, 9, 1), Reviewer = "HR Manager", Rating = 4, Notes = "Consistently meets expectations, good team player." },
                new PerformanceReview { Id = 2, EmployeeId = 2, ReviewDate = new DateTime(2023, 9, 15), Reviewer = "HR Manager", Rating = 5, Notes = "Exceeds expectations, highly skilled and proactive." },
                new PerformanceReview { Id = 3, EmployeeId = 3, ReviewDate = new DateTime(2023, 8, 20), Reviewer = "HR Manager", Rating = 3, Notes = "Meets basic requirements, areas for improvement in patient communication." }
            };
            b.Entity<PerformanceReview>().HasData(performanceReviews);
        }


    }
}
