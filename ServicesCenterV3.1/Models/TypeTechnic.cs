using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ServicesCenterV3._1.Models
{
    [Table("type_technic")]
    public class TypeTechnic
    {
        [Key]
        [Column("type")]
        public string Type { get; set; }
        [Column("cost")]
        public int Cost { get; set; }

        public IEnumerable<Technic> technics { get; set; }
    }
}
