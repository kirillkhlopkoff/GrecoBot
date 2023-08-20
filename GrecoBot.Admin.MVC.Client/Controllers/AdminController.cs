using GrecoBot.Admin.MVC.Client.ViewModels.Transactions;
using GrecoBot.Data;
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

        public AdminController(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<IActionResult> AllTransactions()
        {
            try
            {
                using (var httpClient = new HttpClient())
                {
                    var response = await httpClient.GetAsync("https://localhost:7135/api/main/all-transactions"); // Замените на вашу API URL
                    response.EnsureSuccessStatusCode();

                    var responseContent = await response.Content.ReadAsStringAsync();
                    var transactions = JsonConvert.DeserializeObject<List<TransactionDC>>(responseContent);

                    return View(transactions);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
                return View("Error");
            }
        }

        /*public async Task<IActionResult> AllTransactions()
        {
            var client = _httpClient.CreateClient();
            var response = await client.GetAsync("https://localhost:7135/api/main/all-transactions"); // Замените на вашу базовую URL API

            if (response.IsSuccessStatusCode)
            {
                var responseBody = await response.Content.ReadAsStringAsync();
                var transactions = JsonSerializer.Deserialize<List<TransactionInfoDC>>(responseBody);
                return View(transactions);
            }
            else
            {
                // Обработка ошибки, если не удалось получить данные
                return View("Error");
            }
        }

        public async Task<IActionResult> UserTransactions(long userId)
        {
            var client = _httpClientFactory.CreateClient();
            var response = await client.GetAsync($"https://localhost:7135/api/main/user-transactions/{userId}"); // Замените на вашу базовую URL API

            if (response.IsSuccessStatusCode)
            {
                var responseBody = await response.Content.ReadAsStringAsync();
                var transactions = JsonSerializer.Deserialize<List<TransactionInfoDC>>(responseBody);
                return View(transactions);
            }
            else
            {
                // Обработка ошибки, если не удалось получить данные
                return View("Error");
            }
        }*/
    }
}
