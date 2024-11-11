using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServicesCenterV3._1.Data;
using ServicesCenterV3._1.Models;
using System.Linq;

namespace ServicesCenterV3._1.Controllers
{
    public class TechnicOrderController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly RoleManager<IdentityRole> _roleManager;

        public TechnicOrderController(ApplicationDbContext context,
            RoleManager<IdentityRole> roleManager)
        {
            _roleManager = roleManager;
            _context = context;
        }

        // Сторінка для перегляду списку техніки та замовлень
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

        // POST: Додати нову техніку
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddTechnic(string NameTechnic, string TechnicType, int Cost)
        {
            if (ModelState.IsValid)
            {
                var technic = new Technic
                {
                    NameTechnic = NameTechnic,
                    TechnicType = TechnicType,
                    typeTechnic = _context.TypeTechnics.FirstOrDefault(t => t.Type == TechnicType) ?? new TypeTechnic { Type = TechnicType, Cost = Cost }
                };

                _context.Technics.Add(technic);
                _context.SaveChanges();

                return RedirectToAction(nameof(Index));
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateOrder(string technicId, string reason, DateOnly dateEnd, DateOnly dataCreate, string masterId, string clientId)
        {
            if (ModelState.IsValid)
            {
                // Отримуємо об'єкт техніки на основі переданого technicId
                var technic = _context.Technics.Include(t => t.typeTechnic).FirstOrDefault(t => t.NameTechnic == technicId);
                if (technic == null)
                {
                    ModelState.AddModelError("", "Вибрана техніка не знайдена.");
                    return View("Index");
                }

                // Отримуємо об'єкти майстра та клієнта на основі переданих ідентифікаторів
                var master = _context.Users.FirstOrDefault(u => u.Id == masterId);
                var client = _context.Users.FirstOrDefault(u => u.Id == clientId);

                if (master == null || client == null)
                {
                    ModelState.AddModelError("", "Невірний ідентифікатор майстра або клієнта.");
                    return View("Index");
                }

                // Створюємо новий об'єкт замовлення
                var order = new Order
                {
                    NameTecnic = technic.NameTechnic,
                    Reasons = reason,
                    DateCreate = dataCreate,
                    DateEnd = dateEnd,
                    Status = "В процесі", // Початковий статус
                    UserMaster = master,
                    UserClient = client,
                };

                var priceTechnic = technic.typeTechnic.Cost;

                using (var transaction = _context.Database.BeginTransaction())
                {
                    try
                    {
                        // Спочатку зберігаємо замовлення в базу даних
                        _context.Orders.Add(order);
                        _context.SaveChanges(); // Після цього order.OrderId буде заповнено

                        // Тепер створюємо інвойс з наявним OrderId
                        var invoice = new Invoice
                        {
                            OrderId = order.OrderId,
                            InvoicePrice = priceTechnic
                        };

                        _context.invoices.Add(invoice);
                        _context.SaveChanges();

                        order.InvoiceId = invoice.InvoiceId;
                        _context.Orders.Update(order);
                        _context.SaveChanges();

                        // Створюємо запис у SpareInvoice з наявним InvoiceId
                        var spareInvoice = new SpareInvoice
                        {
                            InvoiceId = invoice.InvoiceId,
                            SapreId = null
                        };

                        _context.spareInvoices.Add(spareInvoice);
                        _context.SaveChanges();

                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                        ModelState.AddModelError("", "Помилка збереження замовлення.");
                        return View("Index");
                    }
                }

                return RedirectToAction(nameof(Index));
            }

            // Якщо модель не пройшла валідацію, повертаємо на сторінку з формою
            return View("Index");
        }

        [HttpGet]
        public IActionResult OpenCheck(int id)
        {
            return View();
        }

        [HttpPost]
        public IActionResult OpenChek()
        {
            return View();
        }


    }

}


