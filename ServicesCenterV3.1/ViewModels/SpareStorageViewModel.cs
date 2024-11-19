using ServicesCenterV3._1.Models;
using System.ComponentModel.DataAnnotations;

namespace ServicesCenterV3._1.ViewModels
{
    public class SpareStorageViewModel
    {

        public int InvoiceSupplierId { get; set; }
        [Required(ErrorMessage = "Назва запчастини обов'язкова.")]
        public string SapreName { get; set; }

        [Required(ErrorMessage = "Кількість запчастин обов'язкова.")]
        [Range(1, int.MaxValue, ErrorMessage = "Кількість повинна бути більшою за 0.")]
        public int Quantity { get; set; }

        [Required(ErrorMessage = "Ціна запчастини обов'язкова.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Ціна повинна бути більшою за 0.")]
        public decimal Price { get; set; }

        public List<SpareStorage> SpareStorages { get; set; }
    }

}
