using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrecoBot.Core
{
    public class CurrencyCalculator
    {
        private readonly CurrencyRatesProvider _ratesProvider;

        public CurrencyCalculator(CurrencyRatesProvider ratesProvider)
        {
            _ratesProvider = ratesProvider;
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
