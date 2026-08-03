using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Schoofs_ATaskO.Models;
using Schoofs_ATaskO.Services;

namespace Schoofs_ATaskO.Pages.Admin.Users
{
    [Authorize(Roles = "Admin")]
    public class DeleteModel : PageModel
    {
        private readonly IUserService _userService;

        [BindProperty]
        public User UserToDelete { get; set; } = new();

        public string ErrorMessage { get; set; } = string.Empty;

        public DeleteModel(IUserService userService)
        {
            _userService = userService;
        }

        public IActionResult OnGet(int id)
        {
            var user = _userService.GetUserById(id);

            if (user == null)
            {
                return RedirectToPage("./UserManagement");
            }

            UserToDelete = user;
            return Page();
        }

        public IActionResult OnPost()
        {
            var result = _userService.DeleteUserById(UserToDelete.UserId);

            if (!result)
            {
                ErrorMessage = _userService.ErrorMessage;

                var user = _userService.GetUserById(UserToDelete.UserId);
                if (user != null)
                {
                    UserToDelete = user;
                }

                return Page();
            }

            return RedirectToPage("./UserManagement");
        }
    }
}