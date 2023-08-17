using GrecoBot.Core;
using GrecoBot.DC;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrecoBot.ClientBot.Methods
{
    public class BotMethods
    {
        private readonly HttpClient _httpClient;
        private readonly CurrencyRatesProvider _ratesProvider;

        public BotMethods(HttpClient httpClient, CurrencyRatesProvider ratesProvider)
        {
            _httpClient = httpClient;
            _ratesProvider = ratesProvider;
        }

        public async Task<string> RegisterUserInApi(UserDC userDC)
        {
            try
            {
                // Преобразуем объект UserDC в JSON
                var jsonUser = JsonConvert.SerializeObject(userDC);

                // Создаем контент для запроса
                var content = new StringContent(jsonUser, Encoding.UTF8, "application/json");

                // Отправляем POST-запрос на метод регистрации
                var response = await _httpClient.PostAsync("https://localhost:7135/api/Bot/register", content);

                response.EnsureSuccessStatusCode();

                var responseContent = await response.Content.ReadAsStringAsync();

                // Возвращаем результат регистрации
                return responseContent;
            }
            catch (Exception ex)
            {
                return $"Произошла ошибка при регистрации: {ex.Message}";
            }
        }

        public async Task<UserDC> GetUserInfoFromApi(long userId)
        {
            var response = await _httpClient.GetAsync($"https://localhost:7135/api/Bot/user-info/{userId}");
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var userInfo = JsonConvert.DeserializeObject<UserDC>(content);

            return userInfo;
        }

        public async Task<decimal> CalculateAmountInUSD(string currency, decimal amount)
        {
            var rates = await _ratesProvider.GetCryptoCurrencyRates();
            if (rates != null && rates.TryGetValue(currency.ToLower(), out decimal rate))
            {
                return amount * rate;
            }
            return -1; // Валюта не найдена
        }

        public async Task<decimal> CalculateAmountInCurrency(string currency, decimal amountInUSD)
        {
            var rates = await _ratesProvider.GetCryptoCurrencyRates();
            if (rates != null && rates.TryGetValue(currency.ToLower(), out decimal rate))
            {
                return amountInUSD / rate;
            }
            return -1; // Валюта не найдена
        }
    }
}
