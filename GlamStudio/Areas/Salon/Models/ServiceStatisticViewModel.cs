using System.ComponentModel.DataAnnotations;

namespace GlamStudio.Areas.Salon.Models
{
    public class ServiceStatisticViewModel
    {
        [Display(Name = "Nazwa Usługi")]
        public string ServiceName { get; set; }

        [Display(Name = "Kategoria")]
        public string Category { get; set; }

        [Display(Name = "Liczba wizyt")]
        public int TotalAppointments { get; set; }

        [Display(Name = "Łączny przychód")]
        [DataType(DataType.Currency)]
        public decimal TotalRevenue { get; set; }
    }
}