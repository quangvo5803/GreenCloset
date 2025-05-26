$(document).ready(function () {
    // Set initial layout state to compact
    document.body.setAttribute('data-pc-layout', 'compact');

    // Initialize SimpleBar for the sidebar content
    if (document.querySelector('.navbar-content')) {
        new SimpleBar(document.querySelector('.navbar-content'));
    }

    // Handle sidebar toggle
    var sidebarHide = document.getElementById('sidebar-hide');
    if (sidebarHide) {
        sidebarHide.addEventListener('click', function () {
            var sidebar = document.querySelector('.pc-sidebar');
            var header = document.querySelector('.pc-header');
            var container = document.querySelector('.pc-container');

            if (sidebar.classList.contains('pc-sidebar-hide')) {
                sidebar.classList.remove('pc-sidebar-hide');
                header.classList.remove('pc-sidebar-hide');
                container.classList.remove('pc-container-menu-hide');
                console.log("Sidebar Open");
            } else {
                sidebar.classList.add('pc-sidebar-hide');
                header.classList.add('pc-sidebar-hide');
                container.classList.add('pc-container-menu-hide');
                console.log("Sidebar Closed");
            }
        });
    }

    // Gọi preloader
    initPreloader();
});

// Hàm preloader
function initPreloader() {
    $('body').addClass('preloader-site');

    $(window).on("load", function () {
        $('.preloader-wrapper').fadeOut();
        $('body').removeClass('preloader-site');
    });
}

