var dataTable;

$(document).ready(function () {
    loadDataTable();
});

function loadDataTable() {
    dataTable = $('#tblData').DataTable({
        "ajax": {
            "url": "/Appointment/GetAll"
        },
        "columns": [
            { "data": "appointmentNo" },
            { "data": "patientName" },
            { "data": "doctorName" },
            { "data": "appointmentDate" },
            { "data": "type" },
            { "data": "reason" },
            { "data": "status" },
            {
                "data": "appointmentId",
                "render": function (data) {
                    return `
                        <a href="/Appointment/Upsert/${data}" class="btn btn-sm btn-warning">Edit</a>
                        <a onclick=Delete('/Appointment/Delete/${data}') class="btn btn-sm btn-danger">Delete</a>
                    `;
                }
            }
        ]
    });
}

function Delete(url) {
    Swal.fire({
        title: "Are you sure?",
        text: "You won’t be able to revert this!",
        icon: "warning",
        showCancelButton: true,
        confirmButtonColor: "#3085d6",
        cancelButtonColor: "#d33",
        confirmButtonText: "Yes, delete it!"
    }).then((result) => {
        if (result.isConfirmed) {
            $.ajax({
                url: url,
                type: 'DELETE',
                success: function (data) {
                    if (data.success) {
                        dataTable.ajax.reload();
                        Swal.fire("Deleted!", data.message, "success");
                    } else {
                        Swal.fire("Error!", data.message, "error");
                    }
                }
            })
        }
    })
}
