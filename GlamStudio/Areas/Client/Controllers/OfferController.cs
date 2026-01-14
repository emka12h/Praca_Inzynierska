using GlamStudio.Data;
using GlamStudio.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GlamStudio.Controllers
{
    [Area("Client")]
    public class OfferController : Controller
    {
        private readonly BeautySalonContext _context;

        public OfferController(BeautySalonContext context)
        {
            _context = context;
        }

        // GET: /Client/Offer/Index
        public async Task<IActionResult> Index()
        {
            var services = await _context.Services
                                 .OrderBy(s => s.Category)
                                 .ThenBy(s => s.ServiceName)
                                 .ToListAsync();

            // Grupuowanie usługi po kategorii
            var groupedServices = services
                .GroupBy(s => s.Category)
                .ToDictionary(g => g.Key, g => g.ToList());

            var viewModel = new OfferViewModel
            {
                ServicesByCategory = groupedServices
            };

            return View(viewModel);
        }
        public async Task<IActionResult> Details(int? id)
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
    }
}