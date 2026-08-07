using Schoofs_ATaskO.Models;
using System.ComponentModel.DataAnnotations;

namespace Schoofs_ATaskO.ViewModels;

public class TaskCreateViewModel
{
    [Required(ErrorMessage = "Der Name ist ein Pflichtfeld.")]
    [StringLength(200, ErrorMessage = "Der Name darf maximal 200 Zeichen lang sein.")]
    [Display(Name = "Name")]
    public string Name { get; set; } = string.Empty;

    [StringLength(2000, ErrorMessage = "Die Beschreibung darf maximal 2000 Zeichen lang sein.")]
    [Display(Name = "Beschreibung")]
    public string? Description { get; set; }

    [Required(ErrorMessage = "Bitte wählen Sie ein Intervall aus.")]
    [Display(Name = "Intervall")]
    public TaskInterval Interval { get; set; }

    [Display(Name = "Aktiv")]
    public bool IsActive { get; set; } = true;
}