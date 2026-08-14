using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Schoofs_ATaskO.Services;

namespace Schoofs_ATaskO.Pages.Setup
{
    public class PepperModel : PageModel
    {
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
                return RedirectToPage("/Index");
            }

            Pepper = _setupService.GeneratePasswordPepper();

            return Page();
        }

        public IActionResult OnPost()
        {
            if (_setupService.HasPasswordPepper())
            {
                return RedirectToPage("/Account/Login");
            }

            if (string.IsNullOrWhiteSpace(Pepper))
            {

                ErrorMessage ="Bitte geben Sie einen PasswordPepper ein.";

                return Page();
            }

            var result = _setupService.CreatePasswordPepper(Pepper);

            if (!result)
            {
                ErrorMessage = _setupService.ErrorMessage;
                return Page();
            }

            SuccessMessage = "Der PasswordPepper wurde erfolgreich gespeichert. " +
                             "Beenden Sie die Anwendung jetzt und starten Sie sie einmal neu. " +
                             "Danach wird die Einrichtung automatisch fortgesetzt.";

            return Page();
        }
    }
}