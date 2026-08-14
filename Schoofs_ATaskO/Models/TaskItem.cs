using Schoofs_ATaskO.Models;
using System.ComponentModel.DataAnnotations;

public class TaskItem
{
    public int Id { get; set; }

    [Display(Name = "Name")]
    public string Name { get; set; } = string.Empty;

    [Display(Name = "Beschreibung")]
    public string? Description { get; set; }

    [Display(Name = "Intervall")]
    public TaskInterval Interval { get; set; }

    [Display(Name = "Aktiv")]
    public bool IsActive { get; set; } = true;

    [Display(Name = "Erstellt am")]
    public DateTime CreatedAt { get; set; }

    [Display(Name = "Erstellt von")]
    public int? CreatedByUserId { get; set; }

    [Display(Name = "Geändert am")]
    public DateTime? ModifiedAt { get; set; }

    [Display(Name = "Geändert von")]
    public int? ModifiedByUserId { get; set; }

    public ICollection<TaskBlock> TaskBlocks { get; set; } = new List<TaskBlock>();
    public TaskWeekMode WeekMode { get; set; } = TaskWeekMode.Weekdays;
    public User? CreatedByUser { get; set; }
    public User? ModifiedByUser { get; set; }
}