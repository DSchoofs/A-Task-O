using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Schoofs_ATaskO.Services;
using Schoofs_ATaskO.ViewModels;

namespace Schoofs_ATaskO.Pages.Setup
{
    public class InitialAdminModel : PageModel
    {
        private readonly ISetupService _databaseSetupService;

        [BindProperty]
        public InitialAdminViewModel Input { get; set; } = new();

        public string ErrorMessage { get; set; } = string.Empty;

        public InitialAdminModel(ISetupService setupService)
        {
            _databaseSetupService = setupService;
        }

        public IActionResult OnGet()
        {
            if (!_databaseSetupService.HasPasswordPepper())
            {
                return RedirectToPage("/Setup/Pepper");
            }

            if (!_databaseSetupService.IsInitialSetupRequired())
            {
                return RedirectToPage("/Account/Login");
            }

            return Page();
        }

        public IActionResult OnPost()
        {
            if (!_databaseSetupService.HasPasswordPepper())
            {
                return RedirectToPage("/Setup/Pepper");
            }

            if (!_databaseSetupService.IsInitialSetupRequired())
            {
                return RedirectToPage("/Account/Login");
            }

            if (!ModelState.IsValid)
            {
                return Page();
            }

            var result = _databaseSetupService.CreateInitialAdmin(Input);

            if (!result)
            {
                ErrorMessage = _databaseSetupService.ErrorMessage;
                return Page();
            }

            return RedirectToPage("/Account/Login");
        }
    }
}