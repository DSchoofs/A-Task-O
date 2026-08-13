using Schoofs_ATaskO.Models;

namespace Schoofs_ATaskO.ViewModels
{
    public class TaskBlockViewModel
    {
        public int Id { get; set; }
        public int TaskItemId { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public TaskBlockStatus Status { get; set; }
        public string StatusText { get; set; } = string.Empty;
        public string CssClass { get; set; } = string.Empty;
        public int? AssignedByUserId { get; set; }
        public string AssignedByUserName { get; set; } = string.Empty;
        public DateTime? AssignedAt { get; set; }
        public int? CompletedByUserId { get; set; }
        public string CompletedByUserName { get; set; } = string.Empty;
        public DateTime? CompletedAt { get; set; }
        public string? Note { get; set; }
        public bool IsSelected { get; set; }
        public bool IsOverdue { get; set; }
        public bool IsCompleted { get; set; }
        public bool CanBeTakenOver { get; set; }
        public bool CanBeCompleted { get; set; }
        public bool CanBeReleased { get; set; }
        public bool CanBeReopened { get; set; }
        public bool CanBeAdminReleased { get; set; }
        public string DisplayTime { get; set; } = string.Empty;
        public bool ContinuesFromPreviousPeriod { get; set; }
        public bool ContinuesIntoNextPeriod { get; set; }
    }
}