using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CMS.Migrations
{
    /// <inheritdoc />
    public partial class IsDeletedForHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Appointments_Dentists_DentistId",
                table: "Appointments");

            migrationBuilder.DropTable(
                name: "Dentists");

            migrationBuilder.DropTable(
                name: "ApplicationUser");

            migrationBuilder.DropIndex(
                name: "IX_Appointments_DentistId",
                table: "Appointments");

            migrationBuilder.DropColumn(
                name: "DentistId",
                table: "Appointments");

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Users",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Payments",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Patients",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Invoices",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Employees",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Doctors",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Appointments",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 1,
                column: "Date",
                value: new DateTime(2025, 10, 23, 0, 0, 0, 0, DateTimeKind.Local));

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 2,
                column: "Date",
                value: new DateTime(2025, 10, 23, 0, 0, 0, 0, DateTimeKind.Local));

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 3,
                column: "Date",
                value: new DateTime(2025, 10, 23, 0, 0, 0, 0, DateTimeKind.Local));

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 4,
                column: "Date",
                value: new DateTime(2025, 10, 22, 0, 0, 0, 0, DateTimeKind.Local));

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 5,
                column: "Date",
                value: new DateTime(2025, 10, 22, 0, 0, 0, 0, DateTimeKind.Local));

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 6,
                column: "Date",
                value: new DateTime(2025, 10, 22, 0, 0, 0, 0, DateTimeKind.Local));

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 7,
                column: "Date",
                value: new DateTime(2025, 10, 21, 0, 0, 0, 0, DateTimeKind.Local));

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 8,
                column: "Date",
                value: new DateTime(2025, 10, 21, 0, 0, 0, 0, DateTimeKind.Local));

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 9,
                column: "Date",
                value: new DateTime(2025, 10, 21, 0, 0, 0, 0, DateTimeKind.Local));

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 10,
                column: "Date",
                value: new DateTime(2025, 10, 20, 0, 0, 0, 0, DateTimeKind.Local));

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 11,
                column: "Date",
                value: new DateTime(2025, 10, 20, 0, 0, 0, 0, DateTimeKind.Local));

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 12,
                column: "Date",
                value: new DateTime(2025, 10, 20, 0, 0, 0, 0, DateTimeKind.Local));

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 13,
                column: "Date",
                value: new DateTime(2025, 10, 19, 0, 0, 0, 0, DateTimeKind.Local));

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 14,
                column: "Date",
                value: new DateTime(2025, 10, 19, 0, 0, 0, 0, DateTimeKind.Local));

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 15,
                column: "Date",
                value: new DateTime(2025, 10, 19, 0, 0, 0, 0, DateTimeKind.Local));

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 16,
                column: "Date",
                value: new DateTime(2025, 10, 18, 0, 0, 0, 0, DateTimeKind.Local));

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 17,
                column: "Date",
                value: new DateTime(2025, 10, 18, 0, 0, 0, 0, DateTimeKind.Local));

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 18,
                column: "Date",
                value: new DateTime(2025, 10, 18, 0, 0, 0, 0, DateTimeKind.Local));

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 19,
                column: "Date",
                value: new DateTime(2025, 10, 17, 0, 0, 0, 0, DateTimeKind.Local));

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 20,
                column: "Date",
                value: new DateTime(2025, 10, 17, 0, 0, 0, 0, DateTimeKind.Local));

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 21,
                column: "Date",
                value: new DateTime(2025, 10, 17, 0, 0, 0, 0, DateTimeKind.Local));

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 22,
                column: "Date",
                value: new DateTime(2025, 10, 16, 0, 0, 0, 0, DateTimeKind.Local));

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 23,
                column: "Date",
                value: new DateTime(2025, 10, 16, 0, 0, 0, 0, DateTimeKind.Local));

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 24,
                column: "Date",
                value: new DateTime(2025, 10, 16, 0, 0, 0, 0, DateTimeKind.Local));

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 25,
                column: "Date",
                value: new DateTime(2025, 10, 15, 0, 0, 0, 0, DateTimeKind.Local));

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 26,
                column: "Date",
                value: new DateTime(2025, 10, 15, 0, 0, 0, 0, DateTimeKind.Local));

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 27,
                column: "Date",
                value: new DateTime(2025, 10, 15, 0, 0, 0, 0, DateTimeKind.Local));

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 28,
                column: "Date",
                value: new DateTime(2025, 10, 14, 0, 0, 0, 0, DateTimeKind.Local));

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 29,
                column: "Date",
                value: new DateTime(2025, 10, 14, 0, 0, 0, 0, DateTimeKind.Local));

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 30,
                column: "Date",
                value: new DateTime(2025, 10, 14, 0, 0, 0, 0, DateTimeKind.Local));

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 31,
                column: "Date",
                value: new DateTime(2025, 10, 13, 0, 0, 0, 0, DateTimeKind.Local));

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 32,
                column: "Date",
                value: new DateTime(2025, 10, 13, 0, 0, 0, 0, DateTimeKind.Local));

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 33,
                column: "Date",
                value: new DateTime(2025, 10, 13, 0, 0, 0, 0, DateTimeKind.Local));

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 34,
                column: "Date",
                value: new DateTime(2025, 10, 12, 0, 0, 0, 0, DateTimeKind.Local));

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 35,
                column: "Date",
                value: new DateTime(2025, 10, 12, 0, 0, 0, 0, DateTimeKind.Local));

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 36,
                column: "Date",
                value: new DateTime(2025, 10, 12, 0, 0, 0, 0, DateTimeKind.Local));

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 37,
                column: "Date",
                value: new DateTime(2025, 10, 11, 0, 0, 0, 0, DateTimeKind.Local));

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 38,
                column: "Date",
                value: new DateTime(2025, 10, 11, 0, 0, 0, 0, DateTimeKind.Local));

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 39,
                column: "Date",
                value: new DateTime(2025, 10, 11, 0, 0, 0, 0, DateTimeKind.Local));

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 40,
                column: "Date",
                value: new DateTime(2025, 10, 10, 0, 0, 0, 0, DateTimeKind.Local));

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 41,
                column: "Date",
                value: new DateTime(2025, 10, 10, 0, 0, 0, 0, DateTimeKind.Local));

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 42,
                column: "Date",
                value: new DateTime(2025, 10, 10, 0, 0, 0, 0, DateTimeKind.Local));

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 43,
                column: "Date",
                value: new DateTime(2025, 10, 9, 0, 0, 0, 0, DateTimeKind.Local));

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 44,
                column: "Date",
                value: new DateTime(2025, 10, 9, 0, 0, 0, 0, DateTimeKind.Local));

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 45,
                column: "Date",
                value: new DateTime(2025, 10, 9, 0, 0, 0, 0, DateTimeKind.Local));

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 46,
                column: "Date",
                value: new DateTime(2025, 10, 8, 0, 0, 0, 0, DateTimeKind.Local));

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 47,
                column: "Date",
                value: new DateTime(2025, 10, 8, 0, 0, 0, 0, DateTimeKind.Local));

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 48,
                column: "Date",
                value: new DateTime(2025, 10, 8, 0, 0, 0, 0, DateTimeKind.Local));

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 49,
                column: "Date",
                value: new DateTime(2025, 10, 7, 0, 0, 0, 0, DateTimeKind.Local));

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 50,
                column: "Date",
                value: new DateTime(2025, 10, 7, 0, 0, 0, 0, DateTimeKind.Local));

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 51,
                column: "Date",
                value: new DateTime(2025, 10, 7, 0, 0, 0, 0, DateTimeKind.Local));

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 52,
                column: "Date",
                value: new DateTime(2025, 10, 6, 0, 0, 0, 0, DateTimeKind.Local));

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 53,
                column: "Date",
                value: new DateTime(2025, 10, 6, 0, 0, 0, 0, DateTimeKind.Local));

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 54,
                column: "Date",
                value: new DateTime(2025, 10, 6, 0, 0, 0, 0, DateTimeKind.Local));

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 55,
                column: "Date",
                value: new DateTime(2025, 10, 5, 0, 0, 0, 0, DateTimeKind.Local));

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 56,
                column: "Date",
                value: new DateTime(2025, 10, 5, 0, 0, 0, 0, DateTimeKind.Local));

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 57,
                column: "Date",
                value: new DateTime(2025, 10, 5, 0, 0, 0, 0, DateTimeKind.Local));

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 58,
                column: "Date",
                value: new DateTime(2025, 10, 4, 0, 0, 0, 0, DateTimeKind.Local));

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 59,
                column: "Date",
                value: new DateTime(2025, 10, 4, 0, 0, 0, 0, DateTimeKind.Local));

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 60,
                column: "Date",
                value: new DateTime(2025, 10, 4, 0, 0, 0, 0, DateTimeKind.Local));

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 61,
                column: "Date",
                value: new DateTime(2025, 10, 3, 0, 0, 0, 0, DateTimeKind.Local));

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 62,
                column: "Date",
                value: new DateTime(2025, 10, 3, 0, 0, 0, 0, DateTimeKind.Local));

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 63,
                column: "Date",
                value: new DateTime(2025, 10, 3, 0, 0, 0, 0, DateTimeKind.Local));

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 64,
                column: "Date",
                value: new DateTime(2025, 10, 2, 0, 0, 0, 0, DateTimeKind.Local));

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 65,
                column: "Date",
                value: new DateTime(2025, 10, 2, 0, 0, 0, 0, DateTimeKind.Local));

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 66,
                column: "Date",
                value: new DateTime(2025, 10, 2, 0, 0, 0, 0, DateTimeKind.Local));

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 67,
                column: "Date",
                value: new DateTime(2025, 10, 1, 0, 0, 0, 0, DateTimeKind.Local));

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 68,
                column: "Date",
                value: new DateTime(2025, 10, 1, 0, 0, 0, 0, DateTimeKind.Local));

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 69,
                column: "Date",
                value: new DateTime(2025, 10, 1, 0, 0, 0, 0, DateTimeKind.Local));

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 70,
                column: "Date",
                value: new DateTime(2025, 9, 30, 0, 0, 0, 0, DateTimeKind.Local));

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 71,
                column: "Date",
                value: new DateTime(2025, 9, 30, 0, 0, 0, 0, DateTimeKind.Local));

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 72,
                column: "Date",
                value: new DateTime(2025, 9, 30, 0, 0, 0, 0, DateTimeKind.Local));

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 73,
                column: "Date",
                value: new DateTime(2025, 9, 29, 0, 0, 0, 0, DateTimeKind.Local));

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 74,
                column: "Date",
                value: new DateTime(2025, 9, 29, 0, 0, 0, 0, DateTimeKind.Local));

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 75,
                column: "Date",
                value: new DateTime(2025, 9, 29, 0, 0, 0, 0, DateTimeKind.Local));

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 76,
                column: "Date",
                value: new DateTime(2025, 9, 28, 0, 0, 0, 0, DateTimeKind.Local));

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 77,
                column: "Date",
                value: new DateTime(2025, 9, 28, 0, 0, 0, 0, DateTimeKind.Local));

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 78,
                column: "Date",
                value: new DateTime(2025, 9, 28, 0, 0, 0, 0, DateTimeKind.Local));

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 79,
                column: "Date",
                value: new DateTime(2025, 9, 27, 0, 0, 0, 0, DateTimeKind.Local));

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 80,
                column: "Date",
                value: new DateTime(2025, 9, 27, 0, 0, 0, 0, DateTimeKind.Local));

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 81,
                column: "Date",
                value: new DateTime(2025, 9, 27, 0, 0, 0, 0, DateTimeKind.Local));

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 82,
                column: "Date",
                value: new DateTime(2025, 9, 26, 0, 0, 0, 0, DateTimeKind.Local));

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 83,
                column: "Date",
                value: new DateTime(2025, 9, 26, 0, 0, 0, 0, DateTimeKind.Local));

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 84,
                column: "Date",
                value: new DateTime(2025, 9, 26, 0, 0, 0, 0, DateTimeKind.Local));

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 85,
                column: "Date",
                value: new DateTime(2025, 9, 25, 0, 0, 0, 0, DateTimeKind.Local));

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 86,
                column: "Date",
                value: new DateTime(2025, 9, 25, 0, 0, 0, 0, DateTimeKind.Local));

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 87,
                column: "Date",
                value: new DateTime(2025, 9, 25, 0, 0, 0, 0, DateTimeKind.Local));

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 88,
                column: "Date",
                value: new DateTime(2025, 9, 24, 0, 0, 0, 0, DateTimeKind.Local));

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 89,
                column: "Date",
                value: new DateTime(2025, 9, 24, 0, 0, 0, 0, DateTimeKind.Local));

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 90,
                column: "Date",
                value: new DateTime(2025, 9, 24, 0, 0, 0, 0, DateTimeKind.Local));

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "Id",
                keyValue: 1,
                column: "IsDeleted",
                value: false);

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "Id",
                keyValue: 2,
                column: "IsDeleted",
                value: false);

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "Id",
                keyValue: 3,
                column: "IsDeleted",
                value: false);

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "Id",
                keyValue: 4,
                column: "IsDeleted",
                value: false);

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "Id",
                keyValue: 5,
                column: "IsDeleted",
                value: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Patients");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Doctors");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Appointments");

            migrationBuilder.AddColumn<int>(
                name: "DentistId",
                table: "Appointments",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ApplicationUser",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "int", nullable: false),
                    Address = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Department = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    HireDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "bit", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    NormalizedEmail = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NormalizedUserName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    Salary = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    SecurityStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TwoFactorEnabled = table.Column<bool>(type: "bit", nullable: false),
                    UserName = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApplicationUser", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Dentists",
                columns: table => new
                {
                    DentistId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ApplicationUserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    LicenseNo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Specialty = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Dentists", x => x.DentistId);
                    table.ForeignKey(
                        name: "FK_Dentists_ApplicationUser_ApplicationUserId",
                        column: x => x.ApplicationUserId,
                        principalTable: "ApplicationUser",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 1,
                column: "Date",
                value: new DateTime(2025, 10, 20, 0, 0, 0, 0, DateTimeKind.Local));

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 2,
                column: "Date",
                value: new DateTime(2025, 10, 20, 0, 0, 0, 0, DateTimeKind.Local));

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 3,
                column: "Date",
                value: new DateTime(2025, 10, 20, 0, 0, 0, 0, DateTimeKind.Local));

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 4,
                column: "Date",
                value: new DateTime(2025, 10, 19, 0, 0, 0, 0, DateTimeKind.Local));

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 5,
                column: "Date",
                value: new DateTime(2025, 10, 19, 0, 0, 0, 0, DateTimeKind.Local));

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 6,
                column: "Date",
                value: new DateTime(2025, 10, 19, 0, 0, 0, 0, DateTimeKind.Local));

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 7,
                column: "Date",
                value: new DateTime(2025, 10, 18, 0, 0, 0, 0, DateTimeKind.Local));

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 8,
                column: "Date",
                value: new DateTime(2025, 10, 18, 0, 0, 0, 0, DateTimeKind.Local));

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 9,
                column: "Date",
                value: new DateTime(2025, 10, 18, 0, 0, 0, 0, DateTimeKind.Local));

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 10,
                column: "Date",
                value: new DateTime(2025, 10, 17, 0, 0, 0, 0, DateTimeKind.Local));

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 11,
                column: "Date",
                value: new DateTime(2025, 10, 17, 0, 0, 0, 0, DateTimeKind.Local));

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 12,
                column: "Date",
                value: new DateTime(2025, 10, 17, 0, 0, 0, 0, DateTimeKind.Local));

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 13,
                column: "Date",
                value: new DateTime(2025, 10, 16, 0, 0, 0, 0, DateTimeKind.Local));

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 14,
                column: "Date",
                value: new DateTime(2025, 10, 16, 0, 0, 0, 0, DateTimeKind.Local));

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 15,
                column: "Date",
                value: new DateTime(2025, 10, 16, 0, 0, 0, 0, DateTimeKind.Local));

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 16,
                column: "Date",
                value: new DateTime(2025, 10, 15, 0, 0, 0, 0, DateTimeKind.Local));

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 17,
                column: "Date",
                value: new DateTime(2025, 10, 15, 0, 0, 0, 0, DateTimeKind.Local));

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 18,
                column: "Date",
                value: new DateTime(2025, 10, 15, 0, 0, 0, 0, DateTimeKind.Local));

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 19,
                column: "Date",
                value: new DateTime(2025, 10, 14, 0, 0, 0, 0, DateTimeKind.Local));

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 20,
                column: "Date",
                value: new DateTime(2025, 10, 14, 0, 0, 0, 0, DateTimeKind.Local));

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 21,
                column: "Date",
                value: new DateTime(2025, 10, 14, 0, 0, 0, 0, DateTimeKind.Local));

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 22,
                column: "Date",
                value: new DateTime(2025, 10, 13, 0, 0, 0, 0, DateTimeKind.Local));

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 23,
                column: "Date",
                value: new DateTime(2025, 10, 13, 0, 0, 0, 0, DateTimeKind.Local));

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 24,
                column: "Date",
                value: new DateTime(2025, 10, 13, 0, 0, 0, 0, DateTimeKind.Local));

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 25,
                column: "Date",
                value: new DateTime(2025, 10, 12, 0, 0, 0, 0, DateTimeKind.Local));

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 26,
                column: "Date",
                value: new DateTime(2025, 10, 12, 0, 0, 0, 0, DateTimeKind.Local));

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 27,
                column: "Date",
                value: new DateTime(2025, 10, 12, 0, 0, 0, 0, DateTimeKind.Local));

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 28,
                column: "Date",
                value: new DateTime(2025, 10, 11, 0, 0, 0, 0, DateTimeKind.Local));

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 29,
                column: "Date",
                value: new DateTime(2025, 10, 11, 0, 0, 0, 0, DateTimeKind.Local));

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 30,
                column: "Date",
                value: new DateTime(2025, 10, 11, 0, 0, 0, 0, DateTimeKind.Local));

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 31,
                column: "Date",
                value: new DateTime(2025, 10, 10, 0, 0, 0, 0, DateTimeKind.Local));

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 32,
                column: "Date",
                value: new DateTime(2025, 10, 10, 0, 0, 0, 0, DateTimeKind.Local));

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 33,
                column: "Date",
                value: new DateTime(2025, 10, 10, 0, 0, 0, 0, DateTimeKind.Local));

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 34,
                column: "Date",
                value: new DateTime(2025, 10, 9, 0, 0, 0, 0, DateTimeKind.Local));

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 35,
                column: "Date",
                value: new DateTime(2025, 10, 9, 0, 0, 0, 0, DateTimeKind.Local));

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 36,
                column: "Date",
                value: new DateTime(2025, 10, 9, 0, 0, 0, 0, DateTimeKind.Local));

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 37,
                column: "Date",
                value: new DateTime(2025, 10, 8, 0, 0, 0, 0, DateTimeKind.Local));

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 38,
                column: "Date",
                value: new DateTime(2025, 10, 8, 0, 0, 0, 0, DateTimeKind.Local));

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 39,
                column: "Date",
                value: new DateTime(2025, 10, 8, 0, 0, 0, 0, DateTimeKind.Local));

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 40,
                column: "Date",
                value: new DateTime(2025, 10, 7, 0, 0, 0, 0, DateTimeKind.Local));

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 41,
                column: "Date",
                value: new DateTime(2025, 10, 7, 0, 0, 0, 0, DateTimeKind.Local));

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 42,
                column: "Date",
                value: new DateTime(2025, 10, 7, 0, 0, 0, 0, DateTimeKind.Local));

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 43,
                column: "Date",
                value: new DateTime(2025, 10, 6, 0, 0, 0, 0, DateTimeKind.Local));

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 44,
                column: "Date",
                value: new DateTime(2025, 10, 6, 0, 0, 0, 0, DateTimeKind.Local));

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 45,
                column: "Date",
                value: new DateTime(2025, 10, 6, 0, 0, 0, 0, DateTimeKind.Local));

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 46,
                column: "Date",
                value: new DateTime(2025, 10, 5, 0, 0, 0, 0, DateTimeKind.Local));

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 47,
                column: "Date",
                value: new DateTime(2025, 10, 5, 0, 0, 0, 0, DateTimeKind.Local));

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 48,
                column: "Date",
                value: new DateTime(2025, 10, 5, 0, 0, 0, 0, DateTimeKind.Local));

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 49,
                column: "Date",
                value: new DateTime(2025, 10, 4, 0, 0, 0, 0, DateTimeKind.Local));

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 50,
                column: "Date",
                value: new DateTime(2025, 10, 4, 0, 0, 0, 0, DateTimeKind.Local));

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 51,
                column: "Date",
                value: new DateTime(2025, 10, 4, 0, 0, 0, 0, DateTimeKind.Local));

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 52,
                column: "Date",
                value: new DateTime(2025, 10, 3, 0, 0, 0, 0, DateTimeKind.Local));

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 53,
                column: "Date",
                value: new DateTime(2025, 10, 3, 0, 0, 0, 0, DateTimeKind.Local));

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 54,
                column: "Date",
                value: new DateTime(2025, 10, 3, 0, 0, 0, 0, DateTimeKind.Local));

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 55,
                column: "Date",
                value: new DateTime(2025, 10, 2, 0, 0, 0, 0, DateTimeKind.Local));

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 56,
                column: "Date",
                value: new DateTime(2025, 10, 2, 0, 0, 0, 0, DateTimeKind.Local));

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 57,
                column: "Date",
                value: new DateTime(2025, 10, 2, 0, 0, 0, 0, DateTimeKind.Local));

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 58,
                column: "Date",
                value: new DateTime(2025, 10, 1, 0, 0, 0, 0, DateTimeKind.Local));

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 59,
                column: "Date",
                value: new DateTime(2025, 10, 1, 0, 0, 0, 0, DateTimeKind.Local));

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 60,
                column: "Date",
                value: new DateTime(2025, 10, 1, 0, 0, 0, 0, DateTimeKind.Local));

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 61,
                column: "Date",
                value: new DateTime(2025, 9, 30, 0, 0, 0, 0, DateTimeKind.Local));

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 62,
                column: "Date",
                value: new DateTime(2025, 9, 30, 0, 0, 0, 0, DateTimeKind.Local));

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 63,
                column: "Date",
                value: new DateTime(2025, 9, 30, 0, 0, 0, 0, DateTimeKind.Local));

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 64,
                column: "Date",
                value: new DateTime(2025, 9, 29, 0, 0, 0, 0, DateTimeKind.Local));

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 65,
                column: "Date",
                value: new DateTime(2025, 9, 29, 0, 0, 0, 0, DateTimeKind.Local));

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 66,
                column: "Date",
                value: new DateTime(2025, 9, 29, 0, 0, 0, 0, DateTimeKind.Local));

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 67,
                column: "Date",
                value: new DateTime(2025, 9, 28, 0, 0, 0, 0, DateTimeKind.Local));

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 68,
                column: "Date",
                value: new DateTime(2025, 9, 28, 0, 0, 0, 0, DateTimeKind.Local));

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 69,
                column: "Date",
                value: new DateTime(2025, 9, 28, 0, 0, 0, 0, DateTimeKind.Local));

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 70,
                column: "Date",
                value: new DateTime(2025, 9, 27, 0, 0, 0, 0, DateTimeKind.Local));

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 71,
                column: "Date",
                value: new DateTime(2025, 9, 27, 0, 0, 0, 0, DateTimeKind.Local));

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 72,
                column: "Date",
                value: new DateTime(2025, 9, 27, 0, 0, 0, 0, DateTimeKind.Local));

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 73,
                column: "Date",
                value: new DateTime(2025, 9, 26, 0, 0, 0, 0, DateTimeKind.Local));

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 74,
                column: "Date",
                value: new DateTime(2025, 9, 26, 0, 0, 0, 0, DateTimeKind.Local));

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 75,
                column: "Date",
                value: new DateTime(2025, 9, 26, 0, 0, 0, 0, DateTimeKind.Local));

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 76,
                column: "Date",
                value: new DateTime(2025, 9, 25, 0, 0, 0, 0, DateTimeKind.Local));

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 77,
                column: "Date",
                value: new DateTime(2025, 9, 25, 0, 0, 0, 0, DateTimeKind.Local));

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 78,
                column: "Date",
                value: new DateTime(2025, 9, 25, 0, 0, 0, 0, DateTimeKind.Local));

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 79,
                column: "Date",
                value: new DateTime(2025, 9, 24, 0, 0, 0, 0, DateTimeKind.Local));

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 80,
                column: "Date",
                value: new DateTime(2025, 9, 24, 0, 0, 0, 0, DateTimeKind.Local));

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 81,
                column: "Date",
                value: new DateTime(2025, 9, 24, 0, 0, 0, 0, DateTimeKind.Local));

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 82,
                column: "Date",
                value: new DateTime(2025, 9, 23, 0, 0, 0, 0, DateTimeKind.Local));

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 83,
                column: "Date",
                value: new DateTime(2025, 9, 23, 0, 0, 0, 0, DateTimeKind.Local));

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 84,
                column: "Date",
                value: new DateTime(2025, 9, 23, 0, 0, 0, 0, DateTimeKind.Local));

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 85,
                column: "Date",
                value: new DateTime(2025, 9, 22, 0, 0, 0, 0, DateTimeKind.Local));

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 86,
                column: "Date",
                value: new DateTime(2025, 9, 22, 0, 0, 0, 0, DateTimeKind.Local));

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 87,
                column: "Date",
                value: new DateTime(2025, 9, 22, 0, 0, 0, 0, DateTimeKind.Local));

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 88,
                column: "Date",
                value: new DateTime(2025, 9, 21, 0, 0, 0, 0, DateTimeKind.Local));

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 89,
                column: "Date",
                value: new DateTime(2025, 9, 21, 0, 0, 0, 0, DateTimeKind.Local));

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 90,
                column: "Date",
                value: new DateTime(2025, 9, 21, 0, 0, 0, 0, DateTimeKind.Local));

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_DentistId",
                table: "Appointments",
                column: "DentistId");

            migrationBuilder.CreateIndex(
                name: "IX_Dentists_ApplicationUserId",
                table: "Dentists",
                column: "ApplicationUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Dentists_LicenseNo",
                table: "Dentists",
                column: "LicenseNo",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Appointments_Dentists_DentistId",
                table: "Appointments",
                column: "DentistId",
                principalTable: "Dentists",
                principalColumn: "DentistId");
        }
    }
}
