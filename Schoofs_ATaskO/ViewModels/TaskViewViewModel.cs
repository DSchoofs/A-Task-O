using Schoofs_ATaskO.Models;

namespace Schoofs_ATaskO.ViewModels
{
    public class TaskViewViewModel
    {
        public int TaskId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public TaskInterval Interval { get; set; }
        public TaskWeekMode WeekMode { get; set; }
        public bool IsActive { get; set; }
        public DateTime FocusDate { get; set; }
        public DateTime PreviousFocusDate { get; set; }
        public DateTime NextFocusDate { get; set; }
        public DateTime CurrentPeriodStart { get; set; }
        public DateTime CurrentPeriodEnd { get; set; }
        public string CurrentPeriodLabel { get; set; } = string.Empty;
        public List<TaskBlockViewModel> PreviousBlocks { get; set; } = new List<TaskBlockViewModel>();
        public List<TaskBlockViewModel> CurrentBlocks { get; set; } = new List<TaskBlockViewModel>();
        public List<TaskBlockViewModel> NextBlocks { get; set; } = new List<TaskBlockViewModel>();
        public TaskBlockViewModel? SelectedBlock { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
        public string PreviousPeriodTitle { get; set; } = string.Empty;
        public string CurrentPeriodTitle { get; set; } = string.Empty;
        public string NextPeriodTitle { get; set; } = string.Empty;
        public string PreviousPeriodDateRange { get; set; } = string.Empty;
        public string CurrentPeriodDateRange { get; set; } = string.Empty;
        public string NextPeriodDateRange { get; set; } = string.Empty;
        public string PreviousCalendarWeek { get; set; } = string.Empty;
        public string CurrentCalendarWeek { get; set; } = string.Empty;
        public string NextCalendarWeek { get; set; } = string.Empty;
    }
}