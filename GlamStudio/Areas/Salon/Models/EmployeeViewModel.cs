using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace GlamStudio.Models
{
    public class EmployeeViewModel
    {
        public string? Id { get; set; }

        [Required(ErrorMessage = "Imię jest wymagane.")]
        [Display(Name = "Imię")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Nazwisko jest wymagane.")]
        [Display(Name = "Nazwisko")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Email jest wymagany.")]
        [EmailAddress]
        [Display(Name = "Adres e-mail")]
        public string Email { get; set; }

        [Phone]
        [Display(Name = "Numer telefonu")]
        public string? PhoneNumber { get; set; }

        [Display(Name = "Adres")]
        [StringLength(200)]
        public string? Address { get; set; }

        [Display(Name = "Miasto")]
        [StringLength(50)]
        public string? City { get; set; }

        [StringLength(100, ErrorMessage = "{0} musi mieć co najmniej {2} i maksymalnie {1} znaków.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Hasło")]
        public string? Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Potwierdź hasło")]
        [Compare("Password", ErrorMessage = "Hasło i potwierdzenie hasła nie są zgodne.")]
        public string? ConfirmPassword { get; set; }

        [Display(Name = "Specjalizacje")]
        public List<SpecializationCheckboxViewModel> Specializations { get; set; } = new List<SpecializationCheckboxViewModel>();
        public IEnumerable<SelectListItem>? AvailableSpecializations { get; set; }
    }

    public class SpecializationCheckboxViewModel
    {
        public ServiceCategory Category { get; set; }
        public string Name { get; set; }
        public bool IsSelected { get; set; }
    }
}