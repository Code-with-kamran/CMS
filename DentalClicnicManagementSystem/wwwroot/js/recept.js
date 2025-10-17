// Global variables
let currentInvoiceId = null;
let currentInvoiceData = null;

// Main function to show receipt modal
function showReceiptModal(invoiceId) {
    currentInvoiceId = invoiceId;
    $('#receiptModal').modal('show');
    loadReceiptDetails(invoiceId);
}

// Load receipt details via AJAX
function loadReceiptDetails(invoiceId) {
    showLoadingState();

    fetch(`/Invoice/GetInvoiceReceipt?id=${invoiceId}`, {
        method: 'GET',
        headers: {
            'Content-Type': 'application/json',
            'X-Requested-With': 'XMLHttpRequest'
        }
    })
        .then(response => {
            if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`);
            }
            return response.json();
        })
        .then(data => {
            if (data.success) {
                currentInvoiceData = data.data;
                populateReceiptModal(data.data);
                showReceiptContent();
            } else {
                showErrorState(data.message || 'Failed to load receipt');
            }
        })
        .catch(error => {
            console.error('Error loading receipt:', error);
            showErrorState('Network error occurred while loading receipt');
        });
}

// Populate modal with receipt data
function populateReceiptModal(data) {
    try {
        // Set receipt title based on invoice type
        const receiptTitles = {
            'Appointment': 'Medical Invoice',
            'PatientPurchase': 'Pharmacy Receipt',
            'LabTest': 'Lab Test Invoice'
        };
        $('#receipt-title').text(receiptTitles[data.invoiceType] || 'Invoice POS');

        // Store information
        $('#store-name').text(data.storeName);
        $('#gst-number').text(data.gstNumber);
        $('#fssai-number').text(data.fssaiNumber);
        $('#store-address').text(data.address);

        // Invoice type badge
        const badgeColors = {
            'Appointment': 'bg-primary',
            'PatientPurchase': 'bg-success',
            'LabTest': 'bg-info'
        };
        $('#invoice-type-badge')
            .removeClass('bg-primary bg-success bg-info bg-warning')
            .addClass(badgeColors[data.invoiceType] || 'bg-primary')
            .text(data.invoiceType);

        // Basic invoice info
        $('#issue-date').text(formatDate(data.issueDate));
        $('#invoice-number').text(data.invoiceNumber);

        // Payment status badge
        const statusColors = {
            'Paid': 'bg-success',
            'Pending': 'bg-warning',
            'PartiallyPaid': 'bg-info',
            'Cancelled': 'bg-danger',
            'InsurancePending': 'bg-secondary'
        };
        $('#payment-status-badge')
            .removeClass('bg-success bg-warning bg-info bg-danger bg-secondary')
            .addClass(statusColors[data.paymentStatus] || 'bg-warning')
            .text(data.paymentStatus);

        // Show/hide type-specific information
        showTypeSpecificInfo(data);

        // Customer information
        $('#customer-name').text(data.customerName);
        $('#customer-email').text(data.customerEmail || '');
        $('#customer-address').text(data.customerAddress || '');

        // Populate items
        populateReceiptItems(data.items, data.currencyCode);

        // Financial totals
        populateFinancialTotals(data);

        // Payment information
        populatePaymentInfo(data);

        // Barcode
        $('#barcode-text').text(data.receiptNumber);
        generateBarcode(data.receiptNumber);

        // Notes
        if (data.notes && data.notes.trim()) {
            $('#notes-text').text(data.notes);
            $('#invoice-notes').show();
        } else {
            $('#invoice-notes').hide();
        }

        // Show/hide record payment button
        toggleRecordPaymentButton(data.paymentStatus, data.amountDue);

    } catch (error) {
        console.error('Error populating receipt modal:', error);
        showErrorState('Error displaying receipt data');
    }
}

// Show type-specific information
function showTypeSpecificInfo(data) {
    // Hide all type-specific sections first
    $('#doctor-info, #appointment-info, #test-info').hide();

    switch (data.invoiceType) {
        case 'Appointment':
            if (data.doctorName) {
                $('#doctor-name').text(data.doctorName);
                $('#doctor-info').show();
            }
            if (data.appointmentDate) {
                $('#appointment-date').text(formatDateTime(data.appointmentDate));
                $('#appointment-info').show();
            }
            break;

        case 'LabTest':
            if (data.doctorName) {
                $('#doctor-name').text(data.doctorName);
                $('#doctor-info').show();
            }
            if (data.testName) {
                $('#test-name').text(data.testName);
                $('#test-info').show();
            }
            break;

        case 'PatientPurchase':
            // No additional info needed for purchases
            break;
    }
}

// Populate receipt items
function populateReceiptItems(items, currencyCode) {
    let itemsHtml = '';

    if (!items || items.length === 0) {
        itemsHtml = '<div class="text-center py-3 fs-12 text-muted">No items found</div>';
    } else {
        items.forEach((item, index) => {
            const taxInfo = item.taxRate > 0 ? ` (Tax: ${item.taxRate}%)` : '';
            itemsHtml += `
                <div class="row py-1 fs-12 border-bottom">
                    <div class="col-6">
                        <div class="fw-medium">${escapeHtml(item.description)}</div>
                        ${taxInfo ? `<div class="text-muted fs-11">${taxInfo}</div>` : ''}
                    </div>
                    <div class="col-2 text-center">${item.quantity}</div>
                    <div class="col-2 text-end">${formatCurrency(item.unitPrice, currencyCode)}</div>
                    <div class="col-2 text-end">${formatCurrency(item.lineTotal, currencyCode)}</div>
                </div>
            `;
        });
    }

    $('#receipt-items').html(itemsHtml);
}

// Populate financial totals
function populateFinancialTotals(data) {
    $('#sub-total').text(formatCurrency(data.subTotal, data.currencyCode));
    $('#tax-amount').text(formatCurrency(data.tax, data.currencyCode));
    $('#discount-amount').text(formatCurrency(data.discount, data.currencyCode));
    $('#grand-total').text(formatCurrency(data.total, data.currencyCode));
}

// Populate payment information
function populatePaymentInfo(data) {
    $('#payment-method').text(data.paymentMethod);
    $('#amount-paid').text(formatCurrency(data.amountPaid, data.currencyCode));
    $('#amount-due').text(formatCurrency(data.amountDue, data.currencyCode));

    // Show payment history if multiple payments
    if (data.payments && data.payments.length > 1) {
        let paymentHistoryHtml = '';
        data.payments.forEach(payment => {
            paymentHistoryHtml += `
                <div class="d-flex justify-content-between fs-12 mb-1">
                    <span>${payment.method} - ${formatDate(payment.date)}</span>
                    <span>${formatCurrency(payment.amount, data.currencyCode)}</span>
                </div>
                ${payment.reference ? `<div class="fs-11 text-muted mb-2">Ref: ${payment.reference}</div>` : ''}
            `;
        });
        $('#payment-list').html(paymentHistoryHtml);
        $('#payment-history').show();
    } else {
        $('#payment-history').hide();
    }
}

// Generate barcode
function generateBarcode(text) {
    try {
        if (typeof JsBarcode !== 'undefined') {
            JsBarcode("#barcode", text, {
                format: "CODE128",
                width: 1,
                height: 40,
                displayValue: false,
                background: "#ffffff",
                lineColor: "#000000",
                margin: 5
            });
        } else {
            // Fallback if JsBarcode is not loaded
            $('#barcode').html(`
                <rect width="200" height="50" fill="white" stroke="black"/>
                <text x="100" y="25" text-anchor="middle" font-family="monospace" font-size="10">${text}</text>
            `);
        }
    } catch (error) {
        console.error('Error generating barcode:', error);
        $('#barcode').html(`
            <rect width="200" height="50" fill="white" stroke="black"/>
            <text x="100" y="25" text-anchor="middle" font-family="monospace" font-size="10">${text}</text>
        `);
    }
}

// Toggle record payment button visibility
function toggleRecordPaymentButton(paymentStatus, amountDue) {
    if (paymentStatus === 'Paid' || amountDue <= 0) {
        $('#record-payment-btn').hide();
    } else {
        $('#record-payment-btn').show();
    }
}

// State management functions
function showLoadingState() {
    $('#receipt-loading').show();
    $('#receipt-content').hide();
    $('#receipt-error').hide();
}

function showReceiptContent() {
    $('#receipt-loading').hide();
    $('#receipt-content').show();
    $('#receipt-error').hide();
}

function showErrorState(message) {
    $('#receipt-loading').hide();
    $('#receipt-content').hide();
    $('#error-message').text(message);
    $('#receipt-error').show();
}

// Retry loading receipt
function retryLoadReceipt() {
    if (currentInvoiceId) {
        loadReceiptDetails(currentInvoiceId);
    }
}

// Print receipt function
function printReceipt() {
    if (!currentInvoiceData) {
        alert('No receipt data available to print');
        return;
    }

    const printWindow = window.open('', '_blank', 'width=400,height=700,scrollbars=yes');
    const receiptContent = document.getElementById('receipt-content').cloneNode(true);

    // Remove modal-specific elements
    const elementsToRemove = receiptContent.querySelectorAll('.modal-header, .modal-footer, .btn-close');
    elementsToRemove.forEach(el => el.remove());

    const printHtml = `
        <!DOCTYPE html>
        <html>
        <head>
            <title>Receipt - ${currentInvoiceData.receiptNumber}</title>
            <style>
                body { 
                    font-family: 'Courier New', monospace; 
                    font-size: 12px; 
                    line-height: 1.4;
                    margin: 0;
                    padding: 15px;
                    width: 380px;
                    color: #000;
                }
                .fs-12 { font-size: 12px; }
                .fs-13 { font-size: 13px; }
                .fs-11 { font-size: 11px; }
                .fw-bold { font-weight: bold; }
                .fw-medium { font-weight: 500; }
                .text-center { text-align: center; }
                .text-end { text-align: right; }
                .text-muted { color: #666; }
                .text-danger { color: #dc3545; }
                .border-top { border-top: 1px solid #000; }
                .border-bottom { border-bottom: 1px dotted #ccc; }
                .py-1 { padding-top: 4px; padding-bottom: 4px; }
                .py-2 { padding-top: 8px; padding-bottom: 8px; }
                .pt-2 { padding-top: 8px; }
                .pb-2 { padding-bottom: 8px; }
                .mb-1 { margin-bottom: 4px; }
                .mb-2 { margin-bottom: 8px; }
                .mb-3 { margin-bottom: 12px; }
                .mt-1 { margin-top: 4px; }
                .row { display: flex; width: 100%; }
                .col-2 { width: 16.666%; }
                .col-6 { width: 50%; }
                .d-flex { display: flex; }
                .justify-content-between { justify-content: space-between; }
                .bg-white { background: white; }
                .p-2 { padding: 8px; }
                .rounded { border-radius: 4px; }
                .badge { 
                    display: inline-block; 
                    padding: 2px 6px; 
                    font-size: 10px; 
                    border-radius: 3px; 
                    background: #007bff; 
                    color: white; 
                }
                .barcode-container { 
                    background: white; 
                    padding: 8px; 
                    border: 1px solid #ccc; 
                    text-align: center; 
                    margin: 10px 0; 
                }
                @media print {
                    body { margin: 0; padding: 10px; }
                    .no-print { display: none; }
                }
            </style>
        </head>
        <body>
            <div class="text-center mb-3">
                <h6 class="fw-bold">${currentInvoiceData.invoiceType} Receipt</h6>
            </div>
            ${receiptContent.querySelector('.modal-body').innerHTML}
        </body>
        </html>
    `;

    printWindow.document.write(printHtml);
    printWindow.document.close();

    // Wait for content to load then print
    setTimeout(() => {
        printWindow.print();
        printWindow.close();
    }, 1000);
}

// Payment recording function
function showPaymentModal() {
    if (!currentInvoiceData) {
        alert('No invoice data available');
        return;
    }

    // You can implement a payment modal here
    const amount = prompt(`Enter payment amount (Due: ${formatCurrency(currentInvoiceData.amountDue, currentInvoiceData.currencyCode)}):`);
    if (amount && !isNaN(amount) && parseFloat(amount) > 0) {
        recordPayment(currentInvoiceId, parseFloat(amount));
    }
}

// Record payment via AJAX
function recordPayment(invoiceId, amount, paymentMethod = 'Cash', reference = '', notes = '') {
    const paymentData = {
        invoiceId: invoiceId,
        amount: amount,
        paymentMethod: paymentMethod,
        transactionReference: reference,
        notes: notes
    };

    fetch('/Invoice/RecordPayment', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            'X-Requested-With': 'XMLHttpRequest'
        },
        body: JSON.stringify(paymentData)
    })
        .then(response => response.json())
        .then(data => {
            if (data.success) {
                alert('Payment recorded successfully!');
                // Reload the receipt to show updated payment status
                loadReceiptDetails(invoiceId);
            } else {
                alert('Error recording payment: ' + data.message);
            }
        })
        .catch(error => {
            console.error('Error recording payment:', error);
            alert('Network error occurred while recording payment');
        });
}

// Utility functions
function formatCurrency(amount, currencyCode = 'PKR') {
    if (isNaN(amount)) return '0.00';
    const symbols = {
        'PKR': 'Rs. ',
        'USD': '$',
        'EUR': '€',
        'GBP': '£'
    };
    const symbol = symbols[currencyCode] || currencyCode + ' ';
    return symbol + parseFloat(amount).toFixed(2);
}

function formatDate(dateString) {
    if (!dateString) return '';
    const date = new Date(dateString);
    return date.toLocaleDateString('en-GB'); // DD/MM/YYYY format
}

function formatDateTime(dateString) {
    if (!dateString) return '';
    const date = new Date(dateString);
    return date.toLocaleDateString('en-GB') + ' ' + date.toLocaleTimeString('en-GB', {
        hour: '2-digit',
        minute: '2-digit'
    });
}

function escapeHtml(text) {
    const div = document.createElement('div');
    div.textContent = text;
    return div.innerHTML;
}

// Initialize modal events
$(document).ready(function () {
    // Reset modal when closed
    $('#receiptModal').on('hidden.bs.modal', function () {
        showLoadingState();
        currentInvoiceId = null;
        currentInvoiceData = null;
    });

    // Handle modal show event
    $('#receiptModal').on('show.bs.modal', function () {
        // Reset to loading state when modal is about to show
        showLoadingState();
    });
});

// Functions to generate different invoice types (call these from your UI)
function generateAppointmentInvoice(appointmentId) {
    fetch('/Invoice/GenerateAppointmentInvoice', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            'X-Requested-With': 'XMLHttpRequest'
        },
        body: JSON.stringify({ appointmentId: appointmentId })
    })
        .then(response => response.json())
        .then(data => {
            if (data.success) {
                alert('Appointment invoice generated successfully!');
                showReceiptModal(data.invoiceId);
            } else {
                alert('Error: ' + data.message);
            }
        })
        .catch(error => {
            console.error('Error generating appointment invoice:', error);
            alert('Network error occurred');
        });
}

function generatePurchaseInvoice(patientId, items) {
    const purchaseData = {
        patientId: patientId,
        items: items // Array of {inventoryItemId, quantity}
    };

    fetch('/Invoice/GeneratePatientPurchaseInvoice', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            'X-Requested-With': 'XMLHttpRequest'
        },
        body: JSON.stringify(purchaseData)
    })
        .then(response => response.json())
        .then(data => {
            if (data.success) {
                alert('Purchase invoice generated successfully!');
                showReceiptModal(data.invoiceId);
            } else {
                alert('Error: ' + data.message);
            }
        })
        .catch(error => {
            console.error('Error generating purchase invoice:', error);
            alert('Network error occurred');
        });
}

function generateLabTestInvoice(labTestOrderId) {
    fetch('/Invoice/GenerateLabTestInvoice', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            'X-Requested-With': 'XMLHttpRequest'
        },
        body: JSON.stringify({ labTestOrderId: labTestOrderId })
    })
        .then(response => response.json())
        .then(data => {
            if (data.success) {
                alert('Lab test invoice generated successfully!');
                showReceiptModal(data.invoiceId);
            } else {
                alert('Error: ' + data.message);
            }
        })
        .catch(error => {
            console.error('Error generating lab test invoice:', error);
            alert('Network error occurred');
        });
}
