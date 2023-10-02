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
            new KeyboardButton[] { new KeyboardButton("⚖️ Текущий курс"), new KeyboardButton("✅ Регистрация") },
            new KeyboardButton[] { new KeyboardButton("📖 Оферта") }
        })
            {
                ResizeKeyboard = true
            };
        }

        public static InlineKeyboardMarkup CreateCryptoCurrencyKeyboard()
        {
            var cryptoCurrencies = new[]
            {
                new[] { "USDT", "TRX" }, // Здесь можно добавить другие криптовалюты "LTC" "BCH" "TON", "DASH", "DOGE" "XMR",
                new[] { "DAI", "BUSD" },
                new[] { "BTC", "ETH" },
                new[] { "VERSE", "MATIC" },
                new[] { "USDC", "BNB" }
            };

            var inlineKeyboard = new InlineKeyboardMarkup(cryptoCurrencies
                .Select(row => row.Select(currency => InlineKeyboardButton.WithCallbackData(currency, $"select_base_{currency}")))
            );

            return inlineKeyboard;
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
