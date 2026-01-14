#nullable disable

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using GlamStudio.Models;

namespace GlamStudio.Areas.Identity.Pages.Account
{
    public class LoginModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<LoginModel> _logger;

        public LoginModel(SignInManager<ApplicationUser> signInManager, ILogger<LoginModel> logger)
        {
            _signInManager = signInManager;
            _logger = logger;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        public string ReturnUrl { get; set; }


        public string LoginType { get; set; }

        [TempData]
        public string ErrorMessage { get; set; }

        public class InputModel
        {
            [Required(ErrorMessage = "Email jest wymagany.")]
            [EmailAddress(ErrorMessage = "Nieprawidłowy adres email.")]
            [Display(Name = "Email")]
            public string Email { get; set; }

            [Required(ErrorMessage = "Hasło jest wymagane.")]
            [DataType(DataType.Password)]
            [Display(Name = "Hasło")]
            public string Password { get; set; }

            [Display(Name = "Zapamiętaj mnie")]
            public bool RememberMe { get; set; }
        }


        public async Task OnGetAsync(string returnUrl = null, string type = null)
        {
            if (!string.IsNullOrEmpty(ErrorMessage))
            {
                ModelState.AddModelError(string.Empty, ErrorMessage);
            }

            ReturnUrl = returnUrl ?? Url.Content("~/");


            LoginType = type?.ToLower();

 
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);


            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
        }


        public async Task<IActionResult> OnPostAsync(string returnUrl = null, string type = null)
        {
            ReturnUrl = returnUrl ?? Url.Content("~/");
            LoginType = type?.ToLower();

            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(Input.Email, Input.Password, Input.RememberMe, lockoutOnFailure: false);

                if (result.Succeeded)
                {
                    _logger.LogInformation($"User logged in as {LoginType ?? "standard"}.");

                    if (LoginType == "salon")
                    {
                        return RedirectToAction("Index", "Home", new { area = "Salon" });
                    }
                    else if (LoginType == "client")
                    {
                        return RedirectToAction("Index", "Home", new { area = "Client" });
                    }

                    return LocalRedirect(ReturnUrl);
                }

                if (result.RequiresTwoFactor)
                {
                    return RedirectToPage("./LoginWith2fa", new { ReturnUrl = ReturnUrl, RememberMe = Input.RememberMe });
                }

                if (result.IsLockedOut)
                {
                    _logger.LogWarning("User account locked out.");
                    return RedirectToPage("./Lockout");
                }

                ModelState.AddModelError(string.Empty, "Nieprawidłowa próba logowania.");
            }


            return Page();
        }
    }
}
