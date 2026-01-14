using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GlamStudio.Models
{
    public class WorkSchedule
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Display(Name = "Początek zmiany")]
        public DateTime StartTime { get; set; }

        [Required]
        [Display(Name = "Koniec zmiany")]
        public DateTime EndTime { get; set; }

        [Display(Name = "Czy urlop?")]
        public bool IsTimeOff { get; set; } = false;

        [Required]
        public string ApplicationUserId { get; set; }

        [ForeignKey("ApplicationUserId")]
        public virtual ApplicationUser Employee { get; set; }
    }
}