using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace GlamStudio.Pages
{
    public class ContactModel : PageModel
    {
        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Required(ErrorMessage = "Adres email jest wymagany.")]
            [EmailAddress(ErrorMessage = "WprowadŸ poprawny adres email.")]
            public string Email { get; set; }

            [Required(ErrorMessage = "Treœæ wiadomoœci nie mo¿e byæ pusta.")]
            [MinLength(10, ErrorMessage = "Wiadomoœæ musi mieæ co najmniej 10 znaków.")]
            public string Message { get; set; }
        }

        public void OnGet()
        {

        }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }


            return Page();
        }
    }
}

