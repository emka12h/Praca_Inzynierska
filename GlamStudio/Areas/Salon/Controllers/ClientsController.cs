using GlamStudio.Areas.Salon.Models;
using GlamStudio.Data;
using GlamStudio.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GlamStudio.Areas.Salon.Controllers
{
    [Area("Salon")]
    [Authorize(Roles = "Admin,Employee")]
    public class ClientsController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly BeautySalonContext _context;

        public ClientsController(UserManager<ApplicationUser> userManager, BeautySalonContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        // GET: Salon/Clients
        public async Task<IActionResult> Index(string searchString)
        {
            var clients = await _userManager.GetUsersInRoleAsync("Client");
            var query = clients.AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                searchString = searchString.ToLower();
                query = query.Where(c => c.LastName.ToLower().Contains(searchString)
                                      || c.FirstName.ToLower().Contains(searchString)
                                      || c.Email.ToLower().Contains(searchString)
                                      || (c.PhoneNumber != null && c.PhoneNumber.Contains(searchString)));
            }

            return View(query.OrderBy(c => c.LastName).ToList());
        }

        // GET: Salon/Clients/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null) return NotFound();

            var client = await _context.Users
                .Include(u => u.ClientAppointments)
                    .ThenInclude(a => a.Service)
                .Include(u => u.ClientAppointments)
                    .ThenInclude(a => a.Employee)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (client == null) return NotFound();

            return View(client);
        }

        // GET: Salon/Clients/Create
        public IActionResult Create()
        {
            return View(new ClientCreateViewModel());
        }

        // POST: Salon/Clients/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ClientCreateViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    PhoneNumber = model.PhoneNumber,
                    Address = model.Address,
                    City = model.City,
                    SalonNotes = model.SalonNotes,
                    EmailConfirmed = true
                };

                // Generowanie hasła
                string generatedPassword = GenerateRandomPassword();

                var result = await _userManager.CreateAsync(user, generatedPassword);

                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, "Client");
                    TempData["SuccessMessage"] = $"Klient dodany pomyślnie. Wygenerowane hasło: {generatedPassword} (Zapisz je!)";

                    return RedirectToAction(nameof(Index));
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            return View(model);
        }

        private string GenerateRandomPassword()
        {
            var options = _userManager.Options.Password;

            int length = options.RequiredLength < 8 ? 8 : options.RequiredLength;

            string[] randomChars = new[] {
                "ABCDEFGHJKLMNOPQRSTUVWXYZ",
                "abcdefghijkmnopqrstuvwxyz",
                "0123456789",
                "!@$?_-"
            };

            Random rand = new Random();
            List<char> chars = new List<char>();

            if (options.RequireUppercase) chars.Insert(rand.Next(0, chars.Count), randomChars[0][rand.Next(0, randomChars[0].Length)]);
            if (options.RequireLowercase) chars.Insert(rand.Next(0, chars.Count), randomChars[1][rand.Next(0, randomChars[1].Length)]);
            if (options.RequireDigit) chars.Insert(rand.Next(0, chars.Count), randomChars[2][rand.Next(0, randomChars[2].Length)]);
            if (options.RequireNonAlphanumeric) chars.Insert(rand.Next(0, chars.Count), randomChars[3][rand.Next(0, randomChars[3].Length)]);

            for (int i = chars.Count; i < length || chars.Count < 10; i++)
            {
                string rcs = randomChars[rand.Next(0, randomChars.Length)];
                chars.Insert(rand.Next(0, chars.Count), rcs[rand.Next(0, rcs.Length)]);
            }

            return new string(chars.ToArray());
        }

        // GET: Salon/Clients/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null) return NotFound();
            var client = await _userManager.FindByIdAsync(id);
            if (client == null) return NotFound();
            return View(client);
        }

        // POST: Salon/Clients/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, ApplicationUser model)
        {
            if (id != model.Id) return NotFound();

            var client = await _userManager.FindByIdAsync(id);
            if (client == null) return NotFound();

            client.FirstName = model.FirstName;
            client.LastName = model.LastName;
            client.PhoneNumber = model.PhoneNumber;
            client.Address = model.Address;
            client.City = model.City;
            client.SalonNotes = model.SalonNotes;

            if (client.Email != model.Email)
            {
                var emailResult = await _userManager.SetEmailAsync(client, model.Email);
                if (!emailResult.Succeeded)
                {
                    foreach (var error in emailResult.Errors) ModelState.AddModelError("", error.Description);
                    return View(model);
                }
                await _userManager.SetUserNameAsync(client, model.Email);
            }

            var result = await _userManager.UpdateAsync(client);
            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "Dane klienta zaktualizowane.";
                return RedirectToAction(nameof(Index));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            return View(model);
        }

        public IActionResult History(string id)
        {
            return RedirectToAction("Details", new { id = id });
        }


        // GET: Salon/Clients/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null) return NotFound();

            var client = await _userManager.FindByIdAsync(id);
            if (client == null) return NotFound();

            return View(client);
        }

        // POST: Salon/Clients/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            try
            {
                var result = await _userManager.DeleteAsync(user);

                if (result.Succeeded)
                {
                    TempData["SuccessMessage"] = "Konto klienta zostało trwale usunięte.";
                    return RedirectToAction(nameof(Index));
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }
            catch (DbUpdateException)
            {
                TempData["ErrorMessage"] = "Nie można usunąć klienta, który posiada historię wizyt. Najpierw usuń lub anuluj jego rezerwacje.";
                return RedirectToAction(nameof(Edit), new { id = id });
            }

            return View(user);
        }

    }
}