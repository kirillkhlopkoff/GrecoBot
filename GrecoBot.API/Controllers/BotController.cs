using GrecoBot.Core;
using GrecoBot.Data;
using GrecoBot.Data.Models;
using GrecoBot.DC;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GrecoBot.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BotController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly ApplicationDbContext _dbContext;

        public BotController(IConfiguration configuration, ApplicationDbContext dbContext)
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
                        Phone = user.Phone
                    };

                    return Ok(userInfo);
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

                var transaction = new Transaction
                {
                    UserId = transactionModel.UserId,
                    TransactionId = transactionModel.TransactionId,
                    Pair = transactionModel.Pair,
                    Amount = transactionModel.Amount,
                    DateTime = transactionModel.DateTime,
                    CurrentCourse = transactionModel.CurrentCourse
                };

                _dbContext.Transactions.Add(transaction);
                await _dbContext.SaveChangesAsync();

                return Ok("Transaction created successfully.");
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
