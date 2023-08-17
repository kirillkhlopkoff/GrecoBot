using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types.ReplyMarkups;

namespace GrecoBot.ClientBot.Keybords
{
    public static class Keyboards
    {
        public static ReplyKeyboardMarkup MainKeyboard()
        {
            return new ReplyKeyboardMarkup(new[]
            {
            new KeyboardButton[] { new KeyboardButton("💶 Обменять"), new KeyboardButton("👤 Личный кабинет") },
            new KeyboardButton[] { new KeyboardButton("💬 Сообщество"), new KeyboardButton("📞 Поддержка") },
            new KeyboardButton[] { new KeyboardButton("⚖️ Текущий курс"), new KeyboardButton("✅ Регистрация") }
        })
            {
                ResizeKeyboard = true
            };
        }

        public static InlineKeyboardMarkup CreateCryptoCurrencyKeyboard()
        {
            var cryptoCurrencies = new[]
            {
        new[] { "USDT", "TRX", "LTC" }, // Здесь можно добавить другие криптовалюты
        new[] { "BCH", "DAI", "BUSD" },
        new[] { "TON", "BTC", "DASH" },
        new[] { "XMR", "VERSE", "DOGE" },
        new[] { "USDC", "MATIC", "BNB" },
        new[] { "ETH" }
    };

            var convertedCryptoCurrencies = cryptoCurrencies.Select(row => row.Select(currency => ConvertCurrencyName(currency)));

            var inlineKeyboard = new InlineKeyboardMarkup(convertedCryptoCurrencies
                .Select(row => row.Select(currency => InlineKeyboardButton.WithCallbackData(currency, $"select_base_{currency}")))
            );

            return inlineKeyboard;
        }
        private static string ConvertCurrencyName(string currency)
        {
            switch (currency)
            {
                case "USDT": return "tether";
                case "TRX": return "bitcoin";
                case "LTC": return "ethereum";

                case "BCH": return "tether";
                case "DAI": return "bitcoin";
                case "BUSD": return "ethereum";

                case "TON": return "tether";
                case "BTC": return "bitcoin";
                case "DASH": return "ethereum";

                case "XMR": return "tether";
                case "VERSE": return "bitcoin";
                case "DOGE": return "ethereum";

                case "USDC": return "tether";
                case "MATIC": return "bitcoin";
                case "BNB": return "ethereum";

                case "ETH": return "tether";
                default: return currency.ToLower();
            }
        }

        public static InlineKeyboardMarkup CreateAmountInputMethodKeyboard(string selectedCurrency)
        {
            var inlineKeyboard = new InlineKeyboardMarkup(new[]
            {
        new[] { InlineKeyboardButton.WithCallbackData("Ввести сумму в USD", $"enter_amount_usd_{selectedCurrency}") },
        new[] { InlineKeyboardButton.WithCallbackData($"Ввести сумму в {selectedCurrency}", $"enter_amount_crypto_{selectedCurrency}") }
    });

            return inlineKeyboard;
        }

        public static InlineKeyboardMarkup ConfirmationKeyboard()
        {
            var inlineKeyboard = new InlineKeyboardMarkup(new[]
            {
        new[] { InlineKeyboardButton.WithCallbackData("Да", "confirm_exchange"), InlineKeyboardButton.WithCallbackData("Нет", "cancel_exchange") }
    });

            return inlineKeyboard;
        }
    }
}
