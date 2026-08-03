using System.ComponentModel.DataAnnotations;

namespace Schoofs_ATaskO.ViewModels
{
    public class UserCreateViewModel
    {
        [Required(ErrorMessage = "Der Vorname ist ein Pflichtfeld.")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Der Nachname ist ein Pflichtfeld.")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Die E-Mail-Adresse ist ein Pflichtfeld.")]
        [EmailAddress(ErrorMessage = "Bitte gib eine gültige E-Mail-Adresse ein.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Das Passwort ist ein Pflichtfeld.")]
        [DataType(DataType.Password)]
        public string PlainPassword { get; set; } = string.Empty;

        public bool IsAdmin { get; set; }
    }
}