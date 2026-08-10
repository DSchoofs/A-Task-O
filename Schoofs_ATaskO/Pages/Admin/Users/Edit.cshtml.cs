using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Schoofs_ATaskO.Models;
using Schoofs_ATaskO.Services;

namespace Schoofs_ATaskO.Pages.Admin.Users
{
    [Authorize(Roles = "Admin")]
    public class EditModel : PageModel
    {
        private readonly IUserService _userService;

        [BindProperty]
        public User Input { get; set; } = new();

        public string ErrorMessage { get; set; } = string.Empty;

        public EditModel(IUserService userService)
        {
            _userService = userService;
        }

        public IActionResult OnGet(int id)
        {
            var user = _userService.GetUserById(id);

            if (user == null)
            {
                ErrorMessage = string.IsNullOrWhiteSpace(_userService.ErrorMessage)
                    ? "Benutzer wurde nicht gefunden."
                    : _userService.ErrorMessage;

                return RedirectToPage("./UserManagement");
            }

            Input = user;
            return Page();
        }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var existingUser = _userService.GetUserById(Input.UserId);

            if (existingUser == null)
            {
                ErrorMessage = "Benutzer wurde nicht gefunden.";
                return Page();
            }

            existingUser.FirstName = Input.FirstName;
            existingUser.LastName = Input.LastName;
            existingUser.Email = Input.Email;
            existingUser.IsAdmin = Input.IsAdmin;
            existingUser.CompletedTasks = Input.CompletedTasks;
            existingUser.LastLogin = Input.LastLogin;

            var result = _userService.UpdateUser(existingUser);

            if (!result)
            {
                ErrorMessage = _userService.ErrorMessage;
                return Page();
            }

            return RedirectToPage("./UserManagement");
        }
    }
}