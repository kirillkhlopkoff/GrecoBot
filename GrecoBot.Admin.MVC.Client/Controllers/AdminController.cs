using GrecoBot.Admin.MVC.Client.ViewModels.Transactions;
using GrecoBot.Data;
using GrecoBot.Data.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GrecoBot.Admin.MVC.Client.Controllers
{
    public class AdminController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly ApplicationDbContext _context;

        public AdminController(HttpClient httpClient, ApplicationDbContext context)
        {
            _httpClient = httpClient;
            _context = context;
        }

        public async Task<IActionResult> AllTransactions(long? userId, string pair)
        {
            try
            {
                // Инициализируйте IQueryable для запроса в базу данных
                var query = _context.Transactions.AsQueryable();

                // Примените фильтрацию по UserId, если он задан
                if (userId.HasValue)
                {
                    query = query.Where(t => t.UserId == userId.Value);
                }

                // Примените фильтрацию по Pair, если он задан
                if (!string.IsNullOrEmpty(pair))
                {
                    query = query.Where(t => t.Pair == pair);
                }

                // Получите отсортированные транзакции
                var transactions = await query.OrderByDescending(transaction => transaction.DateTime).ToListAsync();

                return View(transactions);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
                return View("Error");
            }
        }

        [HttpPost]
        public async Task<IActionResult> UpdateTransactionStatus(string transactionId, StatusTransaction newStatus)
        {
            try
            {
                // Проверьте, что transactionId не пустой
                if (string.IsNullOrEmpty(transactionId))
                {
                    return BadRequest("Invalid transaction ID.");
                }

                // Поиск транзакции по ID в базе данных
                var existingTransaction = await _context.Transactions.FirstOrDefaultAsync(t => t.TransactionId == transactionId);
                if (existingTransaction == null)
                {
                    return NotFound("Transaction not found.");
                }

                // Обновление статуса транзакции
                existingTransaction.StatusTransaction = newStatus;
                await _context.SaveChangesAsync();

                return RedirectToAction("AllTransactions");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
                return View("Error");
            }
        }


    }
}
