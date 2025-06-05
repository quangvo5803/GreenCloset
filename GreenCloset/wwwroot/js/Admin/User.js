var dataCustomerTable;
var dataLessorTable;
$(function () {
    loadDataTable();
});

function loadDataTable() {
    dataCustomerTable = $('#tblCustomerData').DataTable({
        responsive: true,
        autoWidth: false,
        "ajax": {
            url: '/admin/getallcustomer',
            type: 'GET',
            dataSrc: 'data'
        },
        "columns": [
            {
                "data": 'userName',
                "className": "text-center align-middle",
                "width": "10%"
            },
            {
                "data": 'email',
                "width": "10%",
                "className": "text-center align-middle",
                "render": function (data, type, row) {
                    return data ? data : "Chưa cung cấp";
                }
            },
            {
                "data": 'phoneNumber',
                "width": "15%",
                "className": "text-center align-middle",
                "render": function (data, type, row) {
                    return data ? data : "Chưa cung cấp";
                }
            },
            {
                "data": 'rentalOrderCount',
                "width": "15%",
                "className": "text-center align-middle",

            },
            {
                "data": 'totalRentalMoney',
                "width": "15%",
                "className": "text-center align-middle",
                "render": function (data, type, row) {
                    return new Intl.NumberFormat('vi-VN').format(data) + ' VNĐ';
                }
            },
            {
                "data": 'id',
                "width": "20%",
                "render": function (data, type, row) {
                    return `
                    <div class="btn-group d-flex justify-content-between" role="group">
                       <a href="" class="btn btn-dark flex-grow-1 mx-1" disable>Chưa có hành động</a>
                    </div>`;
                }
            }
        ],
        language: {
            url: "//cdn.datatables.net/plug-ins/1.13.6/i18n/vi.json"
        }
    });
    dataLessorTable = $('#tblLessorData').DataTable({
        responsive: true,
        autoWidth: false,
        "ajax": {
            url: '/admin/getalllessor',
            type: 'GET',
            dataSrc: 'data'
        },
        "columns": [
            {
                "data": null,
                "className": "text-center align-middle",
                "width": "10%",
                "render": function (data, type, row) {
                    return row.shopName ? row.shopName : (row.userName ? row.userName : "");
                }
            },

            {
                "data": 'email',
                "width": "10%",
                "className": "text-center align-middle",
            },
            {
                "data": 'phoneNumber',
                "width": "10%",
                "className": "text-center align-middle",
                "render": function (data, type, row) {
                    return data ? data : "Chưa cung cấp";
                }
            },

            {
                "data": "productCount",
                "className": "text-center align-middle",
                "width": "10%",
            },
            {
                "data": 'orderCount',
                "width": "10%",
                "className": "text-center align-middle",
            },
            {
                "data": null,
                "width": "15%",
                "className": "text-center align-middle",
                "render": function (data) {
                    return `${data.averageFeeback.toFixed(1)} ⭐ | ${data.feedbackCount} đánh giá(s)`;
                }
            },
            {
                "data": 'isMonthlyFeePaid',
                "width": "10%",
                "className": "text-center align-middle",
                "render": function (data, type, row) {
                    return data ? 'Đã thanh toán' : 'Chưa thanh toán';
                }
            },
            {
                "data": 'id',
                "width": "15%",
                "render": function (data, type, row) {
                    const isPaid = row.isMonthlyFeePaid;
                    const disabledAttr = isPaid ? '' : 'hidden';
                    const imageUrl = row.paymentReceiptImagePath || '#';
                    return `
                    <div class="btn-group d-flex justify-content-between" role="group">
                       <button type="button" class="btn btn-dark flex-grow-1 mx-1 view-invoice-btn"
                           data-img="${imageUrl}" 
                           data-paid="${isPaid}" 
                           data-id="${row.id}"
                           ${disabledAttr}
                           >
                           Xem hóa đơn thanh toán
                       </button>
                    </div>`;
                }
            }
        ],
        language: {
            url: "//cdn.datatables.net/plug-ins/1.13.6/i18n/vi.json"
        }
    });
}
$(document).ready(function () {
    // Mở modal khi click nút xem hóa đơn
    $(document).on('click', '.view-invoice-btn', function (e) {
        e.preventDefault();
        const imgSrc = $(this).data('img');
        const isPaid = $(this).data('paid');
        const userId = $(this).data('id');
        if (imgSrc && imgSrc !== '#') {
            $('#invoiceImage').attr('src', imgSrc);
            $('#toggleSwitch').prop('checked', isPaid === true || isPaid === "true");
            $('#toggleSwitch').data('id', userId);
            $('#toggleLabel').text(isPaid ? "Đã thanh toán" : "Chưa thanh toán");
            $('#invoiceModal').fadeIn();
        }
    });

    $(document).on('click', '#closeModal', function () {
        $('#invoiceModal').fadeOut();
    });
});

$('#toggleSwitch').on('change', function () {
    const isChecked = $(this).is(':checked');
    const userId = $(this).data('id');

    $('#toggleLabel').text(isChecked ? "Đã thanh toán" : "Chưa thanh toán");
    console.log(userId);
    $.ajax({
        url: '/admin/updatemonthlyfeeadmin',
        method: 'POST',
        data: {
            userId: userId,
            isMonthlyFeePaid: isChecked
        },
        success: function (res) {
            toastr.success(res.message);
            dataLessorTable.ajax.reload(null, false);
        },
        error: function (xhr) {
            const res = xhr.responseJSON;
            toastr.error(res?.message || "Đã xảy ra lỗi khi cập nhật.");
        }
    });
});