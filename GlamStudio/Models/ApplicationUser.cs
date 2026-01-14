using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GlamStudio.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        [MaxLength(50)]
        public string FirstName { get; set; }
        [Required]
        [MaxLength(50)]
        public string LastName { get; set; }
        [StringLength(100)]
        public virtual ICollection<EmployeeSpecialization> Specializations { get; set; } = new List<EmployeeSpecialization>();

        [MaxLength(200)]
        public string? Address { get; set; }

        [MaxLength(50)]
        public string? City { get; set; }
        [Display(Name = "Notatki Salonu (Poufne)")]
        public string? SalonNotes { get; set; }

        public virtual ICollection<Appointment>? ClientAppointments { get; set; }
        public virtual ICollection<Appointment>? EmployeeAppointments { get; set; }
        public virtual ICollection<WorkSchedule>? WorkSchedules { get; set; }

        [NotMapped]
        public string FullName
        {
            get { return FirstName + " " + LastName; }
        }

    }
}
