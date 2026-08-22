using System.ComponentModel.DataAnnotations;

namespace Schoofs_ATaskO.Models
{
    public class User
    {
        public int UserId { get; set; }
        [Required(ErrorMessage = "Der Vorname ist ein Pflichtfeld.")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Der Vorname muss zwischen 2 und 100 Zeichen lang sein.")]
        public string FirstName { get; set; } = "";

        [Required(ErrorMessage = "Der Nachname ist ein Pflichtfeld.")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Der Nachname muss zwischen 2 und 100 Zeichen lang sein.")]
        public string LastName { get; set; } = "";

        [Required(ErrorMessage = "Die E-Mail ist ein Pflichtfeld.")]
        [EmailAddress(ErrorMessage = "Bitte eine gültige E-Mail-Adresse eingeben.")]
        [StringLength(255, ErrorMessage = "Die E-Mail darf maximal 255 Zeichen lang sein.")]
        public string Email { get; set; } = "";
        public string PasswordHash { get; set; } = "";
        public string PasswordSalt { get; set; } = "";
        public bool IsAdmin { get; set; }
        public DateTime? LastLogin { get; set; }
        public int? CompletedTasks { get; set; }
        public ICollection<TaskItem> CreatedTaskItems { get; set; } = new List<TaskItem>();
        public ICollection<TaskItem> ModifiedTaskItems { get; set; } = new List<TaskItem>();
        public ICollection<TaskBlock> AssignedTaskBlocks { get; set; } = new List<TaskBlock>();
        public ICollection<TaskBlock> CompletedTaskBlocks { get; set; } = new List<TaskBlock>();
    }
}