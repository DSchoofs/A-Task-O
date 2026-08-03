using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Schoofs_ATaskO.Models;
using Schoofs_ATaskO.Services;
using System.ComponentModel.DataAnnotations;

namespace Schoofs_ATaskO.Pages.Admin.Users
{
    public class CreateModel : PageModel
    {
        private readonly IUserService _userService;

        [BindProperty]
        public User InputUser { get; set; } = new();

        [BindProperty]
        [Required(ErrorMessage = "Bitte ein Passwort eingeben.")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Das Passwort muss mindestens 6 Zeichen lang sein.")]
        public string PlainPassword { get; set; } = string.Empty;

        public string ErrorMessage { get; set; } = string.Empty;

        public CreateModel(IUserService userService)
        {
            _userService = userService;
        }

        public void OnGet()
        {
            InputUser = new User
            {
                IsAdmin = false,
                CompletedTasks = 0,
                LastLogin = null
            };
        }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var result = _userService.CreateUser(InputUser, PlainPassword);

            if (!result)
            {
                ErrorMessage = _userService.ErrorMessage;
                return Page();
            }

            return RedirectToPage("./UserManagement");
        }
    }
}