using System.ComponentModel.DataAnnotations;

namespace GlamStudio.Areas.Client.Models
{
    public class UserProfileViewModel
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
        [Phone(ErrorMessage = "Niepoprawny format numeru")]
        public string? PhoneNumber { get; set; }

        [Display(Name = "Ulica i numer")]
        public string? Address { get; set; }

        [Display(Name = "Miasto")]
        public string? City { get; set; }
    }
}