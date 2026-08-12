using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Schoofs_ATaskO.Services;
using Schoofs_ATaskO.ViewModels;

namespace Schoofs_ATaskO.Pages.Admin.Users
{
    [Authorize(Roles = "Admin")]
    public class EditModel : PageModel
    {
        private readonly IUserService _userService;

        [BindProperty]
        public UserEditViewModel Input { get; set; } = new();

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
                ErrorMessage = string.IsNullOrWhiteSpace(
                    _userService.ErrorMessage)
                    ? "Benutzer wurde nicht gefunden."
                    : _userService.ErrorMessage;

                return RedirectToPage("./UserManagement");
            }

            Input = new UserEditViewModel
            {
                UserId = user.UserId,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                IsAdmin = user.IsAdmin,
                CompletedTasks = user.CompletedTasks
            };

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
                ErrorMessage = string.IsNullOrWhiteSpace(
                    _userService.ErrorMessage)
                    ? "Benutzer wurde nicht gefunden."
                    : _userService.ErrorMessage;

                return Page();
            }

            existingUser.FirstName = Input.FirstName;
            existingUser.LastName = Input.LastName;
            existingUser.Email = Input.Email;
            existingUser.IsAdmin = Input.IsAdmin;
            existingUser.CompletedTasks = Input.CompletedTasks;

            var result = _userService.UpdateUser(existingUser);

            if (!result)
            {
                ErrorMessage = string.IsNullOrWhiteSpace(
                    _userService.ErrorMessage)
                    ? "Der Benutzer konnte nicht aktualisiert werden."
                    : _userService.ErrorMessage;

                return Page();
            }

            return RedirectToPage("./UserManagement");
        }
    }
}