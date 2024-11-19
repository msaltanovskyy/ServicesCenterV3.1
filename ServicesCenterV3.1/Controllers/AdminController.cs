using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using ServicesCenterV3._1.Data;
using ServicesCenterV3._1.Models;
using ServicesCenterV3._1.ViewModels;
using System.Security.Claims;

namespace ServicesCenterV3._1.Controllers
{
    [Route("Admin")]
    //[Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _context;

        public AdminController(UserManager<User> userManager,
            RoleManager<IdentityRole> roleManager,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
        }

        // Метод для відображення списку користувачів та їхніх ролей
        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            var users = _userManager.Users.ToList();
            var model = new List<UserRolesViewModel>();
            ViewBag.Roles = _roleManager.Roles.ToList();
            ViewBag.Suppliers = _context.suppliers.ToList();

            foreach (var user in users)
            {
                var userRoles = await _userManager.GetRolesAsync(user);
                var userModel = new UserRolesViewModel
                {
                    UserId = user.Id,
                    Name = user.Name,
                    Lastname = user.Lastname,
                    Email = user.UserName,
                    DataBith = user.DataBith,
                    Roles = userRoles,
                    IsLockedOut = await _userManager.IsLockedOutAsync(user) // Додаємо статус блокування
                };
                model.Add(userModel);
            }

