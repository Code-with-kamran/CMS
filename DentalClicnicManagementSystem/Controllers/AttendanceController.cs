using CMS.Data;
using CMS.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering; // For SelectList
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace CMS.Controllers
{
    //[Authorize(Roles = "HR,Admin")]
    public class AttendanceController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AttendanceController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Attendance
        public IActionResult Index()
        {
            return View();
        }

        // GET: Attendance/GetAttendanceRecords
        public async Task<IActionResult> GetAttendanceRecords()
        {
            var draw = HttpContext.Request.Query["draw"].ToString();
            var start = int.Parse(HttpContext.Request.Query["start"].ToString());
            var length = int.Parse(HttpContext.Request.Query["length"].ToString());
            var searchValue = HttpContext.Request.Query["search[value]"].ToString();
            var sortColumnIndex = int.Parse(HttpContext.Request.Query["order[0][column]"].ToString());
            var sortColumnDir = HttpContext.Request.Query["order[0][dir]"].ToString();

            var records = _context.AttendanceRecords
                                  .Include(ar => ar.Employee)
                                  .AsQueryable();

            // Filtering
            if (!string.IsNullOrEmpty(searchValue))
            {
                records = records.Where(ar =>
                    ar.Employee.FirstName.Contains(searchValue) ||
                    ar.Employee.LastName.Contains(searchValue) ||
                    ar.Employee.Code.Contains(searchValue) ||
                    ar.Note.Contains(searchValue) ||
                    ar.Status.ToString().Contains(searchValue));
            }

            // Sorting
            var sortColumn = HttpContext.Request.Query[$"columns[{sortColumnIndex}][data]"].ToString();
            if (!string.IsNullOrEmpty(sortColumn))
            {
                // Handle sorting for nested properties like Employee.FullName
                if (sortColumn == "employeeName") // Use the name from the JSON output
                {
                    records = sortColumnDir == "asc" ?
                        records.OrderBy(ar => ar.Employee.FirstName).ThenBy(ar => ar.Employee.LastName) :
                        records.OrderByDescending(ar => ar.Employee.FirstName).ThenByDescending(ar => ar.Employee.LastName);
                }
                else if (sortColumn == "employeeCode") // Use the name from the JSON output
                {
                    records = sortColumnDir == "asc" ?
                        records.OrderBy(ar => ar.Employee.Code) :
                        records.OrderByDescending(ar => ar.Employee.Code);
                }
                else
                {
                    // Map DataTables column name to model property name
                    var propertyName = sortColumn switch
                    {
                        "date" => "Date",
                        "status" => "Status",
                        "checkIn" => "CheckIn",
                        "checkOut" => "CheckOut",
                        "note" => "Note",
                        _ => null
                    };

                    if (propertyName != null)
                    {
                        records = sortColumnDir == "asc" ?
                            records.OrderBy(ar => EF.Property<object>(ar, propertyName)) :
                            records.OrderByDescending(ar => EF.Property<object>(ar, propertyName));
                    }
                    else
                    {
                        records = records.OrderByDescending(ar => ar.Date); // Fallback
                    }
                }
            }
            else
            {
                records = records.OrderByDescending(ar => ar.Date); // Default sort by date
            }

            var recordsTotal = await records.CountAsync();
            var data = await records.Skip(start).Take(length).Select(ar => new
            {
                ar.Id,
                EmployeeId = ar.Employee.Id,
                EmployeeName = ar.Employee.FullName,
                EmployeeCode = ar.Employee.Code,
                Date = ar.Date.ToString("yyyy-MM-dd"), // Format for JS
                CheckIn = ar.CheckIn.HasValue ? ar.CheckIn.Value.ToString("HH:mm") : "",
                CheckOut = ar.CheckOut.HasValue ? ar.CheckOut.Value.ToString("HH:mm") : "",
                Status = ar.Status.ToString(),
                ar.Note
            }).ToListAsync();

            return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });
        }

        // GET: Attendance/Upsert (Modal content)
        public async Task<IActionResult> Upsert(int? id)
        {
            AttendanceRecord record = new AttendanceRecord();
            if (id != null)
            {
                record = await _context.AttendanceRecords.FindAsync(id);
                if (record == null)
                {
                    return NotFound();
                }
            }

            ViewData["EmployeeList"] = new SelectList(await _context.Employees.Where(e => e.IsActive).ToListAsync(), "Id", "FullName", record.EmployeeId);
            return PartialView("_UpsertPartial", record);
        }

        // POST: Attendance/Upsert
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upsert(AttendanceRecord record)
        {
            if (ModelState.IsValid)
            {
                if (record.Id == 0)
                {
                    // Check for existing record for the same employee on the same date
                    var existingRecord = await _context.AttendanceRecords
                                                       .FirstOrDefaultAsync(ar => ar.EmployeeId == record.EmployeeId && ar.Date.Date == record.Date.Date);
                    if (existingRecord != null)
                    {
                        return Json(new { status = false, message = "An attendance record for this employee on this date already exists. Please edit the existing record." });
                    }

                    _context.AttendanceRecords.Add(record);
                }
                else
                {
                    // Check for existing record for the same employee on the same date, excluding current record
                    var existingRecord = await _context.AttendanceRecords
                                                       .FirstOrDefaultAsync(ar => ar.EmployeeId == record.EmployeeId && ar.Date.Date == record.Date.Date && ar.Id != record.Id);
                    if (existingRecord != null)
                    {
                        return Json(new { status = false, message = "An attendance record for this employee on this date already exists. Please edit the existing record." });
                    }

                    _context.AttendanceRecords.Update(record);
                }
                await _context.SaveChangesAsync();
                return Json(new { status = true, message = "Attendance record saved successfully!" });
            }
            // If model state is not valid, return errors
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            return Json(new { status = false, message = "Validation failed: " + string.Join("; ", errors) });
        }

        // POST: Attendance/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var record = await _context.AttendanceRecords.FindAsync(id);
            if (record == null)
            {
                return Json(new { status = false, message = "Attendance record not found." });
            }

            _context.AttendanceRecords.Remove(record);
            await _context.SaveChangesAsync();

            return Json(new { status = true, message = "Attendance record deleted successfully." });
        }
    }
}