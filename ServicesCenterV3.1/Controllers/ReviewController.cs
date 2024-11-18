using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServicesCenterV3._1.Data;
using ServicesCenterV3._1.Models;
using System.Threading.Tasks;

namespace ServicesCenterV3._1.Controllers
{
    public class ReviewController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReviewController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReviewCreate(int orderId, string text, int rating)
        {
            // Перевіряємо, чи існує замовлення з переданим ID
            var order = await _context.Orders.FirstOrDefaultAsync(o => o.OrderId == orderId);

            if (order == null)
            {
                // Повертаємо помилку, якщо замовлення не знайдено
                ModelState.AddModelError("", "Замовлення не знайдено.");
                return View();
            }

            if (ModelState.IsValid)
            {
                // Створюємо новий відгук
                var review = new Review
                {
                    OrderId = order.OrderId, // Використовуємо ID замовлення
                    Text = text,
                    Rating = rating
                };
 
                // Додаємо відгук до бази даних
                _context.reviews.Add(review);
                
                await _context.SaveChangesAsync();

                var orders = order.ReviewId = review.ReviewId;

                _context.Orders.Update(order);
                await _context.SaveChangesAsync();

                // Повертаємо користувача на список замовлень
                return RedirectToAction("Index","Client");
            }

            // Якщо ModelState не валідний, повертаємо форму
            return View();
        }
    }
}
