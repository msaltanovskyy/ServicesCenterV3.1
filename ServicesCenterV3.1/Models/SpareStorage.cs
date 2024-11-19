using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ServicesCenterV3._1.Models
{
    [Table("spare_storage")]
    public class SpareStorage
    {
        [Key]
        [Column("sapre_storage_id")]
        public int SpareStorageId { get; set; }
        [Column("sapre_name")]
        public string SapreName { get; set; }
        [Column("sapre_quantity")]
        public int Quantity { get; set; }
        [Column("price")]
        public decimal Price { get; set; }

        ///
        /// Ключі
        ///
        [Column("invoice_supplier_id")]
        public int InvoiceSupplierId {  get; set; }
        [ForeignKey("InvoiceSupplierId")]
        public InvoiceSupplier invoiceSupplier { get; set; }

        public IEnumerable<Spare> spares { get; set; }
    }
}
