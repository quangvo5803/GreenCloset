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
                "data": 'email',
                "className": "text-center align-middle",
                "width": "10%"
            },
            {
                "data": 'userName',
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
                "data": 'email',
                "className": "text-center align-middle",
                "width": "15%"
            },
            {
                "data": 'shopName',
                "width": "15%",
                "className": "text-center align-middle",
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
                "width": "20%",
                "className": "text-center align-middle",
                "render": function (data) {
                    return `${data.averageFeeback.toFixed(1)} ⭐ | ${data.feedbackCount} đánh giá(s)`;
                }
            },
            {
                "data": 'id',
                "width": "10%",
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
}