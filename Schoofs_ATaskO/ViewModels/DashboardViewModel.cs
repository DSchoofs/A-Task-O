namespace Schoofs_ATaskO.ViewModels
{
    public class DashboardViewModel
    {
        public string UserName { get; set; } = string.Empty;

        public DateTime Today { get; set; }

        public List<DashboardBlockViewModel> OverdueBlocks { get; set; } = new List<DashboardBlockViewModel>();

        public List<DashboardBlockViewModel> TodayBlocks { get; set; } = new List<DashboardBlockViewModel>();

        public List<DashboardBlockViewModel> UpcomingBlocks { get; set; } = new List<DashboardBlockViewModel>();
    }
}