using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Schoofs_ATaskO.Services;
using Schoofs_ATaskO.ViewModels;

namespace Schoofs_ATaskO.Pages.Setup
{
    public class InitialAdminModel : PageModel
    {
        private readonly ISetupService _setupService;

        [BindProperty]
        public InitialAdminViewModel Input { get; set; } = new();

        public string ErrorMessage { get; set; } = string.Empty;

        public InitialAdminModel(ISetupService setupService)
        {
            _setupService = setupService;
        }

        public IActionResult OnGet()
        {
            if (!_setupService.IsInitialSetupRequired())
            {
                return RedirectToPage("/Account/Login");
            }

            return Page();
        }

        public IActionResult OnPost()
        {
            if (!_setupService.IsInitialSetupRequired())
            {
                return RedirectToPage("/Account/Login");
            }

            if (!ModelState.IsValid)
            {
                return Page();
            }

            var result = _setupService.CreateInitialAdmin(Input);

            if (!result)
            {
                ErrorMessage = _setupService.ErrorMessage;
                return Page();
            }

            return RedirectToPage("/Account/Login");
        }
    }
}