using GlamStudio.Areas.Client.Models;
using GlamStudio.Data;
using GlamStudio.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace GlamStudio.Areas.Client.Controllers
{
    [Area("Client")]
    [Authorize(Roles = "Client")]
    public class BookingController : Controller
    {
        private readonly BeautySalonContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public BookingController(BeautySalonContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Wyświetlenie formularza
        [HttpGet]
        public async Task<IActionResult> Create(int? serviceId)
        {
            var viewModel = new BookingViewModel
            {
                ServicesList = await GetServicesList(),
                EmployeesList = new List<SelectListItem> { new SelectListItem { Text = "-- Najpierw wybierz zabieg --", Value = "", Disabled = true, Selected = true } }
            };

            if (serviceId.HasValue)
            {
                viewModel.ServiceId = serviceId.Value;
                viewModel.EmployeesList = await GetEmployeesListByServiceId(serviceId.Value);
            }

            return View(viewModel);
        }

        // POST: Zapisanie wizyty
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BookingViewModel model)
        {
            if (ModelState.IsValid)
            {
                var service = await _context.Services.FindAsync(model.ServiceId);
                if (service == null) return NotFound();

                bool isAvailable = await CheckAvailability(model.EmployeeId, model.Date, model.Time, service.DurationInMinutes);

                if (!isAvailable)
                {
                    ModelState.AddModelError("", "Niestety, ten termin został już zajęty lub pracownik nie pracuje w tych godzinach.");
                }
                else
                {
                    var user = await _userManager.GetUserAsync(User);

                    var appointment = new Appointment
                    {
                        AppointmentDate = model.Date,
                        AppointmentTime = model.Time,
                        ClientID = user.Id,
                        EmployeeID = model.EmployeeId,
                        ServiceID = model.ServiceId,
                        Status = AppointmentStatus.Scheduled
                    };

                    try
                    {
                        _context.Appointments.Add(appointment);
                        await _context.SaveChangesAsync();

                        TempData["SuccessMessage"] = "Twoja wizyta została zarezerwowana!";
                        return RedirectToAction("MyVisits", "Home", new { area = "Client" });
                    }
                    catch (DbUpdateException)
                    {
                        ModelState.AddModelError("", "Ktoś właśnie zarezerwował ten termin. Spróbuj wybrać inny.");
                    }
                }
            }

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

        // API AJAX: Zwraca wolne godziny
        [HttpGet]
        public async Task<IActionResult> GetAvailableTimeSlots(string employeeId, int serviceId, DateTime date)
        {
            if (string.IsNullOrEmpty(employeeId) || serviceId == 0) return Json(new List<string>());

            var service = await _context.Services.FindAsync(serviceId);
            if (service == null) return Json(new List<string>());

            //pobranie grafkiu pracownika na dany dzień
            var schedule = await _context.WorkSchedules
                .FirstOrDefaultAsync(ws => ws.ApplicationUserId == employeeId
                                           && ws.StartTime.Date == date.Date
                                           && ws.IsTimeOff == false);

            if (schedule == null) return Json(new List<string>());

            //istniejące wizyty pracownika w danym dniu
            var existingAppointments = await _context.Appointments
                .Include(a => a.Service)
                .Where(a => a.EmployeeID == employeeId && a.AppointmentDate.Date == date.Date && a.Status != AppointmentStatus.CancelledBySalon && a.Status != AppointmentStatus.CancelledByClient)
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
                    // Blokada umawiania wizyt w przeszłości
                    if (date.Date > DateTime.Today || (date.Date == DateTime.Today && currentTime > DateTime.Now.TimeOfDay))
                    {
                        availableSlots.Add(currentTime.ToString(@"hh\:mm"));
                    }
                }
                currentTime = currentTime.Add(TimeSpan.FromMinutes(30));
            }

            return Json(availableSlots);
        }

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

            // Filtracja pracowników po kategorii usługi
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

            var conflictingAppointment = await _context.Appointments
                .Include(a => a.Service)
                .Where(a => a.EmployeeID == employeeId
                         && a.AppointmentDate.Date == date.Date
                         && a.Status != AppointmentStatus.CancelledByClient
                         && a.Status != AppointmentStatus.CancelledBySalon)
                .ToListAsync();

            foreach (var app in conflictingAppointment)
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
                Text = $"{s.ServiceName} ({s.Price} zł - {s.DurationInMinutes} min)"
            }).ToListAsync();
        }

        private async Task<IEnumerable<SelectListItem>> GetEmployeesList()
        {
            var employees = await _userManager.GetUsersInRoleAsync("Employee");
            var admins = await _userManager.GetUsersInRoleAsync("Admin");

            return employees.Union(admins).Select(e => new SelectListItem
            {
                Value = e.Id,
                Text = $"{e.FirstName} {e.LastName}"
            });
        }
    }
}