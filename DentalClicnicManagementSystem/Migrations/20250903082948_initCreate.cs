using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace CMS.Migrations
{
    /// <inheritdoc />
    public partial class initCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Address = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HireDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Salary = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Department = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SecurityStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "bit", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "bit", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Attendances",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CheckIn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CheckOut = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Attendances", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Department",
                columns: table => new
                {
                    DepartmentId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DepartmentName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Department", x => x.DepartmentId);
                });

            migrationBuilder.CreateTable(
                name: "Doctors",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FullName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Specialty = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Degrees = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    About = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false),
                    Address = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Department = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ProfileImageUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConsultationCharge = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    ConsultationDurationInMinutes = table.Column<int>(type: "int", nullable: false),
                    MedicalLicenseNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Clinic = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BloodGroup = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    YearOfExperience = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AvailabilityStatus = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Password = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ConfirmPassword = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Doctors", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "InventoryItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    SKU = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ReferenceCode = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Stock = table.Column<int>(type: "int", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Category = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Price = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InventoryItems", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Invoices",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1000, 1"),
                    IssueDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DueDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CustomerName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    CustomerEmail = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    CustomerAddress = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    SubTotal = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Tax = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Discount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Total = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CurrencyCode = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    ExchangeRate = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 2, nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, defaultValue: "Draft")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Invoices", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Patients",
                columns: table => new
                {
                    PatientId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FirstName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DateOfBirth = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Gender = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Address = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    InsuranceProvider = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    InsuranceNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Allergies = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DentalHistory = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ProfileImageUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastVisit = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Patients", x => x.PatientId);
                });

            migrationBuilder.CreateTable(
                name: "PayrollRuns",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Year = table.Column<int>(type: "int", nullable: false),
                    Month = table.Column<int>(type: "int", nullable: false),
                    RunAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PayrollRuns", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderKey = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Dentists",
                columns: table => new
                {
                    DentistId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LicenseNo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Specialty = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ApplicationUserId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Dentists", x => x.DentistId);
                    table.ForeignKey(
                        name: "FK_Dentists_AspNetUsers_ApplicationUserId",
                        column: x => x.ApplicationUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Employees",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    FirstName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Phone = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    DepartmentId = table.Column<int>(type: "int", nullable: true),
                    Designation = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    HireDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    BaseSalary = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    LeaveBalance = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Employees", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Employees_Department_DepartmentId",
                        column: x => x.DepartmentId,
                        principalTable: "Department",
                        principalColumn: "DepartmentId");
                });

            migrationBuilder.CreateTable(
                name: "DoctorAwards",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DoctorId = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Issuer = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Year = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DoctorAwards", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DoctorAwards_Doctors_DoctorId",
                        column: x => x.DoctorId,
                        principalTable: "Doctors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DoctorCertifications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DoctorId = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Authority = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LicenseNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Department = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IssuedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ExpiresOn = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DoctorCertifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DoctorCertifications_Doctors_DoctorId",
                        column: x => x.DoctorId,
                        principalTable: "Doctors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DoctorDateAvailabilities",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DoctorId = table.Column<int>(type: "int", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: true),
                    StartTime = table.Column<TimeSpan>(type: "time", nullable: true),
                    EndTime = table.Column<TimeSpan>(type: "time", nullable: true),
                    IsAvailable = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DoctorDateAvailabilities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DoctorDateAvailabilities_Doctors_DoctorId",
                        column: x => x.DoctorId,
                        principalTable: "Doctors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DoctorEducations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DoctorId = table.Column<int>(type: "int", nullable: false),
                    Degree = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Institution = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Year = table.Column<int>(type: "int", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DoctorEducations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DoctorEducations_Doctors_DoctorId",
                        column: x => x.DoctorId,
                        principalTable: "Doctors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DoctorWeeklyAvailabilities",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DoctorId = table.Column<int>(type: "int", nullable: false),
                    DayOfWeek = table.Column<int>(type: "int", nullable: true),
                    StartTime = table.Column<TimeSpan>(type: "time", nullable: true),
                    EndTime = table.Column<TimeSpan>(type: "time", nullable: true),
                    IsWorkingDay = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DoctorWeeklyAvailabilities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DoctorWeeklyAvailabilities_Doctors_DoctorId",
                        column: x => x.DoctorId,
                        principalTable: "Doctors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StockMovements",
                columns: table => new
                {
                    StockMovementId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InventoryItemId = table.Column<int>(type: "int", nullable: false),
                    Delta = table.Column<int>(type: "int", nullable: false),
                    Reason = table.Column<int>(type: "int", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StockMovements", x => x.StockMovementId);
                    table.ForeignKey(
                        name: "FK_StockMovements_InventoryItems_InventoryItemId",
                        column: x => x.InventoryItemId,
                        principalTable: "InventoryItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "InvoiceItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InvoiceId = table.Column<int>(type: "int", nullable: false),
                    InvoiceId1 = table.Column<int>(type: "int", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    InventoryItemId = table.Column<int>(type: "int", nullable: true),
                    InvoiceId2 = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InvoiceItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InvoiceItems_InventoryItems_InventoryItemId",
                        column: x => x.InventoryItemId,
                        principalTable: "InventoryItems",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_InvoiceItems_Invoices_InvoiceId",
                        column: x => x.InvoiceId,
                        principalTable: "Invoices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InvoiceItems_Invoices_InvoiceId1",
                        column: x => x.InvoiceId1,
                        principalTable: "Invoices",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Xrays",
                columns: table => new
                {
                    XrayId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PatientId = table.Column<int>(type: "int", nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(260)", maxLength: 260, nullable: false),
                    UploadedBy = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    UploadedOn = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Xrays", x => x.XrayId);
                    table.ForeignKey(
                        name: "FK_Xrays_Patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Patients",
                        principalColumn: "PatientId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Appointments",
                columns: table => new
                {
                    AppointmentId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AppointmentNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PatientId = table.Column<int>(type: "int", nullable: false),
                    DoctorId = table.Column<int>(type: "int", nullable: false),
                    AppointmentDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DepartmentId = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false, defaultValue: "Scheduled"),
                    AppointmentType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    DentistId = table.Column<int>(type: "int", nullable: true),
                    PatientId1 = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Appointments", x => x.AppointmentId);
                    table.ForeignKey(
                        name: "FK_Appointments_Dentists_DentistId",
                        column: x => x.DentistId,
                        principalTable: "Dentists",
                        principalColumn: "DentistId");
                    table.ForeignKey(
                        name: "FK_Appointments_Department_DepartmentId",
                        column: x => x.DepartmentId,
                        principalTable: "Department",
                        principalColumn: "DepartmentId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Appointments_Doctors_DoctorId",
                        column: x => x.DoctorId,
                        principalTable: "Doctors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Appointments_Patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Patients",
                        principalColumn: "PatientId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Appointments_Patients_PatientId1",
                        column: x => x.PatientId1,
                        principalTable: "Patients",
                        principalColumn: "PatientId");
                });

            migrationBuilder.CreateTable(
                name: "AttendanceRecords",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmployeeId = table.Column<int>(type: "int", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CheckIn = table.Column<TimeOnly>(type: "time", nullable: true),
                    CheckOut = table.Column<TimeOnly>(type: "time", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Note = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AttendanceRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AttendanceRecords_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LeaveRequests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmployeeId = table.Column<int>(type: "int", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Days = table.Column<int>(type: "int", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    DecisionBy = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    DecisionAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LeaveRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LeaveRequests_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PayrollItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PayrollRunId = table.Column<int>(type: "int", nullable: false),
                    EmployeeId = table.Column<int>(type: "int", nullable: false),
                    BaseSalary = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Allowances = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Deductions = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    NetPay = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PayrollItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PayrollItems_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PayrollItems_PayrollRuns_PayrollRunId",
                        column: x => x.PayrollRunId,
                        principalTable: "PayrollRuns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PerformanceReviews",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmployeeId = table.Column<int>(type: "int", nullable: false),
                    ReviewDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Reviewer = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Rating = table.Column<int>(type: "int", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PerformanceReviews", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PerformanceReviews_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Treatments",
                columns: table => new
                {
                    TreatmentId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AppointmentId = table.Column<int>(type: "int", nullable: false),
                    ToothCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    ProcedureCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Cost = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    InvoiceItemId = table.Column<int>(type: "int", nullable: true),
                    PatientId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Treatments", x => x.TreatmentId);
                    table.ForeignKey(
                        name: "FK_Treatments_Appointments_AppointmentId",
                        column: x => x.AppointmentId,
                        principalTable: "Appointments",
                        principalColumn: "AppointmentId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Treatments_InvoiceItems_InvoiceItemId",
                        column: x => x.InvoiceItemId,
                        principalTable: "InvoiceItems",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Treatments_Patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Patients",
                        principalColumn: "PatientId");
                });

            migrationBuilder.InsertData(
                table: "Department",
                columns: new[] { "DepartmentId", "DepartmentName", "Description", "IsActive" },
                values: new object[,]
                {
                    { 11, "Administration", "Manages clinic operations.", true },
                    { 21, "Dental Assistants", "Assists dentists during procedures.", true },
                    { 31, "Hygienists", "Performs dental cleanings and preventive care.", true },
                    { 41, "Front Desk", "Handles patient scheduling and billing.", true },
                    { 51, "HR", "Manages human resources.", true }
                });

            migrationBuilder.InsertData(
                table: "Employees",
                columns: new[] { "Id", "BaseSalary", "Code", "DepartmentId", "Designation", "Email", "FirstName", "HireDate", "IsActive", "LastName", "LeaveBalance", "Phone" },
                values: new object[,]
                {
                    { 1, 45000.00m, "EMP001", null, "Dental Assistant", "alice.smith@example.com", "Alice", new DateTime(2020, 1, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), true, "Smith", 18, "111-222-3333" },
                    { 2, 60000.00m, "EMP002", null, "Hygienist", "bob.johnson@example.com", "Bob", new DateTime(2019, 3, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), true, "Johnson", 20, "444-555-6666" },
                    { 3, 35000.00m, "EMP003", null, "Front Desk Staff", "charlie.brown@example.com", "Charlie", new DateTime(2021, 7, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), true, "Brown", 15, "777-888-9999" },
                    { 4, 70000.00m, "EMP004", null, "HR Manager", "diana.prince@example.com", "Diana", new DateTime(2018, 5, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), true, "Prince", 25, "123-456-7890" },
                    { 5, 42000.00m, "EMP005", null, "Dental Assistant", "eve.adams@example.com", "Eve", new DateTime(2022, 2, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), true, "Adams", 17, "987-654-3210" }
                });

            migrationBuilder.InsertData(
                table: "InventoryItems",
                columns: new[] { "Id", "Category", "Description", "IsActive", "Name", "Price", "Quantity", "ReferenceCode", "SKU", "Stock", "UnitPrice" },
                values: new object[,]
                {
                    { 1, "Medical Supplies", null, true, "Bandages (Sterile)", 0m, 0, "", "MED-001", 100, 5.50m },
                    { 2, "Pharmaceuticals", null, true, "Pain Reliever (Tablet)", 0m, 0, "", "PHARM-002", 250, 12.75m },
                    { 3, "Medical Devices", null, true, "Syringe (Disposable)", 0m, 0, "", "MED-003", 500, 2.00m },
                    { 4, "Chemicals", null, true, "Antiseptic Solution", 0m, 0, "", "CHEM-004", 75, 25.00m },
                    { 5, "Medical Supplies", null, true, "Surgical Gloves (Box)", 0m, 0, "", "MED-005", 120, 30.00m }
                });

            migrationBuilder.InsertData(
                table: "PayrollRuns",
                columns: new[] { "Id", "Month", "Notes", "RunAt", "Year" },
                values: new object[] { 1, 10, "October 2023 Payroll", new DateTime(2023, 10, 30, 0, 0, 0, 0, DateTimeKind.Unspecified), 2023 });

            migrationBuilder.InsertData(
                table: "AttendanceRecords",
                columns: new[] { "Id", "CheckIn", "CheckOut", "Date", "EmployeeId", "Note", "Status" },
                values: new object[,]
                {
                    { 1, new TimeOnly(9, 0, 0), new TimeOnly(17, 0, 0), new DateTime(2025, 9, 3, 0, 0, 0, 0, DateTimeKind.Local), 1, "Regular day", "Present" },
                    { 2, new TimeOnly(9, 15, 0), new TimeOnly(17, 0, 0), new DateTime(2025, 9, 3, 0, 0, 0, 0, DateTimeKind.Local), 2, "Traffic delay", "Late" },
                    { 3, null, null, new DateTime(2025, 9, 3, 0, 0, 0, 0, DateTimeKind.Local), 3, "Sick leave", "Absent" },
                    { 4, new TimeOnly(9, 0, 0), new TimeOnly(17, 0, 0), new DateTime(2025, 9, 2, 0, 0, 0, 0, DateTimeKind.Local), 1, "Regular day", "Present" },
                    { 5, new TimeOnly(9, 15, 0), new TimeOnly(17, 0, 0), new DateTime(2025, 9, 2, 0, 0, 0, 0, DateTimeKind.Local), 2, "Traffic delay", "Late" },
                    { 6, new TimeOnly(8, 55, 0), new TimeOnly(16, 50, 0), new DateTime(2025, 9, 2, 0, 0, 0, 0, DateTimeKind.Local), 3, "Regular day", "Present" },
                    { 7, new TimeOnly(9, 0, 0), new TimeOnly(17, 0, 0), new DateTime(2025, 9, 1, 0, 0, 0, 0, DateTimeKind.Local), 1, "Regular day", "Present" },
                    { 8, new TimeOnly(9, 15, 0), new TimeOnly(17, 0, 0), new DateTime(2025, 9, 1, 0, 0, 0, 0, DateTimeKind.Local), 2, "Traffic delay", "Late" },
                    { 9, new TimeOnly(8, 55, 0), new TimeOnly(16, 50, 0), new DateTime(2025, 9, 1, 0, 0, 0, 0, DateTimeKind.Local), 3, "Regular day", "Present" },
                    { 10, new TimeOnly(9, 0, 0), new TimeOnly(17, 0, 0), new DateTime(2025, 8, 31, 0, 0, 0, 0, DateTimeKind.Local), 1, "Regular day", "Present" },
                    { 11, new TimeOnly(9, 15, 0), new TimeOnly(17, 0, 0), new DateTime(2025, 8, 31, 0, 0, 0, 0, DateTimeKind.Local), 2, "Traffic delay", "Late" },
                    { 12, new TimeOnly(8, 55, 0), new TimeOnly(16, 50, 0), new DateTime(2025, 8, 31, 0, 0, 0, 0, DateTimeKind.Local), 3, "Regular day", "Present" },
                    { 13, new TimeOnly(9, 0, 0), new TimeOnly(17, 0, 0), new DateTime(2025, 8, 30, 0, 0, 0, 0, DateTimeKind.Local), 1, "Regular day", "Present" },
                    { 14, new TimeOnly(9, 15, 0), new TimeOnly(17, 0, 0), new DateTime(2025, 8, 30, 0, 0, 0, 0, DateTimeKind.Local), 2, "Traffic delay", "Late" },
                    { 15, new TimeOnly(8, 55, 0), new TimeOnly(16, 50, 0), new DateTime(2025, 8, 30, 0, 0, 0, 0, DateTimeKind.Local), 3, "Regular day", "Present" },
                    { 16, new TimeOnly(9, 0, 0), new TimeOnly(17, 0, 0), new DateTime(2025, 8, 29, 0, 0, 0, 0, DateTimeKind.Local), 1, "Regular day", "Present" },
                    { 17, new TimeOnly(9, 15, 0), new TimeOnly(17, 0, 0), new DateTime(2025, 8, 29, 0, 0, 0, 0, DateTimeKind.Local), 2, "Traffic delay", "Late" },
                    { 18, null, null, new DateTime(2025, 8, 29, 0, 0, 0, 0, DateTimeKind.Local), 3, "Sick leave", "Absent" },
                    { 19, new TimeOnly(9, 0, 0), new TimeOnly(17, 0, 0), new DateTime(2025, 8, 28, 0, 0, 0, 0, DateTimeKind.Local), 1, "Regular day", "Present" },
                    { 20, new TimeOnly(9, 15, 0), new TimeOnly(17, 0, 0), new DateTime(2025, 8, 28, 0, 0, 0, 0, DateTimeKind.Local), 2, "Traffic delay", "Late" },
                    { 21, new TimeOnly(8, 55, 0), new TimeOnly(16, 50, 0), new DateTime(2025, 8, 28, 0, 0, 0, 0, DateTimeKind.Local), 3, "Regular day", "Present" },
                    { 22, new TimeOnly(9, 0, 0), new TimeOnly(17, 0, 0), new DateTime(2025, 8, 27, 0, 0, 0, 0, DateTimeKind.Local), 1, "Regular day", "Present" },
                    { 23, new TimeOnly(9, 15, 0), new TimeOnly(17, 0, 0), new DateTime(2025, 8, 27, 0, 0, 0, 0, DateTimeKind.Local), 2, "Traffic delay", "Late" },
                    { 24, new TimeOnly(8, 55, 0), new TimeOnly(16, 50, 0), new DateTime(2025, 8, 27, 0, 0, 0, 0, DateTimeKind.Local), 3, "Regular day", "Present" },
                    { 25, new TimeOnly(9, 0, 0), new TimeOnly(17, 0, 0), new DateTime(2025, 8, 26, 0, 0, 0, 0, DateTimeKind.Local), 1, "Regular day", "Present" },
                    { 26, new TimeOnly(9, 15, 0), new TimeOnly(17, 0, 0), new DateTime(2025, 8, 26, 0, 0, 0, 0, DateTimeKind.Local), 2, "Traffic delay", "Late" },
                    { 27, new TimeOnly(8, 55, 0), new TimeOnly(16, 50, 0), new DateTime(2025, 8, 26, 0, 0, 0, 0, DateTimeKind.Local), 3, "Regular day", "Present" },
                    { 28, new TimeOnly(9, 0, 0), new TimeOnly(17, 0, 0), new DateTime(2025, 8, 25, 0, 0, 0, 0, DateTimeKind.Local), 1, "Regular day", "Present" },
                    { 29, new TimeOnly(9, 15, 0), new TimeOnly(17, 0, 0), new DateTime(2025, 8, 25, 0, 0, 0, 0, DateTimeKind.Local), 2, "Traffic delay", "Late" },
                    { 30, new TimeOnly(8, 55, 0), new TimeOnly(16, 50, 0), new DateTime(2025, 8, 25, 0, 0, 0, 0, DateTimeKind.Local), 3, "Regular day", "Present" },
                    { 31, new TimeOnly(9, 0, 0), new TimeOnly(17, 0, 0), new DateTime(2025, 8, 24, 0, 0, 0, 0, DateTimeKind.Local), 1, "Regular day", "Present" },
                    { 32, new TimeOnly(9, 15, 0), new TimeOnly(17, 0, 0), new DateTime(2025, 8, 24, 0, 0, 0, 0, DateTimeKind.Local), 2, "Traffic delay", "Late" },
                    { 33, null, null, new DateTime(2025, 8, 24, 0, 0, 0, 0, DateTimeKind.Local), 3, "Sick leave", "Absent" },
                    { 34, new TimeOnly(9, 0, 0), new TimeOnly(17, 0, 0), new DateTime(2025, 8, 23, 0, 0, 0, 0, DateTimeKind.Local), 1, "Regular day", "Present" },
                    { 35, new TimeOnly(9, 15, 0), new TimeOnly(17, 0, 0), new DateTime(2025, 8, 23, 0, 0, 0, 0, DateTimeKind.Local), 2, "Traffic delay", "Late" },
                    { 36, new TimeOnly(8, 55, 0), new TimeOnly(16, 50, 0), new DateTime(2025, 8, 23, 0, 0, 0, 0, DateTimeKind.Local), 3, "Regular day", "Present" },
                    { 37, new TimeOnly(9, 0, 0), new TimeOnly(17, 0, 0), new DateTime(2025, 8, 22, 0, 0, 0, 0, DateTimeKind.Local), 1, "Regular day", "Present" },
                    { 38, new TimeOnly(9, 15, 0), new TimeOnly(17, 0, 0), new DateTime(2025, 8, 22, 0, 0, 0, 0, DateTimeKind.Local), 2, "Traffic delay", "Late" },
                    { 39, new TimeOnly(8, 55, 0), new TimeOnly(16, 50, 0), new DateTime(2025, 8, 22, 0, 0, 0, 0, DateTimeKind.Local), 3, "Regular day", "Present" },
                    { 40, new TimeOnly(9, 0, 0), new TimeOnly(17, 0, 0), new DateTime(2025, 8, 21, 0, 0, 0, 0, DateTimeKind.Local), 1, "Regular day", "Present" },
                    { 41, new TimeOnly(9, 15, 0), new TimeOnly(17, 0, 0), new DateTime(2025, 8, 21, 0, 0, 0, 0, DateTimeKind.Local), 2, "Traffic delay", "Late" },
                    { 42, new TimeOnly(8, 55, 0), new TimeOnly(16, 50, 0), new DateTime(2025, 8, 21, 0, 0, 0, 0, DateTimeKind.Local), 3, "Regular day", "Present" },
                    { 43, new TimeOnly(9, 0, 0), new TimeOnly(17, 0, 0), new DateTime(2025, 8, 20, 0, 0, 0, 0, DateTimeKind.Local), 1, "Regular day", "Present" },
                    { 44, new TimeOnly(9, 15, 0), new TimeOnly(17, 0, 0), new DateTime(2025, 8, 20, 0, 0, 0, 0, DateTimeKind.Local), 2, "Traffic delay", "Late" },
                    { 45, new TimeOnly(8, 55, 0), new TimeOnly(16, 50, 0), new DateTime(2025, 8, 20, 0, 0, 0, 0, DateTimeKind.Local), 3, "Regular day", "Present" },
                    { 46, new TimeOnly(9, 0, 0), new TimeOnly(17, 0, 0), new DateTime(2025, 8, 19, 0, 0, 0, 0, DateTimeKind.Local), 1, "Regular day", "Present" },
                    { 47, new TimeOnly(9, 15, 0), new TimeOnly(17, 0, 0), new DateTime(2025, 8, 19, 0, 0, 0, 0, DateTimeKind.Local), 2, "Traffic delay", "Late" },
                    { 48, null, null, new DateTime(2025, 8, 19, 0, 0, 0, 0, DateTimeKind.Local), 3, "Sick leave", "Absent" },
                    { 49, new TimeOnly(9, 0, 0), new TimeOnly(17, 0, 0), new DateTime(2025, 8, 18, 0, 0, 0, 0, DateTimeKind.Local), 1, "Regular day", "Present" },
                    { 50, new TimeOnly(9, 15, 0), new TimeOnly(17, 0, 0), new DateTime(2025, 8, 18, 0, 0, 0, 0, DateTimeKind.Local), 2, "Traffic delay", "Late" },
                    { 51, new TimeOnly(8, 55, 0), new TimeOnly(16, 50, 0), new DateTime(2025, 8, 18, 0, 0, 0, 0, DateTimeKind.Local), 3, "Regular day", "Present" },
                    { 52, new TimeOnly(9, 0, 0), new TimeOnly(17, 0, 0), new DateTime(2025, 8, 17, 0, 0, 0, 0, DateTimeKind.Local), 1, "Regular day", "Present" },
                    { 53, new TimeOnly(9, 15, 0), new TimeOnly(17, 0, 0), new DateTime(2025, 8, 17, 0, 0, 0, 0, DateTimeKind.Local), 2, "Traffic delay", "Late" },
                    { 54, new TimeOnly(8, 55, 0), new TimeOnly(16, 50, 0), new DateTime(2025, 8, 17, 0, 0, 0, 0, DateTimeKind.Local), 3, "Regular day", "Present" },
                    { 55, new TimeOnly(9, 0, 0), new TimeOnly(17, 0, 0), new DateTime(2025, 8, 16, 0, 0, 0, 0, DateTimeKind.Local), 1, "Regular day", "Present" },
                    { 56, new TimeOnly(9, 15, 0), new TimeOnly(17, 0, 0), new DateTime(2025, 8, 16, 0, 0, 0, 0, DateTimeKind.Local), 2, "Traffic delay", "Late" },
                    { 57, new TimeOnly(8, 55, 0), new TimeOnly(16, 50, 0), new DateTime(2025, 8, 16, 0, 0, 0, 0, DateTimeKind.Local), 3, "Regular day", "Present" },
                    { 58, new TimeOnly(9, 0, 0), new TimeOnly(17, 0, 0), new DateTime(2025, 8, 15, 0, 0, 0, 0, DateTimeKind.Local), 1, "Regular day", "Present" },
                    { 59, new TimeOnly(9, 15, 0), new TimeOnly(17, 0, 0), new DateTime(2025, 8, 15, 0, 0, 0, 0, DateTimeKind.Local), 2, "Traffic delay", "Late" },
                    { 60, new TimeOnly(8, 55, 0), new TimeOnly(16, 50, 0), new DateTime(2025, 8, 15, 0, 0, 0, 0, DateTimeKind.Local), 3, "Regular day", "Present" },
                    { 61, new TimeOnly(9, 0, 0), new TimeOnly(17, 0, 0), new DateTime(2025, 8, 14, 0, 0, 0, 0, DateTimeKind.Local), 1, "Regular day", "Present" },
                    { 62, new TimeOnly(9, 15, 0), new TimeOnly(17, 0, 0), new DateTime(2025, 8, 14, 0, 0, 0, 0, DateTimeKind.Local), 2, "Traffic delay", "Late" },
                    { 63, null, null, new DateTime(2025, 8, 14, 0, 0, 0, 0, DateTimeKind.Local), 3, "Sick leave", "Absent" },
                    { 64, new TimeOnly(9, 0, 0), new TimeOnly(17, 0, 0), new DateTime(2025, 8, 13, 0, 0, 0, 0, DateTimeKind.Local), 1, "Regular day", "Present" },
                    { 65, new TimeOnly(9, 15, 0), new TimeOnly(17, 0, 0), new DateTime(2025, 8, 13, 0, 0, 0, 0, DateTimeKind.Local), 2, "Traffic delay", "Late" },
                    { 66, new TimeOnly(8, 55, 0), new TimeOnly(16, 50, 0), new DateTime(2025, 8, 13, 0, 0, 0, 0, DateTimeKind.Local), 3, "Regular day", "Present" },
                    { 67, new TimeOnly(9, 0, 0), new TimeOnly(17, 0, 0), new DateTime(2025, 8, 12, 0, 0, 0, 0, DateTimeKind.Local), 1, "Regular day", "Present" },
                    { 68, new TimeOnly(9, 15, 0), new TimeOnly(17, 0, 0), new DateTime(2025, 8, 12, 0, 0, 0, 0, DateTimeKind.Local), 2, "Traffic delay", "Late" },
                    { 69, new TimeOnly(8, 55, 0), new TimeOnly(16, 50, 0), new DateTime(2025, 8, 12, 0, 0, 0, 0, DateTimeKind.Local), 3, "Regular day", "Present" },
                    { 70, new TimeOnly(9, 0, 0), new TimeOnly(17, 0, 0), new DateTime(2025, 8, 11, 0, 0, 0, 0, DateTimeKind.Local), 1, "Regular day", "Present" },
                    { 71, new TimeOnly(9, 15, 0), new TimeOnly(17, 0, 0), new DateTime(2025, 8, 11, 0, 0, 0, 0, DateTimeKind.Local), 2, "Traffic delay", "Late" },
                    { 72, new TimeOnly(8, 55, 0), new TimeOnly(16, 50, 0), new DateTime(2025, 8, 11, 0, 0, 0, 0, DateTimeKind.Local), 3, "Regular day", "Present" },
                    { 73, new TimeOnly(9, 0, 0), new TimeOnly(17, 0, 0), new DateTime(2025, 8, 10, 0, 0, 0, 0, DateTimeKind.Local), 1, "Regular day", "Present" },
                    { 74, new TimeOnly(9, 15, 0), new TimeOnly(17, 0, 0), new DateTime(2025, 8, 10, 0, 0, 0, 0, DateTimeKind.Local), 2, "Traffic delay", "Late" },
                    { 75, new TimeOnly(8, 55, 0), new TimeOnly(16, 50, 0), new DateTime(2025, 8, 10, 0, 0, 0, 0, DateTimeKind.Local), 3, "Regular day", "Present" },
                    { 76, new TimeOnly(9, 0, 0), new TimeOnly(17, 0, 0), new DateTime(2025, 8, 9, 0, 0, 0, 0, DateTimeKind.Local), 1, "Regular day", "Present" },
                    { 77, new TimeOnly(9, 15, 0), new TimeOnly(17, 0, 0), new DateTime(2025, 8, 9, 0, 0, 0, 0, DateTimeKind.Local), 2, "Traffic delay", "Late" },
                    { 78, null, null, new DateTime(2025, 8, 9, 0, 0, 0, 0, DateTimeKind.Local), 3, "Sick leave", "Absent" },
                    { 79, new TimeOnly(9, 0, 0), new TimeOnly(17, 0, 0), new DateTime(2025, 8, 8, 0, 0, 0, 0, DateTimeKind.Local), 1, "Regular day", "Present" },
                    { 80, new TimeOnly(9, 15, 0), new TimeOnly(17, 0, 0), new DateTime(2025, 8, 8, 0, 0, 0, 0, DateTimeKind.Local), 2, "Traffic delay", "Late" },
                    { 81, new TimeOnly(8, 55, 0), new TimeOnly(16, 50, 0), new DateTime(2025, 8, 8, 0, 0, 0, 0, DateTimeKind.Local), 3, "Regular day", "Present" },
                    { 82, new TimeOnly(9, 0, 0), new TimeOnly(17, 0, 0), new DateTime(2025, 8, 7, 0, 0, 0, 0, DateTimeKind.Local), 1, "Regular day", "Present" },
                    { 83, new TimeOnly(9, 15, 0), new TimeOnly(17, 0, 0), new DateTime(2025, 8, 7, 0, 0, 0, 0, DateTimeKind.Local), 2, "Traffic delay", "Late" },
                    { 84, new TimeOnly(8, 55, 0), new TimeOnly(16, 50, 0), new DateTime(2025, 8, 7, 0, 0, 0, 0, DateTimeKind.Local), 3, "Regular day", "Present" },
                    { 85, new TimeOnly(9, 0, 0), new TimeOnly(17, 0, 0), new DateTime(2025, 8, 6, 0, 0, 0, 0, DateTimeKind.Local), 1, "Regular day", "Present" },
                    { 86, new TimeOnly(9, 15, 0), new TimeOnly(17, 0, 0), new DateTime(2025, 8, 6, 0, 0, 0, 0, DateTimeKind.Local), 2, "Traffic delay", "Late" },
                    { 87, new TimeOnly(8, 55, 0), new TimeOnly(16, 50, 0), new DateTime(2025, 8, 6, 0, 0, 0, 0, DateTimeKind.Local), 3, "Regular day", "Present" },
                    { 88, new TimeOnly(9, 0, 0), new TimeOnly(17, 0, 0), new DateTime(2025, 8, 5, 0, 0, 0, 0, DateTimeKind.Local), 1, "Regular day", "Present" },
                    { 89, new TimeOnly(9, 15, 0), new TimeOnly(17, 0, 0), new DateTime(2025, 8, 5, 0, 0, 0, 0, DateTimeKind.Local), 2, "Traffic delay", "Late" },
                    { 90, new TimeOnly(8, 55, 0), new TimeOnly(16, 50, 0), new DateTime(2025, 8, 5, 0, 0, 0, 0, DateTimeKind.Local), 3, "Regular day", "Present" }
                });

            migrationBuilder.InsertData(
                table: "LeaveRequests",
                columns: new[] { "Id", "Days", "DecisionAt", "DecisionBy", "EmployeeId", "EndDate", "Reason", "StartDate", "Status", "Type" },
                values: new object[,]
                {
                    { 1, 5, new DateTime(2023, 10, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), "HR Manager", 1, new DateTime(2023, 10, 27, 0, 0, 0, 0, DateTimeKind.Unspecified), "Family vacation", new DateTime(2023, 10, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), "Approved", "Annual" },
                    { 2, 1, null, null, 2, new DateTime(2023, 11, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Flu", new DateTime(2023, 11, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Pending", "Sick" },
                    { 3, 2, new DateTime(2023, 11, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "HR Manager", 3, new DateTime(2023, 11, 16, 0, 0, 0, 0, DateTimeKind.Unspecified), "Personal errands", new DateTime(2023, 11, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), "Rejected", "Casual" },
                    { 4, 5, null, null, 4, new DateTime(2023, 12, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), "Extended travel", new DateTime(2023, 12, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Pending", "Unpaid" }
                });

            migrationBuilder.InsertData(
                table: "PayrollItems",
                columns: new[] { "Id", "Allowances", "BaseSalary", "Deductions", "EmployeeId", "NetPay", "PayrollRunId" },
                values: new object[,]
                {
                    { 1, 500.00m, 3750.00m, 200.00m, 1, 4050.00m, 1 },
                    { 2, 750.00m, 5000.00m, 300.00m, 2, 5450.00m, 1 },
                    { 3, 300.00m, 2916.6666666666666666666666667m, 150.00m, 3, 3066.6666666666666666666666667m, 1 },
                    { 4, 1000.00m, 5833.3333333333333333333333333m, 400.00m, 4, 6433.3333333333333333333333333m, 1 }
                });

            migrationBuilder.InsertData(
                table: "PerformanceReviews",
                columns: new[] { "Id", "EmployeeId", "Notes", "Rating", "ReviewDate", "Reviewer" },
                values: new object[,]
                {
                    { 1, 1, "Consistently meets expectations, good team player.", 4, new DateTime(2023, 9, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "HR Manager" },
                    { 2, 2, "Exceeds expectations, highly skilled and proactive.", 5, new DateTime(2023, 9, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), "HR Manager" },
                    { 3, 3, "Meets basic requirements, areas for improvement in patient communication.", 3, new DateTime(2023, 8, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), "HR Manager" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_DentistId",
                table: "Appointments",
                column: "DentistId");

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_DepartmentId",
                table: "Appointments",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_DoctorId",
                table: "Appointments",
                column: "DoctorId");

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_PatientId",
                table: "Appointments",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_PatientId1",
                table: "Appointments",
                column: "PatientId1");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true,
                filter: "[NormalizedName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true,
                filter: "[NormalizedUserName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AttendanceRecords_EmployeeId",
                table: "AttendanceRecords",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_Dentists_ApplicationUserId",
                table: "Dentists",
                column: "ApplicationUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Dentists_LicenseNo",
                table: "Dentists",
                column: "LicenseNo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DoctorAwards_DoctorId",
                table: "DoctorAwards",
                column: "DoctorId");

            migrationBuilder.CreateIndex(
                name: "IX_DoctorCertifications_DoctorId",
                table: "DoctorCertifications",
                column: "DoctorId");

            migrationBuilder.CreateIndex(
                name: "IX_DoctorDateAvailabilities_DoctorId",
                table: "DoctorDateAvailabilities",
                column: "DoctorId");

            migrationBuilder.CreateIndex(
                name: "IX_DoctorEducations_DoctorId",
                table: "DoctorEducations",
                column: "DoctorId");

            migrationBuilder.CreateIndex(
                name: "IX_DoctorWeeklyAvailabilities_DoctorId",
                table: "DoctorWeeklyAvailabilities",
                column: "DoctorId");

            migrationBuilder.CreateIndex(
                name: "IX_Employees_Code",
                table: "Employees",
                column: "Code",
                unique: true,
                filter: "[Code] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Employees_DepartmentId",
                table: "Employees",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Employees_Email",
                table: "Employees",
                column: "Email",
                unique: true,
                filter: "[Email] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryItems_SKU",
                table: "InventoryItems",
                column: "SKU",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceItems_InventoryItemId",
                table: "InvoiceItems",
                column: "InventoryItemId");

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceItems_InvoiceId",
                table: "InvoiceItems",
                column: "InvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceItems_InvoiceId1",
                table: "InvoiceItems",
                column: "InvoiceId1");

            migrationBuilder.CreateIndex(
                name: "IX_LeaveRequests_EmployeeId",
                table: "LeaveRequests",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_PayrollItems_EmployeeId",
                table: "PayrollItems",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_PayrollItems_PayrollRunId",
                table: "PayrollItems",
                column: "PayrollRunId");

            migrationBuilder.CreateIndex(
                name: "IX_PerformanceReviews_EmployeeId",
                table: "PerformanceReviews",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_StockMovements_InventoryItemId",
                table: "StockMovements",
                column: "InventoryItemId");

            migrationBuilder.CreateIndex(
                name: "IX_Treatments_AppointmentId",
                table: "Treatments",
                column: "AppointmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Treatments_InvoiceItemId",
                table: "Treatments",
                column: "InvoiceItemId");

            migrationBuilder.CreateIndex(
                name: "IX_Treatments_PatientId",
                table: "Treatments",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_Xrays_PatientId",
                table: "Xrays",
                column: "PatientId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "AttendanceRecords");

            migrationBuilder.DropTable(
                name: "Attendances");

            migrationBuilder.DropTable(
                name: "DoctorAwards");

            migrationBuilder.DropTable(
                name: "DoctorCertifications");

            migrationBuilder.DropTable(
                name: "DoctorDateAvailabilities");

            migrationBuilder.DropTable(
                name: "DoctorEducations");

            migrationBuilder.DropTable(
                name: "DoctorWeeklyAvailabilities");

            migrationBuilder.DropTable(
                name: "LeaveRequests");

            migrationBuilder.DropTable(
                name: "PayrollItems");

            migrationBuilder.DropTable(
                name: "PerformanceReviews");

            migrationBuilder.DropTable(
                name: "StockMovements");

            migrationBuilder.DropTable(
                name: "Treatments");

            migrationBuilder.DropTable(
                name: "Xrays");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "PayrollRuns");

            migrationBuilder.DropTable(
                name: "Employees");

            migrationBuilder.DropTable(
                name: "Appointments");

            migrationBuilder.DropTable(
                name: "InvoiceItems");

            migrationBuilder.DropTable(
                name: "Dentists");

            migrationBuilder.DropTable(
                name: "Department");

            migrationBuilder.DropTable(
                name: "Doctors");

            migrationBuilder.DropTable(
                name: "Patients");

            migrationBuilder.DropTable(
                name: "InventoryItems");

            migrationBuilder.DropTable(
                name: "Invoices");

            migrationBuilder.DropTable(
                name: "AspNetUsers");
        }
    }
}
