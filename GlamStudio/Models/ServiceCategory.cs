using System.ComponentModel.DataAnnotations;

namespace GlamStudio.Models
{
    public enum ServiceCategory
    {
        [Display(Name = "Zabiegi na twarz")]
        Twarz,

        [Display(Name = "Zabiegi na ciało")]
        Ciało,

        [Display(Name = "Zabiegi na dłonie")]
        Dłonie,

        [Display(Name = "Fryzjerstwo")]
        Fryzjerstwo
    }
}
