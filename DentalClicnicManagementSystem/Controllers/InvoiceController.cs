using CMS.Data;
using CMS.Extensions;
using CMS.Models;
using CMS.Services;
using CMS.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System.Drawing.Printing;
using System.Linq.Expressions;
using System.Security.Claims;

namespace CMS.Controllers
{
    [Authorize(Roles = "Admin,Receptionist")]
    public class InvoiceController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly ICurrencyService _currencyService;
        private readonly ILogger<InvoiceController> _logger;

        public InvoiceController(ApplicationDbContext db, ICurrencyService currencyService, ILogger<InvoiceController> logger)
        {
            _db = db;
            _currencyService = currencyService;
            _logger = logger;
        }

        // GET: /Invoice/
        public IActionResult Index()
        {
            return View();
        }
        /* returns partial view with correct dropdown / textbox */
        public IActionResult GetItemControls(string itemType, int index)
        => itemType switch
        {
            "Treatment" => PartialView("_TreatmentSelect", (index, _db.Treatments.ToList())),
            "Test" => PartialView("_TestSelect", (index, _db.LabTestOrders.ToList())),
            "Medication" => PartialView("_MedicationSelect", (index, _db.Medications.ToList())),
            "Inventory" => PartialView("_InventorySelect", (index, _db.InventoryItems.ToList())),
            "Consultation" => PartialView("_ConsultationInput", index),
            _ => Content("")
        };

        /* AJAX save */
        [HttpPost]
        public async Task<IActionResult> UpsertAjax(InvoiceUpsertViewModel vm)
        {
            if (!ModelState.IsValid)
                return Json(new { ok = false, message = "Validation failed" });

            var invoice = vm.Invoice;

            // remove empty rows
            invoice.Items.RemoveAll(i =>
                !i.InventoryItemId.HasValue && !i.TreatmentId.HasValue && !i.MedicationsId.HasValue &&
                string.IsNullOrWhiteSpace(i.AppointmentWIth) &&
                string.IsNullOrWhiteSpace(i.TreatmentName) &&
                string.IsNullOrWhiteSpace(i.TestName) &&
                string.IsNullOrWhiteSpace(i.MedicationsName));

            // stock deduction for medication / inventory
            foreach (var it in invoice.Items.Where(x => x.ItemType is "Medication" or "Inventory"))
            {
                var inv = await _db.InventoryItems
                                   .FirstOrDefaultAsync(x => x.Id == it.MedicationsId);
                if (inv != null)
                {
                    inv.Stock -= it.Quantity;
                    _db.InventoryItems.Update(inv);
                }
            }

            // totals
            invoice.SubTotal = invoice.Items.Sum(i => i.Quantity * i.UnitPrice);
            invoice.Total = invoice.SubTotal + invoice.Tax - invoice.Discount;
            invoice.AmountDue = invoice.Total - invoice.AmountPaid;

            // persist
            if (invoice.Id == 0) _db.Invoices.Add(invoice);
            else _db.Invoices.Update(invoice);

            await _db.SaveChangesAsync();
            return Json(new { ok = true, redirect = Url.Action("Index") });
        }
      

       

        
        [HttpGet]
        public IActionResult GetInventoryList()
=> Json(_db.InventoryItems
          .Where(i => i.Stock > 0)
          .Select(i => new { id = i.Id, name = i.Name, unitPrice = i.UnitPrice, description = i.Description })
          .ToList());

        [HttpGet]
        public IActionResult GetTreatmentList() =>
    Json(_db.Treatments
            .Where(t => t.IsActive)
            .Select(t => new {
                id = t.TreatmentId,
                name = t.Name,
                unitPrice = t.UnitPrice,
                description = t.Description ?? ""
            })
            .ToList());
        private List<SelectListItem> GetEnumSelectList<T>() where T : Enum
        {
            return Enum.GetValues(typeof(T))
                .Cast<T>()
                .Select(e => new SelectListItem
                {
                    Value = e.ToString(),
                    Text = e.ToString()
                })
                .ToList();
        }
        public IActionResult GetItemNameField(string itemType, int index)
        {
            switch (itemType)
            {
                case "Inventory":
                    var inventoryItems = _db.InventoryItems.ToList();
                    ViewData["FieldName"] = $"Items[{index}].InventoryItemId";
                    ViewData["Placeholder"] = "Select Inventory Item";
                    return PartialView("_ItemNameDropdown", inventoryItems.Cast<dynamic>().ToList());

                case "Test":
                    ViewData["Index"] = index;
                    return PartialView("_ConsultationField");

                case "Treatment":
                    var treatments = _db.Treatments.ToList();
                    ViewData["FieldName"] = $"Items[{index}].TreatmentId";
                    ViewData["Placeholder"] = "Select Treatment";
                    return PartialView("_ItemNameDropdown", treatments.Cast<dynamic>().ToList());

                case "Medication":
                    var medications = _db.Medications.ToList();
                    ViewData["FieldName"] = $"Items[{index}].MedicationsId";
                    ViewData["Placeholder"] = "Select Medication";
                    return PartialView("_ItemNameDropdown", medications.Cast<dynamic>().ToList());

                case "Consultation":
                    ViewData["Index"] = index;
                    return PartialView("_ConsultationField");

                default:
                    return Content("<div class='text-muted'>Select item type first</div>");
            }
        }
        public async Task<IActionResult> Upsert(int? id, string type)
        {
            ViewData["CompanyName"] = "Your Company Name";
            ViewData["CompanyAddress"] = "Your Company Address";
            ViewData["CompanyPhone"] = "Your Company Phone Number";

            var doctors = await _db.Doctors.ToListAsync();
            var appointments = await _db.Appointments
       .Include(a => a.Patient)
       .Where(a => a.Status == "Completed" || a.Status == "Confirmed")
       .Select(a => new SelectListItem
       {
           Value = a.AppointmentId.ToString(),
           Text = $"APT-{a.AppointmentId} - {a.Patient.FirstName} {a.Patient.LastName} - {a.AppointmentDate:MMM dd, yyyy}"
       })
       .ToListAsync();
            var viewModel = new InvoiceUpsertViewModel
            {
                InventoryItems = await _db.InventoryItems.Where(i => i.IsActive).ToListAsync(),
                Doctors = doctors.Select(d => new SelectListItem
                {
                    Value = d.Id.ToString(),
                    Text = d.FullName
                }).ToList(),
                PaymentMethods = await _db.PaymentMethods
                    .Where(p => p.IsActive)
                    .Select(p => new SelectListItem
                    {
                        Value = p.Id.ToString(),
                        Text = p.Name
                    })
                    .ToListAsync(),
                Appointments = appointments,
                InvoiceTypes = GetEnumSelectList<InvoiceType>(),
                ConsultationCharge = doctors.ToDictionary(d => d.Id, d => new Dictionary<string, decimal>
                {
                    ["General"] = d.ConsultationCharge,
                    ["Specialist"] = d.ConsultationCharge * 1.5m,
                    ["Emergency"] = d.ConsultationCharge * 2m
                }),
                Treatments = await GetTreatmentsSelectList(),
                LaboratoryTests = await GetLaboratoryTestsSelectList(),
                Invoice = new Invoice()
            };

            if (id.HasValue && id > 0)
            {
                var invoiceFromDb = await _db.Invoices
                    .Include(i => i.Items)
                    .FirstOrDefaultAsync(i => i.Id == id.Value);

                if (invoiceFromDb == null)
                {
                    TempData["error"] = "Invoice not found.";
                    return RedirectToAction(nameof(Index));
                }

                viewModel.Invoice = invoiceFromDb;
                viewModel.SetInvoiceTypeFlags(); // This sets all the boolean flags

              
            }
            else
            {

                if (!string.IsNullOrEmpty(type) && Enum.TryParse<InvoiceType>(type, out var invoiceType))
                {
                    viewModel.Invoice.InvoiceType = invoiceType;
                }
                else
                {
                    viewModel.Invoice.InvoiceType = InvoiceType.Appointment;
                }

                // Setup for a new invoice
                viewModel.SetInvoiceTypeFlags();


                viewModel.Invoice.IssueDate = DateTime.UtcNow.Date;
                viewModel.Invoice.DueDate = DateTime.UtcNow.Date.AddDays(30);
                viewModel.Invoice.status = "Draft";
                viewModel.Invoice.CurrencyCode = "USD";
                viewModel.Invoice.ExchangeRate = 1m;
                viewModel.Invoice.Items.Add(new InvoiceItem());

            }
            ViewData["ItemTypes"] = new List<SelectListItem>
                {
                    new SelectListItem { Value = "Inventory", Text = "Medication" },
                    
                    new SelectListItem { Value = "Treatment", Text = "Treatment" },
                    
                    new SelectListItem { Value = "Test", Text = "Laboratory Test" },
                    new SelectListItem { Value = "General", Text = "General" }
                };



            return View("Upsert",viewModel);
        }

