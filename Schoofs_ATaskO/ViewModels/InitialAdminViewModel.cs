using System.ComponentModel.DataAnnotations;

namespace Schoofs_ATaskO.ViewModels
{
    public class InitialAdminViewModel
    {
        [Required(ErrorMessage = "Bitte eine E-Mail-Adresse eingeben.")]
        [EmailAddress(ErrorMessage = "Bitte eine gültige E-Mail-Adresse eingeben.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Bitte ein Passwort eingeben.")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Bitte das Passwort wiederholen.")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Die Passwörter stimmen nicht überein.")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}