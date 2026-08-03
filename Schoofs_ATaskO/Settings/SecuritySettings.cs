using System.ComponentModel.DataAnnotations;

namespace Schoofs_ATaskO.Settings
{
    public class SecuritySettings
    {
        public const string SectionName = "Security";

        [Required(ErrorMessage = "PasswordPepper muss gesetzt sein.")]
        public string PasswordPepper { get; set; } = "";
    }
}