        // Add helper method for laboratory tests
        private async Task<List<SelectListItem>> GetLaboratoryTestsSelectList()
        {
            return await _db.LaboratoryOrders.AsNoTracking()
                .Where(t => t.Status == OrderStatus.Completed || t.Status == OrderStatus.SampleCollected) // Adjust the condition based on what you want to filter
                .Select(t => new SelectListItem
                {
                    Value = t.Id.ToString(),
                    Text = $"{t.TestName} - ${t.TestPrice:F2}" // Ensure Price is formatted correctly with two decimal places
                })
                .ToListAsync();
        }



        public async Task<IActionResult> ThermalReceipt(int id, int? appointmentId = null)
        {
            if (appointmentId.HasValue)
            {
                var invoices = await _db.Invoices
                    .Include(i => i.Items)
                        .ThenInclude(item => item.Treatment)
                    .Include(i => i.Items)
                        .ThenInclude(item => item.InventoryItem)
                    .Include(i => i.Patient)
                    .Include(i => i.Doctor)
                    .Where(i => i.AppointmentId == appointmentId.Value)
                    .OrderByDescending(i => i.IssueDate)
                    .ToListAsync();

                if (!invoices.Any())
                {
                    TempData["error"] = "No invoices found for this appointment.";
                    return RedirectToAction(nameof(Index));
                }

                // Assign item names dynamically
                foreach (var invoice in invoices)
                {
                    foreach (var item in invoice.Items)
                    {
                        if (item.ItemType == "Test")
                        {
                            item.MedicationsName = _db.LabTestOrders
                                .FirstOrDefault(t => t.TestName == item.TestName)?.TestName ?? "Test Name Not Available";
                        }
                        else if (item.ItemType == "Treatment")
                        {
                            item.MedicationsName = _db.Treatments
                                .FirstOrDefault(t => t.Name == item.TreatmentName)?.Name ?? "Treatment Name Not Available";
                        }
                        else if (item.ItemType == "Medication")
                        {
                            item.MedicationsName = _db.Medications
                                .FirstOrDefault(m => m.Name == item.MedicationsName)?.Name ?? "Medication Name Not Available";
                        }

                        item.ItemDisplayName = item.DisplayName;
                    }
                }

                return View("ThermalReceipt", invoices);
            }
            else
            {
                var invoice = await _db.Invoices
                    .Include(i => i.Items)
                        .ThenInclude(item => item.Treatment)
                    .Include(i => i.Items)
                        .ThenInclude(item => item.InventoryItem)
                    .Include(i => i.Patient)
                    .Include(i => i.Doctor)
                    .FirstOrDefaultAsync(i => i.Id == id);

                if (invoice == null)
                {
                    TempData["error"] = "Invoice not found.";
                    return RedirectToAction(nameof(Index));
                }

                // Assign item names dynamically
                foreach (var item in invoice.Items)
                {
                    if (item.ItemType == "Test")
                    {
                        item.MedicationsName = _db.LabTestOrders
                            .FirstOrDefault(t => t.TestName == item.TestName)?.TestName ?? "Test Name Not Available";
                    }
                    else if (item.ItemType == "Treatment")
                    {
                        item.MedicationsName = _db.Treatments
                            .FirstOrDefault(t => t.Name == item.TreatmentName)?.Name ?? "Treatment Name Not Available";
                    }
                    else if (item.ItemType == "Medication")
                    {
                        item.MedicationsName = _db.Medications
                            .FirstOrDefault(m => m.Name == item.MedicationsName)?.Name ?? "Medication Name Not Available";
                    }

                    item.ItemDisplayName = item.DisplayName;
                }

                return View("ThermalReceipt", invoice);
            }
        }
        private void RemoveModelStateErrorsFor(params string[] propertyNames)
        {
            foreach (var key in ModelState.Keys.Where(k =>
                         propertyNames.Any(p => k.EndsWith(p, StringComparison.OrdinalIgnoreCase))))
            {
                ModelState.Remove(key);
            }
        }

