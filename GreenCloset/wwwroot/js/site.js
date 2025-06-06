﻿(function ($) {
    var signUpBtn = document.getElementById("signUpBtn");
    if (signUpBtn) {
        signUpBtn.addEventListener("click", function (event) {
            event.preventDefault(); 
            var email = document.getElementById("emailInput").value.trim();
            if (email) {
                window.location.href = "/Home/Register?email=" + encodeURIComponent(email);
            } else {
                Swal.fire({
                    icon: 'warning',
                    title: 'Chưa nhập email!',
                    text: 'Vui lòng nhập email trước khi đăng ký.',
                    confirmButtonText: 'OK'
                });
            }
        });
    }
    "use strict";
    var searchPopup = function () {
        // open search box
        $('.secondary-nav').on('click', '.search-button', function (e) {
            $('.search-popup').toggleClass('is-visible');
        });

        $('#header-nav').on('click', '.btn-close-search', function (e) {
            $('.search-popup').toggleClass('is-visible');
        });

        $(".search-popup-trigger").on("click", function (b) {
            b.preventDefault();
            $(".search-popup").addClass("is-visible"),
                setTimeout(function () {
                    $(".search-popup").find("#search-popup").focus()
                }, 350)
        }),
            $(".search-popup").on("click", function (b) {
                ($(b.target).is(".search-popup-close") || $(b.target).is(".search-popup-close svg") || $(b.target).is(".search-popup-close path") || $(b.target).is(".search-popup")) && (b.preventDefault(),
                    $(this).removeClass("is-visible"))
            }),
            $(document).keyup(function (b) {
                "27" === b.which && $(".search-popup").removeClass("is-visible")
            })
    }

    // Preloader
    // Preloader
    var initPreloader = function () {
        $(document).ready(function () {
            $('body').addClass('preloader-site');

            // Ẩn preloader khi DOM sẵn sàng
            $('.preloader-wrapper').fadeOut('slow', function () {
                $('body').removeClass('preloader-site');
            });
        });
    };


    // init jarallax parallax
    var initJarallax = function () {
        jarallax(document.querySelectorAll(".jarallax"));

        jarallax(document.querySelectorAll(".jarallax-img"), {
            keepImg: true,
        });
    }

    // Tab Section
    var initTabs = function () {
        const tabs = document.querySelectorAll('[data-tab-target]')
        const tabContents = document.querySelectorAll('[data-tab-content]')

        tabs.forEach(tab => {
            tab.addEventListener('click', () => {
                const target = document.querySelector(tab.dataset.tabTarget)
                tabContents.forEach(tabContent => {
                    tabContent.classList.remove('active')
                })
                tabs.forEach(tab => {
                    tab.classList.remove('active')
                })
                tab.classList.add('active')
                target.classList.add('active')
            })
        });
    }
    initPreloader();

    // document ready
    $(document).ready(function () {
        searchPopup();
        initTabs();
        initJarallax();

        jQuery(document).ready(function ($) {
            jQuery('.stellarnav').stellarNav({
                position: 'right'
            });
        });

        $(".user-items .icon-search").click(function () {
            $(".search-box").toggleClass('active');
            $(".search-box .search-input").focus();
        });
        $(".close-button").click(function () {
            $(".search-box").toggleClass('active');
        });
        var mainSwiper = new Swiper(".main-swiper", {
            speed: 500,
            loop: true,
            navigation: {
                nextEl: ".button-next",
                prevEl: ".button-prev",
            },
            pagination: {
                el: "#billboard .swiper-pagination",
                clickable: true,
            },
        });

        var productSwiper = new Swiper("#featured-products .product-swiper", {
            speed: 500,
            loop: false,
            slidesPerView: 4,
            spaceBetween: 30,
            navigation: {
                nextEl: ".swiper-button-next",
                prevEl: ".swiper-button-prev",
            },
            breakpoints: {
                0: {
                    slidesPerView: 1,
                    spaceBetween: 30,
                },
                768: {
                    slidesPerView: 2,
                    spaceBetween: 30,
                },
                999: {
                    slidesPerView: 3,
                    spaceBetween: 30,
                },
                1299: {
                    slidesPerView: 4,
                    spaceBetween: 30,
                },
            },
        });     

        var salesSwiper = new Swiper("#flash-sales .product-swiper", {
            pagination: {
                el: "#flash-sales .product-swiper .swiper-pagination",
                clickable: true,
            },
            breakpoints: {
                0: {
                    slidesPerView: 1,
                    spaceBetween: 30,
                },
                768: {
                    slidesPerView: 2,
                    spaceBetween: 30,
                },
                999: {
                    slidesPerView: 3,
                    spaceBetween: 30,
                },
                1299: {
                    slidesPerView: 4,
                    spaceBetween: 30,
                },
            },
        });

        var testimonialSwiper = new Swiper(".testimonial-swiper", {
            loop: true,
            navigation: {
                nextEl: ".next-button",
                prevEl: ".prev-button",
            },
        });

        var thumb_slider = new Swiper(".thumb-swiper", {
            slidesPerView: 1,
        });
        var large_slider = new Swiper(".large-swiper", {
            spaceBetween: 10,
            effect: 'fade',
            thumbs: {
                swiper: thumb_slider,
            },
        });

        // Initialize Isotope
        var $grid = $('.entry-container').isotope({
            itemSelector: '.entry-item',
            layoutMode: 'masonry'
        });
        $grid.imagesLoaded().progress(function () {
            $grid.isotope('layout');
        });

        $(".gallery").colorbox({
            rel: 'gallery'
        });

        $(".youtube").colorbox({
            iframe: true,
            innerWidth: 960,
            innerHeight: 585,
        });

    });

})(jQuery);