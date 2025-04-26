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

document.getElementById('apply-filter').addEventListener('click', function () {
    const selectedCategories = [...document.querySelectorAll('input[name="SelectedCategories"]:checked')].map(checkbox => checkbox.value);
    const selectedColors = [...document.querySelectorAll('input[name="SelectedColors"]:checked')].map(checkbox => checkbox.value);
    const selectedClotherSizes = [...document.querySelectorAll('input[name="SelectedClotherSizes"]:checked')].map(checkbox => checkbox.value);
    const selectedShoeSizes = [...document.querySelectorAll('input[name="SelectedShoeSizes"]:checked')].map(checkbox => checkbox.value);
    const priceFrom = document.getElementById('price-from').value;
    const priceTo = document.getElementById('price-to').value;
    const searchString = document.getElementById('search-string').value;

    const params = new URLSearchParams();
    selectedCategories.forEach(category => params.append('categoryIds', category));
    selectedColors.forEach(color => params.append('colors', color));
    selectedClotherSizes.forEach(size => params.append('clotherSizes', size));
    selectedShoeSizes.forEach(size => params.append('shoeSizes', size));
    if (priceFrom) params.append('priceFrom', priceFrom);
    if (priceTo) params.append('priceTo', priceTo);
    if (searchString) params.append('search', searchString);

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
    document.getElementById('search-string').value = '';
    window.location.href = `/Home/Shop`;
});