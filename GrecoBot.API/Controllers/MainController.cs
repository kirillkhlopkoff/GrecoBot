using GrecoBot.Core;
using GrecoBot.Data;
using GrecoBot.Data.Enums;
using GrecoBot.Data.Models;
using GrecoBot.DC;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GrecoBot.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MainController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly ApplicationDbContext _dbContext;

        public MainController(IConfiguration configuration, ApplicationDbContext dbContext)
        {
            _configuration = configuration;
            _dbContext = dbContext;
        }

        [HttpPost("register")]
        public async Task<IActionResult> RegisterUser([FromBody] UserDC userRegistrationModel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Invalid data.");
            }

            // Проверяем, существует ли пользователь с указанным Id
            var existingUser = await _dbContext.User.FirstOrDefaultAsync(u => u.Id == userRegistrationModel.Id);

            if (existingUser != null)
            {
                return BadRequest("User with the specified Id already exists.");
            }

            var user = new User
            {
                Id = userRegistrationModel.Id,
                Phone = userRegistrationModel.Phone
            };

            _dbContext.User.Add(user);
            await _dbContext.SaveChangesAsync();

            return Ok("User registered successfully.");
        }

        [HttpPost("addReferalCode")]
        public async Task<IActionResult> AddReferalCode(long userId, string referalCode)
        {
            try
            {
                var user = await _dbContext.User.FirstOrDefaultAsync(u => u.Id == userId);
                if (user != null)
                {
                    user.ReferalCode = referalCode;
                }
                try
                {
                    await _dbContext.SaveChangesAsync();
                    return Ok("Referal code added successfully.");
                }
                catch (Exception ex)
                {
                    return StatusCode(500, "Internal server error: " + ex.Message);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }

        }


        [HttpGet("user-info/{userId}")]
        public async Task<IActionResult> GetUserInfo(long userId)
        {
            try
            {
                var user = await _dbContext.User.FirstOrDefaultAsync(u => u.Id == userId);

                if (user != null)
                {
                    var userInfo = new UserDC
                    {
                        Id = user.Id,
                        Phone = user.Phone,
                        ReferalCode = user.ReferalCode
                    };

                    var transactions = await _dbContext.Transactions
                        .Where(t => t.UserId == userId)
                        .OrderByDescending(t => t.DateTime)
                        .Take(10)
                        .Select(t => new TransactionInfoDC
                        {
                            TransactionId = t.TransactionId.ToString(),
                            Pair = t.Pair,
                            Amount = t.Amount,
                            DateTime = t.DateTime.AddHours(3),
                            StatusTransaction = t.StatusTransaction
                        })
                        .ToListAsync();

                    return Ok(new { UserInfo = userInfo, Transactions = transactions });
                }
                else
                {
                    return NotFound("User not found.");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }


        [HttpGet("crypto-currency-rates")]
        public async Task<IActionResult> GetCryptoCurrencyRates()
        {
            try
            {
                var ratesProvider = new CurrencyRatesProvider();
                var cryptoCurrencyRates = await ratesProvider.GetCryptoCurrencyRates();

                if (cryptoCurrencyRates != null)
                {
                    return Ok(cryptoCurrencyRates);
                }
                else
                {
                    return BadRequest("Unable to fetch crypto currency rates.");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        [HttpGet("current-course/{pair}")]
        public IActionResult GetCurrentCourse(string pair)
        {
            var ratesProvider = new CurrencyRatesProvider();
            var exchangeRate = ratesProvider.GetExchangeRate(pair);

            if (exchangeRate > 0)
            {
                return Ok($"Current exchange rate for {pair}: {exchangeRate:F2}");
            }
            else
            {
                return NotFound("Exchange rate not found for the specified pair.");
            }
        }

        [HttpPost("create-transaction")]
        public async Task<IActionResult> CreateTransaction([FromBody] TransactionDC transactionModel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Invalid data.");
            }

            try
            {
                var user = await _dbContext.User.FirstOrDefaultAsync(u => u.Id == transactionModel.UserId);
                if (user == null)
                {
                    return NotFound("User not found.");
                }

                var ratesProvider = new CurrencyRatesProvider();
                var currentCourse = ratesProvider.GetExchangeRate(transactionModel.Pair); // Получение курса автоматически

                if (currentCourse > 0)
                {
                    var transaction = new Transaction
                    {
                        UserId = transactionModel.UserId,
                        TransactionId = transactionModel.TransactionId,
                        Pair = transactionModel.Pair,
                        Amount = transactionModel.Amount,
                        DateTime = transactionModel.DateTime,
                        CurrentCourse = currentCourse.ToString("F2"), // Форматируем курс с двумя знаками после запятой
                        StatusTransaction = transactionModel.StatusTransaction
                    };

                    _dbContext.Transactions.Add(transaction);
                    await _dbContext.SaveChangesAsync();

                    return Ok("Transaction created successfully.");
                }
                else
                {
                    return BadRequest("Unable to fetch current course for the specified pair.");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        [HttpPatch("update-transaction-status")]
        public async Task<IActionResult> UpdateTransactionStatus([FromHeader] string transactionId, [FromBody] StatusTransaction newStatus)
        {
            if (string.IsNullOrEmpty(transactionId) || !ModelState.IsValid)
            {
                return BadRequest("Invalid data.");
            }

            try
            {
                var existingTransaction = await _dbContext.Transactions.FirstOrDefaultAsync(t => t.TransactionId == transactionId);
                if (existingTransaction == null)
                {
                    return NotFound("Transaction not found.");
                }

                existingTransaction.StatusTransaction = newStatus;
                await _dbContext.SaveChangesAsync();

                return Ok("Transaction status updated successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        [HttpGet("all-transactions")]
        public async Task<IActionResult> GetAllTransactions()
        {
            try
            {
                var transactions = await _dbContext.Transactions.ToListAsync();

                var transactionInfoList = transactions.Select(transaction => new TransactionDC
                {
                    UserId = transaction.UserId,
                    TransactionId = transaction.TransactionId,
                    Pair = transaction.Pair,
                    Amount = transaction.Amount,
                    DateTime = transaction.DateTime,
                    StatusTransaction = transaction.StatusTransaction
                }).ToList();

                return Ok(transactionInfoList);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        [HttpGet("user-transactions/{userId}")]
        public async Task<IActionResult> GetUserTransactions(long userId)
        {
            try
            {
                var transactions = await _dbContext.Transactions.Where(t => t.UserId == userId).ToListAsync();

                var transactionInfoList = transactions.Select(transaction => new TransactionInfoDC
                {
                    TransactionId = transaction.TransactionId,
                    Pair = transaction.Pair,
                    Amount = transaction.Amount,
                    DateTime = transaction.DateTime
                }).ToList();

                return Ok(transactionInfoList);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        /*[HttpGet("calculate-in-usd")]
        public async Task<IActionResult> CalculateInUSD(string currency, decimal amount)
        {
            try
            {
                var calculator = new CurrencyCalculator(new CurrencyRatesProvider());
                var result = await calculator.CalculateAmountInUSD(currency, amount);

                if (result >= 0)
                {
                    return Ok(result);
                }
                else
                {
                    return BadRequest("Currency not found.");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        [HttpGet("calculate-in-currency")]
        public async Task<IActionResult> CalculateInCurrency(string currency, decimal amountInUSD)
        {
            try
            {
                var calculator = new CurrencyCalculator(new CurrencyRatesProvider());
                var result = await calculator.CalculateAmountInCurrency(currency, amountInUSD);

                if (result >= 0)
                {
                    return Ok(result);
                }
                else
                {
                    return BadRequest("Currency not found.");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }*/
    }
}
