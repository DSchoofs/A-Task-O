using Schoofs_ATaskO.Models;

namespace Schoofs_ATaskO.ViewModels;

public class TaskManagementViewModel
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public TaskInterval Interval { get; set; }
    public TaskWeekMode WeekMode { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public string CreatedByUserName { get; set; } = "Unbekannt";

    public DateTime? ModifiedAt { get; set; }

    public string ModifiedByUserName { get; set; } = "Noch nicht geändert";
}