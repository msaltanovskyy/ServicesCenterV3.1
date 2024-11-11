using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;

namespace ServicesCenterV3._1.Models
{
    [Table("user")]
    public class User : IdentityUser
    {

        [Display(Name = "Імя")]
        [Column("name")]
        [Required(ErrorMessage = "Ведіть імя")]
        [StringLength(20, ErrorMessage = "Максимальна довжина 15 символів")]
        public string Name { get; set; } = string.Empty;
        [Display(Name = "Прізвище")]
        [Column("lastname")]
        [Required(ErrorMessage = "Ведіть прізвище")]
        [StringLength(20, ErrorMessage = "Максимальна довжина 15 символів")]
        public string Lastname { get; set; } = string.Empty;
        [Display(Name = "Номер телефону")]
        [Column("phone")]
        [Required(ErrorMessage = "Ведіть номер телефону")]
        [StringLength(20, ErrorMessage = "Максимальна довжина 15 символів")]
        public string Phonenummer { get; set; } = string.Empty;

        [Display(Name = "Дата народження")]
        [Column("date_bith")]
        [DataType(DataType.Date)]
        public DateOnly DataBith { get; set; }

        
    }
}
