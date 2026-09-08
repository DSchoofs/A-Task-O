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

        private readonly ISetupService _setupService;

        public PepperModel(ISetupService setupService)
        {
            _setupService = setupService;
        }

        public IActionResult OnGet()
        {
            if (_setupService.HasPasswordPepper())
            {
                return RedirectToPage("/Setup/Database");
            }

            Pepper = _setupService.GeneratePasswordPepper();

            return Page();
        }

        public IActionResult OnPost()
        {
            if (_setupService.HasPasswordPepper())
            {
                return RedirectToPage("/Setup/Database");
            }

            if (string.IsNullOrWhiteSpace(Pepper))
            {
                ErrorMessage = "Bitte geben Sie einen PasswordPepper ein.";

                return Page();
            }

            var result = _setupService.CreatePasswordPepper(Pepper);

            if (!result)
            {
                ErrorMessage = _setupService.ErrorMessage;
                return Page();
            }

            HttpContext.Session.SetString(PepperWasStoredInSessionKey, "true");

            return RedirectToPage("/Setup/Database");
        }
    }
}