using System.ComponentModel.DataAnnotations;

namespace Schoofs_ATaskO.Models
{
    public enum TaskInterval
    {
        [Display(Name = "Minütlich")]
        Minutely = 1,

        [Display(Name = "Stündlich")]
        Hourly = 2,

        [Display(Name = "Täglich")]
        Daily = 3,

        [Display(Name = "Wöchentlich")]
        Weekly = 4,

        [Display(Name = "Monatlich")]
        Monthly = 5,

        [Display(Name = "Jährlich")]
        Yearly = 6
    }
}