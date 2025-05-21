window.addEventListener('DOMContentLoaded', () => {
        // Khi trang vừa load, kiểm tra nếu có checkbox nào đã được chọn

        let clothingCheckboxes = document.querySelectorAll('input[name="SelectedClotherSizes"]');
    let shoeCheckboxes = document.querySelectorAll('input[name="SelectedShoeSizes"]');

            let anyClothingChecked = Array.from(clothingCheckboxes).some(c => c.checked);
            let anyShoesChecked = Array.from(shoeCheckboxes).some(s => s.checked);

    // Nếu đã chọn size quần áo => disable size giày
    if (anyClothingChecked) {
        shoeCheckboxes.forEach(cb => cb.disabled = true);
            }

    // Nếu đã chọn size giày => disable size quần áo
    if (anyShoesChecked) {
        clothingCheckboxes.forEach(cb => cb.disabled = true);
            }
        });
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
    // Xóa ảnh mới chuẩn bị thêm vào Db
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
    //Xóa ảnh cũ
    function removeOldGalleryImage(imageId, btn) {
        Swal.fire({
            title: 'Bạn có chắc?',
            text: "Ảnh này sẽ bị xóa vĩnh viễn!",
            icon: 'warning',
            showCancelButton: true,
            confirmButtonColor: '#3085d6',
            cancelButtonColor: '#d33',
            confirmButtonText: 'Vâng, xóa nó!',
            cancelButtonText: 'Hủy'
        }).then((result) => {
            if (result.isConfirmed) {
                fetch(`/Admin/DeleteImageProduct?imageId=${imageId}`, {
                    method: 'DELETE'
                })
                    .then(response => {
                        if (response.ok) {
                            Swal.fire(
                                'Đã xóa!',
                                'Ảnh đã được xóa.',
                                'success'
                            );
                            // Xóa ảnh khỏi UI
                            const wrapper = btn.closest('.position-relative');
                            wrapper.remove();
                        } else {
                            Swal.fire(
                                'Thất bại!',
                                'Không thể xóa ảnh. Vui lòng thử lại.',
                                'error'
                            );
                        }
                    })
                    .catch(error => {
                        console.error('Lỗi:', error);
                        Swal.fire(
                            'Lỗi!',
                            'Xảy ra lỗi khi kết nối server.',
                            'error'
                        );
                    });
            }
        });
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

