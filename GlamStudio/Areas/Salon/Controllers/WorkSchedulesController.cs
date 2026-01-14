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
    public class WorkSchedulesController : Controller
    {
        private readonly BeautySalonContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        private readonly List<string> _colorPalette = new List<string>
        {
           "#c07b6a", "#a4b893", "#8eaac2", "#c9a962", "#8c7b99",
           "#6a9595", "#a9a29d", "#c49ca4", "#7b6b5d", "#7f8b9d"
        };

        public WorkSchedulesController(BeautySalonContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<IActionResult> GetSchedules(System.DateTime start, System.DateTime end)
        {
            var query = _context.WorkSchedules.Include(w => w.Employee);
            IQueryable<WorkSchedule> schedulesInPeriod;

            if (User.IsInRole("Admin"))
            {
                schedulesInPeriod = query.Where(ws => ws.StartTime < end && ws.EndTime > start);
            }
            else
            {
                var userId = _userManager.GetUserId(User);
                schedulesInPeriod = query.Where(ws => ws.ApplicationUserId == userId && ws.StartTime < end && ws.EndTime > start);
            }

            var employeeIds = await schedulesInPeriod
                                    .Select(ws => ws.ApplicationUserId)
                                    .Distinct()
                                    .ToListAsync();

            var userColorMap = new Dictionary<string, string>();
            for (int i = 0; i < employeeIds.Count; i++)
            {
                userColorMap.Add(employeeIds[i], _colorPalette[i % _colorPalette.Count]);
            }

            var events = new List<object>();
            foreach (var e in await schedulesInPeriod.ToListAsync())
            {
                bool isAllDay = (e.StartTime.TimeOfDay == TimeSpan.Zero &&
                        e.EndTime.TimeOfDay == TimeSpan.Zero &&
                        e.StartTime.Date != e.EndTime.Date);

                string eventTitle;
                string eventColor;

                if (isAllDay)
                {
                    eventTitle = $"Urlop {e.Employee.FullName}";
                    eventColor = "#dc3545";
                }
                else
                {
                    eventTitle = e.Employee.FullName;
                    eventColor = userColorMap[e.ApplicationUserId];
                }

                events.Add(new
                {
                    id = e.Id,
                    title = eventTitle,
                    start = e.StartTime,
                    end = e.EndTime,
                    color = eventColor,
                    allDay = isAllDay
                });
            }

            return new JsonResult(events);
        }
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create()
        {
            var viewModel = new WorkScheduleCreateViewModel
            {
                EmployeeList = await GetEmployeeSelectList()
            };
            return View(viewModel);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(WorkScheduleCreateViewModel vm)
        {
            if (ModelState.IsValid)
            {
                var selectedDate = vm.Date.Date;
                var nextDay = selectedDate.AddDays(1);

                var existingEntry = await _context.WorkSchedules
                  .FirstOrDefaultAsync(ws =>
                    ws.ApplicationUserId == vm.ApplicationUserId &&
                    ws.StartTime >= selectedDate &&
                    ws.StartTime < nextDay
                  );

                if (existingEntry != null)
                {
                    var employee = await _userManager.FindByIdAsync(vm.ApplicationUserId);
                    string employeeName = employee?.FullName ?? "Ten pracownik";

                    ModelState.AddModelError(string.Empty,
                      $"{employeeName} ma już przypisany wpis w dniu {selectedDate.ToShortDateString()}. Można mieć tylko jeden wpis (zmianę lub urlop) na dzień.");

                    vm.EmployeeList = await GetEmployeeSelectList(vm.ApplicationUserId);
                    return View(vm);
                }
                DateTime startTime;
                DateTime endTime;

                switch (vm.ShiftType)
                {
                    case ShiftType.Morning:
                        startTime = vm.Date.Date.AddHours(8);
                        endTime = vm.Date.Date.AddHours(16);
                        break;
                    case ShiftType.Afternoon:
                        startTime = vm.Date.Date.AddHours(13);
                        endTime = vm.Date.Date.AddHours(21);
                        break;
                    case ShiftType.Vacation:
                        startTime = vm.Date.Date;
                        endTime = vm.Date.Date.AddDays(1);
                        break;
                    default:
                        ModelState.AddModelError("ShiftType", "Nieprawidłowy typ zmiany.");
                        vm.EmployeeList = await GetEmployeeSelectList(vm.ApplicationUserId);
                        return View(vm);
                }

                var workSchedule = new WorkSchedule
                {
                    ApplicationUserId = vm.ApplicationUserId,
                    StartTime = startTime,
                    EndTime = endTime
                };

                _context.Add(workSchedule);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Dodano nowy wpis do grafiku.";
                return RedirectToAction(nameof(Index));
            }

            vm.EmployeeList = await GetEmployeeSelectList(vm.ApplicationUserId);
            return View(vm);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var workSchedule = await _context.WorkSchedules.FindAsync(id);
            if (workSchedule == null)
            {
                return NotFound();
            }

            var viewModel = new WorkScheduleEditViewModel
            {
                Id = workSchedule.Id,
                ApplicationUserId = workSchedule.ApplicationUserId,
                Date = workSchedule.StartTime.Date,
                EmployeeList = await GetEmployeeSelectList(workSchedule.ApplicationUserId)
            };

            if (workSchedule.StartTime.TimeOfDay == TimeSpan.Zero)
            {
                viewModel.ShiftType = ShiftType.Vacation;
            }
            else if (workSchedule.StartTime.TimeOfDay == new TimeSpan(8, 0, 0))
            {
                viewModel.ShiftType = ShiftType.Morning;
            }
            else if (workSchedule.StartTime.TimeOfDay == new TimeSpan(13, 0, 0))
            {
                viewModel.ShiftType = ShiftType.Afternoon;
            }
            else
            {
                viewModel.ShiftType = ShiftType.Morning;
            }

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, WorkScheduleEditViewModel vm)
        {
            if (id != vm.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var selectedDate = vm.Date.Date;
                var nextDay = selectedDate.AddDays(1);

                var existingEntry = await _context.WorkSchedules
                    .FirstOrDefaultAsync(ws =>
                        ws.ApplicationUserId == vm.ApplicationUserId &&
                        ws.StartTime >= selectedDate &&
                        ws.StartTime < nextDay &&
                        ws.Id != vm.Id
                    );

                if (existingEntry != null)
                {
                    var employee = await _userManager.FindByIdAsync(vm.ApplicationUserId);
                    string employeeName = employee?.FullName ?? "Ten pracownik";
                    ModelState.AddModelError(string.Empty,
                        $"{employeeName} ma już przypisany wpis w dniu {selectedDate.ToShortDateString()}.");

                    vm.EmployeeList = await GetEmployeeSelectList(vm.ApplicationUserId);
                    return View(vm);
                }

                DateTime startTime;
                DateTime endTime;

                switch (vm.ShiftType)
                {
                    case ShiftType.Morning:
                        startTime = vm.Date.Date.AddHours(8);
                        endTime = vm.Date.Date.AddHours(16);
                        break;
                    case ShiftType.Afternoon:
                        startTime = vm.Date.Date.AddHours(13);
                        endTime = vm.Date.Date.AddHours(21);
                        break;
                    case ShiftType.Vacation:
                        startTime = vm.Date.Date;
                        endTime = vm.Date.Date.AddDays(1);
                        break;
                    default:
                        ModelState.AddModelError("ShiftType", "Nieprawidłowy typ zmiany.");
                        vm.EmployeeList = await GetEmployeeSelectList(vm.ApplicationUserId);
                        return View(vm);
                }

                try
                {
                    var workScheduleToUpdate = await _context.WorkSchedules.FindAsync(vm.Id);
                    if (workScheduleToUpdate == null)
                    {
                        return NotFound();
                    }

                    workScheduleToUpdate.ApplicationUserId = vm.ApplicationUserId;
                    workScheduleToUpdate.StartTime = startTime;
                    workScheduleToUpdate.EndTime = endTime;

                    _context.Update(workScheduleToUpdate);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.WorkSchedules.Any(e => e.Id == vm.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                TempData["SuccessMessage"] = "Grafik został zaktualizowany.";
                return RedirectToAction(nameof(Index));
            }

            vm.EmployeeList = await GetEmployeeSelectList(vm.ApplicationUserId);
            return View(vm);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var workSchedule = await _context.WorkSchedules
                .Include(w => w.Employee)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (workSchedule == null)
            {
                return NotFound();
            }

            return View(workSchedule);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var workSchedule = await _context.WorkSchedules.FindAsync(id);
            if (workSchedule != null)
            {
                _context.WorkSchedules.Remove(workSchedule);
            }

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Wpis z grafiku został usunięty.";
            return RedirectToAction(nameof(Index));
        }

        private async Task PopulateEmployeesDropDownList(object selectedEmployee = null)
        {
            var employees = await _userManager.GetUsersInRoleAsync("Employee");
            var admins = await _userManager.GetUsersInRoleAsync("Admin");

            var staffList = employees.Union(admins).OrderBy(u => u.LastName).ToList();

            ViewBag.EmployeeList = new SelectList(staffList, "Id", "FullName", selectedEmployee);
        }
        private async Task<SelectList> GetEmployeeSelectList(object selectedEmployee = null)
        {
            var employees = await _userManager.GetUsersInRoleAsync("Employee") ?? new List<ApplicationUser>();
            var admins = await _userManager.GetUsersInRoleAsync("Admin") ?? new List<ApplicationUser>();

            var staffList = employees.Union(admins).OrderBy(u => u.LastName).ToList();

            return new SelectList(staffList, "Id", "FullName", selectedEmployee);
        }

    }

}