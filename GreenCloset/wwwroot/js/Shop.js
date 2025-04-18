function toggleCollapse(id) {
    var collapseElement = document.getElementById(id);
    if (collapseElement.classList.contains('collapse')) {
        collapseElement.classList.remove('collapse');
        document.querySelector(`[onclick="toggleCollapse('${id}')"] i`).classList.replace('fa-plus', 'fa-minus');

    } else {
        collapseElement.classList.add('collapse');
        document.querySelector(`[onclick="toggleCollapse('${id}')"] i`).classList.replace('fa-minus', 'fa-plus');
    }
}

// Hàm để kiểm tra và vô hiệu hóa các checkbox khi chọn một nhóm
function toggleSizes(type) {
    if (type === 'clothing') {
        let shoeCheckboxes = document.querySelectorAll('input[name="SelectedShoeSizes"]');
        shoeCheckboxes.forEach(checkbox => checkbox.disabled = true); // Disable shoe sizes
        // Add collapse class to hide shoe size filter
        document.getElementById('sizeshoes-collapse').classList.add('collapse');
    } else if (type === 'shoes') {
        let clothingCheckboxes = document.querySelectorAll('input[name="SelectedClotherSizes"]');
        clothingCheckboxes.forEach(checkbox => checkbox.disabled = true); // Disable clothing sizes
        // Add collapse class to hide clothing size filter
        document.getElementById('sizeclother-collapse').classList.add('collapse');
    }
}

// Reset filters and enable the other section when checkboxes are unchecked
document.querySelectorAll('input[name="SelectedClotherSizes"]').forEach(checkbox => {
    checkbox.addEventListener('change', function () {
        if (!this.checked) {
            // If no clothing size is selected, enable shoe sizes and remove collapse class to show shoe filter
            let anyClothingChecked = Array.from(document.querySelectorAll('input[name="SelectedClotherSizes"]')).some(c => c.checked);
            if (!anyClothingChecked) {
                document.querySelectorAll('input[name="SelectedShoeSizes"]').forEach(c => c.disabled = false);
                document.getElementById('sizeshoes-collapse').classList.remove('collapse'); // Show shoe size filter
            }
        }
    });
});

document.querySelectorAll('input[name="SelectedShoeSizes"]').forEach(checkbox => {
    checkbox.addEventListener('change', function () {
        if (!this.checked) {
            // If no shoe size is selected, enable clothing sizes and remove collapse class to show clothing filter
            let anyShoesChecked = Array.from(document.querySelectorAll('input[name="SelectedShoeSizes"]')).some(s => s.checked);
            if (!anyShoesChecked) {
                document.querySelectorAll('input[name="SelectedClotherSizes"]').forEach(s => s.disabled = false);
                document.getElementById('sizeclother-collapse').classList.remove('collapse'); // Show clothing size filter
            }
        }
    });
});

document.getElementById('apply-filter').addEventListener('click', function () {
    const selectedCategories = [...document.querySelectorAll('input[name="SelectedCategories"]:checked')].map(checkbox => checkbox.value);
    const selectedColors = [...document.querySelectorAll('input[name="SelectedColors"]:checked')].map(checkbox => checkbox.value);
    const selectedClotherSizes = [...document.querySelectorAll('input[name="SelectedClotherSizes"]:checked')].map(checkbox => checkbox.value);
    const selectedShoeSizes = [...document.querySelectorAll('input[name="SelectedShoeSizes"]:checked')].map(checkbox => checkbox.value);
    const priceFrom = document.getElementById('price-from').value;
    const priceTo = document.getElementById('price-to').value;

    const params = new URLSearchParams();
    selectedCategories.forEach(category => params.append('categoryIds', category));
    selectedColors.forEach(color => params.append('colors', color));
    selectedClotherSizes.forEach(size => params.append('clotherSizes', size));
    selectedShoeSizes.forEach(size => params.append('shoeSizes', size));
    if (priceFrom) params.append('priceFrom', priceFrom);
    if (priceTo) params.append('priceTo', priceTo);

    window.location.href = `/Home/Shop?${params.toString()}`;
});
document.getElementById('reset-filter').addEventListener('click', function () {
    // Reset all checkboxes
    document.querySelectorAll('input[name="SelectedCategories"]').forEach(checkbox => checkbox.checked = false);
    document.querySelectorAll('input[name="SelectedColors"]').forEach(checkbox => checkbox.checked = false);
    document.querySelectorAll('input[name="SelectedClotherSizes"]').forEach(checkbox => checkbox.checked = false);
    document.querySelectorAll('input[name="SelectedShoeSizes"]').forEach(checkbox => checkbox.checked = false);
    // Reset price inputs
    document.getElementById('price-from').value = '';
    document.getElementById('price-to').value = '';
    window.location.href = `/Home/Shop`;
});