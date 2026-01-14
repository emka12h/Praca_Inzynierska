using System.ComponentModel.DataAnnotations;

namespace GlamStudio.Models
{
    public enum AppointmentStatus
    {
        [Display(Name = "Zaplanowana")]
        Scheduled,

        [Display(Name = "Potwierdzona")]
        Confirmed,

        [Display(Name = "Zakończona")]
        Completed,

        [Display(Name = "Anulowana przez klienta")]
        CancelledByClient,

        [Display(Name = "Anulowana przez salon")]
        CancelledBySalon,

        [Display(Name = "Nieobecność")]
        NoShow
    }
}
