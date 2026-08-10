using System.ComponentModel.DataAnnotations;

namespace Schoofs_ATaskO.Models
{
    public enum TaskWeekMode
    {
        [Display(Name = "Montag bis Freitag")]
        Weekdays,

        [Display(Name = "Montag bis Samstag")]
        MondayToSaturday,

        [Display(Name = "Montag bis Sonntag")]
        FullWeek
    }
}