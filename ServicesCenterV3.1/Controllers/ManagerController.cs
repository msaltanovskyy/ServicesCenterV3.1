using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ServicesCenterV3._1.Models;

namespace ServicesCenterV3._1.Controllers
{

    [Route("Manager")]
    public class ManagerController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public ManagerController(UserManager<User> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // Метод для відображення списку користувачів та їхніх ролей
        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            var users = _userManager.Users.ToList(); // Отримуємо всіх користувачів
            var model = new List<UserRolesViewModel>();
            ViewBag.Roles = _roleManager.Roles.ToList(); // Отримуємо всі ролі

            foreach (var user in users)
            {
                var userRoles = await _userManager.GetRolesAsync(user);

                // Перевіряємо, чи є у користувача роль "Client"
                if (userRoles.Contains("Client"))
                {
                    var userModel = new UserRolesViewModel
                    {
                        UserId = user.Id,
                        Name = user.Name,
                        Lastname = user.Lastname,
                        Email = user.UserName,
                        DataBith = user.DataBith,
                        Roles = userRoles
                    };
                    model.Add(userModel); // Додаємо користувача в модель, якщо має роль "Client"
                }
            }

            return View(model);
        }

    }
}
