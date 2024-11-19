using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ServicesCenterV3._1.Models
{
    [Table("supplier")]
    public class Supplier
    {
        [Key]
        [Column("supplier_id")]
        public int SupplierId { get; set; }
        [Column("supplier_name")]
        [Required(ErrorMessage = "Введіть назву компанії")]
        public string SupplierName { get; set; } = string.Empty;

        [Column("supplier_adress")]
        [Required(ErrorMessage = "Введіть назву адрессу компанії")]
        [Display(Name = "Адресс")]
        public string SupplierAdress { get; set; } = string.Empty;

        [Column("website")]
        [Display(Name = "Сайт")]
        [DataType(DataType.Url)]
        [Required(ErrorMessage = "Введіть коректний веб-сайт")]
        public string Website {  get; set; }

        [Column("email")]
        [Display(Name = "Пошта")]
        [DataType(DataType.EmailAddress, ErrorMessage = "Введіть корекнтний адресс")]
        [Required(ErrorMessage = "Введіть пошту")]
        public string Email { get; set; }

        [Column("telefon")]
        [Display(Name = "Телефон")]
        [Required(ErrorMessage = "Ведіть телефон")]
        public string Telefon { get; set; }

        ///<summary>
        ///Ключі звязку
        ///</summary>   
        public IEnumerable<InvoiceSupplier> invoiceSuppliers { get; set; }
    }
}
