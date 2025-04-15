document.addEventListener('DOMContentLoaded', function () {
    document.addEventListener('click', function (e) {
        const btn = e.target.closest('.add-to-cart-btn');
        if (!btn) return;

        e.preventDefault();
        const productId = btn.dataset.productId;
        const pageType = document.getElementById('pageContainer')?.dataset.pageType;
        const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value;
        const size = document.querySelector('input[name="size"]:checked')?.value || null;
        const startDate = document.getElementById('startDate')?.value || null;
        const endDate = document.getElementById('endDate')?.value || null;
        if (!userId) {
            window.location.href = "/Home/Login";
            return;
        }
        if (pageType === "product-detail") {
            if (!size) {
                toastr.error("Bạn chưa chọn size.");
                return;
            }
            if (!startDate) {
                toastr.error("Vui lòng chọn ngày thuê.");
                return;
            }
            if (!endDate) {
                toastr.error("Vui lòng chọn ngày trả.");
                return;
            }
            if (startDate >= endDate) {
                toastr.error("Ngày trả phải lơn hơn ngày thuê.");
                return;
            }
        }

        console.log("Ngày thuê:", startDate);
        console.log("Ngày trả:", endDate);

        if (!token) {
            console.error('Không tìm thấy RequestVerificationToken.');
            toastr.error("Không tìm thấy mã xác thực.");
            return;
        }
        console.log("Selected size:", size);

        $.ajax({
            url: '/Customer/AddToCart',
            type: 'POST',
            headers: {
                'RequestVerificationToken': token
            },
            data: {
                productId: productId,
                count: 1,
                size: size,
                startDate: startDate,
                endDate: endDate
            },
            success: function (data) {


                if (data.success) {
                    toastr.success(data.message);
                } else {
                    toastr.error(data.message);
                }
            },
            error: function (xhr, status, error) {
                toastr.error("Đã xảy ra lỗi khi thêm vào giỏ hàng.");
                console.error("AJAX error:", error);
            }
        });
    });
}); 