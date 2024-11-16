namespace ServicesCenterV3._1.Models
{
    public class TechnicOrderViewModel
    {
        // Список технік для вибору
        public IEnumerable<Technic> Technics { get; set; }

        // Список замовлень
        public IEnumerable<Order> Orders { get; set; }

        // Список типів технік (це потрібно для форми додавання техніки)
        public IEnumerable<TypeTechnic> typeTechnics { get; set; }

        // Список клієнтів для вибору
        public IEnumerable<User> Client { get; set; }

        // Список майстрів для вибору
        public IEnumerable<User> Master { get; set; }

        public Spare NewSpare { get; set; } = new Spare();
        public IEnumerable<Spare> Spares { get; set; }

        public List<int> SelectedSpareIds { get; set; } = new List<int>();
        public int OrderId { get; set; }

    }
}
