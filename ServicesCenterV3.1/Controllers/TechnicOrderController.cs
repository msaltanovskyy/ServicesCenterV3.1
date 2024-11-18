using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServicesCenterV3._1.Data;
using ServicesCenterV3._1.Models;
using ServicesCenterV3._1.ViewModels;
using System.Linq;

namespace ServicesCenterV3._1.Controllers
{
    //[Authorize(Roles = "Manager")]
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
            // Збір даних для діаграм
            var orderStats = _context.Orders
                .GroupBy(o => o.Status)
                .Select(group => new {
                    Status = group.Key,
                    Count = group.Count()
                }).ToList();

            var technicTypeStats = _context.Technics
                .GroupBy(t => t.TechnicType)
                .Select(group => new {
                    TechnicType = group.Key,
                    Count = group.Count()
                }).ToList();

            var sparePartStats = _context.spares
                    .GroupBy(s => s.SpareName)
                    .Select(group => new {
                    SpareName = group.Key,
                    Count = group.Sum(s => s.SpareValue) // Сума кількості запчастин
                    }).ToList();

            var masterStats = _context.Orders
                .GroupBy(o => o.UserMaster.UserName)
                .Select(group => new {
                    MasterName = group.Key,
                     Count = group.Count()
                 }).ToList();

            var clientStats = _context.Orders
            .GroupBy(o => o.UserClient.UserName)
                .Select(group => new {
                 ClientName = group.Key,
                 Count = group.Count()
                 }).ToList();

            ViewBag.ClientStats = clientStats;

            ViewBag.MasterStats = masterStats;

            // Передати дані в представлення
            ViewBag.OrderStats = orderStats;
            ViewBag.TechnicTypeStats = technicTypeStats;
            ViewBag.SparePartStats = sparePartStats;

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
            var spares = _context.spares.ToList();
 

            // Фільтруємо замовлення по номеру, якщо пошуковий параметр не порожній
            var ordersQuery = _context.Orders.Include(i => i.Invoice).Include(r => r.review).AsQueryable();

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
                Client = client,
                Spares = spares,
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
                            SpareId = null
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


        public async Task<IActionResult> Details(int id)
        {
            var invoice = await _context.invoices
                .Include(i => i.order) // Включаем заказ
                .Include(i => i.SpareInvoices) // Включаем данные из таблицы SpareInvoice
                    .ThenInclude(si => si.Spare) // Включаем данные о запчастях
                .FirstOrDefaultAsync(i => i.InvoiceId == id);

            if (invoice == null)
            {
                return NotFound();
            }

            var viewModel = new InvoiceDetailsViewModel
            {
                Invoice = invoice,
                Order = invoice.order,
                Spares = invoice.SpareInvoices
                    .Where(si => si.SpareId != null)
                    .Select(si => si.Spare)
                    .ToList()
            };

            return View(viewModel);
        }

        public IActionResult AddSpare(string spareName, double spareCost, int spareValue)
        {
            if (ModelState.IsValid)
            {
                var spare = new Spare
                {
                    SpareName = spareName,
                    SpareCost = spareCost,
                    SpareValue = spareValue,
                };

                _context.spares.Add(spare);
                _context.SaveChanges();

                return RedirectToAction(nameof(Index));
            }

            return View();
        }

        [HttpPost]
        public IActionResult EditStatusCancel(int orderId)
        {
            var order = _context.Orders.Find(orderId);
            if (order == null)
            {
                return NotFound();
            }

            order.Status = "Відмінено"; // Змінюємо статус на "Відмінити замовлення"
            _context.SaveChanges();

            return RedirectToAction("Index"); // Повертаємо на сторінку зі списком замовлень
        }

        [HttpPost]
        public IActionResult EditStatusReady(int orderId)
        {
            var order = _context.Orders.Find(orderId);
            if (order == null)
            {
                return NotFound();
            }

            order.Status = "Видано"; // Змінюємо статус на "Відати замовлення"
            _context.SaveChanges();

            return RedirectToAction("Index"); // Повертаємо на сторінку зі списком замовлень
        }

    }

}


