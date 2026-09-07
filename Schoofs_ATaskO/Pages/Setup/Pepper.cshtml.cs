using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Schoofs_ATaskO.Services;

namespace Schoofs_ATaskO.Pages.Setup
{
    public class PepperModel : PageModel
    {
        private const string PepperWasStoredInSessionKey =
            "PepperWasStoredInSession";

        [BindProperty]
        public string Pepper { get; set; } = string.Empty;
        public string ErrorMessage { get; set; } = string.Empty;
        public string SuccessMessage { get; set; } = string.Empty;

        private readonly ISetupService _databaseSetupService;

        public PepperModel(ISetupService setupService)
        {
            _databaseSetupService = setupService;
        }

        public IActionResult OnGet()
        {
            if (_databaseSetupService.HasPasswordPepper())
            {
                return RedirectToPage("/Index");
            }

            Pepper = _databaseSetupService.GeneratePasswordPepper();

            return Page();
        }

        public IActionResult OnPost()
        {
            if (_databaseSetupService.HasPasswordPepper())
            {
                return RedirectToPage("/Account/Login");
            }

            if (string.IsNullOrWhiteSpace(Pepper))
            {
                ErrorMessage = "Bitte geben Sie einen PasswordPepper ein.";

                return Page();
            }

            var result = _databaseSetupService.CreatePasswordPepper(Pepper);

            if (!result)
            {
                ErrorMessage = _databaseSetupService.ErrorMessage;
                return Page();
            }

            HttpContext.Session.SetString(PepperWasStoredInSessionKey, "true");

            return RedirectToPage("/Setup/Database");
        }
    }
}