        // POST: /Invoice/Upsert
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upsert(Invoice invoice)
        {
            try
            {
                if (!Request.Headers["X-Requested-With"].Contains("XMLHttpRequest"))
                    return BadRequest("This form must be submitted via AJAX.");

                // Your existing validation logic...
                TryValidateModel(invoice);

                // Remove unnecessary model state entries
                foreach (var k in ModelState.Keys.ToList())
                    if (k.EndsWith(".Invoice"))
                        ModelState.Remove(k);

                ModelState.Remove("InvoiceNumber");
                ModelState.Remove("InvoiceType");

                invoice.Items ??= new List<InvoiceItem>();
                invoice.Items.RemoveAll(item =>
                    !item.InventoryItemId.HasValue &&
                    !item.TreatmentId.HasValue &&
                    !item.MedicationsId.HasValue &&
                    string.IsNullOrWhiteSpace(item.Description) &&
                    item.Quantity == 0 &&
                    item.UnitPrice == 0
                );

                var currentUser = User?.FindFirstValue(ClaimTypes.Name) ?? "System";
                invoice.UpdatedBy = currentUser;

                if (invoice.Id == 0)
                    invoice.CreatedBy = currentUser;

                // Clear the old validation errors
                RemoveModelStateErrorsFor(nameof(Invoice.CreatedBy), nameof(Invoice.UpdatedBy), nameof(Invoice.InvoiceType));

                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();

                    return Json(new { success = false, message = "Validation failed", errors });
                }

                // Calculate totals
                invoice.SubTotal = invoice.Items.Sum(i => i.Quantity * i.UnitPrice);
                invoice.Total = invoice.SubTotal + invoice.Tax - invoice.Discount;
                invoice.AmountDue = invoice.Total - invoice.AmountPaid;

                // Transactional save
                await using var transaction = await _db.Database.BeginTransactionAsync();
                try
                {
                    if (invoice.Id == 0)
                        await CreateInvoiceAsync(invoice);
                    else
                        await UpdateInvoiceAsync(invoice);

                    await _db.SaveChangesAsync();

                    // ✅ ADD AUTO-PAYMENT CREATION HERE (inside transaction)
                    if (invoice.PaymentStatus == PaymentStatus.Paid && invoice.AmountPaid > 0)
                    {
                        // Auto-create payment record
                        var payment = new Payment
                        {
                            InvoiceId = invoice.Id,
                            Total = invoice.AmountPaid,
                            PaymentDate = DateTime.Now,
                            Status = PaymentStatus.Completed,
                            PaymentMethodId = (int)invoice.PaymentMethodId,
                            Notes = "Auto-created from invoice payment",
                            CreatedBy = User.Identity?.Name ?? "System"
                        };
                        _db.Payments.Add(payment);
                        await _db.SaveChangesAsync(); // Second save for payment
                    }

                    await transaction.CommitAsync();

                    return Json(new
                    {
                        success = true,
                        message = invoice.Id == 0 ? "Invoice created successfully" : "Invoice updated successfully",
                        redirectUrl = Url.Action(nameof(Index)),
                        invoiceId = invoice.Id
                    });
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Error saving invoice (ID: {InvoiceId})", invoice.Id);
                    return Json(new { success = false, message = "An unexpected error occurred: " + ex.Message });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "An unexpected error occurred: " + ex.Message });
            }
        } //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Upsert(Invoice invoice)
        //{
        //    try {

        //        TryValidateModel(invoice);
        //        foreach (var k in ModelState.Keys.ToList())
        //            if (k.EndsWith(".Invoice"))
        //                ModelState.Remove(k);

        //        ModelState.Remove("InvoiceNumber");
        //        ModelState.Remove("InvoiceType");





        //        invoice.Items ??= new List<InvoiceItem>();
        //        invoice.Items.RemoveAll(item =>
        //            !item.InventoryItemId.HasValue &&
        //            !item.TreatmentId.HasValue &&
        //            !item.MedicationsId.HasValue &&
        //            string.IsNullOrWhiteSpace(item.Description) &&
        //            item.Quantity == 0 &&
        //            item.UnitPrice == 0
        //        );
        //        var currentUser = User?.FindFirstValue(ClaimTypes.Name) ?? "System";
        //        invoice.UpdatedBy = currentUser;

        //        if (invoice.Id == 0) // new invoice
        //            invoice.CreatedBy = currentUser;

        //        // Clear the old validation errors and re-validate
        //        RemoveModelStateErrorsFor(nameof(Invoice.CreatedBy), nameof(Invoice.UpdatedBy),nameof(Invoice.InvoiceType));



        //        if (!ModelState.IsValid)
        //        {
        //            return await ReturnUpsertViewWithError(invoice);
        //        }

        //        // ✅ 3. Calculate totals
        //        invoice.SubTotal = invoice.Items.Sum(i => i.Quantity * i.UnitPrice);
        //        invoice.Total = invoice.SubTotal + invoice.Tax - invoice.Discount;
        //        invoice.AmountDue = invoice.Total - invoice.AmountPaid;

        //        // ✅ 4. Transactional save
        //        await using var transaction = await _db.Database.BeginTransactionAsync();
        //        try
        //        {
        //            if (invoice.Id == 0)
        //                await CreateInvoiceAsync(invoice);
        //            else
        //                await UpdateInvoiceAsync(invoice);

        //            await _db.SaveChangesAsync();
        //            await transaction.CommitAsync();
        //            return RedirectToAction(nameof(Index));
        //        }
        //        catch (Exception ex)
        //        {
        //            await transaction.RollbackAsync();
        //            _logger.LogError(ex, "Error saving invoice (ID: {InvoiceId})", invoice.Id);
        //            TempData["error"] = "An unexpected error occurred: " + ex.Message;
        //            return await ReturnUpsertViewWithError(invoice);
        //        }
        //    }catch(Exception ex)
        //    {
        //        TempData["error"] = "An unexpected error occurred: " + ex.Message;
        //        return await ReturnUpsertViewWithError(invoice);
        //    }

        //    }

        // ✅ Helper: always repopulates PaymentMethods for the view

        #region Invoice Generation Actions

        // GET: /Invoice/CreateAppointmentInvoice/5
        [HttpGet]
        public async Task<IActionResult> CreateAppointmentInvoice(int appointmentId)
        {
            return await CreateInvoiceFromContext(appointmentId, InvoiceType.Appointment);
        }

        // GET: /Invoice/CreateTreatmentInvoice/5
        [HttpGet]
        public async Task<IActionResult> CreateTreatmentInvoice(int appointmentId)
        {
            return await CreateInvoiceFromContext(appointmentId, InvoiceType.Treatment);
        }

        // GET: /Invoice/CreateCombinedInvoice/5
        [HttpGet]
        public async Task<IActionResult> CreateCombinedInvoice(int appointmentId)
        {
            return await CreateInvoiceFromContext(appointmentId, InvoiceType.Combined);
        }

        // POST: /Invoice/AutoGenerateAppointmentInvoice
        [HttpPost]
        public async Task<IActionResult> AutoGenerateAppointmentInvoice(int appointmentId)
        {
            return await AutoGenerateInvoiceFromContext(appointmentId, InvoiceType.Appointment, "APT", "Consultation Fee", a => a.Doctor.ConsultationCharge);
        }

        // POST: /Invoice/GenerateLaboratoryInvoice
        [HttpPost]
        public async Task<IActionResult> GenerateLaboratoryInvoice(int laboratoryOrderId)
        {
            try
            {
                var labOrder = await _db.LaboratoryOrders.Include(l => l.Patient).FirstOrDefaultAsync(l => l.Id == laboratoryOrderId);
                if (labOrder == null) return Json(new { success = false, message = "Lab order not found." });

                if (await _db.Invoices.AnyAsync(i => i.LaboratoryOrderId == laboratoryOrderId))
                    return Json(new { success = false, message = "An invoice for this lab order already exists." });

                var invoice = new Invoice
                {
                    InvoiceNumber = GenerateInvoiceNumber("LAB"),
                    InvoiceType = InvoiceType.Laboratory,
                    LaboratoryOrderId = laboratoryOrderId,
                    PatientId = labOrder.PatientId,
                    CustomerName = $"{labOrder.Patient.FirstName} {labOrder.Patient.LastName}",
                    CustomerEmail = labOrder.Patient.Email ?? "",
                    CustomerAddress = labOrder.Patient.Address ?? "",
                    IssueDate = DateTime.Now,
                    DueDate = DateTime.Now.AddDays(7),
                    status = "Pending",
                    //PaymentStatus = PaymentStatus.Pending,
                    CreatedBy = User.Identity?.Name ?? "System",
                    Items = new List<InvoiceItem> { new InvoiceItem { Description = labOrder.TestName, Quantity = 1, UnitPrice = labOrder.TestPrice } }
                };
                CalculateInvoiceTotals(invoice);
                _db.Invoices.Add(invoice);
                await _db.SaveChangesAsync();
                return Json(new { success = true, invoiceId = invoice.Id, message = "Lab test invoice generated successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating lab invoice for Order ID {LabOrderId}", laboratoryOrderId);
                return Json(new { success = false, message = "An error occurred while generating the lab invoice." });
            }
        }

        #endregion

        #region Payment and Receipt Actions

        // POST: /Invoice/RecordPayment
        [HttpPost]
        public async Task<IActionResult> RecordPayment([FromBody] PaymentRequest request)
        {
            try
            {
                var invoice = await _db.Invoices.FindAsync(request.InvoiceId);
                if (invoice == null) return Json(new { success = false, message = "Invoice not found." });

                // Validate payment method exists
                var paymentMethod = await _db.PaymentMethods.FindAsync(request.PaymentMethodId);
                if (paymentMethod == null) return Json(new { success = false, message = "Invalid payment method." });

                var invoiceWithPatient = await _db.Invoices
        .Include(i => i.Patient)
        .FirstOrDefaultAsync(i => i.Id == invoice.Id);
                // Create payment record
                var payment = new Payment
                {
                    InvoiceId = request.InvoiceId,
                    PatientId = invoiceWithPatient.PatientId.Value,
                    Total = request.Amount,
                    PaymentDate = DateTime.Now,
                    Status = PaymentStatus.Completed,
                    PaymentMethodId = request.PaymentMethodId, // Use ID only
                    Notes = request.Notes,
                    CreatedBy = User.Identity?.Name ?? "System"
                };

                _db.Payments.Add(payment);

                // Update invoice
                invoice.AmountPaid += request.Amount;
                invoice.AmountDue = invoice.Total - invoice.AmountPaid;
                invoice.PaymentStatus = invoice.AmountDue <= 0 ? PaymentStatus.Paid : PaymentStatus.PartiallyPaid;
                invoice.status = invoice.PaymentStatus.ToString();

                await _db.SaveChangesAsync();

                return Json(new { success = true, message = "Payment recorded successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error recording payment");
                return Json(new { success = false, message = "Error recording payment." });
            }
        }
        // GET: /Invoice/GetInvoiceReceipt/5
        [HttpGet]
        public async Task<IActionResult> GetInvoiceReceipt(int id)
        {
            var invoice = await _db.Invoices
                .AsNoTracking()
                .Include(i => i.Patient)
                .Include(i => i.Doctor)
                .Include(i => i.Items)
                .Include(i => i.Payments)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (invoice == null) return Json(new { success = false, message = "Invoice not found." });

            return Json(new { success = true, data = invoice });
        }

        #endregion

        #region API and Data Actions



       
        public async Task<IActionResult> GetInvoicesList()
        {
            try
            {
                var draw = Request.Query["draw"].FirstOrDefault();
                var start = int.Parse(Request.Query["start"].FirstOrDefault() ?? "0");
                var length = int.Parse(Request.Query["length"].FirstOrDefault() ?? "10");
                var searchValue = Request.Query["search[value]"].FirstOrDefault()?.ToLower() ?? "";

                var query = _db.Invoices.AsNoTracking();
                var recordsTotal = await query.CountAsync();
                var aptNo = Request.Query["appointmentId"].FirstOrDefault()?.Trim();

                /* ----------  single search box understands both formats  ---------- */
                if (!string.IsNullOrWhiteSpace(searchValue))
                {
                    var searchNumeric = decimal.TryParse(searchValue, out var n) ? n : (decimal?)null;
                    var searchInt = int.TryParse(searchValue, out var k) ? k : (int?)null;   // ← add this

                    query = query.Where(i =>
                        (i.CustomerName ?? "").ToLower().Contains(searchValue) ||
                        i.status.ToLower().Contains(searchValue) ||
                        ((int)i.InvoiceType).ToString() == searchValue ||
                        (searchNumeric.HasValue && i.Total == searchNumeric) ||
                        (i.InvoiceNumber ?? "").ToLower().Contains(searchValue) ||
                        (searchInt.HasValue && i.AppointmentId == searchInt) // ← raw id match
                    );
                }

                var recordsFiltered = await query.CountAsync();

                var data = await query
                                  .OrderByDescending(i => i.CreatedDate)
                                  .Skip(start).Take(length)
                                  .Select(i => new
                                  {
                                      id = i.Id,
                                      invoiceNumber = i.InvoiceNumber,
                                      customerName = i.CustomerName,
                                      invoiceType = i.InvoiceType.ToString(),
                                      issueDate = i.IssueDate.ToString("dd MMM yyyy"),
                                      dueDate = i.DueDate.ToString("dd MMM yyyy"),
                                      status = i.PaymentStatus.ToString(),
                                      total = i.Total,          // send numeric – let JS format
                                      currencyCode = i.CurrencyCode,
                                      appointmentId = i.AppointmentId
                                  })
                                  .ToListAsync();

                return Json(new { draw = draw, recordsFiltered, recordsTotal, data });
            }
            catch (Exception ex)
            {
                // **Log the real exception** – you’ll see it in VS output / file
                _logger.LogError(ex, "GetInvoicesList failed during search");
                // Return valid DataTables payload so grid does not crash
                return Json(new { draw = Request.Query["draw"].FirstOrDefault(), recordsFiltered = 0, recordsTotal = 0, data = Array.Empty<object>() });
            }
        }
        

        [HttpGet]
        public async Task<IActionResult> GetInvoiceSummary()
        {
            var now = DateTime.UtcNow;
            var today = now.Date;
            var yesterday = today.AddDays(-1);
            var thisMonthStart = new DateTime(now.Year, now.Month, 1);
            var lastMonthStart = thisMonthStart.AddMonths(-1);
            var lastMonthEnd = thisMonthStart.AddDays(-1);

            try
            {
                // Today's invoice count
                var todayCount = await _db.Invoices
                    .Where(i => i.IssueDate.Date == today)
                    .CountAsync();

                // Yesterday's invoice count for comparison
                var yesterdayCount = await _db.Invoices
                    .Where(i => i.IssueDate.Date == yesterday)
                    .CountAsync();

                // This month invoice count (for "Total Invoice" card)
                var thisMonthCount = await _db.Invoices
                    //.Where(i => i.IssueDate >= thisMonthStart)
                    .CountAsync();

                // Last month invoice count for percentage calculation
                var lastMonthCount = await _db.Invoices
                    .Where(i => i.IssueDate >= lastMonthStart && i.IssueDate < thisMonthStart)
                    .CountAsync();

                // Outstanding invoice count (current)
                var currentOutstandingCount = await _db.Invoices
                    .Where(i => i.PaymentStatus == PaymentStatus.Pending
                        || i.PaymentStatus == PaymentStatus.PartiallyPaid
                        || i.PaymentStatus == PaymentStatus.Overdue)
                    .CountAsync();

                // Outstanding invoice count (last month)
                var lastMonthOutstandingCount = await _db.Invoices
                    .Where(i => (i.PaymentStatus == PaymentStatus.Pending
                        || i.PaymentStatus == PaymentStatus.PartiallyPaid
                        || i.PaymentStatus == PaymentStatus.Overdue)
                        && i.IssueDate >= lastMonthStart && i.IssueDate < thisMonthStart)
                    .CountAsync();

                // Draft invoice count (current)
                var currentDraftCount = await _db.Invoices
                    .Where(i => i.status == "Draft")
                    .CountAsync();

                // Draft invoice count (last month)
                var lastMonthDraftCount = await _db.Invoices
                    .Where(i => i.status == "Draft" && i.IssueDate >= lastMonthStart && i.IssueDate < thisMonthStart)
                    .CountAsync();

                // Overdue invoice count (current)
                var currentOverdueCount = await _db.Invoices
                    .Where(i => i.PaymentStatus == PaymentStatus.Overdue)
                    .CountAsync();

                // Overdue invoice count (last month)
                var lastMonthOverdueCount = await _db.Invoices
                    .Where(i => i.PaymentStatus == PaymentStatus.Overdue && i.IssueDate >= lastMonthStart && i.IssueDate < thisMonthStart)
                    .CountAsync();

                // Calculate percentage changes
                decimal CalculatePercentChange(int current, int previous)
                {
                    if (previous == 0) return 0;
                    return ((current - previous) / (decimal)previous) * 100m;
                }

                var percentChangeToday = CalculatePercentChange(todayCount, yesterdayCount);
                var percentChangeTotalInvoice = CalculatePercentChange(thisMonthCount, lastMonthCount);
                var percentChangeOutstanding = CalculatePercentChange(currentOutstandingCount, lastMonthOutstandingCount);
                var percentChangeDraft = CalculatePercentChange(currentDraftCount, lastMonthDraftCount);
                var percentChangeOverdue = CalculatePercentChange(currentOverdueCount, lastMonthOverdueCount);

                return Json(new
                {
                    // Card data according to headings - now returning COUNTS instead of amounts
                    TodayInvoice = new
                    {
                        Count = todayCount,
                        PercentChange = percentChangeToday,
                        ComparisonText = "from yesterday"
                    },
                    TotalInvoice = new
                    {
                        Count = thisMonthCount,
                        PercentChange = percentChangeTotalInvoice,
                        ComparisonText = "from last month"
                    },
                    Outstanding = new
                    {
                        Count = currentOutstandingCount,
                        PercentChange = percentChangeOutstanding,
                        ComparisonText = "from last month"
                    },
                    Draft = new
                    {
                        Count = currentDraftCount,
                        PercentChange = percentChangeDraft,
                        ComparisonText = "from last month"
                    },
                    TotalOverdue = new
                    {
                        Count = currentOverdueCount,
                        PercentChange = percentChangeOverdue,
                        ComparisonText = "from last month"
                    }
                });
            }
            catch (Exception ex)
            {
                // Log the exception
                return Json(new
                {
                    TodayInvoice = new { Count = 0, PercentChange = 0m, ComparisonText = "from yesterday" },
                    TotalInvoice = new { Count = 0, PercentChange = 0m, ComparisonText = "from last month" },
                    Outstanding = new { Count = 0, PercentChange = 0m, ComparisonText = "from last month" },
                    Draft = new { Count = 0, PercentChange = 0m, ComparisonText = "from last month" },
                    TotalOverdue = new { Count = 0, PercentChange = 0m, ComparisonText = "from last month" }
                });
            }
        }
        [HttpGet]
        public async Task<IActionResult> GetCurrentCurrency()
        {
            var currency = await _currencyService.GetCurrentCurrencyAsync();
            return Json(new { symbol = currency.Symbol, code = currency.Code, name = currency.Name });
        }

        #endregion

        // POST: /Invoice/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var invoice = await _db.Invoices.FindAsync(id);
                if (invoice == null) return Json(new { success = false, message = "Invoice not found." });

                _db.Invoices.Remove(invoice);
                await _db.SaveChangesAsync();
                return Json(new { success = true, message = "Invoice deleted successfully." });
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Error deleting invoice ID {InvoiceId} due to database constraints.", id);
                return Json(new { success = false, message = "Cannot delete this invoice as it is linked to other records (like payments)." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting invoice ID {InvoiceId}", id);
                return Json(new { success = false, message = "An error occurred while deleting the invoice." });
            }
        }

        #region Private Helper Methods

        private async Task<IActionResult> CreateInvoiceFromContext(int appointmentId, InvoiceType type)
        {
            var appointment = await _db.Appointments.Include(a => a.Patient).Include(a => a.Doctor).FirstOrDefaultAsync(a => a.AppointmentId == appointmentId);
            if (appointment == null) return NotFound();

            var doctors = await _db.Doctors.ToListAsync();
            var appointments = await _db.Appointments
                .Include(a => a.Patient)
                .Where(a => a.Status == "Completed" || a.Status == "Confirmed")
                .Select(a => new SelectListItem
                {
                    Value = a.AppointmentId.ToString(),
                    Text = $"APT-{a.AppointmentId} - {a.Patient.FirstName} {a.Patient.LastName} - {a.AppointmentDate:MMM dd, yyyy}"
                })
                .ToListAsync();

            var viewModel = new InvoiceUpsertViewModel
            {
                Invoice = new Invoice
                {
                    InvoiceType = type,
                    AppointmentId = appointmentId,
                    PatientId = appointment.PatientId,
                    DoctorId = appointment.DoctorId,
                    CustomerName = $"{appointment.Patient.FirstName} {appointment.Patient.LastName}",
                    CustomerEmail = appointment.Patient.Email ?? "",
                    CustomerAddress = appointment.Patient.Address ?? "",
                    IssueDate = DateTime.Now,
                    DueDate = DateTime.Now.AddDays(30),
                    status = "Pending"
                },
                IsAppointmentInvoice = (type == InvoiceType.Appointment),
                IsCombinedInvoice = (type == InvoiceType.Combined),
                Doctors = doctors.Select(d => new SelectListItem { Value = d.Id.ToString(), Text = d.FullName, Selected = d.Id == appointment.DoctorId }).ToList(),
                Appointments = appointments,
                InvoiceTypes = GetEnumSelectList<InvoiceType>(),
                Treatments = await GetTreatmentsSelectList(),
                ConsultationCharge = doctors.ToDictionary(d => d.Id, d => new Dictionary<string, decimal> { ["General"] = d.ConsultationCharge, ["Specialist"] = d.ConsultationCharge * 1.5m, ["Emergency"] = d.ConsultationCharge * 2m }),
                InventoryItems = await _db.InventoryItems.Where(i => i.IsActive).ToListAsync(),
                PaymentMethods = await _db.PaymentMethods
                    .Where(p => p.IsActive)
                    .Select(p => new SelectListItem
                    {
                        Value = p.Id.ToString(),
                        Text = p.Name
                    })
                    .ToListAsync()
            };

            if (type == InvoiceType.Appointment || type == InvoiceType.Combined)
            {
                viewModel.Invoice.Items.Add(new InvoiceItem { Description = "Consultation Fee", Quantity = 1, UnitPrice = appointment.Doctor?.ConsultationCharge ?? 0 });
            }

            return View("Upsert", viewModel);
        }
        private async Task<IActionResult> AutoGenerateInvoiceFromContext(int contextId, InvoiceType type, string prefix, string itemDescription, Expression<Func<Appointment, decimal>> priceSelector)
        {
            try
            {
                var appointment = await _db.Appointments.Include(a => a.Patient).Include(a => a.Doctor).FirstOrDefaultAsync(a => a.AppointmentId == contextId);
                if (appointment == null) return Json(new { success = false, message = "Context not found." });

                if (await _db.Invoices.AnyAsync(i => i.AppointmentId == contextId && i.InvoiceType == type))
                    return Json(new { success = false, message = "An invoice of this type already exists." });

                var price = priceSelector.Compile()(appointment);
                var invoice = new Invoice
                {
                    InvoiceNumber = GenerateInvoiceNumber(prefix),
                    InvoiceType = type,
                    AppointmentId = contextId,
                    PatientId = appointment.PatientId,
                    DoctorId = appointment.DoctorId,
                    CustomerName = $"{appointment.Patient.FirstName} {appointment.Patient.LastName}",
                    CustomerEmail = appointment.Patient.Email ?? "",
                    CustomerAddress = appointment.Patient.Address ?? "",
                    IssueDate = DateTime.Now,
                    DueDate = DateTime.Now.AddDays(30),
                    status = "Pending",
                    PaymentStatus = PaymentStatus.Pending,
                    CreatedBy = User.Identity?.Name ?? "System",
                    Items = new List<InvoiceItem> { new InvoiceItem { Description = itemDescription, Quantity = 1, UnitPrice = price } }
                };
                CalculateInvoiceTotals(invoice);
                _db.Invoices.Add(invoice);
                await _db.SaveChangesAsync();
                return Json(new { success = true, invoiceId = invoice.Id, message = "Invoice generated successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error auto-generating invoice for context ID {ContextId}", contextId);
                return Json(new { success = false, message = "An error occurred during invoice generation." });
            }
        }

        private async Task CreateInvoiceAsync(Invoice invoice)
        {
            invoice.InvoiceNumber = GenerateInvoiceNumber(GetInvoicePrefix((InvoiceType)invoice.InvoiceType));
            invoice.CreatedDate = DateTime.Now;
            invoice.CreatedBy = User.Identity?.Name ?? "System";

            // Let EF create the parent first so we get an Id to stamp children
            _db.Invoices.Add(invoice);
            await _db.SaveChangesAsync();

            // Ensure all children point to parent (safety for model binder edge cases)
            foreach (var it in invoice.Items ?? Enumerable.Empty<InvoiceItem>())
            {
                it.InvoiceId = invoice.Id;
            }

            // Now that we know exact deltas, adjust inventory
            await UpdateInventoryForItems(invoice.Items ?? new List<InvoiceItem>(), new List<InvoiceItem>());

            TempData["success"] = "Invoice created successfully.";

        }

        private async Task UpdateInvoiceAsync(Invoice invoice)
        {
            var dbInvoice = await _db.Invoices
    .Include(i => i.Items)
    .FirstOrDefaultAsync(i => i.Id == invoice.Id);
            if (dbInvoice == null) throw new InvalidOperationException("Invoice not found for update.");

            // 0) Snapshot old items BEFORE removing them
            var oldItems = dbInvoice.Items.Select(i => new InvoiceItem
            {
                Id = i.Id,
                InventoryItemId = i.InventoryItemId,
                Quantity = i.Quantity
            }).ToList();

            // 1) Map editable fields
            dbInvoice.CustomerName = invoice.CustomerName;
            dbInvoice.CustomerEmail = invoice.CustomerEmail;
            dbInvoice.CustomerAddress = invoice.CustomerAddress;
            dbInvoice.IssueDate = invoice.IssueDate;
            dbInvoice.DueDate = invoice.DueDate;
            dbInvoice.PaymentMethodId = invoice.PaymentMethodId;
            dbInvoice.PaymentStatus = invoice.PaymentStatus;
            dbInvoice.status = invoice.status;
            dbInvoice.Notes = invoice.Notes;
            dbInvoice.SubTotal = invoice.SubTotal;
            dbInvoice.Tax = invoice.Tax;
            dbInvoice.Discount = invoice.Discount;
            dbInvoice.Total = invoice.Total;
            dbInvoice.AmountPaid = invoice.AmountPaid;
            dbInvoice.AmountDue = invoice.AmountDue;

            // 2) System fields
            dbInvoice.UpdatedDate = DateTime.Now;
            dbInvoice.UpdatedBy = User.Identity?.Name ?? "System";

            // 3) Replace children with posted ones (ensure FK set)
            _db.InvoiceItems.RemoveRange(dbInvoice.Items);
            dbInvoice.Items = new List<InvoiceItem>();
            foreach (var newItem in invoice.Items)
            {
                newItem.Id = 0; // force insert for posted items
                newItem.InvoiceId = dbInvoice.Id; // ensure FK
                dbInvoice.Items.Add(newItem);
            }

            // 4) Adjust inventory using NEW vs OLD snapshot
            await UpdateInventoryForItems(dbInvoice.Items.ToList(), oldItems);

            TempData["success"] = "Invoice updated successfully.";

        }
        private async Task UpdateInventoryForItems(List<InvoiceItem> newItems, List<InvoiceItem> oldItems)
        {
            var allItemIds = newItems.Concat(oldItems).Where(i => i.InventoryItemId.HasValue).Select(i => i.InventoryItemId.Value).Distinct().ToList();
            var inventory = await _db.InventoryItems.Where(i => allItemIds.Contains(i.Id)).ToDictionaryAsync(i => i.Id);

            foreach (var oldItem in oldItems.Where(i => i.InventoryItemId.HasValue))
            {
                if (inventory.TryGetValue(oldItem.InventoryItemId.Value, out var item)) item.Stock += oldItem.Quantity;
            }
            foreach (var newItem in newItems.Where(i => i.InventoryItemId.HasValue))
            {
                if (inventory.TryGetValue(newItem.InventoryItemId.Value, out var item))
                {
                    if (item.Stock < newItem.Quantity) throw new InvalidOperationException($"Insufficient stock for '{item.Name}'.");
                    item.Stock -= newItem.Quantity;
                }
            }
        }

        // Replace the body of your existing method with this:
        private async Task<IActionResult> ReturnUpsertViewWithError(Invoice invoice)
        {
            var doctors = await _db.Doctors.ToListAsync();
            var appointments = await _db.Appointments
                .Include(a => a.Patient)
                .Where(a => a.Status == "Completed" || a.Status == "Confirmed")
                .Select(a => new SelectListItem
                {
                    Value = a.AppointmentId.ToString(),
                    Text = $"APT-{a.AppointmentId} - {a.Patient.FirstName} {a.Patient.LastName} - {a.AppointmentDate:MMM dd, yyyy}"
                })
                .ToListAsync();

            var vm = new InvoiceUpsertViewModel
            {
                Invoice = invoice,
                PaymentMethods = await _db.PaymentMethods
                    .Where(p => p.IsActive)
                    .Select(p => new SelectListItem
                    {
                        Value = p.Id.ToString(),
                        Text = p.Name
                    })
                    .ToListAsync(),
                Appointments = appointments,
                InvoiceTypes = GetEnumSelectList<InvoiceType>(),
                InventoryItems = await _db.InventoryItems.Where(i => i.IsActive).ToListAsync(),
                Doctors = doctors.Select(d => new SelectListItem
                {
                    Value = d.Id.ToString(),
                    Text = d.FullName
                }).ToList()
            };

            vm.SetInvoiceTypeFlags();

            return View("Upsert", vm);
        }
        private async Task<List<SelectListItem>> GetTreatmentsSelectList()
        {
            return await _db.Treatments.AsNoTracking().Where(t => t.IsActive)
                .Select(t => new SelectListItem { Value = t.TreatmentId.ToString(), Text = $"{t.Name} - ${t.UnitPrice:F2}" }).ToListAsync();
        }

        private void CalculateInvoiceTotals(Invoice invoice)
        {
            invoice.SubTotal = invoice.Items.Sum(i => i.Quantity * i.UnitPrice);
            invoice.Total = invoice.SubTotal + invoice.Tax - invoice.Discount;
            invoice.AmountDue = invoice.Total - invoice.AmountPaid;
        }

        private string GenerateInvoiceNumber(string prefix) => $"{prefix}-{DateTime.Now:yyyyMMddHHmmss}";
        private string GenerateReceiptNumber() => $"RCP-{DateTime.Now:yyyyMMddHHmmss}";
        private string GetInvoicePrefix(InvoiceType type) => type switch
        {
            InvoiceType.Appointment => "APT",
            InvoiceType.Treatment => "TRT",
            InvoiceType.Laboratory => "LAB",
            InvoiceType.Medication => "MED",
            InvoiceType.Combined => "CMB",
            _ => "INV"
        };





// Add these methods to your existing InvoiceController

/// <summary>
/// Gets all invoices for a specific appointment (for doctor's treatment view)
/// </summary>
[HttpGet]
public async Task<IActionResult> GetAppointmentInvoices(int appointmentId)
        {
            try
            {
                var invoices = await _db.Invoices
                    .Include(i => i.Items)
                    .Where(i => i.AppointmentId == appointmentId)
                    .OrderByDescending(i => i.CreatedDate)
                    .Select(i => new
                    {
                        i.Id,
                        i.InvoiceNumber,
                        i.InvoiceType,
                        i.Total,
                        i.status,
                        i.PaymentStatus,
                        IssueDate = i.IssueDate.ToString("dd MMM yyyy"),
                        ItemCount = i.Items.Count,
                        IsAppointmentInvoice = i.IsAppointmentInvoice,
                        IsCombinedInvoice = i.IsCombinedInvoice,
                        IsMedicationInvoice = i.IsMedicationInvoice,
                        IsLaboratoryInvoice = i.IsLaboratoryInvoice
                    })
                    .ToListAsync();

                return Json(new { success = true, data = invoices });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting invoices for appointment {AppointmentId}", appointmentId);
                return Json(new { success = false, message = "Error retrieving invoices." });
            }
        }

        /// <summary>
        /// Updates invoice type when treatments/medications are added by doctor
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> UpdateInvoiceType(int invoiceId, string newType)
        {
            try
            {
                var invoice = await _db.Invoices.FindAsync(invoiceId);
                if (invoice == null)
                    return Json(new { success = false, message = "Invoice not found." });

                // Parse the new invoice type
                if (Enum.TryParse<InvoiceType>(newType, out var invoiceType))
                {
                    invoice.InvoiceType = invoiceType;

                    // Update the boolean flags
                    invoice.IsAppointmentInvoice = (invoiceType == InvoiceType.Appointment);
                    invoice.IsCombinedInvoice = (invoiceType == InvoiceType.Combined);
                    invoice.IsMedicationInvoice = (invoiceType == InvoiceType.Medication);
                    invoice.IsLaboratoryInvoice = (invoiceType == InvoiceType.Laboratory);

                    invoice.UpdatedDate = DateTime.Now;
                    invoice.UpdatedBy = User.Identity?.Name ?? "System";

                    await _db.SaveChangesAsync();
                    return Json(new { success = true, message = "Invoice type updated successfully." });
                }

                return Json(new { success = false, message = "Invalid invoice type." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating invoice type for Invoice ID {InvoiceId}", invoiceId);
                return Json(new { success = false, message = "Error updating invoice type." });
            }
        }

        /// <summary>
        /// Gets the appropriate edit URL based on invoice type
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetInvoiceEditUrl(int invoiceId)
        {
            try
            {
                var invoice = await _db.Invoices
                    .AsNoTracking()
                    .FirstOrDefaultAsync(i => i.Id == invoiceId);

                if (invoice == null)
                    return Json(new { success = false, message = "Invoice not found." });

                var editUrl = GetEditUrlByInvoiceType((InvoiceType)invoice.InvoiceType, invoiceId);
                return Json(new { success = true, editUrl });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting edit URL for invoice {InvoiceId}", invoiceId);
                return Json(new { success = false, message = "Error getting edit URL." });
            }
        }

        /// <summary>
        /// Helper method to determine edit URL based on invoice type
        /// </summary>
        private string GetEditUrlByInvoiceType(InvoiceType type, int invoiceId)
        {
            return type switch
            {
                InvoiceType.Appointment => $"/Invoice/Upsert/{invoiceId}",
                InvoiceType.Medication => $"/Invoice/Upsert/{invoiceId}",
                InvoiceType.Combined => $"/Invoice/Upsert/{invoiceId}",
                InvoiceType.Laboratory => $"/Invoice/Upsert/{invoiceId}",
                _ => $"/Invoice/Upsert/{invoiceId}"
            };
        }

        /// <summary>
        /// Auto-converts appointment invoice to combined when treatments are added
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> ConvertToCombinedInvoice(int appointmentId)
        {
            try
            {
                var appointment = await _db.Appointments
                    .Include(a => a.Patient)
                    .Include(a => a.Doctor)
                    .FirstOrDefaultAsync(a => a.AppointmentId == appointmentId);

                if (appointment == null)
                    return Json(new { success = false, message = "Appointment not found." });

                // Find existing appointment invoice
                var existingInvoice = await _db.Invoices
                    .FirstOrDefaultAsync(i => i.AppointmentId == appointmentId && i.IsAppointmentInvoice);

                if (existingInvoice != null)
                {
                    // Convert to combined invoice
                    existingInvoice.InvoiceType = InvoiceType.Combined;
                    existingInvoice.IsAppointmentInvoice = false;
                    existingInvoice.IsCombinedInvoice = true;
                    existingInvoice.UpdatedDate = DateTime.Now;
                    existingInvoice.UpdatedBy = User.Identity?.Name ?? "System";

                    await _db.SaveChangesAsync();
                    return Json(new
                    {
                        success = true,
                        invoiceId = existingInvoice.Id,
                        message = "Invoice converted to combined invoice."
                    });
                }

                return Json(new { success = false, message = "No appointment invoice found to convert." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error converting invoice to combined for Appointment ID {AppointmentId}", appointmentId);
                return Json(new { success = false, message = "Error converting invoice." });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetInventoryItemDetails(int id)
        {
            try
            {
                var item = await _db.InventoryItems.FindAsync(id);
                if (item == null)
                {
                    return Json(new { success = false, message = "Item not found" });
                }

                return Json(new
                {
                    success = true,
                    unitPrice = item.UnitPrice,
                    description = item.Description ?? item.Name
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting inventory item details");
                return Json(new { success = false, message = "Error retrieving item details" });
            }
        }



        [HttpGet]
        public async Task<IActionResult> Receipt(int id, int? appointmentId = null)
        {
            if (appointmentId.HasValue)
            {
                var invoices = await _db.Invoices
                    .Include(i => i.Items)
                           .ThenInclude(item => item.Treatment)
                    .Include(i => i.Items)
                             .ThenInclude(item => item.InventoryItem)
                    .Include(i => i.Patient)
                    .Include(i => i.Doctor)
                    .Where(i => i.AppointmentId == appointmentId.Value)
                    .OrderByDescending(i => i.IssueDate)
                    .ToListAsync();

                if (!invoices.Any())
                {
                    TempData["error"] = "No invoices found for this appointment.";
                    return RedirectToAction(nameof(Index));
                }

                // Assign item names dynamically based on ItemType
                foreach (var invoice in invoices)
                {
                    foreach (var item in invoice.Items)
                    {
                        if (item.ItemType == "Test")
                        {
                            item.MedicationsName = _db.LabTestOrders
                                .FirstOrDefault(t => t.TestName == item.TestName)?.TestName ?? "Test Name Not Available";
                        }
                        else if (item.ItemType == "Treatment")
                        {
                            item.MedicationsName = _db.Treatments
                                .FirstOrDefault(t => t.Name == item.TreatmentName)?.Name ?? "Treatment Name Not Available";
                        }
                        else if (item.ItemType == "Medication")
                        {
                            item.MedicationsName = _db.Medications
                                .FirstOrDefault(m => m.Name == item.MedicationsName)?.Name ?? "Medication Name Not Available";
                        }

                        // Ensure DisplayName is computed
                        item.ItemDisplayName = item.DisplayName;
                    }
                }

                return View("FullReceiptA4ByAppointment", invoices);
            }
            else
            {
                var invoice = await _db.Invoices
                    .Include(i => i.Items)
                        .ThenInclude(item => item.Treatment)
                    .Include(i => i.Items)
                        .ThenInclude(item => item.InventoryItem)
                    .Include(i => i.Items)
                        .ThenInclude(item => item.Appointment)
                    //.Include(i => i.Treatment)
                    .Include(i => i.Patient)
                    .Include(i => i.Doctor)
                    .FirstOrDefaultAsync(i => i.Id == id);

                if (invoice == null)
                {
                    TempData["error"] = "Invoice not found.";
                    return RedirectToAction(nameof(Index));
                }

                // Assign item names dynamically based on ItemType
                foreach (var item in invoice.Items)
                {
                    if (item.ItemType == "Test")
                    {
                        item.MedicationsName = _db.LabTestOrders
                            .FirstOrDefault(t => t.TestName == item.TestName)?.TestName ?? "Test Name Not Available";
                    }
                    else if (item.ItemType == "Treatment")
                    {
                        item.MedicationsName = _db.Treatments
                            .FirstOrDefault(t => t.Name == item.TreatmentName)?.Name ?? "Treatment Name Not Available";
                    }
                    else if (item.ItemType == "Medication")
                    {
                        item.MedicationsName = _db.Medications
                            .FirstOrDefault(m => m.Name == item.MedicationsName)?.Name ?? "Medication Name Not Available";
                    }

                    // Ensure DisplayName is computed
                    item.ItemDisplayName = item.DisplayName;
                }

                return View("FullReceiptA4Single", invoice);
            }
        }



        #endregion
    }
}
