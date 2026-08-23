using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Schoofs_ATaskO.Demo;

namespace Schoofs_ATaskO.Pages
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly IDemoDataService _demoDataService;

        public bool CanCreateDemoData { get; set; }

        public string SuccessMessage { get; set; } = string.Empty;

        public string ErrorMessage { get; set; } = string.Empty;

        public IndexModel(IDemoDataService demoDataService)
        {
            _demoDataService = demoDataService;
        }

        public void OnGet()
        {
            CanCreateDemoData = User.IsInRole("Admin") && _demoDataService.CanCreateDemoData();
        }

        
        public void OnPost()
        {
            CanCreateDemoData = _demoDataService.CanCreateDemoData();

            if (!CanCreateDemoData)
            {
                ErrorMessage = _demoDataService.ErrorMessage;

                return;
            }

            var success = _demoDataService.CreateDemoData();

            if (success)
            {
                SuccessMessage = "Die Demo-Daten wurden erfolgreich erstellt.";

                CanCreateDemoData = false;
            }
            else
            {
                ErrorMessage = _demoDataService.ErrorMessage;
            }
        }
    }
}