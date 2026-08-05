using Schoofs_ATaskO.Models;
using System.ComponentModel.DataAnnotations;

public class TaskBlock
{
    public int Id { get; set; }

    [Display(Name = "Aufgabe")]
    public int TaskItemId { get; set; }

    public TaskItem? TaskItem { get; set; }

    [Display(Name = "Start")]
    public DateTime Start { get; set; }

    [Display(Name = "Ende")]
    public DateTime End { get; set; }

    [Display(Name = "Status")]
    public TaskBlockStatus Status { get; set; } = TaskBlockStatus.Free;

    [Display(Name = "Übernommen von")]
    public int? AssignedByUserId { get; set; }

    [Display(Name = "Übernommen am")]
    public DateTime? AssignedAt { get; set; }

    [Display(Name = "Erledigt von")]
    public int? CompletedByUserId { get; set; }

    [Display(Name = "Erledigt am")]
    public DateTime? CompletedAt { get; set; }

    [Display(Name = "Anmerkung")]
    public string? Note { get; set; }
}