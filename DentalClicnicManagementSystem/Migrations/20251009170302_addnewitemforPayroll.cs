using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CMS.Migrations
{
    /// <inheritdoc />
    public partial class addnewitemforPayroll : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "RunAt",
                table: "PayrollRuns",
                type: "datetimeoffset",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<string>(
                name: "AttendanceSummary",
                table: "PayrollItems",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "HousingAllowance",
                table: "Employees",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TransportAllowance",
                table: "Employees",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "OvertimeHours",
                table: "AttendanceRecords",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 1,
                column: "OvertimeHours",
                value: null);

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 2,
                column: "OvertimeHours",
                value: null);

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 3,
                column: "OvertimeHours",
                value: null);

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 4,
                column: "OvertimeHours",
                value: null);

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 5,
                column: "OvertimeHours",
                value: null);

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 6,
                column: "OvertimeHours",
                value: null);

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 7,
                column: "OvertimeHours",
                value: null);

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 8,
                column: "OvertimeHours",
                value: null);

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 9,
                column: "OvertimeHours",
                value: null);

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 10,
                column: "OvertimeHours",
                value: null);

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 11,
                column: "OvertimeHours",
                value: null);

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 12,
                column: "OvertimeHours",
                value: null);

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 13,
                column: "OvertimeHours",
                value: null);

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 14,
                column: "OvertimeHours",
                value: null);

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 15,
                column: "OvertimeHours",
                value: null);

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 16,
                column: "OvertimeHours",
                value: null);

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 17,
                column: "OvertimeHours",
                value: null);

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 18,
                column: "OvertimeHours",
                value: null);

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 19,
                column: "OvertimeHours",
                value: null);

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 20,
                column: "OvertimeHours",
                value: null);

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 21,
                column: "OvertimeHours",
                value: null);

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 22,
                column: "OvertimeHours",
                value: null);

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 23,
                column: "OvertimeHours",
                value: null);

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 24,
                column: "OvertimeHours",
                value: null);

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 25,
                column: "OvertimeHours",
                value: null);

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 26,
                column: "OvertimeHours",
                value: null);

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 27,
                column: "OvertimeHours",
                value: null);

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 28,
                column: "OvertimeHours",
                value: null);

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 29,
                column: "OvertimeHours",
                value: null);

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 30,
                column: "OvertimeHours",
                value: null);

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 31,
                column: "OvertimeHours",
                value: null);

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 32,
                column: "OvertimeHours",
                value: null);

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 33,
                column: "OvertimeHours",
                value: null);

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 34,
                column: "OvertimeHours",
                value: null);

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 35,
                column: "OvertimeHours",
                value: null);

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 36,
                column: "OvertimeHours",
                value: null);

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 37,
                column: "OvertimeHours",
                value: null);

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 38,
                column: "OvertimeHours",
                value: null);

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 39,
                column: "OvertimeHours",
                value: null);

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 40,
                column: "OvertimeHours",
                value: null);

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 41,
                column: "OvertimeHours",
                value: null);

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 42,
                column: "OvertimeHours",
                value: null);

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 43,
                column: "OvertimeHours",
                value: null);

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 44,
                column: "OvertimeHours",
                value: null);

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 45,
                column: "OvertimeHours",
                value: null);

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 46,
                column: "OvertimeHours",
                value: null);

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 47,
                column: "OvertimeHours",
                value: null);

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 48,
                column: "OvertimeHours",
                value: null);

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 49,
                column: "OvertimeHours",
                value: null);

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 50,
                column: "OvertimeHours",
                value: null);

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 51,
                column: "OvertimeHours",
                value: null);

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 52,
                column: "OvertimeHours",
                value: null);

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 53,
                column: "OvertimeHours",
                value: null);

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 54,
                column: "OvertimeHours",
                value: null);

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 55,
                column: "OvertimeHours",
                value: null);

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 56,
                column: "OvertimeHours",
                value: null);

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 57,
                column: "OvertimeHours",
                value: null);

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 58,
                column: "OvertimeHours",
                value: null);

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 59,
                column: "OvertimeHours",
                value: null);

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 60,
                column: "OvertimeHours",
                value: null);

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 61,
                column: "OvertimeHours",
                value: null);

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 62,
                column: "OvertimeHours",
                value: null);

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 63,
                column: "OvertimeHours",
                value: null);

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 64,
                column: "OvertimeHours",
                value: null);

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 65,
                column: "OvertimeHours",
                value: null);

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 66,
                column: "OvertimeHours",
                value: null);

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 67,
                column: "OvertimeHours",
                value: null);

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 68,
                column: "OvertimeHours",
                value: null);

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 69,
                column: "OvertimeHours",
                value: null);

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 70,
                column: "OvertimeHours",
                value: null);

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 71,
                column: "OvertimeHours",
                value: null);

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 72,
                column: "OvertimeHours",
                value: null);

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 73,
                column: "OvertimeHours",
                value: null);

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 74,
                column: "OvertimeHours",
                value: null);

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 75,
                column: "OvertimeHours",
                value: null);

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 76,
                column: "OvertimeHours",
                value: null);

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 77,
                column: "OvertimeHours",
                value: null);

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 78,
                column: "OvertimeHours",
                value: null);

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 79,
                column: "OvertimeHours",
                value: null);

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 80,
                column: "OvertimeHours",
                value: null);

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 81,
                column: "OvertimeHours",
                value: null);

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 82,
                column: "OvertimeHours",
                value: null);

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 83,
                column: "OvertimeHours",
                value: null);

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 84,
                column: "OvertimeHours",
                value: null);

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 85,
                column: "OvertimeHours",
                value: null);

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 86,
                column: "OvertimeHours",
                value: null);

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 87,
                column: "OvertimeHours",
                value: null);

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 88,
                column: "OvertimeHours",
                value: null);

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 89,
                column: "OvertimeHours",
                value: null);

            migrationBuilder.UpdateData(
                table: "AttendanceRecords",
                keyColumn: "Id",
                keyValue: 90,
                column: "OvertimeHours",
                value: null);

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "HousingAllowance", "TransportAllowance" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "HousingAllowance", "TransportAllowance" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "HousingAllowance", "TransportAllowance" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "HousingAllowance", "TransportAllowance" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "HousingAllowance", "TransportAllowance" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "PayrollItems",
                keyColumn: "Id",
                keyValue: 1,
                column: "AttendanceSummary",
                value: "Present: 23, Absent: 0, Half-days: 0");

            migrationBuilder.UpdateData(
                table: "PayrollItems",
                keyColumn: "Id",
                keyValue: 2,
                column: "AttendanceSummary",
                value: "Present: 12, Absent: 10, Half-days: 0");

            migrationBuilder.UpdateData(
                table: "PayrollItems",
                keyColumn: "Id",
                keyValue: 3,
                column: "AttendanceSummary",
                value: "Present: 52, Absent: 0, Half-days: 0");

            migrationBuilder.UpdateData(
                table: "PayrollItems",
                keyColumn: "Id",
                keyValue: 4,
                column: "AttendanceSummary",
                value: "Present: 2, Absent: 0, Half-days: 5");

            migrationBuilder.UpdateData(
                table: "PayrollRuns",
                keyColumn: "Id",
                keyValue: 1,
                column: "RunAt",
                value: new DateTimeOffset(new DateTime(2023, 10, 30, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 5, 0, 0, 0)));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AttendanceSummary",
                table: "PayrollItems");

            migrationBuilder.DropColumn(
                name: "HousingAllowance",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "TransportAllowance",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "OvertimeHours",
                table: "AttendanceRecords");

            migrationBuilder.AlterColumn<DateTime>(
                name: "RunAt",
                table: "PayrollRuns",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset");

            migrationBuilder.UpdateData(
                table: "PayrollRuns",
                keyColumn: "Id",
                keyValue: 1,
                column: "RunAt",
                value: new DateTime(2023, 10, 30, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }
    }
}
