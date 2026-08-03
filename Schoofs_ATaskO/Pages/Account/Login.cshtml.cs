using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Schoofs_ATaskO.Repositories;

namespace Schoofs_ATaskO.Pages.Account
{
    public class LoginModel : PageModel
    {
        private readonly IUserRepository _userRepository;

        [BindProperty]
        public InputModel Input { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public string? ReturnUrl { get; set; }

        public string ErrorMessage { get; set; } = string.Empty;

        public LoginModel(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var user = _userRepository.GetUserByEmail(Input.Email);

            if (user == null || !_userRepository.ValidatePassword(user, Input.Password))
            {
                ErrorMessage = "Ungültige E-Mail oder ungültiges Passwort.";
                return Page();
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Name, $"{user.FirstName} {user.LastName}"),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.IsAdmin ? "Admin" : "User")
            };

            var identity = new ClaimsIdentity(
                claims,
                CookieAuthenticationDefaults.AuthenticationScheme);

            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                new AuthenticationProperties
                {
                    IsPersistent = false,
                    ExpiresUtc = DateTime.UtcNow.AddHours(8)
                });

            if (!string.IsNullOrWhiteSpace(ReturnUrl) && Url.IsLocalUrl(ReturnUrl))
            {
                return LocalRedirect(ReturnUrl);
            }

            return RedirectToPage("/Index");
        }

        public class InputModel
        {
            [Required(ErrorMessage = "Bitte eine E-Mail eingeben.")]
            [EmailAddress(ErrorMessage = "Bitte eine gültige E-Mail-Adresse eingeben.")]
            public string Email { get; set; } = string.Empty;

            [Required(ErrorMessage = "Bitte ein Passwort eingeben.")]
            public string Password { get; set; } = string.Empty;
        }
    }
}