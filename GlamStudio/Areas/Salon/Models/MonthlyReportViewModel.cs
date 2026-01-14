namespace GlamStudio.Areas.Salon.Models
{
    public class MonthlyReportViewModel
    {
        public string MonthLabel { get; set; }
        public int Year { get; set; }
        public int Month { get; set; }
        public int TotalAppointments { get; set; }
        public decimal TotalRevenue { get; set; }
    }
}