using System.ComponentModel.DataAnnotations;

namespace GlamStudio.Models
{
    public enum ShiftType
    {
        [Display(Name = "Zmiana poranna (8:00 - 16:00)")]
        Morning,

        [Display(Name = "Zmiana popołudniowa (13:00 - 21:00)")]
        Afternoon,

        [Display(Name = "Urlop (Cały dzień)")]
        Vacation
    }
}