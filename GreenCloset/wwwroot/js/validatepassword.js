document.querySelectorAll(".toggle-password").forEach(icon => {
    icon.addEventListener("click", function () {
        var input = document.getElementById(this.dataset.target);
        if (input.type === "password") {
            input.type = "text";
            this.classList.remove("bi-eye-slash");
            this.classList.add("bi-eye");
        } else {
            input.type = "password";
            this.classList.remove("bi-eye");
            this.classList.add("bi-eye-slash");
        }
    });
});
document.getElementById("password").addEventListener("input", function () {
    let password = this.value;

    // Kiểm tra từng tiêu chí
    let hasLength = password.length >= 8;
    let hasUppercase = /[A-Z]/.test(password);
    let hasNumber = /[0-9]/.test(password);
    let hasSpecial = /[!@@#$%^&*]/.test(password);

    // Cập nhật giao diện
    updateCriteria("criteria-length", hasLength);
    updateCriteria("criteria-uppercase", hasUppercase);
    updateCriteria("criteria-number", hasNumber);
    updateCriteria("criteria-special", hasSpecial);
});

function updateCriteria(id, isValid) {
    let element = document.getElementById(id);
    if (isValid) {
        element.classList.remove("text-muted", "text-danger");
        element.classList.add("text-success");
        element.innerHTML = '<i class="bi bi-check-circle"></i> ' + element.textContent.trim();
    } else {
        element.classList.remove("text-success");
        element.classList.add("text-muted", "text-danger");
        element.innerHTML = '<i class="bi bi-x-circle"></i> ' + element.textContent.trim();
    }
}