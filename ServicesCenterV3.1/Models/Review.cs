using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ServicesCenterV3._1.Models
{
    [Table("review")]
    public class Review
    {
        [Column("review_id")]
        public int ReviewId { get; set; }

        [Column("order_id")]
        public int? OrderId { get; set; } = null;

        [ForeignKey("OrderId")]
        public Order order { get; set; }

        [Range(1, 5, ErrorMessage = "Рейтинг повинен бути від 1 до 5")]
        [Display(Name = "Рейтинг")]
        [Column("rating")]
        public int Rating { get; set; } 

        [Column("text")]
        [Display(Name = "Відгук")]
        [StringLength(200, ErrorMessage = "Максимална к-ть символів 200!")]
        public string Text { get; set; }
    }
}
