using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GlamStudio.Models
{
    public class Service
    {
        public Service()
        {
            Appointments = new List<Appointment>();
        }
        [Key]
        public int ServiceID { get; set; }

        [Required(ErrorMessage = "Nazwa usługi jest wymagana.")]
        [MaxLength(100)]
        public string ServiceName { get; set; }

        public string? Description { get; set; }

        [Required(ErrorMessage = "Cena jest wymagana.")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Czas trwania jest wymagany (w minutach).")]
        public TimeSpan Duration { get; set; }

        [NotMapped]
        [Required(ErrorMessage = "Czas trwania jest wymagany (w minutach).")]
        [Display(Name = "Czas trwania (w minutach)")]
        [Range(1, int.MaxValue, ErrorMessage = "Czas trwania musi być dodatni.")]
        public int DurationInMinutes
        {
            get
            {
                return (int)Duration.TotalMinutes;
            }
            set
            {
                Duration = TimeSpan.FromMinutes(value);
            }
        }

        [Required(ErrorMessage = "Kategoria jest wymagana.")]
        [Display(Name = "Kategoria")]
        public ServiceCategory Category { get; set; }
        public virtual ICollection<Appointment> Appointments { get; set; }
    }
}
