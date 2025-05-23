var dataTable;
$(document).ready(function () {
    loadDataTable();
});

function loadDataTable() {
    dataTable = $('#tblData').DataTable({
        "ajax": {
            url: '/admin/getallproduct',
            type: 'GET',
            dataSrc: 'data'
        },
        "columns": [
            {
                "data": null,
                "width": "5%",
                "render": function (data, type, row, meta) {
                    return meta.row + 1;
                }
            },
            {
                "data": 'name',
                "width": "10%"
            },
            {
                "data": 'price',
                "width": "10%",
                "render": function (data, type, row) {
                    return new Intl.NumberFormat('vn-VN', { style: 'currency', currency: 'VND' }).format(data);
                }
            },
            {
                "data": 'categories',
                "width": "10%",
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
                "width": "10%",
                "render": function (data) {
                    return `${data.avgRating.toFixed(1)} ⭐ | ${data.feedbackCount} feedback(s)`;
                }
            },
            {
                "data": 'id',
                "width": "20%",
                "render": function (data, type, row) {
                    return `
                    <div class="btn-group d-flex justify-content-between" role="group">
                       <a href="/Admin/UpdateProduct?id=${row.id}" class="btn btn-dark flex-grow-1 mx-1">Chỉnh sửa</a>
                       <a href="/Admin/ViewFeedbackProduct?id=${row.id}" class="btn btn-primary flex-grow-1 mx-1">Xem đánh giá</a>
                       <a onClick=Delete('/Admin/DeleteProduct?id=${row.id}') class="btn btn-danger flex-grow-1 mx-1">Xóa</a>
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