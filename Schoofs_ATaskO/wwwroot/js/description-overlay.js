document.addEventListener("DOMContentLoaded", function () {
    const overlay = document.getElementById("descriptionOverlay");
    const title = document.getElementById("descriptionDialogTitle");
    const text = document.getElementById("descriptionText");

    if (!overlay || !title || !text) {
        return;
    }

    let lastFocusedElement = null;

    document.querySelectorAll("[data-description-open]")
        .forEach(function (button) {
            button.addEventListener("click", function () {
                lastFocusedElement = document.activeElement;

                title.textContent =
                    "Beschreibung: " +
                    (button.dataset.descriptionTitle || "");

                text.value =
                    button.dataset.description || "";

                overlay.classList.add("is-open");
                document.body.classList.add("description-overlay-open");

                text.focus();
            });
        });

    document.querySelectorAll("[data-description-close]")
        .forEach(function (button) {
            button.addEventListener("click", closeOverlay);
        });

    overlay.addEventListener("click", function (event) {
        if (event.target === overlay) {
            closeOverlay();
        }
    });

    document.addEventListener("keydown", function (event) {
        if (event.key === "Escape" &&
            overlay.classList.contains("is-open")) {
            closeOverlay();
        }
    });

    function closeOverlay() {
        overlay.classList.remove("is-open");
        document.body.classList.remove("description-overlay-open");

        if (lastFocusedElement) {
            lastFocusedElement.focus();
        }
    }
});