using System.ComponentModel.DataAnnotations;

namespace Schoofs_ATaskO.Models
{
    public enum TaskBlockStatus
    {
        [Display(Name = "Frei")]
        Free,

        [Display(Name = "Übernommen")]
        Assigned,

        [Display(Name = "Erledigt")]
        Completed,

        [Display(Name = "Überfällig")]
        Overdue
    }
}