            return View(model);
        }

        // Метод для оновлення ролей користувача
        [HttpPost("UpdateUserRole")]
        public async Task<IActionResult> UpdateUserRole([FromBody] UpdateUserRoleModel model)
        {
            // Перевірка на null і коректність даних
            if (model == null || string.IsNullOrEmpty(model.UserId) || string.IsNullOrEmpty(model.RoleName))
            {
                return BadRequest("Невірні дані для оновлення ролі.");
            }

            var user = await _userManager.FindByIdAsync(model.UserId);
            if (user == null)
            {
                return NotFound("Користувача не знайдено.");
            }

            // Додаємо або видаляємо роль
            if (model.IsSelected)
            {
                if (!await _userManager.IsInRoleAsync(user, model.RoleName))
                {
                    await _userManager.AddToRoleAsync(user, model.RoleName);
                }
            }
            else
            {
                if (await _userManager.IsInRoleAsync(user, model.RoleName))
                {
                    await _userManager.RemoveFromRoleAsync(user, model.RoleName);
                }
            }

            return Ok("Роль оновлено успішно.");
        }

        [HttpPost("UpdateUserLockout")]
        public async Task<IActionResult> UpdateUserLockout([FromBody] UpdateUserLockoutModel model)
        {
            // Перевірка на null і коректність даних
            if (model == null || string.IsNullOrEmpty(model.UserId))
            {
                return BadRequest("Невірні дані для оновлення блокування.");
            }

            var user = await _userManager.FindByIdAsync(model.UserId);
            if (user == null)
            {
                return NotFound("Користувача не знайдено.");
            }

            // Блокування або розблокування користувача
            if (model.Lockout)
            {
                user.LockoutEnd = DateTimeOffset.UtcNow.AddYears(100); // Блокувати на тривалий час
                user.LockoutEnabled = true;
            }
            else
            {
                user.LockoutEnd = null; // Розблокувати
                user.LockoutEnabled = false;
            }

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                return StatusCode(500, "Помилка при оновленні блокування.");
            }

            return Ok("Статус блокування користувача оновлено успішно.");
        }


        [HttpPost("EditUser")]
        public async Task<IActionResult> EditUser([FromBody] EditUserModel model)
        {
            if (model == null || string.IsNullOrEmpty(model.UserId))
            {
                return BadRequest("Невірні дані для редагування користувача.");
            }

            var user = await _userManager.FindByIdAsync(model.UserId);
            if (user == null)
            {
                return NotFound("Користувача не знайдено.");
            }

            user.Name = model.UserName;
            user.Lastname = model.UserLastname;
            user.Email = model.UserEmail;
            user.DataBith = model.UserDataBith;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                return StatusCode(500, "Помилка при оновленні користувача.");
            }

            return Ok("Дані користувача оновлено успішно.");
        }

        [HttpPost]
        public IActionResult SupplierCreate(string SupplierAdress,
            string Website,string SupplierName,string Email,string Telefon)
        {
            var supplier = new Supplier
            {
                Email = Email,
                Telefon = Telefon,
                SupplierAdress = SupplierAdress,
                SupplierName = SupplierName,
                Website = Website
            };

            if (ModelState.IsValid)
            {
                _context.suppliers.Add(supplier);
                _context.SaveChanges();
                return RedirectToAction(nameof(Index));
            }
            else if(supplier == null)
            {
                ModelState.TryAddModelError("", "Помилка в додавані");
                return View();
            }
            return View();
            
        }


        [HttpGet("Admin/InvoiceDelivery/{supplierid}")]
        public async Task<IActionResult> InvoiceDelivery(int supplierid)
        {
            var supplier = (await _context.suppliers.FindAsync(supplierid)).SupplierId;

            if(supplierid == null)
            {
                ModelState.TryAddModelError("", "Постачальник не знайдений");
            }

            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null) {

                userId = "11";
            }

            var invoice_delivery = new InvoiceSupplier
            {
                SupplierId = supplierid,
                UserId = userId,
                DateCreate = DateTime.UtcNow,
            };
 
            if (ModelState.IsValid)
            {
                await _context.invoicesSupplier.AddAsync(invoice_delivery);
                await _context.SaveChangesAsync();
                int id_delivery = invoice_delivery.InvoiceSupplierId;
                return RedirectToAction("AddSpareStorages", new { id_delivery });
            }

            return View(nameof(Index));
        }

        [HttpGet("Admin/AddSpareStorages/{id_delivery}")]
        public IActionResult AddSpareStorages(int id_delivery)
        {
            if (id_delivery == 0)
            {
                // Логика обработки id_delivery = 0
                // Например, можно перенаправить на другую страницу или вернуть ошибку
                ModelState.AddModelError("", "Невірний ідентифікатор інвойсу.");
                return RedirectToAction("Index");
            }

            var spareStorage = new SpareStorageViewModel
            {
                InvoiceSupplierId = id_delivery,
                SpareStorages = new List<SpareStorage> { new SpareStorage() }  // Инициализируем пустым элементом для первого ввода
            };
            return View(spareStorage);
        }
        [HttpPost("Admin/AddSpareStorages/{id_delivery}")]
        public async Task<IActionResult> AddSpareStorages(SpareStorageViewModel sapreStorage, int id_delivery)
        {

            if (!ModelState.IsValid)
            {
                // Перевірка, чи інвойс (id_delivery) існує
                var invoice = await _context.invoicesSupplier.FindAsync(id_delivery);
                if (invoice == null)
                {
                    ModelState.AddModelError("", "Інвойс не знайдений.");
                    return View(sapreStorage); // Повертаємо форму з повідомленням про помилку
                }
         
                foreach (var spareStorage in sapreStorage.SpareStorages)
                {
                    spareStorage.InvoiceSupplierId = id_delivery; // Призначаємо InvoiceSupplierId
                    _context.spareStorages.Add(spareStorage); // Додаємо кожну запчастину до бази

                    await _context.SaveChangesAsync();

                    var spare = new Spare
                    {
                        SpareName = spareStorage.SapreName,
                        SpareCost = (double)spareStorage.Price,
                        SpareValue = spareStorage.Quantity,
                        SpareStorageId = spareStorage.SpareStorageId,
                    };

                    _context.spares.Add(spare);
                    await _context.SaveChangesAsync(); // Зберігаємо зміни в базі
                }
               
                try
                {
                    
                    return RedirectToAction(nameof(Index)); // Перехід на потрібну сторінку після збереження
                }
                catch (Exception ex)
                {
                    // Логування помилки та повернення на форму з повідомленням про помилку
                    ModelState.AddModelError("", $"Сталася помилка при збереженні: {ex.Message}");
                }
            }
            return View(sapreStorage); // Якщо модель не валідна, повертаємо на форму
        }




        // Модель для отримання даних з JavaScript-функції
        public class UpdateUserRoleModel
        {
            public string UserId { get; set; }
            public string RoleName { get; set; }
            public bool IsSelected { get; set; }
        }

        public class UpdateUserLockoutModel
        {
            public string UserId { get; set; }
            public bool Lockout { get; set; }

        }

        public class EditUserModel
        {
            public string UserId { get; set; }
            public string UserName { get; set; }
            public string UserLastname { get; set; }
            public string UserEmail { get; set; }
            public DateOnly UserDataBith { get; set; }
        }


    }
}
