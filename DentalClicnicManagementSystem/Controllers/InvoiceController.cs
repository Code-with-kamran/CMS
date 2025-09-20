using CMS.Data;
using CMS.Extensions;
using CMS.Models;
using CMS.Services;
using CMS.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace CMS.Controllers
{
    public class InvoiceController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly ICurrencyService _currencyService;

        public InvoiceController(ApplicationDbContext db, ICurrencyService currencyService)
        {
            _db = db;
            _currencyService = currencyService;
        }

        // GET: Invoice
        public IActionResult Index()
        {
            return View();
        }



        // GET: Invoice/Upsert/{id?}
        //public async Task<IActionResult> Upsert(int? id)
        //{
        //    var viewModel = new InvoiceUpsertViewModel
        //    {
        //        InventoryItems = await _db.InventoryItems
        //                                  .Where(i => i.IsActive)
        //                                  .ToListAsync(),
        //        Invoice = new Invoice() // ✅ ALWAYS instantiate it first
        //    };

        //    if (id.HasValue && id > 0)
        //    {
        //        var invoiceFromDb = await _db.Invoices
        //            .Include(i => i.Items)
        //            .FirstOrDefaultAsync(i => i.Id == id.Value);

        //        if (invoiceFromDb == null)
        //        {
        //            TempData["error"] = "Invoice not found.";
        //            return RedirectToAction(nameof(Index));
        //        }

        //        viewModel.Invoice = invoiceFromDb;

        //        // Set IsAppointmentInvoice flag based on the invoice type (auto-generated or manual)
        //        viewModel.IsAppointmentInvoice = invoiceFromDb.IsAppointmentInvoice;
        //    }
        //    else
        //    {
        //        // Defaults for new invoice
        //        viewModel.Invoice.IssueDate = DateTime.UtcNow.Date;
        //        viewModel.Invoice.DueDate = DateTime.UtcNow.Date.AddDays(30);
        //        viewModel.Invoice.Status = "Draft";
        //        viewModel.Invoice.CurrencyCode = "PKR";
        //        viewModel.Invoice.ExchangeRate = 1m;
        //        viewModel.Invoice.Items.Add(new InvoiceItem());

        //        // Set IsAppointmentInvoice flag for a new invoice (default to false for manual)
        //        viewModel.IsAppointmentInvoice = false;
        //    }

        //    return View(viewModel);
        //}


        [HttpGet]
        public async Task<IActionResult> GetCurrentCurrency()
        {
            var currency = await _currencyService.GetCurrentCurrencyAsync();
            return Json(new { symbol = currency.Symbol, code = currency.Code, name = currency.Name });
        }

        public async Task<IActionResult> Upsert(int? id)
        {
            var doctors = await _db.Doctors.ToListAsync();

            var viewModel = new InvoiceUpsertViewModel
            {
                InventoryItems = await _db.InventoryItems.Where(i => i.IsActive).ToListAsync(),
                Doctors = doctors.Select(d => new SelectListItem
                {
                    Value = d.Id.ToString(),
                    Text = d.FullName,
                    Selected = false
                }).ToList(),
                // Map each doctor's consultation charges
                ConsultationCharge = doctors
                    .Where(d => d.Id == id)  // Filter by doctor Id
                    .Select(d => d.ConsultationCharge)  // Select the ConsultationCharge
                    .FirstOrDefault(),  // Get the first result, or 0 if not found

                Appointments = await _db.Appointments.Select(a => new SelectListItem
                {
                    Text = a.AppointmentId.ToString(),
                    Value = a.AppointmentId.ToString()
                }).ToListAsync(),
                Invoice = new Invoice()
            };

            if (id.HasValue && id > 0)
            {
                var invoiceFromDb = await _db.Invoices
                    .Include(i => i.Items)
                    .Include(i => i.Doctor)
                    .Include(i => i.Patient)
                    .FirstOrDefaultAsync(i => i.Id == id.Value);

                if (invoiceFromDb == null)
                {
                    TempData["error"] = "Invoice not found.";
                    return RedirectToAction(nameof(Index));
                }

                viewModel.Invoice = invoiceFromDb;
                viewModel.IsAppointmentInvoice = invoiceFromDb.IsAppointmentInvoice;

                // Set the selected doctor
                if (invoiceFromDb.DoctorId.HasValue)
                {
                    var selectedDoctor = viewModel.Doctors.FirstOrDefault(d => d.Value == invoiceFromDb.DoctorId.ToString());
                    if (selectedDoctor != null)
                    {
                        selectedDoctor.Selected = true;
                    }
                }

                // Ensure appointment invoices have the consultation fee item
                if (viewModel.IsAppointmentInvoice && !viewModel.Invoice.Items.Any())
                {
                    // Get the default consultation charge (e.g., "General" service)
                    var defaultCharge = invoiceFromDb.Doctor.ConsultationCharge;

                    viewModel.Invoice.Items.Add(new InvoiceItem
                    {
                        Description = "Consultation Fee",
                        Quantity = 1,
                        UnitPrice = defaultCharge
                    });
                }
            }
            else
            {
                // New invoice setup
                viewModel.Invoice.IssueDate = DateTime.UtcNow.Date;
                viewModel.Invoice.DueDate = DateTime.UtcNow.Date.AddDays(30);
                viewModel.Invoice.Status = "Draft";
                viewModel.Invoice.CurrencyCode = "PKR";
                viewModel.Invoice.ExchangeRate = 1m;
                viewModel.Invoice.Items.Add(new InvoiceItem());
                viewModel.IsAppointmentInvoice = false;
            }

            return View(viewModel);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upsert(Invoice invoice)
        {
            // Remove empty items first
            invoice.Items.RemoveAll(item => string.IsNullOrWhiteSpace(item.Description)
                                          && item.Quantity == 0 && item.UnitPrice == 0);

            if (!ModelState.IsValid)
            {
                var doctors = await _db.Doctors.ToListAsync();
                return View(new InvoiceUpsertViewModel
                {
                    Invoice = invoice,
                    InventoryItems = await _db.InventoryItems.Where(i => i.IsActive).ToListAsync(),
                    Doctors = doctors.Select(d => new SelectListItem { Text = d.FullName, Value = d.Id.ToString() }).ToList(),
                    //ConsultationCharge  = doctors.ToDictionary(d => d.Id, d => d.ConsultationCharge),
                    Appointments = await _db.Appointments.Select(a => new SelectListItem { Text = a.AppointmentId.ToString(), Value = a.AppointmentId.ToString() }).ToListAsync()
                });
            }

            // Calculate totals
            invoice.SubTotal = invoice.Items.Sum(i => i.Quantity * i.UnitPrice);
            invoice.Total = invoice.SubTotal + invoice.Tax - invoice.Discount;

            using var transaction = await _db.Database.BeginTransactionAsync();
            try
            {
                if (invoice.Id == 0)
                {
                    // -------- CREATE --------
                    // For new invoices, determine if it's appointment-based
                    invoice.IsAppointmentInvoice = invoice.AppointmentId != null;

                    foreach (var item in invoice.Items)
                    {
                        if (item.InventoryItemId.HasValue)
                        {
                            var inventoryItem = await _db.InventoryItems.FindAsync(item.InventoryItemId.Value);
                            if (inventoryItem == null || inventoryItem.Stock < item.Quantity)
                            {
                                TempData["error"] = $"Invalid or insufficient stock for item ID {item.InventoryItemId}.";
                                return View(new InvoiceUpsertViewModel
                                {
                                    Invoice = invoice,
                                    InventoryItems = await _db.InventoryItems.Where(i => i.IsActive).ToListAsync()
                                });
                            }
                            inventoryItem.Stock -= item.Quantity;
                        }
                    }

                    _db.Invoices.Add(invoice);
                    TempData["success"] = "Invoice created successfully.";
                }
                else
                {
                    // -------- UPDATE --------
                    var invoiceFromDb = await _db.Invoices
                        .Include(i => i.Items)
                        .FirstOrDefaultAsync(i => i.Id == invoice.Id);

                    if (invoiceFromDb == null)
                    {
                        TempData["error"] = "Invoice not found.";
                        return RedirectToAction(nameof(Index));
                    }

                    // IMPORTANT: Preserve the original IsAppointmentInvoice flag
                    invoice.IsAppointmentInvoice = invoiceFromDb.IsAppointmentInvoice;

                    // Also preserve the original AppointmentId if it was an appointment invoice
                    if (invoiceFromDb.IsAppointmentInvoice)
                    {
                        invoice.AppointmentId = invoiceFromDb.AppointmentId;
                    }

                    // Update the invoice properties
                    _db.Entry(invoiceFromDb).CurrentValues.SetValues(invoice);

                    // Handle Items (your existing logic)
                    foreach (var item in invoice.Items)
                    {
                        if (item.InventoryItemId.HasValue)
                        {
                            var inventoryItem = await _db.InventoryItems.FindAsync(item.InventoryItemId.Value);
                            if (inventoryItem == null) continue;

                            var existingItem = invoiceFromDb.Items.FirstOrDefault(i => i.Id == item.Id);

                            if (existingItem == null)
                            {
                                if (inventoryItem.Stock < item.Quantity)
                                {
                                    TempData["error"] = $"Insufficient stock for item '{inventoryItem.Name}'.";
                                    return View(new InvoiceUpsertViewModel { Invoice = invoice });
                                }

                                inventoryItem.Stock -= item.Quantity;
                                invoiceFromDb.Items.Add(item);
                            }
                            else
                            {
                                var qtyDiff = item.Quantity - existingItem.Quantity;
                                if (qtyDiff > 0 && inventoryItem.Stock < qtyDiff)
                                {
                                    TempData["error"] = $"Insufficient stock for item '{inventoryItem.Name}'.";
                                    return View(new InvoiceUpsertViewModel { Invoice = invoice });
                                }
                                inventoryItem.Stock -= qtyDiff;
                                _db.Entry(existingItem).CurrentValues.SetValues(item);
                            }
                        }
                    }

                    var toRemove = invoiceFromDb.Items.Where(i => !invoice.Items.Any(x => x.Id == i.Id)).ToList();
                    foreach (var item in toRemove)
                    {
                        if (item.InventoryItemId.HasValue)
                        {
                            var inventoryItem = await _db.InventoryItems.FindAsync(item.InventoryItemId.Value);
                            if (inventoryItem != null)
                                inventoryItem.Stock += item.Quantity;
                        }

                        _db.InvoiceItems.Remove(item);
                    }

                    TempData["success"] = "Invoice updated successfully.";
                }

                await _db.SaveChangesAsync();
                await transaction.CommitAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                TempData["error"] = "Error: " + ex.Message;
                return View(new InvoiceUpsertViewModel { Invoice = invoice });
            }
        }


        public async Task<IActionResult> Receipt(int id)
        {
            var invoice = await _db.Invoices
                .Include(i => i.Items)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (invoice == null)
            {
                TempData["error"] = "Invoice not found.";
                return RedirectToAction(nameof(Index));
            }

            return View(invoice);
        }


        [HttpGet]
        public async Task<IActionResult> GetInvoiceSummary()
        {
            // Current totals
            var totalInvoiceAmount = await _db.Invoices.SumAsync(i => i.Total);
            var outstandingAmount = await _db.Invoices
                .Where(i => i.Status.ToLower() == "pending" || i.Status.ToLower() == "overdue")
                .SumAsync(i => i.Total);
            var draftAmount = await _db.Invoices
                .Where(i => i.Status.ToLower() == "draft")
                .SumAsync(i => i.Total);
            var overdueAmount = await _db.Invoices
                .Where(i => i.Status.ToLower() == "overdue")
                .SumAsync(i => i.Total);

            var totalInvoiceCount = await _db.Invoices.CountAsync();

          
            // Calculate month-over-month percent changes
            decimal CalcPercentChange(decimal current, decimal previous)
            {
                if (previous == 0) return 0;
                return ((current - previous) / previous) * 100;
            }

            // Calculate progress bar width % (just example relative to total invoice amount)
            decimal CalcProgressPercent(decimal value, decimal max)
            {
                if (max == 0) return 0;
                var percent = (value / max) * 100;
                return percent > 100 ? 100 : percent;
            }


            // Example previous month data, replace with real past values if available
            decimal prevTotalInvoice = totalInvoiceAmount * 0.75m;
            decimal prevOutstanding = outstandingAmount * 1.05m;
            decimal prevDraft = draftAmount * 0.90m;
            decimal prevOverdue = overdueAmount * 1.10m;

            // Computed changes with fallback zero on NaN
            decimal ChangeTotalInvoice = CalcPercentChange(totalInvoiceAmount, prevTotalInvoice);
            decimal ChangeOutstanding = CalcPercentChange(outstandingAmount, prevOutstanding);
            decimal ChangeDraft = CalcPercentChange(draftAmount, prevDraft);
            decimal ChangeOverdue = CalcPercentChange(overdueAmount, prevOverdue);



            return Json(new
            {
                TotalInvoice = totalInvoiceAmount,
                Outstanding = outstandingAmount,
                Draft = draftAmount,
                TotalOverdue = overdueAmount,
                TotalCount = totalInvoiceCount,

                // Progress bar widths (percent of totalInvoiceAmount)
                ProgressTotalInvoice = CalcProgressPercent(totalInvoiceAmount, totalInvoiceAmount),
                ProgressOutstanding = CalcProgressPercent(outstandingAmount, totalInvoiceAmount),
                ProgressDraft = CalcProgressPercent(draftAmount, totalInvoiceAmount),
                ProgressOverdue = CalcProgressPercent(overdueAmount, totalInvoiceAmount),

                // Percent changes from previous month (round to 2 decimals)
                ChangeTotalInvoice = Math.Round(CalcPercentChange(totalInvoiceAmount, prevTotalInvoice), 2),
                ChangeOutstanding = Math.Round(CalcPercentChange(outstandingAmount, prevOutstanding), 2),
                ChangeDraft = Math.Round(CalcPercentChange(draftAmount, prevDraft), 2),
                ChangeOverdue = Math.Round(CalcPercentChange(overdueAmount, prevOverdue), 2),
            });
        }


        // API: GetInvoicesData for DataTables AJAX
        [HttpGet]
        public async Task<IActionResult> GetInvoicesList()
        {
            var draw = HttpContext.Request.Query["draw"].FirstOrDefault();
            var start = Convert.ToInt32(HttpContext.Request.Query["start"].FirstOrDefault() ?? "0");
            var length = Convert.ToInt32(HttpContext.Request.Query["length"].FirstOrDefault() ?? "10");
            var searchValue = HttpContext.Request.Query["search[value]"].FirstOrDefault()?.ToLower();

            var sortColumn = HttpContext.Request.Query["order[0][column]"].FirstOrDefault();
            var sortColumnDirection = HttpContext.Request.Query["order[0][dir]"].FirstOrDefault();

            var query = _db.Invoices.AsQueryable();

            // Filtering
            //if (!string.IsNullOrEmpty(searchValue))
            //{
            //    query = query.Where(i =>
            //        i.CustomerName.ToLower().Contains(searchValue) ||
            //        i.Status.ToLower().Contains(searchValue) ||
            //        i.CurrencyCode.ToLower().Contains(searchValue) ||
            //        i.IssueDate.ToString().ToLower().Contains(searchValue) || // Allow searching by date string
            //        i.DueDate.ToString().ToLower().Contains(searchValue) ||
            //        i.Total.ToString().ToLower().Contains(searchValue));
            //}
            if (!string.IsNullOrEmpty(searchValue))
            {
                query = query.Where(i =>
                    i.CustomerName.ToLower().Contains(searchValue) ||
                    i.Status.ToLower().Contains(searchValue) ||
                    i.CurrencyCode.ToLower().Contains(searchValue) ||
                    i.Total.ToString().Contains(searchValue)
                );

                // Optional: handle date search
                if (DateTime.TryParse(searchValue, out var parsedDate))
                {
                    query = query.Where(i => i.IssueDate.Date == parsedDate.Date || i.DueDate.Date == parsedDate.Date);
                }
            }

            var recordsTotal = await _db.Invoices.CountAsync();
            var recordsFiltered = await query.CountAsync();

            // Ordering
            if (!string.IsNullOrEmpty(sortColumn) && !string.IsNullOrEmpty(sortColumnDirection))
            {
                var columns = new string[] { "CustomerName", "IssueDate", "DueDate", "Status", "Total", "CurrencyCode" };

                IOrderedQueryable<Invoice>? orderedQuery = null;

                int sortCount = HttpContext.Request.Query.Count(k => k.Key.StartsWith("order[") && k.Key.EndsWith("][column]"));

                for (int i = 0; i < sortCount; i++)
                {
                    var columnIndex = HttpContext.Request.Query[$"order[{i}][column]"].FirstOrDefault();
                    var direction = HttpContext.Request.Query[$"order[{i}][dir]"].FirstOrDefault();

                    if (int.TryParse(columnIndex, out int colIdx) && colIdx < columns.Length)
                    {
                        var orderByColumn = columns[colIdx];
                        bool ascending = direction?.ToLower() == "asc";

                        if (i == 0)
                            orderedQuery = query.OrderByDynamic(orderByColumn, ascending);
                        else if (orderedQuery != null)
                            orderedQuery = orderedQuery.ThenByDynamic(orderByColumn, ascending);
                    }
                }

                if (orderedQuery != null)
                    query = orderedQuery;
            }
            else
            {
                query = query.OrderByDescending(i => i.IssueDate);
            }


            var data = await query
                .Skip(start)
                .Take(length)
                .Select(i => new
                {
                    i.Id,
                    CustomerName = i.CustomerName ?? string.Empty,
                    IssueDate = i.IssueDate.ToString("dd MMM yyyy"), // Format for display
                    DueDate = i.DueDate.ToString("dd MMM yyyy"),     // Format for display
                    i.Status,
                    Total = i.Total.ToString("N2"), // Format as currency
                    CurrencyCode = i.CurrencyCode ?? string.Empty
                })
                .ToListAsync();

            return Json(new
            {
                draw = draw,
                recordsTotal = recordsTotal,
                recordsFiltered = recordsFiltered,
                data = data
            });
        }

        // POST: Invoice/Delete
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var invoice = await _db.Invoices.Include(i => i.Items).FirstOrDefaultAsync(i => i.Id == id);
            if (invoice == null)
                return Json(new { success = false, message = "Invoice not found." });

            _db.InvoiceItems.RemoveRange(invoice.Items); // Delete child items first
            _db.Invoices.Remove(invoice);
            await _db.SaveChangesAsync();

            return Json(new { success = true, message = "Invoice deleted successfully." });
        }


        
        [HttpGet]
        public async Task<IActionResult> GetInventoryItems(string term)
        {
            // term = what user has typed so far
            var matches = await _db.InventoryItems
                .Where(i => i.IsActive && i.Name.Contains(term))
                .Select(i => new {
                    id = i.Id,
                    label = i.Name,
                    unitPrice = i.UnitPrice,
                    stock = i.Stock
                })
                .Take(20)
                .ToListAsync();

            return Json(matches);
        }

        [HttpPost]
        public IActionResult CreateFromAppointment(int appointmentId)
        {
            var appointment = _db.Appointments
                                 .Include(a => a.Patient)
                                 .FirstOrDefault(a => a.AppointmentId == appointmentId);

            if (appointment == null) return NotFound();

            // 🔹 Guard clause: Patient is required
            if (appointment.Patient == null)
                return BadRequest("Patient information is missing for this appointment.");

            // 🔹 Generate invoice
            var invoice = new Invoice
            {
                CustomerName = appointment.Patient.FirstName + " " + appointment.Patient.LastName,
                CustomerEmail = appointment.Patient.Email ?? "N/A",
                CustomerAddress = appointment.Patient.Address ?? "N/A",
                IssueDate = DateTime.Now,
                DueDate = DateTime.Now.AddDays(7), // ensure this is explicitly set
                Status = "Pending",
                CurrencyCode = "PKR",
                Tax = 0,
                Discount = 0,
                Notes = "Auto-generated from appointment",
                Items = new List<InvoiceItem>()
            };

            // 🔹 Add InvoiceItem (ensure list is initialized first)
            var item = new InvoiceItem
            {
                Description = "Appointment Fee",
                Quantity = 1,
                UnitPrice = appointment.Fee,
                InventoryItemId = null // Optional: Set if tied to an inventory item
            };
            invoice.Items.Add(item);

            // 🔹 Compute totals manually to ensure consistency
            invoice.SubTotal = invoice.Items.Sum(i => i.UnitPrice * i.Quantity);
            invoice.Total = invoice.SubTotal + invoice.Tax - invoice.Discount;

            _db.Invoices.Add(invoice);
            _db.SaveChanges();

            return RedirectToAction("Details", "Invoice", new { id = invoice.Id });
        }



    }
}
