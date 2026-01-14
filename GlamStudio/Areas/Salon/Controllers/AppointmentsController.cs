using GlamStudio.Areas.Salon.Models;
using GlamStudio.Data;
using GlamStudio.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace GlamStudio.Areas.Salon.Controllers
{
    [Area("Salon")]
    [Authorize(Roles = "Admin,Employee")]
    public class AppointmentsController : Controller
    {
        private readonly BeautySalonContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public AppointmentsController(BeautySalonContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Salon/Appointments
        public async Task<IActionResult> Index(string filter = "upcoming", string employeeId = "")
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            bool isAdmin = await _userManager.IsInRoleAsync(user, "Admin");

            if (isAdmin)
            {
                var employees = await _userManager.GetUsersInRoleAsync("Employee");
                var admins = await _userManager.GetUsersInRoleAsync("Admin");

                var allStaff = employees.Concat(admins)
                    .GroupBy(x => x.Id).Select(x => x.First())
                    .OrderBy(x => x.LastName)
                    .Select(u => new SelectListItem
                    {
                        Value = u.Id,
                        Text = $"{u.FirstName} {u.LastName}",
                        Selected = u.Id == employeeId
                    }).ToList();

                ViewBag.Employees = allStaff;
            }

            var query = _context.Appointments
                .Include(a => a.Client)
                .Include(a => a.Employee)
                .Include(a => a.Service)
                .AsQueryable();

            // Filtrowanie według pracownika
            if (!isAdmin)
            {
                query = query.Where(a => a.EmployeeID == user.Id);
                employeeId = user.Id;
            }
            else if (!string.IsNullOrEmpty(employeeId))
            {
                query = query.Where(a => a.EmployeeID == employeeId);
            }

            // Filtrowanie według czasu (Dzisiaj, Nadchodzące, Historia)
            switch (filter)
            {
                case "history":
                    query = query.Where(a => a.AppointmentDate < DateTime.Today ||
                                             a.Status == AppointmentStatus.CancelledByClient ||
                                             a.Status == AppointmentStatus.CancelledBySalon);
                    break;
                case "today":
                    query = query.Where(a => a.AppointmentDate.Date == DateTime.Today &&
                                             a.Status != AppointmentStatus.CancelledByClient &&
                                             a.Status != AppointmentStatus.CancelledBySalon);
                    break;
                case "upcoming":
                default:
                    query = query.Where(a => a.AppointmentDate.Date >= DateTime.Today &&
                                             a.Status != AppointmentStatus.CancelledByClient &&
                                             a.Status != AppointmentStatus.CancelledBySalon);
                    break;
            }

            var appointments = await query
                .OrderBy(a => a.AppointmentDate)
                .ThenBy(a => a.AppointmentTime)
                .ToListAsync();

            ViewData["CurrentFilter"] = filter;
            ViewData["CurrentEmployeeId"] = employeeId;

            return View(appointments);
        }

        // POST: Zmiana statusu
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeStatus(int id, AppointmentStatus newStatus)
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment == null)
            {
                return NotFound();
            }

            appointment.Status = newStatus;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Status wizyty został zmieniony na: {GetStatusName(newStatus)}";
            return RedirectToAction(nameof(Index));
        }

        private string GetStatusName(AppointmentStatus status)
        {
            return status switch
            {
                AppointmentStatus.Confirmed => "Potwierdzona",
                AppointmentStatus.Completed => "Zakończona",
                AppointmentStatus.CancelledBySalon => "Anulowana przez salon",
                AppointmentStatus.NoShow => "Nieobecność",
                _ => status.ToString()
            };
        }

        // GET: Salon/Appointments/Create
        public async Task<IActionResult> Create()
        {
            var viewModel = new ManualBookingViewModel
            {
                ClientsList = await GetClientsList(),
                ServicesList = await GetServicesList(),
                EmployeesList = new List<SelectListItem> { new SelectListItem { Text = "-- Najpierw wybierz usługę --", Value = "", Disabled = true, Selected = true } }
            };
            return View(viewModel);
        }

        // POST: Salon/Appointments/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ManualBookingViewModel model)
        {
            if (ModelState.IsValid)
            {
                var service = await _context.Services.FindAsync(model.ServiceId);
                if (service == null) return NotFound();

                bool isAvailable = await CheckAvailability(model.EmployeeId, model.Date, model.Time, service.DurationInMinutes);

                if (!isAvailable)
                {
                    ModelState.AddModelError("", "Termin jest zajęty lub pracownik nie pracuje.");
                }
                else
                {
                    var appointment = new Appointment
                    {
                        AppointmentDate = model.Date,
                        AppointmentTime = model.Time,
                        ClientID = model.ClientId,
                        EmployeeID = model.EmployeeId,
                        ServiceID = model.ServiceId,
                        Status = AppointmentStatus.Confirmed
                    };

                    try
                    {
                        _context.Appointments.Add(appointment);
                        await _context.SaveChangesAsync();
                        TempData["SuccessMessage"] = "Wizyta została pomyślnie umówiona.";
                        return RedirectToAction(nameof(Index));
                    }
                    catch (DbUpdateException)
                    {
                        ModelState.AddModelError("", "Wystąpił błąd podczas zapisu (możliwy duplikat).");
                    }
                }
            }

            model.ClientsList = await GetClientsList();
            model.ServicesList = await GetServicesList();

            if (model.ServiceId != 0)
            {
                model.EmployeesList = await GetEmployeesListByServiceId(model.ServiceId);
            }
            else
            {
                model.EmployeesList = new List<SelectListItem>();
            }

            return View(model);
        }

        //Lista pracowników według usługi - API AJAX
        [HttpGet]
        public async Task<IActionResult> GetEmployeesByService(int serviceId)
        {
            var employeesList = await GetEmployeesListByServiceId(serviceId);
            var jsonResult = employeesList.Select(e => new
            {
                value = e.Value,
                text = e.Text
            });

            return Json(jsonResult);
        }

        private async Task<IEnumerable<SelectListItem>> GetEmployeesListByServiceId(int serviceId)
        {
            var service = await _context.Services.FindAsync(serviceId);
            if (service == null) return new List<SelectListItem>();

            var employees = await _userManager.GetUsersInRoleAsync("Employee");
            var admins = await _userManager.GetUsersInRoleAsync("Admin");
            var staffIds = employees.Concat(admins).Select(u => u.Id).Distinct().ToList();

            // Porównujemy Enumy kategorii
            var qualifiedStaff = await _context.Users
                .Include(u => u.Specializations)
                .Where(u => staffIds.Contains(u.Id))
                .Where(u => u.Specializations.Any(s => s.Category == service.Category))
                .ToListAsync();

            return qualifiedStaff.Select(e => new SelectListItem
            {
                Value = e.Id,
                Text = $"{e.FirstName} {e.LastName}"
            });
        }

        //Sloty czasowe
        [HttpGet]
        public async Task<IActionResult> GetAvailableTimeSlots(string employeeId, int serviceId, DateTime date)
        {
            if (string.IsNullOrEmpty(employeeId) || serviceId == 0) return Json(new List<string>());

            var service = await _context.Services.FindAsync(serviceId);
            if (service == null) return Json(new List<string>());

            var schedule = await _context.WorkSchedules
                .FirstOrDefaultAsync(ws => ws.ApplicationUserId == employeeId
                                           && ws.StartTime.Date == date.Date
                                           && ws.IsTimeOff == false);

            if (schedule == null) return Json(new List<string>());

            var existingAppointments = await _context.Appointments
                .Include(a => a.Service)
                .Where(a => a.EmployeeID == employeeId && a.AppointmentDate.Date == date.Date &&
                            a.Status != AppointmentStatus.CancelledBySalon && a.Status != AppointmentStatus.CancelledByClient)
                .ToListAsync();

            var availableSlots = new List<string>();
            var currentTime = schedule.StartTime.TimeOfDay;
            var endTime = schedule.EndTime.TimeOfDay;
            var serviceDuration = TimeSpan.FromMinutes(service.DurationInMinutes);

            while (currentTime + serviceDuration <= endTime)
            {
                bool isSlotFree = true;
                var slotEnd = currentTime + serviceDuration;

                foreach (var appointment in existingAppointments)
                {
                    var appStart = appointment.AppointmentTime;
                    var appEnd = appStart + TimeSpan.FromMinutes(appointment.Service.DurationInMinutes);

                    if (currentTime < appEnd && slotEnd > appStart)
                    {
                        isSlotFree = false;
                        break;
                    }
                }

                if (isSlotFree)
                {
                    availableSlots.Add(currentTime.ToString(@"hh\:mm"));
                }
                currentTime = currentTime.Add(TimeSpan.FromMinutes(30));
            }
            return Json(availableSlots);
        }

        private async Task<bool> CheckAvailability(string employeeId, DateTime date, TimeSpan time, int durationMinutes)
        {
            var schedule = await _context.WorkSchedules
              .FirstOrDefaultAsync(ws => ws.ApplicationUserId == employeeId
                                       && ws.StartTime.Date == date.Date
                                       && ws.IsTimeOff == false);
            if (schedule == null) return false;

            var slotStart = time;
            var slotEnd = time.Add(TimeSpan.FromMinutes(durationMinutes));

            if (slotStart < schedule.StartTime.TimeOfDay || slotEnd > schedule.EndTime.TimeOfDay) return false;

            var conflictingAppointments = await _context.Appointments
                .Include(a => a.Service)
                .Where(a => a.EmployeeID == employeeId
                         && a.AppointmentDate.Date == date.Date
                         && a.Status != AppointmentStatus.CancelledByClient
                         && a.Status != AppointmentStatus.CancelledBySalon)
                .ToListAsync();

            foreach (var app in conflictingAppointments)
            {
                var appStart = app.AppointmentTime;
                var appEnd = appStart.Add(TimeSpan.FromMinutes(app.Service.DurationInMinutes));
                if (slotStart < appEnd && slotEnd > appStart) return false;
            }
            return true;
        }

        private async Task<IEnumerable<SelectListItem>> GetServicesList()
        {
            return await _context.Services.Select(s => new SelectListItem
            {
                Value = s.ServiceID.ToString(),
                Text = $"{s.ServiceName} ({s.Duration.TotalMinutes} min)"
            }).ToListAsync();
        }

        private async Task<IEnumerable<SelectListItem>> GetClientsList()
        {
            var clients = await _userManager.GetUsersInRoleAsync("Client");
            return clients.OrderBy(u => u.LastName).Select(u => new SelectListItem
            {
                Value = u.Id,
                Text = $"{u.LastName} {u.FirstName} ({u.Email})"
            });
        }
    }
}