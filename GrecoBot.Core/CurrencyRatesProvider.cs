using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace GrecoBot.Core
{
    public class CurrencyRatesProvider
    {
        private Dictionary<string, Dictionary<string, decimal>> _data;
        private Dictionary<string, decimal> _fiatData;
        private readonly HttpClient httpClient;

        public CurrencyRatesProvider()
        {
            httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri("https://api.coingecko.com/api/v3/");
            httpClient.DefaultRequestHeaders.Accept.Clear();
            httpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
        }

        private async Task<Dictionary<string, Dictionary<string, decimal>>> LoadCryptoCurrencyRates()
        {
            try
            {
                string url = "simple/price?ids=tether,bitcoin,ethereum,litecoin,cardano,dai,tron,bitcoin-cash,binance-usd,tontoken,dash,verse-bitcoin,dogecoin,matic-network,binancecoin,usd-coin,monero&vs_currencies=usd";
                HttpResponseMessage response = await httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();
                _data= JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, decimal>>>(responseBody);
                return (_data);
            }
            catch (HttpRequestException ex)
            {
                // Обработайте ошибку загрузки данных
                Console.WriteLine($"Ошибка при получении данных о курсах криптовалют: {ex.Message}");
                return null;
            }
            catch (JsonException ex)
            {
                // Обработайте ошибку разбора данных
                Console.WriteLine($"Ошибка при разборе данных о курсах криптовалют: {ex.Message}");
                return null;
            }
        }

        public async Task<Dictionary<string, decimal>> GetCryptoCurrencyRates()
        {
            Dictionary<string, Dictionary<string, decimal>> data = await LoadCryptoCurrencyRates();

            if (data != null && data.TryGetValue("tether", out Dictionary<string, decimal> tetherData) &&
                data.TryGetValue("bitcoin", out Dictionary<string, decimal> bitcoinData) &&
                data.TryGetValue("ethereum", out Dictionary<string, decimal> ethereumData) &&
                data.TryGetValue("litecoin", out Dictionary<string, decimal> litecoinData) &&
                data.TryGetValue("cardano", out Dictionary<string, decimal> cardanoData) &&
                data.TryGetValue("dai", out Dictionary<string, decimal> daiData) &&
                data.TryGetValue("tron", out Dictionary<string, decimal> tronData) &&
                data.TryGetValue("bitcoin-cash", out Dictionary<string, decimal> bitcoincashData) &&
                data.TryGetValue("binance-usd", out Dictionary<string, decimal> binanceusdData) &&
                data.TryGetValue("tontoken", out Dictionary<string, decimal> tontokenData) &&
                data.TryGetValue("dash", out Dictionary<string, decimal> dashData) &&
                data.TryGetValue("verse-bitcoin", out Dictionary<string, decimal> versebitcoinData) &&
                data.TryGetValue("dogecoin", out Dictionary<string, decimal> dogecoinData) &&
                data.TryGetValue("matic-network", out Dictionary<string, decimal> maticnetworkData) &&
                data.TryGetValue("binancecoin", out Dictionary<string, decimal> binancecoinData) &&
                data.TryGetValue("usd-coin", out Dictionary<string, decimal> usdcoinData) &&
                data.TryGetValue("monero", out Dictionary<string, decimal> moneroData))
            {
                Dictionary<string, decimal> rates = new Dictionary<string, decimal>
                {
                    { "tether", tetherData["usd"] },
                    { "bitcoin", bitcoinData["usd"] },
                    { "ethereum", ethereumData["usd"] },
                    { "litecoin", litecoinData["usd"] },
                    { "Cardano", cardanoData["usd"] },

                    { "Tron", tronData["usd"] },
                    { "Bitcoin cash", bitcoincashData["usd"] },
                    { "Monero", moneroData["usd"] },
                    { "Dai", daiData["usd"] },

                    { "Binance-USD", binanceusdData["usd"] },
                    { "Ton-token", tontokenData["usd"] },
                    { "Dash", dashData["usd"] },
                    { "Verse", versebitcoinData["usd"] },

                    { "Dogecoin", dogecoinData["usd"] },
                    { "USD Coin", usdcoinData["usd"] },
                    { "Polygon", maticnetworkData["usd"] },
                    { "Binance coin", binancecoinData["usd"] }
                };
                return rates;
            }
            else
            {
                return null;
            }
        }

        public decimal GetExchangeRate(string selectedCurrencyPair)
        {
            selectedCurrencyPair = WebUtility.UrlDecode(selectedCurrencyPair); // Декодирование пары

            _data = LoadCryptoCurrencyRates().GetAwaiter().GetResult(); // Инициализация _data

            if (_data.TryGetValue(selectedCurrencyPair.Split('/')[0].ToLower(), out var currencyData))
            {
                if (currencyData.TryGetValue("usd", out decimal exchangeRate))
                {
                    return exchangeRate;
                }
            }

            // Если валютная пара или курс обмена не найдены, можно вернуть значение по умолчанию или обработать ошибку
            return 0;
        }

    }
}
