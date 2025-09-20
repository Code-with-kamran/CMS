// File: wwwroot/js/hr-common.js
(function () {
    window.myCodeDataTable = function (selector, ajaxUrl, columns, extraOptions) {
        if ($.fn.DataTable.isDataTable(selector)) {
            $(selector).DataTable().destroy();
        }
        const options = $.extend(true, {
            processing: true,
            serverSide: true,
            ajax: { url: ajaxUrl, type: 'GET' },
            dom: 'rtip',
            paging: true,
            pagingType: 'simple_numbers',
            info: true,
            autoWidth: false,
            responsive: {
                details: { type: 'inline', target: 'tr' }
            },
            columns: columns,
            language: {
                emptyTable: "No data found",
                processing: "Loading..."
            },
            drawCallback: function () { /* host page may override */ }
        }, extraOptions || {});
        return $(selector).DataTable(options);
    };

    // Antiforgery token helper
    window.postWithCsrf = function (url, data, done, fail) {
        const token = $('input[name="__RequestVerificationToken"]').val();
        $.ajax({
            url, type: 'POST', data,
            headers: { 'RequestVerificationToken': token }
        }).done(done).fail(fail || function (xhr) {
            Swal.fire('Error', xhr.responseText || 'Request failed', 'error');
        });
    };
})();
