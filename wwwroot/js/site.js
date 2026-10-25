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

    // Demo credentials autofill setup
    setupDemoLoginAutofill();

    // Sticky Back to Top button setup
    setupBackToTop();
});

/**
 * Sets up autofill buttons for HR and Candidate demo accounts on the login page.
 * Strictly populates input fields without automatically submitting the form.
 */
function setupDemoLoginAutofill() {
    const hrBtn = document.getElementById("btn-autofill-hr");
    const candBtn = document.getElementById("btn-autofill-candidate");
    const oneClickBtn = document.getElementById("btn-one-click-demo");
    const emailInput = document.getElementById("Input_Email");
    const passInput = document.getElementById("Input_Password");
    const feedback = document.getElementById("demo-autofill-feedback");

    if (!emailInput || !passInput) {
        return;
    }

    function populate(email, password, roleLabel) {
        emailInput.value = email;
        passInput.value = password;

        // Trigger input/change events for client-side validators
        emailInput.dispatchEvent(new Event("input", { bubbles: true }));
        emailInput.dispatchEvent(new Event("change", { bubbles: true }));
        passInput.dispatchEvent(new Event("input", { bubbles: true }));
        passInput.dispatchEvent(new Event("change", { bubbles: true }));

        // Clear previous validation error highlights
        emailInput.classList.remove("is-invalid");
        passInput.classList.remove("is-invalid");

        if (feedback) {
            feedback.innerHTML = `<i class="bi bi-check-circle-fill me-1"></i> ${roleLabel} demo credentials populated. Click <strong>Sign In to Portal</strong> below.`;
            feedback.className = "alert alert-success py-1.5 px-2.5 small mb-2.5 animate-enter text-center";
            feedback.classList.remove("d-none");
        }
    }

    if (oneClickBtn) {
        oneClickBtn.addEventListener("click", function () {
            // If HR demo is already populated, toggle to Candidate; otherwise populate HR Manager
            if (emailInput.value === "rifat@gmail.com") {
                populate("rifatcandidate@gmail.com", "Cd@1234", "Candidate");
            } else {
                populate("rifat@gmail.com", "Hr@123456", "HR Manager");
            }
        });
    }

    if (hrBtn) {
        hrBtn.addEventListener("click", function () {
            populate("rifat@gmail.com", "Hr@123456", "HR Manager");
        });
    }

    if (candBtn) {
        candBtn.addEventListener("click", function () {
            populate("rifatcandidate@gmail.com", "Cd@1234", "Candidate");
        });
    }
}

/**
 * Sticky Back to Top Button
 * Displays only after scrolling down > 300px; smoothly returns to page top.
 * Respects user's reduced-motion preference.
 */
function setupBackToTop() {
    const btn = document.getElementById("rcssBackToTop");
    if (!btn) return;

    let isScrolling = false;

    function handleScroll() {
        if (!isScrolling) {
            window.requestAnimationFrame(function () {
                if (window.scrollY > 300) {
                    btn.classList.add("visible");
                } else {
                    btn.classList.remove("visible");
                }
                isScrolling = false;
            });
            isScrolling = true;
        }
    }

    window.addEventListener("scroll", handleScroll, { passive: true });

    btn.addEventListener("click", function () {
        const prefersReducedMotion = window.matchMedia("(prefers-reduced-motion: reduce)").matches;
        window.scrollTo({
            top: 0,
            behavior: prefersReducedMotion ? "auto" : "smooth"
        });
    });
}
