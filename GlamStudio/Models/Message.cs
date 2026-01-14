using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GlamStudio.Models
{
    public class Message
    {
        [Key]
        public int MessageId { get; set; }
        [Required(ErrorMessage = "Imię jest wymagane.")]
        [StringLength(100)]
        public string SendersName { get; set; }

        [Required(ErrorMessage = "E-mail jest wymagany.")]
        [EmailAddress]
        [StringLength(100)]
        public string SendersEmail { get; set; }

        [Required(ErrorMessage = "Treść wiadomości jest wymagana.")]
        public string Contents { get; set; }

        public DateTime DateSent { get; set; }

        public bool IsReadBySalon { get; set; } = false;
        public bool IsReadByClient { get; set; } = true;
        public string? ApplicationUserId { get; set; }
        [ForeignKey("ApplicationUserId")]
        public virtual ApplicationUser? ApplicationUser { get; set; }

        [Display(Name = "Treść odpowiedzi")]
        public string? ReplyContent { get; set; }
        [Display(Name = "Data odpowiedzi")]
        public DateTime? ReplyDate { get; set; }

        public string? ReplySentById { get; set; }

        [ForeignKey("ReplySentById")]
        [Display(Name = "Odpowiedział")]
        public virtual ApplicationUser? ReplySentBy { get; set; }
    }
}