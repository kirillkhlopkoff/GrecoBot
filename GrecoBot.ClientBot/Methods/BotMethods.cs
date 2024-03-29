﻿using GrecoBot.Core;
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
                var response = await _httpClient.PostAsync("https://localhost:7135/api/main/register", content);

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

        public async Task<(UserDC UserInfo, List<TransactionInfoDC> Transactions)> GetUserInfoFromApi(long userId)
        {
            try
            {
                using (var httpClient = new HttpClient())
                {
                    var response = await httpClient.GetAsync($"https://localhost:7135/api/main/user-info/{userId}");
                    response.EnsureSuccessStatusCode();

                    var responseContent = await response.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeAnonymousType(responseContent, new
                    {
                        UserInfo = new UserDC(),
                        Transactions = new List<TransactionInfoDC>()
                    });

                    return (result.UserInfo, result.Transactions);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
                return (null, null);
            }
        }

        public async Task<bool> AddReferalCode(string referalCode, long userId)
        {
            try
            {
                const string prefix = "GBRFC";

                if (referalCode.StartsWith(prefix) && referalCode.Length > prefix.Length)
                {
                    string referalUserId = referalCode.Substring(prefix.Length);

                    using (var httpClient = new HttpClient())
                    {
                        var responseInfo = await httpClient.GetAsync($"https://localhost:7135/api/main/user-info/{referalUserId}");
                        if (responseInfo.IsSuccessStatusCode && (userId.ToString() != referalUserId))
                        {
                            var response = await httpClient.PostAsync($"https://localhost:7135/api/main/addReferalCode?userId={userId}&referalCode={referalCode}", null);
                            if (response.IsSuccessStatusCode)
                            {
                                return true;
                            }
                            return false;
                        }
                        else
                        {
                            return false;
                        }
                    }
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                return false;
            }
        }


        public async Task<string> CreateTransactionInApi(TransactionDC transactionModel)
        {
            try
            {
                using (var httpClient = new HttpClient())
                {
                    var json = JsonConvert.SerializeObject(transactionModel);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    var response = await httpClient.PostAsync("https://localhost:7135/api/main/create-transaction", content);
                    response.EnsureSuccessStatusCode();

                    var responseContent = await response.Content.ReadAsStringAsync();
                    return responseContent;
                }
            }
            catch (Exception ex)
            {
                return $"An error occurred: {ex.Message}";
            }
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
