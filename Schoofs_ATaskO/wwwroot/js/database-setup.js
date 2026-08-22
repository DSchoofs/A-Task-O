let setupActionStarted = false;

function showSetupLoading(title, message) {
    if (setupActionStarted) {
        return false;
    }

    setupActionStarted = true;

    const loadingOverlay = document.getElementById("setupLoadingOverlay");
    const loadingTitle = document.getElementById("setupLoadingTitle");
    const loadingMessage = document.getElementById("setupLoadingMessage");

    loadingTitle.textContent = title;
    loadingMessage.textContent = message;

    loadingOverlay.classList.add("is-visible");
    loadingOverlay.setAttribute("aria-hidden", "false");

    document.body.classList.add("setup-loading-active");

    return true;
}

document.addEventListener("DOMContentLoaded", function () {
    const defaultOption = document.getElementById("useDefaultConfiguration");
    const advancedOption = document.getElementById("useAdvancedConfiguration");
    const defaultConfiguration = document.getElementById("defaultConfiguration");
    const advancedConfiguration = document.getElementById("advancedConfiguration");
    const authenticationMode = document.getElementById("authenticationMode");
    const sqlServerCredentials = document.getElementById("sqlServerCredentials");

    function updateConfigurationMode() {
        const useAdvanced = advancedOption.checked;

        defaultConfiguration.hidden = useAdvanced;
        advancedConfiguration.hidden = !useAdvanced;
    }

    function updateAuthenticationMode() {
        const useSqlServerAuthentication = authenticationMode.value === "SqlServer";

        sqlServerCredentials.hidden = !useSqlServerAuthentication;
    }

    defaultOption.addEventListener("change", updateConfigurationMode);
    advancedOption.addEventListener("change", updateConfigurationMode);
    authenticationMode.addEventListener("change", updateAuthenticationMode);

    updateConfigurationMode();
    updateAuthenticationMode();
});