using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GlamStudio.Models
{
    public class EmployeeSpecialization
    {
        [Key]
        public int Id { get; set; }

        public ServiceCategory Category { get; set; }

        [Required]
        public string ApplicationUserId { get; set; }

        [ForeignKey("ApplicationUserId")]
        public virtual ApplicationUser Employee { get; set; }
    }
}