using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ServicesCenterV3._1.Models
{
    [Table("order")]
    public class Order
    {
        [Key]
        [Column("order_id")]
        public int OrderId { get; set; }

        [Display(Name = "Назва техніки")]
        [Required(ErrorMessage = "Оберіть техніку")]
        [Column("technic_name")]
        public string NameTecnic { get; set; }
        [ForeignKey("NameTecnic")]
        public Technic technic { get; set; }

        [Display(Name = "Причина")]
        [DataType(DataType.MultilineText)]
        [Required(ErrorMessage = "Введіть причину")]
        [StringLength(200, ErrorMessage = "Максимально 200 символів")]
        [Column("reason")]
        public string Reasons { get; set; }

        [Display(Name = "Дата створення")]
        [DataType(DataType.Date)]
        [Column("date_create")]
        public DateOnly DateCreate { get; set; }    

        [Display(Name = "Дата закінчення")]
        [Required(ErrorMessage = "Введіть дату закінчення")]
        [DataType(DataType.Date)]
        [Column("date_end")]
        public DateOnly DateEnd { get; set; }

        [Display(Name = "Статус")]
        [Column("status")]
        public string Status { get; set; }

        /// <summary>
        /// Ключи для связку с таблицями
        /// </summary>
        /*________________________________________________________________________________*/ 

        [Column("master_id")]
        [Display(Name = "Майстер")]
        public string ?MasterId { get; set; }

        [ForeignKey("MasterId")]
        public User UserMaster { get; set; }

        [Column("client_id")]
        [Display(Name = "Клієнт")]
        public string ClientId { get; set; }

        [ForeignKey("ClientId")]
        public User UserClient { get; set; }

        [Column("invoice_id")]
        [Display(Name = "Номер чеку")]
        public int ?InvoiceId { get; set; }

        [ForeignKey("InvoiceId")]
        public Invoice Invoice { get; set; }

        [Column("review_id")]
        public int ?ReviewId { get; set; }
        public Review review { get; set; }


    }
}
