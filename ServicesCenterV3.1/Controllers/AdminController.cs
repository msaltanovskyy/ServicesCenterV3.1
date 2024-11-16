using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ServicesCenterV3._1.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ServicesCenterV3._1.Controllers
{
    [Route("Admin")]
    //[Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AdminController(UserManager<User> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // Метод для відображення списку користувачів та їхніх ролей
        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            var users = _userManager.Users.ToList();
            var model = new List<UserRolesViewModel>();
            ViewBag.Roles = _roleManager.Roles.ToList();

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
