using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Schoofs_ATaskO.Models;
using Schoofs_ATaskO.Services;

namespace Schoofs_ATaskO.Pages.Admin.Users
{
    [Authorize(Roles = "Admin")]
    public class UserManagementModel : PageModel
    {
        private readonly IUserService _userService;

        public List<User> Users { get; set; } = new();
        public string ErrorMessage { get; set; } = string.Empty;

        public UserManagementModel(IUserService userService)
        {
            _userService = userService;
        }

        public void OnGet()
        {
            Users = _userService.GetAllUsers();
            ErrorMessage = _userService.ErrorMessage;
        }
    }
}