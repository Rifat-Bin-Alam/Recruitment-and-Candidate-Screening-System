// TalentScreen Client Script
document.addEventListener("DOMContentLoaded", function () {
    // Auto-close mobile navbar on clicking an internal link
    const navbarCollapse = document.getElementById("mainNavbar");
    if (navbarCollapse && typeof bootstrap !== "undefined") {
        const navLinks = navbarCollapse.querySelectorAll(".nav-link:not(.dropdown-toggle), .rcss-user-pill");
        navLinks.forEach(function (link) {
            link.addEventListener("click", function () {
                if (window.innerWidth < 992 && navbarCollapse.classList.contains("show")) {
                    const bsCollapse = bootstrap.Collapse.getInstance(navbarCollapse);
                    if (bsCollapse) {
                        bsCollapse.hide();
                    }
                }
            });
        });
    }
});
