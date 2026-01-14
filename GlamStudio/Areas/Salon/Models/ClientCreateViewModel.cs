using System.ComponentModel.DataAnnotations;

namespace GlamStudio.Areas.Salon.Models
{
    public class ClientCreateViewModel
    {
        [Required(ErrorMessage = "Imię jest wymagane.")]
        [Display(Name = "Imię")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Nazwisko jest wymagane.")]
        [Display(Name = "Nazwisko")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "E-mail jest wymagany.")]
        [EmailAddress(ErrorMessage = "Niepoprawny format adresu e-mail.")]
        [Display(Name = "Adres e-mail (Login)")]
        public string Email { get; set; }

        [Phone]
        [Display(Name = "Numer telefonu")]
        public string? PhoneNumber { get; set; }

        [Display(Name = "Ulica i numer")]
        public string? Address { get; set; }

        [Display(Name = "Miasto")]
        public string? City { get; set; }

        [Display(Name = "Notatki Salonu (Poufne)")]
        public string? SalonNotes { get; set; }
    }
}