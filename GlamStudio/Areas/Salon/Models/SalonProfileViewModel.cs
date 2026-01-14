using System.ComponentModel.DataAnnotations;

namespace GlamStudio.Areas.Salon.Models
{
    public class SalonProfileViewModel
    {
        [Display(Name = "Imię")]
        [Required(ErrorMessage = "Imię jest wymagane")]
        public string FirstName { get; set; }

        [Display(Name = "Nazwisko")]
        [Required(ErrorMessage = "Nazwisko jest wymagane")]
        public string LastName { get; set; }

        [Display(Name = "Email")]
        public string Email { get; set; }

        [Display(Name = "Numer telefonu")]
        [Phone]
        public string? PhoneNumber { get; set; }

        [Display(Name = "Ulica i numer")]
        public string? Address { get; set; }

        [Display(Name = "Miasto")]
        public string? City { get; set; }
    }
}