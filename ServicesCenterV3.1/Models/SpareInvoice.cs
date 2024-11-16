using System.ComponentModel.DataAnnotations.Schema;

namespace ServicesCenterV3._1.Models
{
    [Table("spare_invoice")]
    public class SpareInvoice
    {
        [Column("spare_invoice_id")]
        public int SpareInvoiceId { get; set; }

        [Column("spare_id")] // Виправлено ідентифікатор
        public int? SpareId { get; set; } // Виправлено ім'я поля

        [ForeignKey("SpareId")]
        public Spare? Spare { get; set; } // Виправлено ім'я поля

        [Column("invoice_id")]
        public int InvoiceId { get; set; }

        public Invoice Invoice { get; set; }
    }
}
