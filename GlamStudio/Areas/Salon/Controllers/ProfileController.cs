using GlamStudio.Areas.Salon.Models;
using GlamStudio.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace GlamStudio.Areas.Salon.Controllers
{
    [Area("Salon")]
    [Authorize(Roles = "Admin,Employee")]
    public class ProfileController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public ProfileController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        // GET: Salon/Profile
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account", new { area = "Identity" });

            var model = new SalonProfileViewModel
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Address = user.Address,
                City = user.City
            };

            return View(model);
        }

        // GET: Salon/Profile/Edit
        public async Task<IActionResult> Edit()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            var model = new SalonProfileViewModel
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Address = user.Address,
                City = user.City
            };

            return View(model);
        }

        // POST: Salon/Profile/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(SalonProfileViewModel model)
        {
            ModelState.Remove("Email");
            if (!ModelState.IsValid) return View(model);

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.PhoneNumber = model.PhoneNumber;
            user.Address = model.Address;
            user.City = model.City;

            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "Profil został zaktualizowany.";
                return RedirectToAction(nameof(Index));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            return View(model);
        }

        // GET: Salon/Profile/ChangePassword
        public IActionResult ChangePassword()
        {
            return View();
        }

        // POST: Salon/Profile/ChangePassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(SalonChangePasswordViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);

            if (result.Succeeded)
            {
                await _signInManager.RefreshSignInAsync(user);
                TempData["SuccessMessage"] = "Hasło zostało zmienione pomyślnie.";
                return RedirectToAction(nameof(Index));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            return View(model);
        }
    }
}