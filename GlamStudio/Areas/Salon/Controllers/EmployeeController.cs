using GlamStudio.Data;
using GlamStudio.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GlamStudio.Areas.Salon.Controllers
{
    [Area("Salon")]
    [Authorize(Roles = "Admin")]
    public class EmployeesController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly BeautySalonContext _context;

        public EmployeesController(UserManager<ApplicationUser> userManager, BeautySalonContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        // GET: /Salon/Employees
        public async Task<IActionResult> Index()
        {
            var employees = await _userManager.GetUsersInRoleAsync("Employee");
            var admins = await _userManager.GetUsersInRoleAsync("Admin");
            var staffIds = employees.Union(admins).Select(u => u.Id).ToList();

            var allStaff = await _context.Users
                .Include(u => u.Specializations)
                .Where(u => staffIds.Contains(u.Id))
                .ToListAsync();

            var employeeListViewModel = new List<EmployeeListViewModel>();

            foreach (var user in allStaff)
            {
                var specString = user.Specializations != null && user.Specializations.Any()
     ? string.Join(", ", user.Specializations.Select(s => s.Category.GetDisplayName()))
     : "Brak";

                employeeListViewModel.Add(new EmployeeListViewModel
                {
                    UserId = user.Id,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email,
                    Specialization = specString,
                    Roles = await _userManager.GetRolesAsync(user)
                });
            }

            return View(employeeListViewModel);
        }

        // GET: /Salon/Employees/Create
        public IActionResult Create()
        {
            var viewModel = new EmployeeViewModel
            {
                Specializations = GetSpecializationsForUser()
            };
            return View(viewModel);
        }

        // POST: /Salon/Employees/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(EmployeeViewModel model)
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
                    EmailConfirmed = true,

                    Specializations = model.Specializations
                        .Where(x => x.IsSelected)
                        .Select(x => new EmployeeSpecialization { Category = x.Category })
                        .ToList()
                };

                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, "Employee");
                    TempData["SuccessMessage"] = "Pracownik został pomyślnie dodany.";
                    return RedirectToAction(nameof(Index));
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            return View(model);
        }

        // GET: Salon/Employees/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users
                .Include(u => u.Specializations)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // GET: Salon/Employees/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users
                .Include(u => u.Specializations)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
            {
                return NotFound();
            }

            var viewModel = new EmployeeViewModel
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Address = user.Address,
                City = user.City,
                Specializations = GetSpecializationsForUser(user)
            };

            return View(viewModel);
        }

        // POST: Salon/Employees/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, EmployeeViewModel model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            if (string.IsNullOrEmpty(model.Password))
            {
                ModelState.Remove("Password");
                ModelState.Remove("ConfirmPassword");
            }

            if (ModelState.IsValid)
            {
                var user = await _context.Users
                    .Include(u => u.Specializations)
                    .FirstOrDefaultAsync(u => u.Id == id);

                if (user == null)
                {
                    return NotFound();
                }

                user.FirstName = model.FirstName;
                user.LastName = model.LastName;
                user.Email = model.Email;
                user.UserName = model.Email;
                user.PhoneNumber = model.PhoneNumber;
                user.Address = model.Address;
                user.City = model.City;

                user.Specializations.Clear();

                foreach (var spec in model.Specializations.Where(x => x.IsSelected))
                {
                    user.Specializations.Add(new EmployeeSpecialization { Category = spec.Category });
                }

                var updateResult = await _userManager.UpdateAsync(user);

                if (updateResult.Succeeded)
                {
                    if (!string.IsNullOrEmpty(model.Password))
                    {
                        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                        var passwordResult = await _userManager.ResetPasswordAsync(user, token, model.Password);
                        if (!passwordResult.Succeeded)
                        {
                            foreach (var error in passwordResult.Errors)
                            {
                                ModelState.AddModelError(string.Empty, error.Description);
                            }
                            return View(model);
                        }
                    }

                    TempData["SuccessMessage"] = "Dane pracownika zostały zaktualizowane.";
                    return RedirectToAction(nameof(Index));
                }

                foreach (var error in updateResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return View(model);
        }


        private List<SpecializationCheckboxViewModel> GetSpecializationsForUser(ApplicationUser user = null)
        {
            var allCategories = Enum.GetValues(typeof(ServiceCategory)).Cast<ServiceCategory>();
            var viewModel = new List<SpecializationCheckboxViewModel>();

            foreach (var category in allCategories)
            {
                viewModel.Add(new SpecializationCheckboxViewModel
                {
                    Category = category,
                    Name = category.GetDisplayName(),

                    IsSelected = user != null && user.Specializations.Any(s => s.Category == category)
                });
            }
            return viewModel;
        }


        // GET: Salon/Employees/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null) return NotFound();

            var currentUserId = _userManager.GetUserId(User);
            if (currentUserId == id)
            {
                TempData["ErrorMessage"] = "Nie możesz usunąć własnego konta administratora.";
                return RedirectToAction(nameof(Index));
            }

            var user = await _context.Users
                .Include(u => u.Specializations)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null) return NotFound();

            return View(user);
        }

        // POST: Salon/Employees/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            if (_userManager.GetUserId(User) == id)
            {
                TempData["ErrorMessage"] = "Nie możesz usunąć własnego konta.";
                return RedirectToAction(nameof(Index));
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var schedules = await _context.WorkSchedules.Where(ws => ws.ApplicationUserId == id).ToListAsync();
                if (schedules.Any())
                {
                    _context.WorkSchedules.RemoveRange(schedules);
                }

                var appointments = await _context.Appointments.Where(a => a.EmployeeID == id).ToListAsync();
                if (appointments.Any())
                {
                    _context.Appointments.RemoveRange(appointments);
                }
                await _context.SaveChangesAsync();

                var result = await _userManager.DeleteAsync(user);

                if (result.Succeeded)
                {
                    await transaction.CommitAsync();

                    TempData["SuccessMessage"] = "Pracownik oraz cała jego historia zostali usunięci.";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    await transaction.RollbackAsync();
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                }
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                Console.WriteLine(ex.Message);

                TempData["ErrorMessage"] = "Wystąpił błąd podczas usuwania danych powiązanych.";
                return RedirectToAction(nameof(Edit), new { id = id });
            }

            return View(user);
        }
    }
}