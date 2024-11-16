using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServicesCenterV3._1.Data;
using ServicesCenterV3._1.Models;

namespace ServicesCenterV3._1.Controllers
{
    [Authorize(Roles = "Client")]
    public class ClientController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly RoleManager<IdentityRole> _roleManager;

        public ClientController(ApplicationDbContext context,
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
                Orders = orders,
                typeTechnics = typeTechnics,
                Master = master,
                Client = client
            };

            return View(model);
        }

    }
}
