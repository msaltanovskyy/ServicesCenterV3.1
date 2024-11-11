using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ServicesCenterV3._1.Models
{
    [Table("technic")]
    public class Technic
    {
        [Key]
        [Display(Name = "Назва")]
        [Required(ErrorMessage = "Введіть названня техніки")]
        [StringLength(50, ErrorMessage = "Максимально 50 символів")]
        [Column("name")]
        public string NameTechnic { get; set; }

        [Display(Name = "Тип техніки")]
        [Required(ErrorMessage = "Оберіть тип")]
        [Column("type")]
        public string TechnicType { get; set; }

        [ForeignKey("TechnicType")]
        public TypeTechnic typeTechnic { get; set; }

        public IEnumerable<Order> orders { get; set; }
    }
}
