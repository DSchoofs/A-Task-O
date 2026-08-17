using System.ComponentModel.DataAnnotations;

namespace Schoofs_ATaskO.ViewModels
{
    public class DatabaseConfigurationViewModel
    {
        public bool UseDefaultConfiguration { get; set; } = true;

        [Display(Name = "Server oder Instanz")]
        [Required(ErrorMessage = "Bitte einen Server oder eine Instanz angeben.")]
        public string ServerName { get; set; } = @"(localdb)\MSSQLLocalDB";

        [Display(Name = "Authentifizierung")]
        [Required(ErrorMessage = "Bitte eine Authentifizierungsart auswählen.")]
        public string AuthenticationMode { get; set; } = "Windows";

        [Display(Name = "Benutzername")]
        public string? Username { get; set; }

        [Display(Name = "Passwort")]
        public string? Password { get; set; }

        public string DatabaseName { get; set; } = "ATaskO";
    }
}