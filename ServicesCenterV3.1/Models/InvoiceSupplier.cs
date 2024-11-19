using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ServicesCenterV3._1.Models
{
    [Table("invoice_supplier")]
    public class InvoiceSupplier
    {
        [Key]
        [Column("invoice_supplier_id")]
        public int InvoiceSupplierId { get; set; }  
        [Column("admin_id")]
        public string UserId { get; set; }
        [ForeignKey("UserId")]
        public User User { get; set; }

        [Column("supplier_id")]
        public int SupplierId { get; set; }
        [ForeignKey("SupplierId")]
        public Supplier Supplier { get; set; }

        [Column("date_create")]
        public DateTime DateCreate { get; set; } = DateTime.Now;
    }
}
