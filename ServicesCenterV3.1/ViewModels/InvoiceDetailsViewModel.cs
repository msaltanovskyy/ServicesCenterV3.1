using ServicesCenterV3._1.Models;

namespace ServicesCenterV3._1.ViewModels
{
    public class InvoiceDetailsViewModel
    {
        public Invoice Invoice { get; set; }
        public Order Order { get; set; }
        public List<Spare> Spares { get; set; }
    }
}
