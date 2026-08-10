using System.ComponentModel.DataAnnotations;

namespace Schoofs_ATaskO.Models
{
    public enum TaskInterval
    {
        [Display(Name = "Täglich")]
        Daily = 3,

        [Display(Name = "Wöchentlich")]
        Weekly = 4,

        [Display(Name = "Monatlich")]
        Monthly = 5,
    }
}