using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Identity.Client;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ServicesCenterV3._1.Models
{
    [Table("spare")]
    public class Spare
    {
        [Column("spare_id")]
        public int SpareId { get; set; }
        [Column("spare_name")]
        [Display(Name = "Назва деталі")]
        [StringLength(50, ErrorMessage ="Максммальна кількість символів 50")]
        public string SpareName { get; set; }
        [Column("spare_cost")]
        [Display(Name = "Ціна деталі")]
        public double SpareCost { get; set; }
        [Column("spare_value")]
        [Display(Name = "Кількість деталей")]
        public int SpareValue { get; set; }

        [Column("spare_palace_storage_id")]
        [Display(Name="Місце на складі")]
        public int SpareStorageId { get; set; }
        [ForeignKey(nameof(SpareStorageId))]
        public SpareStorage spareStorage { get; set; }


    }
}
