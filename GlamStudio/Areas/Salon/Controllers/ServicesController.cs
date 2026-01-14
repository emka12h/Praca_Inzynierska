using GlamStudio.Data;
using GlamStudio.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GlamStudio.Controllers
{
    [Area("Salon")]
    [Authorize(Roles = "Admin,Employee")]
    public class ServicesController : Controller
    {
        private readonly BeautySalonContext _context;

        public ServicesController(BeautySalonContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var services = await _context.Services.ToListAsync();
            return View(services);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ServiceID,ServiceName,Description,Price,DurationInMinutes,Category")] Service service)
        {
            if (ModelState.IsValid)
            {
                _context.Add(service);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Usługa została dodana.";
                return RedirectToAction(nameof(Index));
            }
            return View(service);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var service = await _context.Services.FindAsync(id);
            if (service == null)
            {
                return NotFound();
            }
            return View(service);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ServiceID,ServiceName,Description,Price,DurationInMinutes,Category")] Service service)
        {
            if (id != service.ServiceID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(service);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Services.Any(e => e.ServiceID == service.ServiceID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                TempData["SuccessMessage"] = "Usługa została zaktualizowana.";
                return RedirectToAction(nameof(Index));
            }
            return View(service);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var service = await _context.Services
                .FirstOrDefaultAsync(m => m.ServiceID == id);

            if (service == null)
            {
                return NotFound();
            }

            return View(service);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var service = await _context.Services
                .FirstOrDefaultAsync(m => m.ServiceID == id);

            if (service == null)
            {
                return NotFound();
            }

            return View(service);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var service = await _context.Services.FindAsync(id);
            if (service == null)
            {
                return RedirectToAction(nameof(Index));
            }

            try
            {
                _context.Services.Remove(service);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Usługa została pomyślnie usunięta.";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException /* ex */)
            {
                TempData["ErrorMessage"] = "Nie można usunąć usługi, ponieważ jest ona powiązana z istniejącymi wizytami.";
                return RedirectToAction(nameof(Delete), new { id = id });
            }
        }
    }
}