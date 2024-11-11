using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using ServicesCenterV3._1.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading;
using System.Threading.Tasks;

namespace ServicesCenterV3._1.Areas.Identity.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<User> _signInManager;
        private readonly UserManager<User> _userManager;
        private readonly IUserStore<User> _userStore;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IEmailSender _emailSender;
        private readonly RoleManager<IdentityRole> _roleManager;

        public RegisterModel(
            UserManager<User> userManager,
            IUserStore<User> userStore,
            SignInManager<User> signInManager,
            ILogger<RegisterModel> logger,
            RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _userStore = userStore;
            _signInManager = signInManager;
            _logger = logger;
            _roleManager = roleManager;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public string ReturnUrl { get; set; }

        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        // Список ролей для вибору
        public List<string> Roles { get; set; }

        public class InputModel
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
            [EmailAddress]
            [Display(Name = "Пошта")]
            [Column("email")]
            [Required(ErrorMessage = "Введіть пошту")]
            public string Email { get; set; }

            [Display(Name = "Пароль")]
            [Required(ErrorMessage = "Введіть пароль")]
            [MinLength(8, ErrorMessage = "Мінімальна довжина 8 символів")]
            [StringLength(36, ErrorMessage = "Максимальна довжина 36 символів")]
            [DataType(DataType.Password)]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Підтвердження паролю")]
            [Compare("Password", ErrorMessage = "Паролі не співпадають.")]
            public string ConfirmPassword { get; set; }

            [Display(Name = "Роль")]
            [Required(ErrorMessage = "Оберіть роль")]
            public string Role { get; set; }

            [Display(Name = "Дата народження")]
            [Column("date_bith")]
            [DataType(DataType.Date)]
            public DateOnly DataBith { get; set; }
        }

        public async Task OnGetAsync(string returnUrl = null)
        {
            ReturnUrl = returnUrl;
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
            Roles = _roleManager.Roles.Select(r => r.Name).ToList(); // Отримуємо всі ролі з бази даних

        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
            if (ModelState.IsValid)
            {
                var user = CreateUser();

                await _userStore.SetUserNameAsync(user, Input.Email, CancellationToken.None);
                var result = await _userManager.CreateAsync(user, Input.Password);

                if (result.Succeeded)
                {
                    _logger.LogInformation("User created a new account with password.");

                    // Додаємо користувача до вибраної ролі
                    if (!string.IsNullOrEmpty(Input.Role) && await _roleManager.RoleExistsAsync(Input.Role))
                    {
                        await _userManager.AddToRoleAsync(user, Input.Role);
                        user.EmailConfirmed = true;
                        user.Email = Input.Email;
                        user.Name = Input.Name;
                        user.Lastname = Input.Lastname;
                        user.DataBith = Input.DataBith;
                    }

                    var userId = await _userManager.GetUserIdAsync(user);
                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                  

                    if (_userManager.Options.SignIn.RequireConfirmedAccount)
                    {
                        return RedirectToPage("RegisterConfirmation", new { email = Input.Email, returnUrl = returnUrl });
                    }
                    else
                    {
                        var currentUser = await _userManager.GetUserAsync(User);
                        if (currentUser == null || !await _userManager.IsInRoleAsync(currentUser, "Admin"))
                        {
                            // Якщо користувач не адміністратор, то увійти в систему
                            await _signInManager.SignInAsync(user, isPersistent: false);
                            return LocalRedirect(returnUrl);
                        }
                        else
                        {
                            // Якщо користувач є адміністратором, перенаправити на сторінку адміністратора
                            return RedirectToAction("Index", "Admin");
                        }
                    }
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            // Якщо сталася помилка, перезавантажуємо ролі
            Roles = _roleManager.Roles.Select(r => r.Name).ToList();
            return Page();
        }

        private User CreateUser()
        {

            try
            {
                return new User
                {
                    Name = Input.Name,
                    Lastname = Input.Lastname,
                    DataBith = Input.DataBith,
                    EmailConfirmed = true,
                    Email = Input.Email,
                };
            }
            catch
            {
                throw new InvalidOperationException($"Can't create an instance of '{nameof(IdentityUser)}'. " +
                    $"Ensure that '{nameof(IdentityUser)}' is not an abstract class and has a parameterless constructor, or alternatively " +
                    $"override the register page in /Areas/Identity/Pages/Account/Register.cshtml");
            }
        }
    }
}
