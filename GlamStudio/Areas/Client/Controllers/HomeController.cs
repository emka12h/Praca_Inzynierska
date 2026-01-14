using GlamStudio.Areas.Client.Models;
using GlamStudio.Data;
using GlamStudio.Models;
using GlamStudio.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GlamStudio.Areas.Client.Controllers
{
    [Area("Client")]
    public class HomeController : Controller
    {
        private readonly BeautySalonContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public HomeController(BeautySalonContext context, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [AllowAnonymous]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Contact()
        {
            var model = new ContactFormViewModel();

            // Jeśli użytkownik jest zalogowany, wstępnie wypełnij formularz
            if (User.Identity.IsAuthenticated)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user != null)
                {
                    model.SendersName = $"{user.FirstName} {user.LastName}";
                    model.SendersEmail = user.Email;
                    model.IsUserLoggedIn = true;
                }
            }

            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Contact(ContactFormViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);

                var message = new Message
                {
                    SendersName = model.SendersName,
                    SendersEmail = model.SendersEmail,
                    Contents = model.Contents,
                    DateSent = DateTime.Now,
                    IsReadBySalon = false,
                    ApplicationUserId = user?.Id
                };

                _context.Messages.Add(message);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Dziękujemy! Twoja wiadomość została wysłana.";

                return RedirectToAction("Contact");
            }

            model.IsUserLoggedIn = User.Identity.IsAuthenticated;
            return View(model);
        }

        [AllowAnonymous]
        public IActionResult AboutUs()
        {
            return View();
        }

        [Authorize(Roles = "Client")]
        public async Task<IActionResult> MyMessages()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge();
            }

            var messages = await _context.Messages
                .Where(w => w.ApplicationUserId == user.Id)
                .OrderByDescending(w => w.ReplyDate)
                .ToListAsync();

            return View(messages);
        }

        [HttpPost]
        [Authorize(Roles = "Client")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitClientReply(int messageId, string clientReply)
        {
            if (string.IsNullOrWhiteSpace(clientReply))
            {
                TempData["ErrorMessage"] = "Treść odpowiedzi nie może być pusta.";
                return RedirectToAction("MyMessages");
            }

            var user = await _userManager.GetUserAsync(User);
            var message = await _context.Messages
                .FirstOrDefaultAsync(m => m.MessageId == messageId && m.ApplicationUserId == user.Id);

            if (message == null)
            {
                return NotFound();
            }

            // nowy wpis do rozmowy
            string newReplyEntry =
                $"\n\n--- Odpowiedź Klienta ({user.FirstName} @ {DateTime.Now:g}) ---\n{clientReply}";

            // Dodanie odpowiedź klienta do historii
            message.ReplyContent += newReplyEntry;

            message.IsReadByClient = true;
            message.IsReadBySalon = false;

            _context.Update(message);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Twoja odpowiedź została wysłana.";
            return RedirectToAction("MyMessages");
        }

        //Moje wziyty
        [Authorize(Roles = "Client")]
        public async Task<IActionResult> MyVisits()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var visits = await _context.Appointments
                .Include(a => a.Service)
                .Include(a => a.Employee)
                .Where(a => a.ClientID == user.Id)
                .OrderByDescending(a => a.AppointmentDate)
                .ThenByDescending(a => a.AppointmentTime)
                .ToListAsync();

            return View(visits);
        }

        [HttpPost]
        [Authorize(Roles = "Client")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelAppointment(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var appointment = await _context.Appointments
                .FirstOrDefaultAsync(a => a.AppointmentID == id && a.ClientID == user.Id);

            if (appointment == null)
            {
                TempData["ErrorMessage"] = "Nie znaleziono wizyty lub nie masz do niej uprawnień.";
                return RedirectToAction("MyVisits");
            }

            if (appointment.Status == AppointmentStatus.Completed ||
                appointment.Status == AppointmentStatus.CancelledByClient ||
                appointment.Status == AppointmentStatus.CancelledBySalon)
            {
                TempData["ErrorMessage"] = "Tej wizyty nie można już anulować.";
                return RedirectToAction("MyVisits");
            }

            // Zmiana statusu
            appointment.Status = AppointmentStatus.CancelledByClient;

            _context.Update(appointment);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Wizyta została pomyślnie anulowana.";
            return RedirectToAction("MyVisits");
        }
        // GET: Client/Home/Profile
        [Authorize(Roles = "Client")]
        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var model = new UserProfileViewModel
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

        // GET: Client/Home/Settings
        [Authorize(Roles = "Client")]
        public async Task<IActionResult> Settings()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var model = new UserProfileViewModel
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

        // POST: Client/Home/Settings
        [HttpPost]
        [Authorize(Roles = "Client")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Settings(UserProfileViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            // Aktualizacja danych
            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.PhoneNumber = model.PhoneNumber;
            user.Address = model.Address;
            user.City = model.City;

            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "Twoje dane zostały zaktualizowane.";
                return RedirectToAction(nameof(Profile));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            return View(model);
        }

        [Authorize(Roles = "Client")]
        public IActionResult ChangePassword()
        {
            return View();
        }

        // POST: Client/Home/ChangePassword
        [HttpPost]
        [Authorize(Roles = "Client")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account", new { area = "Identity" });
            }

            // Próba zmiany hasła
            var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);

            if (result.Succeeded)
            {

                await _signInManager.RefreshSignInAsync(user);

                TempData["SuccessMessage"] = "Twoje hasło zostało zmienione pomyślnie.";
                return RedirectToAction(nameof(Profile));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }

        [AllowAnonymous]
        public IActionResult Offline()
        {
            return View();
        }
    }
}
