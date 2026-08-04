using System.ComponentModel.DataAnnotations;
using Schoofs_ATaskO.Models;

namespace Schoofs_ATaskO.ViewModels
{
    public class CreateTaskInputModel
    {
        [Required(ErrorMessage = "Bitte gib einen Aufgabennamen ein.")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Der Aufgabenname muss zwischen 3 und 100 Zeichen lang sein.")]
        [Display(Name = "Aufgabenname")]
        public string TaskName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Bitte wähle ein Intervall aus.")]
        [Display(Name = "Intervall")]
        public TaskInterval? TaskInterval { get; set; }
    }
}