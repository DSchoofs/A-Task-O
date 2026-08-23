using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Schoofs_ATaskO.Demo;
using Schoofs_ATaskO.Services;
using Schoofs_ATaskO.ViewModels;

namespace Schoofs_ATaskO.Pages
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly IDemoDataService _demoDataService;
        private readonly IDashboardService _dashboardService;

        public DashboardViewModel Dashboard { get; private set; } = new DashboardViewModel();

        public bool CanCreateDemoData { get; set; }

        public string SuccessMessage { get; set; } = string.Empty;

        public string ErrorMessage { get; set; } = string.Empty;

        public IndexModel(
            IDemoDataService demoDataService,
            IDashboardService dashboardService)
        {
            _demoDataService = demoDataService;
            _dashboardService = dashboardService;
        }

        public void OnGet()
        {
            LoadDashboard();

            CanCreateDemoData = User.IsInRole("Admin") &&
                                _demoDataService.CanCreateDemoData();
        }

        public void OnPost()
        {
            if (!User.IsInRole("Admin"))
            {
                ErrorMessage = "Nur Administratoren dürfen Demo-Daten anlegen.";

                LoadDashboard();

                return;
            }

            CanCreateDemoData = _demoDataService.CanCreateDemoData();

            if (!CanCreateDemoData)
            {
                ErrorMessage = _demoDataService.ErrorMessage;

                LoadDashboard();

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

            LoadDashboard();
        }

        private void LoadDashboard()
        {
            Dashboard = _dashboardService.GetDashboard();
        }
    }
}