using System.Collections.Generic;

namespace ServicesCenterV3._1.Models
{
    public class UserRolesViewModel
    {
        public string UserId { get; set; }
        public string Name { get; set; }
        public string Lastname { get; set; }
        public string Email { get; set; }
        public DateOnly DataBith { get; set; } // тип DateTime для сумісності з HTML
        public IList<string> Roles { get; set; }

        public bool IsLockedOut { get; set; } // Додаємо поле для статусу блокування

        public List<Supplier> suppliers { get; set; }
    }

}
