using System.ComponentModel.DataAnnotations.Schema;

namespace ServicesCenterV3._1.Models
{
    [Table("spare_invoice")]
    public class SpareInvoice
    {
        [Column("spare_invoice_id")]
        public int SpareInvoiceId { get; set; }
        [Column("spare_id")]
        public int ?SapreId { get; set; }
        public Spare ?spare { get; set; }

        [Column("invoice_id")]
        public int InvoiceId { get; set; }
        public Invoice Invoice { get; set; }    
    }
}
