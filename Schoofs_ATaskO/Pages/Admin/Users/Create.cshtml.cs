using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Schoofs_ATaskO.Models;
using Schoofs_ATaskO.Services;
using Schoofs_ATaskO.ViewModels;

namespace Schoofs_ATaskO.Pages.Admin.Users
{
    [Authorize(Roles = "Admin")]
    public class CreateModel : PageModel
    {
        private readonly IUserService _userService;

        [BindProperty]
        public UserCreateViewModel Input { get; set; } = new();

        public string ErrorMessage { get; set; } = string.Empty;

        public CreateModel(IUserService userService)
        {
            _userService = userService;
        }

        public void OnGet()
        {
        }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var user = new User
            {
                FirstName = Input.FirstName,
                LastName = Input.LastName,
                Email = Input.Email,
                IsAdmin = Input.IsAdmin,
                CompletedTasks = 0,
                LastLogin = null
            };

            var result = _userService.CreateUser(user, Input.PlainPassword);

            if (!result)
            {
                ErrorMessage = _userService.ErrorMessage;
                return Page();
            }

            return RedirectToPage("./UserManagement");
        }
    }
}