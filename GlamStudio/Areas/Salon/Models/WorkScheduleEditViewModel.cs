using GlamStudio.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace GlamStudio.Areas.Salon.Models
{
    public class WorkScheduleEditViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Musisz wybrać pracownika.")]
        [Display(Name = "Pracownik")]
        public string ApplicationUserId { get; set; }

        [Required(ErrorMessage = "Musisz wybrać datę.")]
        [Display(Name = "Data")]
        [DataType(DataType.Date)] // 
        public DateTime Date { get; set; }

        [Required(ErrorMessage = "Musisz wybrać typ zmiany.")]
        [Display(Name = "Rodzaj zmiany")]
        public ShiftType ShiftType { get; set; }

        [BindNever]
        public IEnumerable<SelectListItem>? EmployeeList { get; set; }
    }
}