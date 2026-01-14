using System.ComponentModel.DataAnnotations;

namespace GlamStudio.Areas.Client.Models
{
    public class ChangePasswordViewModel
    {
        [Required(ErrorMessage = "Podaj obecne hasło")]
        [DataType(DataType.Password)]
        [Display(Name = "Obecne hasło")]
        public string CurrentPassword { get; set; }

        [Required(ErrorMessage = "Podaj nowe hasło")]
        [StringLength(100, ErrorMessage = "{0} musi mieć co najmniej {2} znaków.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Nowe hasło")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Potwierdź nowe hasło")]
        [Compare("NewPassword", ErrorMessage = "Nowe hasło i potwierdzenie nie są identyczne.")]
        public string ConfirmPassword { get; set; }
    }
}