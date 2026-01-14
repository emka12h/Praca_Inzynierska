using System.ComponentModel.DataAnnotations;

namespace GlamStudio.ViewModels
{
    public class ContactFormViewModel
    {
        [Required(ErrorMessage = "Imię jest wymagane.")]
        [Display(Name = "Twoje imię")]
        public string SendersName { get; set; }

        [Required(ErrorMessage = "E-mail jest wymagany.")]
        [EmailAddress]
        [Display(Name = "Twój e-mail")]
        public string SendersEmail { get; set; }

        [Required(ErrorMessage = "Treść wiadomości jest wymagana.")]
        [Display(Name = "Wiadomość")]
        [DataType(DataType.MultilineText)]
        public string Contents { get; set; }

        public bool IsUserLoggedIn { get; set; } = false;
    }
}
