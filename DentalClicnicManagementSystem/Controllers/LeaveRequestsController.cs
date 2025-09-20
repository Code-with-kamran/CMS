
// File: Controllers/LeaveRequestsController.cs
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
    public class LeaveRequestsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public LeaveRequestsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: LeaveRequests
        public IActionResult Index()
        {
            return View();
        }

        // GET: LeaveRequests/GetLeaveRequests
        public async Task<IActionResult> GetLeaveRequests()
        {
            var draw = HttpContext.Request.Query["draw"].ToString();
            var start = int.Parse(HttpContext.Request.Query["start"].ToString());
            var length = int.Parse(HttpContext.Request.Query["length"].ToString());
            var searchValue = HttpContext.Request.Query["search[value]"].ToString();
            var sortColumnIndex = int.Parse(HttpContext.Request.Query["order[0][column]"].ToString());
            var sortColumnDir = HttpContext.Request.Query["order[0][dir]"].ToString();

            var requests = _context.LeaveRequests
                                   .Include(lr => lr.Employee)
                                   .AsQueryable();

            // Filtering
            if (!string.IsNullOrEmpty(searchValue))
            {
                requests = requests.Where(lr =>
                    lr.Employee.FirstName.Contains(searchValue) ||
                    lr.Employee.LastName.Contains(searchValue) ||
                    lr.Employee.Code.Contains(searchValue) ||
                    lr.Reason.Contains(searchValue) ||
                    lr.Type.ToString().Contains(searchValue) ||
                    lr.Status.ToString().Contains(searchValue));
            }

            // Sorting
            var sortColumn = HttpContext.Request.Query[$"columns[{sortColumnIndex}][data]"].ToString();
            if (!string.IsNullOrEmpty(sortColumn))
            {
                if (sortColumn == "employeeName")
                {
                    requests = sortColumnDir == "asc" ?
                        requests.OrderBy(lr => lr.Employee.FirstName).ThenBy(lr => lr.Employee.LastName) :
                        requests.OrderByDescending(lr => lr.Employee.FirstName).ThenByDescending(lr => lr.Employee.LastName);
                }
                else if (sortColumn == "employeeCode")
                {
                    requests = sortColumnDir == "asc" ?
                        requests.OrderBy(lr => lr.Employee.Code) :
                        requests.OrderByDescending(lr => lr.Employee.Code);
                }
                else
                {
                    var propertyName = sortColumn switch
                    {
                        "type" => "Type",
                        "startDate" => "StartDate",
                        "endDate" => "EndDate",
                        "days" => "Days",
                        "reason" => "Reason",
                        "status" => "Status",
                        "decisionBy" => "DecisionBy",
                        "decisionAt" => "DecisionAt",
                        _ => null
                    };

                    if (propertyName != null)
                    {
                        requests = sortColumnDir == "asc" ?
                            requests.OrderBy(lr => EF.Property<object>(lr, propertyName)) :
                            requests.OrderByDescending(lr => EF.Property<object>(lr, propertyName));
                    }
                    else
                    {
                        requests = requests.OrderByDescending(lr => lr.StartDate); // Fallback
                    }
                }
            }
            else
            {
                requests = requests.OrderByDescending(lr => lr.StartDate); // Default sort by start date
            }

            var recordsTotal = await requests.CountAsync();
            var data = await requests.Skip(start).Take(length).Select(lr => new
            {
                lr.Id,
                EmployeeId = lr.Employee.Id,
                EmployeeName = lr.Employee.FullName,
                EmployeeCode = lr.Employee.Code,
                Type = lr.Type.ToString(),
                StartDate = lr.StartDate.ToString("yyyy-MM-dd"),
                EndDate = lr.EndDate.ToString("yyyy-MM-dd"),
                lr.Days,
                lr.Reason,
                Status = lr.Status.ToString(),
                lr.DecisionBy,
                DecisionAt = lr.DecisionAt.HasValue ? lr.DecisionAt.Value.ToString("yyyy-MM-dd HH:mm") : ""
            }).ToListAsync();

            return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });
        }

        // GET: LeaveRequests/Create (Modal content)
        public async Task<IActionResult> Create()
        {
            ViewData["EmployeeList"] = new SelectList(await _context.Employees.Where(e => e.IsActive).ToListAsync(), "Id", "FullName");
            return PartialView("_CreatePartial", new LeaveRequest());
        }

        // POST: LeaveRequests/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(LeaveRequest request)
        {
            if (ModelState.IsValid)
            {
                // Calculate days if not provided or incorrect
                if (request.StartDate > request.EndDate)
                {
                    return Json(new { status = false, message = "Start Date cannot be after End Date." });
                }
                request.Days = (int)((request.EndDate - request.StartDate).TotalDays + 1); // Inclusive of start and end date

                _context.LeaveRequests.Add(request);
                await _context.SaveChangesAsync();
                return Json(new { status = true, message = "Leave request submitted successfully!" });
            }
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            return Json(new { status = false, message = "Validation failed: " + string.Join("; ", errors) });
        }

        // POST: LeaveRequests/Approve/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int id)
        {
            var request = await _context.LeaveRequests.Include(lr => lr.Employee).FirstOrDefaultAsync(lr => lr.Id == id);
            if (request == null)
            {
                return Json(new { status = false, message = "Leave request not found." });
            }

            if (request.Status == LeaveStatus.Approved)
            {
                return Json(new { status = false, message = "Leave request is already approved." });
            }

            request.Status = LeaveStatus.Approved;
            request.DecisionBy = User.Identity.Name ?? "System"; // Get current user or default
            request.DecisionAt = DateTime.Now;

            // Deduct leave balance
            if (request.Employee != null)
            {
                request.Employee.LeaveBalance -= (int)request.Days; // Assuming integer days for balance
                if (request.Employee.LeaveBalance < 0) request.Employee.LeaveBalance = 0; // Prevent negative balance
                _context.Employees.Update(request.Employee);
            }

            _context.LeaveRequests.Update(request);
            await _context.SaveChangesAsync();

            return Json(new { status = true, message = "Leave request approved successfully!" });
        }

        // POST: LeaveRequests/Reject/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(int id)
        {
            var request = await _context.LeaveRequests.Include(lr => lr.Employee).FirstOrDefaultAsync(lr => lr.Id == id);
            if (request == null)
            {
                return Json(new { status = false, message = "Leave request not found." });
            }

            if (request.Status == LeaveStatus.Rejected)
            {
                return Json(new { status = false, message = "Leave request is already rejected." });
            }

            // If it was previously approved, restore leave balance
            if (request.Status == LeaveStatus.Approved && request.Employee != null)
            {
                request.Employee.LeaveBalance += (int)request.Days;
                _context.Employees.Update(request.Employee);
            }

            request.Status = LeaveStatus.Rejected;
            request.DecisionBy = User.Identity.Name ?? "System"; // Get current user or default
            request.DecisionAt = DateTime.Now;

            _context.LeaveRequests.Update(request);
            await _context.SaveChangesAsync();

            return Json(new { status = true, message = "Leave request rejected successfully!" });
        }
    }
}

