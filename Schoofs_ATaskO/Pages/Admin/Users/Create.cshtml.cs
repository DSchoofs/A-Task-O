using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Schoofs_ATaskO.Models;
using Schoofs_ATaskO.Repositories;

namespace Schoofs_ATaskO.Pages.Admin.Users
{
    public class CreateModel : PageModel
    {
        private readonly IUserRepository _userRepository;

        [BindProperty]
        public User InputUser { get; set; } = new();

        [BindProperty]
        public string PlainPassword { get; set; } = string.Empty;

        public string ErrorMessage { get; set; } = string.Empty;

        public CreateModel(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public void OnGet()
        {
        }

        public IActionResult OnPost()
        {
            if (string.IsNullOrWhiteSpace(PlainPassword))
            {
                ModelState.AddModelError(string.Empty, "Bitte ein Passwort eingeben.");
            }

            if (!ModelState.IsValid)
            {
                return Page();
            }

            InputUser.PasswordHash = PlainPassword;

            var result = _userRepository.AddUser(InputUser);

            if (!result)
            {
                ErrorMessage = _userRepository.ErrorMessage;
                return Page();
            }

            return RedirectToPage("./UserManagement");
        }
    }
}