// Common Delete Handler
$(document).on('click', '.delete-btn-doctor', function () {
    let id = $(this).data('id');
    let url = $(this).data('url');   // coming from button attribute
    let tableId = $(this).data('table'); // coming from button attribute
    const name = $(this).data('name');
    let table = $(`#${tableId}`).DataTable();

    handleDelete(url, id, table,name);
});



// ------------------------------Patient Delete------------------------------
$(document).on('click', '.delete-btn-patient', function () {
    let id = $(this).data('id');
    let url = $(this).data('url');   // coming from button attribute
    let tableId = $(this).data('table'); // coming from button attribute
    const name = $(this).data('name');
    let table = $(`#${tableId}`).DataTable();

    handleDelete(url, id, table,name);
});
// ------------------------------Appointment Delete------------------------------
$(document).on('click', '.delete-btn-appointment', function () {
    let id = $(this).data('id');
    let url = $(this).data('url');   // coming from button attribute
    let tableId = $(this).data('table'); // coming from button attribute
    const name = $(this).data('name');
    let table = $(`#${tableId}`).DataTable();

    handleDelete(url, id, table,name);
});






// The main delete function
function handleDelete(url, id, table, name) {
    let token = $('input[name="__RequestVerificationToken"]').val();

    Swal.fire({
        title: `Delete ${name}?`,
        text: 'You will not be able to revert this!',
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#d33',
        cancelButtonColor: '#aaa',
        confirmButtonText: 'Yes, delete!'
    }).then(res => {
        if (res.isConfirmed) {
            $.ajax({
                url: url,   // 👉 call /Doctor/Delete
                type: 'POST',
                data: {
                    __RequestVerificationToken: token,
                    id: id     // 👉 send ID in body
                },
                success: r => {
                    if ( r.status || r.success) {
                        Swal.fire('Deleted', r.message, 'success');
                        
                        table.row($(`button[data-id="${id}"]`).parents('tr')).remove().draw(false);

                       
                    } else {
                        Swal.fire('Error', r.message || 'Delete failed', 'error');
                    }
                },
                error: () => Swal.fire('Error', 'Something went wrong', 'error')
            });
        }
    });
}




//function handleDelete(url, id, table,name) {
//    let token = $('input[name="__RequestVerificationToken"]').val();

//    Swal.fire({
//        title: `Delete ${name}?`,
//        text: 'You will not be able to revert this!',
//        icon: 'warning',
//        showCancelButton: true,
//        confirmButtonColor: '#d33',
//        cancelButtonColor: '#aaa',
//        confirmButtonText: 'Yes, delete!'
//    }).then(res => {
//        if (res.isConfirmed) {
//            $.ajax({
//                url: `${ url }/${ id }`,
//                type: 'POST',
//                headers: { '__RequestVerificationToken': token },
//                success: r => {
//                    if (r.status) {
//                        Swal.fire('Deleted', r.message, 'success');
//                        table.ajax.reload(null, false);
//                    } else {
//                        Swal.fire('Error', r.message || 'Delete failed', 'error');
//                    }
//                },
//                error: () => Swal.fire('Error', 'Something went wrong', 'error')
//            });
//        }

//    });
//}
