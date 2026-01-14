using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace GlamStudio.Areas.Salon.Models
{
    public class ManualBookingViewModel
    {
        [Required(ErrorMessage = "Wybierz klienta")]
        [Display(Name = "Klient")]
        public string ClientId { get; set; }

        [Required(ErrorMessage = "Wybierz usługę")]
        [Display(Name = "Usługa")]
        public int ServiceId { get; set; }

        [Required(ErrorMessage = "Wybierz specjalistę")]
        [Display(Name = "Specjalista")]
        public string EmployeeId { get; set; }

        [Required(ErrorMessage = "Wybierz datę")]
        [DataType(DataType.Date)]
        [Display(Name = "Data wizyty")]
        public DateTime Date { get; set; } = DateTime.Today.AddDays(1);

        [Required(ErrorMessage = "Wybierz godzinę")]
        [Display(Name = "Godzina")]
        public TimeSpan Time { get; set; }

        public IEnumerable<SelectListItem>? ClientsList { get; set; }
        public IEnumerable<SelectListItem>? ServicesList { get; set; }
        public IEnumerable<SelectListItem>? EmployeesList { get; set; }
    }
}