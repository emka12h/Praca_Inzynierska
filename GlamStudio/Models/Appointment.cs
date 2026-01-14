using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GlamStudio.Models
{
    public class Appointment
    {
        [Key]
        public int AppointmentID { get; set; }
        [Required]
        public DateTime AppointmentDate { get; set; }
        [Required]
        public TimeSpan AppointmentTime { get; set; }
        public AppointmentStatus Status { get; set; } = AppointmentStatus.Scheduled;
        [Required]
        public string EmployeeID { get; set; }
        [Required]
        public string ClientID { get; set; }

        [Required]
        public int ServiceID { get; set; }
        [ForeignKey("ServiceID")]
        public virtual Service Service { get; set; }

        [ForeignKey("EmployeeID")]
        [InverseProperty("EmployeeAppointments")]
        public virtual ApplicationUser Employee { get; set; }
        [ForeignKey("ClientID")]
        [InverseProperty("ClientAppointments")]
        public virtual ApplicationUser Client { get; set; }
    }
}
