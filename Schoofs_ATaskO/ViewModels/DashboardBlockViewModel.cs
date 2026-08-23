namespace Schoofs_ATaskO.ViewModels
{
    public class DashboardBlockViewModel
    {
        public int TaskId { get; set; }

        public int BlockId { get; set; }

        public string TaskName { get; set; } = string.Empty;

        public DateTime Start { get; set; }

        public DateTime End { get; set; }

        public string DisplayTime { get; set; } = string.Empty;

        public string StatusText { get; set; } = string.Empty;

        public string CssClass { get; set; } = string.Empty;

        public int? AssignedByUserId { get; set; }

        public string AssignedByUserName { get; set; } = string.Empty;
    }
}