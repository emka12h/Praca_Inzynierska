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
    public class MessagesController : Controller
    {
        private readonly BeautySalonContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public MessagesController(BeautySalonContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var messages = await _context.Messages
                                     .OrderByDescending(m => m.DateSent)
                                     .ToListAsync();
            return View(messages);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var message = await _context.Messages.Include(m => m.ReplySentBy)
                                                 .FirstOrDefaultAsync(m => m.MessageId == id);

            if (message == null)
            {
                return NotFound();
            }

            if (!message.IsReadBySalon)
            {
                message.IsReadBySalon = true;
                await _context.SaveChangesAsync();
            }

            return View(message);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendReply(int id, string replyText)
        {
            if (string.IsNullOrWhiteSpace(replyText))
            {
                TempData["ErrorMessage"] = "Treść odpowiedzi nie może być pusta.";
                return RedirectToAction("Details", new { id = id });
            }

            var message = await _context.Messages.FindAsync(id);
            if (message == null)
            {
                return NotFound();
            }

            var adminUser = await _userManager.GetUserAsync(User);

            string newReplyEntry =
        $"\n\n--- Odpowiedź Salonu ({adminUser.FirstName} @ {DateTime.Now:g}) ---\n{replyText}";

            message.ReplyContent = (message.ReplyContent ?? "") + newReplyEntry;

            message.ReplyDate = DateTime.Now;
            message.ReplySentById = adminUser.Id;
            message.IsReadBySalon = true;
            message.IsReadByClient = false;

            try
            {
                _context.Update(message);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Odpowiedź została wysłana!";
            }
            catch (DbUpdateException)
            {
                TempData["ErrorMessage"] = "Wystąpił błąd podczas zapisywania odpowiedzi.";
            }

            return RedirectToAction("Details", new { id = id });
        }
    }
}
