using GrecoBot.Admin.MVC.Client.ViewModels.Transactions;
using GrecoBot.Data;
using GrecoBot.Data.Enums;
using GrecoBot.Data.Models;
using GrecoBot.DC;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Text.Json;

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

        public async Task<IActionResult> AllTransactions()
        {
            try
            {
                var transactions = await _context.Transactions.ToListAsync();
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
