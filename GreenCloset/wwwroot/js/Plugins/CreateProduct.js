// Avatar preview handling
document.querySelector('input[name="Avatar"]').addEventListener('change', function (e) {
    const file = e.target.files[0];
    const previewContainer = document.querySelector('.avatar-preview-container');
    const preview = document.getElementById('avatarPreview');

    if (file) {
        const reader = new FileReader();
        reader.onloadend = function () {
            preview.src = reader.result;
            previewContainer.style.display = 'block';
        }
        reader.readAsDataURL(file);
    } else {
        previewContainer.style.display = 'none';
    }
});

// Remove avatar preview
function removeAvatarPreview() {
    const input = document.querySelector('input[name="Avatar"]');
    input.value = ''; // Clear the file input
    document.querySelector('.avatar-preview-container').style.display = 'none';
}

// Gallery images preview
document.querySelector('input[name="Gallery"]').addEventListener('change', function (e) {
    handleGalleryFiles(e.target.files);
});

let currentFiles = []; // Mảng lưu trữ các file hiện tại

function handleGalleryFiles(files) {
    const previewContainer = document.getElementById('galleryPreviewContainer');
    const galleryInput = document.getElementById('galleryInput');

    const newFiles = Array.from(files);

    // Nối thêm file mới vào currentFiles
    newFiles.forEach((file) => {
        currentFiles.push(file);

        const reader = new FileReader();
        reader.onloadend = function () {
            const previewWrapper = document.createElement('div');
            previewWrapper.className = 'position-relative m-2';
            previewWrapper.dataset.fileIndex = currentFiles.length - 1;

            const img = document.createElement('img');
            img.src = reader.result;
            img.className = 'img-thumbnail';
            img.style.width = '150px';
            img.style.height = '150px';
            img.style.objectFit = 'cover';

            const removeBtn = document.createElement('button');
            removeBtn.className = 'btn-close position-absolute top-0 end-0 bg-danger text-white';
            removeBtn.setAttribute('aria-label', 'Close');
            removeBtn.addEventListener('click', function () {
                removeGalleryImage(previewWrapper);
            });

            previewWrapper.appendChild(img);
            previewWrapper.appendChild(removeBtn);
            previewContainer.appendChild(previewWrapper);
        };

        reader.readAsDataURL(file);
    });

    // Cập nhật input file
    const dataTransfer = new DataTransfer();
    currentFiles.forEach(file => dataTransfer.items.add(file));
    galleryInput.files = dataTransfer.files;
}

function removeGalleryImage(previewElement) {
    const galleryInput = document.getElementById('galleryInput');
    const previewContainer = document.getElementById('galleryPreviewContainer');

    const index = Array.from(previewContainer.children).indexOf(previewElement);

    if (index > -1) {
        currentFiles.splice(index, 1);
    }

    previewElement.remove();

    const dataTransfer = new DataTransfer();
    currentFiles.forEach(file => dataTransfer.items.add(file));
    galleryInput.files = dataTransfer.files;

    if (currentFiles.length === 0) {
        galleryInput.value = '';
    }
}
// Hàm để kiểm tra và vô hiệu hóa các checkbox khi chọn một nhóm
function toggleSizes(type) {
    if (type === 'clothing') {
        let shoeCheckboxes = document.querySelectorAll('input[name="SelectedShoeSizes"]');
        shoeCheckboxes.forEach(checkbox => checkbox.disabled = true); // Vô hiệu hóa size giày
    } else if (type === 'shoes') {
        let clothingCheckboxes = document.querySelectorAll('input[name="SelectedClotherSizes"]');
        clothingCheckboxes.forEach(checkbox => checkbox.disabled = true); // Vô hiệu hóa size quần áo
    }
}

// Hàm để xử lý khi bỏ chọn các checkbox
document.querySelectorAll('input[name="SelectedClotherSizes"]').forEach(checkbox => {
    checkbox.addEventListener('change', function () {
        if (!this.checked) {
            // Bỏ vô hiệu hóa size giày nếu không có size quần áo nào được chọn
            let anyClothingChecked = Array.from(document.querySelectorAll('input[name="SelectedClotherSizes"]')).some(c => c.checked);
            if (!anyClothingChecked) {
                document.querySelectorAll('input[name="SelectedShoeSizes"]').forEach(c => c.disabled = false);
            }
        }
    });
});

document.querySelectorAll('input[name="SelectedShoeSizes"]').forEach(checkbox => {
    checkbox.addEventListener('change', function () {
        if (!this.checked) {
            // Bỏ vô hiệu hóa size quần áo nếu không có size giày nào được chọn
            let anyShoesChecked = Array.from(document.querySelectorAll('input[name="SelectedShoeSizes"]')).some(s => s.checked);
            if (!anyShoesChecked) {
                document.querySelectorAll('input[name="SelectedClotherSizes"]').forEach(s => s.disabled = false);
            }
        }
    });
});

$("#create-product-form").on("submit", function (e) {
    e.preventDefault();

    $('body').addClass('preloader-site');
    $('.preloader-wrapper').show();
    const role = document.getElementById('user-role').value.toLowerCase();
    const url = `/${role}/CreateProduct`;
    let redirectUrl;
    if (role == 'admin') {
        redirectUrl = '/Admin/ManageProduct';
    } else {
        redirectUrl = '/Lessor/Index';
    }
    var formData = new FormData(this);
    $.ajax({
        url: url,
        method: 'POST',
        data: formData,
        processData: false,
        contentType: false,
        success: function () {
            // Upload thành công
            window.location.href = redirectUrl;
            localStorage.setItem('addSuccess', 'true');
        },
        complete: function () {
            // Ẩn preloader dù thành công hay thất bại
            $('.preloader-wrapper').fadeOut();
            $('body').removeClass('preloader-site');
        }
    });
});
