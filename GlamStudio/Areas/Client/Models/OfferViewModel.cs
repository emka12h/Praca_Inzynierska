using GlamStudio.Models;

namespace GlamStudio.ViewModels
{
    public class OfferViewModel
    {
        public Dictionary<ServiceCategory, List<Service>> ServicesByCategory { get; set; }

        public OfferViewModel()
        {
            ServicesByCategory = new Dictionary<ServiceCategory, List<Service>>();
        }
    }
}