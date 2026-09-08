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

        public async Task<IActionResult> OnGetAsync()
        {
            if (!_setupService.HasPasswordPepper())
            {
                return RedirectToPage("/Setup/Pepper");
            }

            if (!await _setupService.IsDatabaseReadyAsync())
            {
                return RedirectToPage("/Setup/Database");
            }

            if (!_setupService.IsInitialSetupRequired())
            {
                return RedirectToPage("/Account/Login");
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!_setupService.HasPasswordPepper())
            {
                return RedirectToPage("/Setup/Pepper");
            }

            if (!await _setupService.IsDatabaseReadyAsync())
            {
                return RedirectToPage("/Setup/Database");
            }

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

            var setupStatus = HttpContext.RequestServices.GetRequiredService<ISetupStatusService>();

            setupStatus.Refresh();

            return RedirectToPage("/Account/Login");
        }
    }
}