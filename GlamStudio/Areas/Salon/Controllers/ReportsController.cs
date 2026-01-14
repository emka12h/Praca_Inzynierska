using GlamStudio.Areas.Salon.Models;
using GlamStudio.Data;
using GlamStudio.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace GlamStudio.Areas.Salon.Controllers
{
    [Area("Salon")]
    [Authorize(Roles = "Admin")]
    public class ReportsController : Controller
    {
        private readonly BeautySalonContext _context;

        public ReportsController(BeautySalonContext context)
        {
            _context = context;
        }

        // GET: Salon/Reports/Index 
        public IActionResult Index()
        {
            return RedirectToAction(nameof(ServiceReports));
        }

        // GET: Salon/Reports/ServiceReports
        public async Task<IActionResult> ServiceReports()
        {
            // Pobieranie tylko zakończonych wizyt
            var completedAppointments = _context.Appointments
                .Include(a => a.Service)
                .Where(a => a.Status == AppointmentStatus.Completed);


            var stats = await completedAppointments
                .GroupBy(a => a.Service)
                .Select(g => new ServiceStatisticViewModel
                {
                    ServiceName = g.Key.ServiceName,
                    Category = g.Key.Category.ToString(),
                    TotalAppointments = g.Count(),
                    TotalRevenue = g.Sum(a => a.Service.Price)
                })
                .OrderByDescending(s => s.TotalRevenue)
                .ToListAsync();

            return View(stats);
        }
        // GET: Salon/Reports/MonthlyStats
        public async Task<IActionResult> MonthlyStats()
        {
            var query = _context.Appointments
                .Include(a => a.Service)
                .Where(a => a.Status == AppointmentStatus.Completed);


            var groupedData = await query
                .GroupBy(a => new { a.AppointmentDate.Year, a.AppointmentDate.Month })
                .Select(g => new
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    Count = g.Count(),
                    Revenue = g.Sum(a => a.Service.Price)
                })
                .OrderBy(g => g.Year).ThenBy(g => g.Month)
                .ToListAsync();

            var culture = new CultureInfo("pl-PL");
            var model = new List<MonthlyReportViewModel>();

            foreach (var item in groupedData)
            {
                string monthName = culture.DateTimeFormat.GetMonthName(item.Month);
                monthName = char.ToUpper(monthName[0]) + monthName.Substring(1);

                model.Add(new MonthlyReportViewModel
                {
                    Year = item.Year,
                    Month = item.Month,
                    MonthLabel = $"{monthName} {item.Year}",
                    TotalAppointments = item.Count,
                    TotalRevenue = item.Revenue
                });
            }

            return View(model);
        }
    }
}