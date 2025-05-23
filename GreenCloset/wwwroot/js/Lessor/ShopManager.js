var dataProductTable;
var dataOrderTable;
$(function () {
    loadDataTable();
});

function loadDataTable() {
    dataProductTable = $('#tblProductData').DataTable({
        responsive: true,
        autoWidth: false,
        "ajax": {
            url: '/lessor/getallproduct',
            type: 'GET',
            dataSrc: 'data'
        },
        "columns": [
            {
                "data": null,
                "width": "5%",
                "className": "text-center align-middle",
                "render": function (data, type, row, meta) {
                    return meta.row + 1;
                }
            },
            {
                "data": 'name',
                "className": "text-center align-middle",
                "width": "10%"
            },
            {
                "data": 'price',
                "width": "10%",
                "className": "text-center align-middle",
                "render": function (data, type, row) {
                    return new Intl.NumberFormat('vi-VN').format(data) + ' VNĐ';
                }
            },
            {
                "data": 'categories',
                "width": "15%",
                "className": "text-center align-middle",
                "render": function (data) {
                    if (Array.isArray(data) && data.length > 0) {
                        return data.map(category =>
                            `<span class="badge bg-info me-1">${category.categoryName}</span>`
                        ).join('');
                    } else if (data && data.categoryName) {
                        return `<span class="badge bg-info">${data.categoryName}</span>`;
                    }
                    return '<span class="badge bg-info">Chưa được phân loại</span>';
                }
            },
            {
                "data": null,
                "width": "15%",
                "className": "text-center align-middle",
                "render": function (data) {
                    return `${data.avgRating.toFixed(1)} ⭐ | ${data.feedbackCount} đánh giá`;
                }
            },
            {
                "data": 'id',
                "width": "20%",
                "render": function (data, type, row) {
                    return `
                    <div class="btn-group d-flex justify-content-between" role="group">
                       <a href="/Lessor/UpdateProduct?id=${row.id}" class="btn btn-dark flex-grow-1 mx-1">Chỉnh sửa</a>
                       <a href="/Lessor/ViewProductFeedback?id=${row.id}" class="btn btn-warning flex-grow-1 mx-1">Xem đánh giá</a>
                       <a onClick=Delete('/Lessor/DeleteProduct?id=${row.id}') class="btn btn-danger flex-grow-1 mx-1">Xóa</a>
                    </div>`;
                }
            }
        ],
        language: {
            url: "//cdn.datatables.net/plug-ins/1.13.6/i18n/vi.json"
        }
    });
    dataOrderTable = $('#tblOrderData').DataTable({
        responsive: true,
        autoWidth: false,
        "ajax": {
            url: '/lessor/getallorder',
            type: 'GET',
            dataSrc: 'data'
        },
        "columns": [
            {
                "data": 'id',
                "className": "text-center align-middle",
                "width": "5%"
            },
            
            {
                "data": 'deliveryOption',
                "width": "25%",
                "className": "text-center align-middle",
                "render": function (data) {
                    switch (data) {
                        case 0: return 'Giao tận nơi';
                        case 1: return 'Nhận tại cửa hàng';
                        default: return 'Không xác định';
                    }
                }
            },
            {
                "data": 'status',
                "width": "25%",
                "className": "text-center align-middle",
                "render": function (data) {
                    switch (data) {
                        case 0: return '<span class="badge bg-warning">Chờ xác nhận</span>';
                        case 1: return '<span class="badge bg-info">Đang giao hàng</span>';
                        case 2: return '<span class="badge bg-success">Hoàn thành</span>';
                        case 3: return '<span class="badge bg-danger">Đã hủy</span>';
                        case 4: return '<span class="badge bg-dark">Đang thuê</span>';
                        case 5: return '<span class="badge bg-secondary">Đang trả hàng</span>';
                        default: return 'Không xác định';
                    }
                }
            },
            {
                "data": "productCount",
                "className": "text-center align-middle",
                "width": "10%",
            },
            {
                "data": 'totalPrice',
                "width": "20%",
                "className": "text-center align-middle",
                "render": function (data, type, row) {
                    return new Intl.NumberFormat('vi-VN').format(data) + ' VNĐ';
                }
            },
            {
                "data": 'id',
                "width": "10%",
                "render": function (data, type, row) {
                    return `
                    <div class="btn-group d-flex justify-content-between" role="group">
                       <a href="/Lessor/OrderDetail?orderId=${row.id}" class="btn btn-dark flex-grow-1 mx-1">Xem chi tiết đơn hàng</a>
                    </div>`;
                }
            }
        ],
        language: {
            url: "//cdn.datatables.net/plug-ins/1.13.6/i18n/vi.json"
        }
    });
}
function Delete(url) {
    Swal.fire({
        title: "Bạn có chắc chắn muốn xóa không",
        text: "Bạn không thể hoàn tác!",
        icon: "warning",
        showCancelButton: true,
        confirmButtonColor: "#3085d6",
        cancelButtonColor: "#d33",
        confirmButtonText: "Xóa!",
        cancelButtonText: "Hủy"
    }).then((result) => {
        if (result.isConfirmed) {
            $.ajax({
                url: url,
                type: 'DELETE',
                success: function (data) {
                    location.reload();
                    toastr.success(data.message);
                }
            })
        }
    });
}