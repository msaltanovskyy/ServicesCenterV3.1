using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServicesCenterV3._1.Data;
using ServicesCenterV3._1.Models;

namespace ServicesCenterV3._1.Controllers
{
    [Authorize(Roles = "Master")]
    public class MasterController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly RoleManager<IdentityRole> _roleManager;

        public MasterController(ApplicationDbContext context,
            RoleManager<IdentityRole> roleManager)
        {
            _roleManager = roleManager;
            _context = context;
        }
        public IActionResult Index(string searchOrderId)
        {

            var masterRoleId = _roleManager.Roles.FirstOrDefault(r => r.Name == "Master")?.Id;
            var master = _context.Users
                .Where(u => u.LockoutEnd == null)
                .Join(_context.UserRoles,
                    user => user.Id,
                    userRole => userRole.UserId,
                    (user, userRole) => new { user, userRole })
                .Where(ur => ur.userRole.RoleId == masterRoleId)
            .Select(ur => ur.user)
                .ToList();

            // Отримуємо користувачів з роллю "Client"
            var clientRoleId = _roleManager.Roles.FirstOrDefault(r => r.Name == "Client")?.Id;
            var client = _context.Users
                .Where(u => u.LockoutEnd == null)
                .Join(_context.UserRoles,
                    user => user.Id,
                    userRole => userRole.UserId,
                    (user, userRole) => new { user, userRole })
                .Where(ur => ur.userRole.RoleId == clientRoleId)
                .Select(ur => ur.user)
                .ToList();


            // Отримуємо всі техніки, типи технік, майстрів та клієнтів
            var technics = _context.Technics.Include(t => t.typeTechnic).ToList();
            var typeTechnics = _context.TypeTechnics.ToList();


            // Фільтруємо замовлення по номеру, якщо пошуковий параметр не порожній
            var ordersQuery = _context.Orders.Include(i => i.Invoice).AsQueryable();

            if (!string.IsNullOrEmpty(searchOrderId))
            {
                ordersQuery = ordersQuery.Where(o => o.OrderId.ToString().Contains(searchOrderId));
            }

            // Отримуємо замовлення після фільтрації
            var orders = ordersQuery.ToList();

            // Створюємо модель для View
            var model = new TechnicOrderViewModel
            {
                Technics = technics,
                Orders = orders.Where(o=> o.Status != "Видано" && o.Status != "Відмінено"),
                typeTechnics = typeTechnics,
                Master = master,
                Client = client,
                Spares = _context.spares.ToList()
            };

            return View(model);
        }

        [HttpPost("EditNextReady/{id}")]
        public IActionResult EditNextReady(int id)
        {
            var order = _context.Orders.Find(id);
            if (order == null)
            {
                return NotFound();
            }

            order.Status = "Готово до видачі"; // Змінюємо статус на "Відати замовлення"
            _context.SaveChanges();

            return RedirectToAction("Index","Master"); // Повертаємо на сторінку зі списком замовлень
        }

        [HttpPost]
        public IActionResult AddSpare(int orderId, List<int> selectedSpareIds)
        {
            if (selectedSpareIds == null || !selectedSpareIds.Any())
            {
                return BadRequest("Не вибрано жодної запчастини.");
            }

            // Отримуємо замовлення разом з чеком і специфікацією запасних частин
            var order = _context.Orders
                                .Include(o => o.Invoice)
                                .ThenInclude(i => i.SpareInvoices)
                                .FirstOrDefault(o => o.OrderId == orderId);

            if (order == null)
            {
                return NotFound("Замовлення не знайдено.");
            }

            // Перевіряємо, чи є у замовлення чек, якщо ні, створюємо новий
            if (order.Invoice == null)
            {
                order.Invoice = new Invoice { OrderId = orderId, InvoicePrice = 0 };
                _context.invoices.Add(order.Invoice);
                _context.SaveChanges();
            }

            // Додаємо запасні частини до чека та оновлюємо загальну ціну
            double totalSpareCost = 0;

            foreach (var spareId in selectedSpareIds)
            {
                var spare = _context.spares.Find(spareId);
                if (spare == null) continue;

                var spareInvoice = new SpareInvoice
                {
                    InvoiceId = order.Invoice.InvoiceId,
                    SpareId = spareId
                };

                _context.spareInvoices.Add(spareInvoice);
                totalSpareCost += spare.SpareCost;
            }

            // Оновлюємо загальну ціну в чеку
            order.Invoice.InvoicePrice += totalSpareCost;
            _context.SaveChanges();

            return RedirectToAction("Index");
        }




    }
}
