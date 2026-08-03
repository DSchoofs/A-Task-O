using System.ComponentModel.DataAnnotations;

namespace Schoofs_ATaskO.Viewmodels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Bitte eine E-Mail eingeben.")]
        [EmailAddress(ErrorMessage = "Bitte eine gültige E-Mail-Adresse eingeben.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Bitte ein Passwort eingeben.")]
        public string Password { get; set; } = string.Empty;
    }
}
