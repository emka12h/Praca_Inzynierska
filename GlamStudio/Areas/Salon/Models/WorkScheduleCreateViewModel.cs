using GlamStudio.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace GlamStudio.Areas.Salon.Models
{
    public class WorkScheduleCreateViewModel
    {
        [Required(ErrorMessage = "Musisz wybrać pracownika.")]
        [Display(Name = "Pracownik")]
        public string ApplicationUserId { get; set; }

        [Required(ErrorMessage = "Musisz wybrać datę.")]
        [DataType(DataType.Date)]
        [Display(Name = "Data")]
        public DateTime Date { get; set; } = DateTime.Today;

        [Required(ErrorMessage = "Musisz wybrać typ zmiany.")]
        [Display(Name = "Typ zmiany")]
        public ShiftType ShiftType { get; set; }

        [BindNever]
        public IEnumerable<SelectListItem>? EmployeeList { get; set; }
    }
}