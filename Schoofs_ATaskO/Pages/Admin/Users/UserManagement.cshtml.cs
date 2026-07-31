using Microsoft.AspNetCore.Mvc.RazorPages;
using Schoofs_ATaskO.Models;
using Schoofs_ATaskO.Repositories;
using System;
using System.Collections.Generic;

namespace Schoofs_ATaskO.Pages.Admin.Users
{
    public class UserManagementModel : PageModel
    {
        private readonly IUserRepository _userRepository;

        public List<User> Users { get; set; } = new();
        public string ErrorMessage { get; set; } = string.Empty;

        public UserManagementModel(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public void OnGet()
        {
            Users = _userRepository.GetAllUsers();
            ErrorMessage = _userRepository.ErrorMessage;
        }
    }
}