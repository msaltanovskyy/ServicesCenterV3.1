using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ServicesCenterV3._1.Models
{
    [Table("invoice")]
    public class Invoice
    {
        [Column("invoice_id")]
        public int InvoiceId { get; set; }

        [Column("cost")]
        [Display(Name = "Ціна")]
        public double InvoicePrice { get; set; }

        [Column("order_id")]
        [Display(Name = "Номер замовлення")]
        public int OrderId { get; set; }    
        public Order order { get; set; }    

    }
}
 // Создать Invoice хранилище чеков, Spare - запчасти, Ордер + Sapre-Invocie