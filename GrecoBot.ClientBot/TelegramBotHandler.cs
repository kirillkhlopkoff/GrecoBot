using GrecoBot.ClientBot.Keybords;
using GrecoBot.ClientBot.Methods;
using GrecoBot.Core;
using GrecoBot.DC;
using Newtonsoft.Json;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace GrecoBot.ClientBot
{
    public class TelegramBotHandler
    {
        public enum OperationStep
        {
            SelectTargetCurrency,
            EnterAmount,
            EnterWallet
        }

        static string changePair;
        static string selectedTargetCurrency = string.Empty;

        public static TelegramBotClient _client;
        private readonly HttpClient _httpClient;
        private readonly BotMethods _botMethods;
        private readonly CurrencyRatesProvider _ratesProvider;
        public static CurrentCourse _currentCourse;

        public TelegramBotHandler(string apikey, HttpClient httpClient)
        {
            _client = new TelegramBotClient(apikey);
            _httpClient = httpClient;
            _botMethods = new BotMethods(_httpClient, _ratesProvider);
            _currentCourse = new CurrentCourse();
        }

        private Dictionary<long, UserState> _userStates = new Dictionary<long, UserState>();
        private static Dictionary<long, string> _userSelectedBaseCurrencies = new Dictionary<long, string>();
        private static Dictionary<long, string> _userSelectedTargetCurrencies = new Dictionary<long, string>();
        private static Dictionary<long, OperationState> _userOperations = new Dictionary<long, OperationState>();
        private static readonly Dictionary<string, string> CurrencyPairMappings = new Dictionary<string, string>
{
    { "USDT/UAH", "tether/usd" },
    { "TRX/UAH", "tron/usd" },
    { "LTC/UAH", "litecoin/usd" },
    { "BCH/UAH", "bitcoin-cash/usd" },
    { "DAI/UAH", "dai/usd" },
    { "BUSD/UAH", "binance-usd/usd" },
    { "TON/UAH", "tontoken/usd" },
    { "BTC/UAH", "bitcoin/usd" },
    { "DASH/UAH", "dash/usd" },
    { "XMR/UAH", "monero/usd" },
    { "VERSE/UAH", "verse-bitcoin/usd" },
    { "DOGE/UAH", "dogecoin/usd" },
    { "USDC/UAH", "usd-coin/usd" },
    { "MATIC/UAH", "matic-network/usd" },
    { "BNB/UAH", "binancecoin/usd" },
    { "ETH/UAH", "ethereum/usd" },
    // Add other exchange options and corresponding currency pairs here
};

        public async Task RunBotAsync()
        {
            using var cts = new CancellationTokenSource();
            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = { }
            };
            _client.StartReceiving(UpdateAsync, Error, receiverOptions, cancellationToken: cts.Token);

            var me = await _client.GetMeAsync();
            Console.WriteLine($"Start speaking with @{me.Username}");
            Console.ReadLine();
            cts.Cancel();
        }

        private async Task UpdateAsync(ITelegramBotClient client, Update update, CancellationToken token)
        {
            var message = update.Message;
            if (message?.Type == MessageType.Photo)
            {
                await client.SendTextMessageAsync(message.Chat.Id, "Спасибо, платеж отправлен в обработку. Ожидайте зачисления средств.");

                // Получаем ID фото, которое хотим переслать
                var photoId = message.Photo[0].FileId;

                // ID чата, куда нужно переслать фото (в данном случае ID технического чата)
                long technicalChatId = 6642646501; // Замените на реальный ID вашего технического чата

                // Пересылаем фото в технический чат
                await client.ForwardMessageAsync(technicalChatId, message.Chat.Id, message.MessageId);

                return;
            }
            if (update.Type == UpdateType.Message && update.Message?.Text != null)
            {
                await Console.Out.WriteLineAsync($"{update.Message.Chat.FirstName} | {update.Message.Text}");
                await HandleMessage(client, update.Message);
            }
            else if (update.Type == UpdateType.CallbackQuery && update.CallbackQuery?.Data != null)
            {
                await HandleCallbackQuery(client, update.CallbackQuery);
            }
            // Обработка получения контакта от пользователя
            else if (update.Type == UpdateType.Message && update.Message.Type == MessageType.Contact)
            {
                var contact = message.Contact;

                var userRegistrationModel = new UserDC
                {
                    Id = message.Chat.Id, // Используем Chat Id как Id пользователя
                    Phone = contact.PhoneNumber
                };

                // Вызываем метод регистрации в API
                var registerResponse = await _botMethods.RegisterUserInApi(userRegistrationModel);

                // Отправляем сообщение пользователю с результатом регистрации
                await _client.SendTextMessageAsync(message.Chat.Id, registerResponse);

                // Закрываем клавиатуру и возвращаем в основное меню
                var keyboard = Keyboards.MainKeyboard(); // Используем метод из класса Keyboards
                await _client.SendTextMessageAsync(message.Chat.Id, "Спасибо, вы зарегистрированы.", replyMarkup: keyboard);
            }

            /*else
            {
                // Если для данного пользователя есть сохраненное состояние операции
                if (_userOperations.TryGetValue(message.Chat.Id, out var operationState))
                {
                    string answerMessade = string.Empty;
                    var consoleAnswer = Console.Out.WriteLineAsync(selectedTargetCurrency);
                    string walletUAH = "4149 6293 5338 5008";

                    switch (operationState.CurrentStep)
                    {

                        case OperationStep.EnterAmount:
                            // Код обработки суммы, например, вы можете сохранить сумму в operationState и перейти к следующему шагу:
                            operationState.Amount = message.Text;
                            operationState.CurrentStep = OperationStep.EnterWallet;

                            await client.SendTextMessageAsync(message.Chat.Id, $"Вы хотите купить {message.Text} {selectedTargetCurrency} \nId вашей операции:{operationState.OperationId}. \nУкажите его в назначении платежа. \n\nВведите ваш кошелек для зачисления. \nИ отправьте на карту \n{walletUAH} \nследующую сумму:");
                            // Рассчитать итоговую сумму на основе выбранной криптовалюты и введенной суммы
                            await _currentCourse.CalculateAmountInUSD(message.Chat.Id, operationState.Amount, changePair, "uah");
                            operationState.OrderAmount = message.Text;
                            break;

                        case OperationStep.EnterWallet:
                            // Код обработки кошелька, например, сохранить его в operationState и вывести сообщение "Спасибо, ожидаем ваш платеж":
                            operationState.Wallet = message.Text;
                            _userOperations.Remove(message.Chat.Id); // Удаляем состояние операции после завершения операции
                            await client.SendTextMessageAsync(message.Chat.Id, "Спасибо, ожидаем ваш платеж. \nПосле отправки перешлите в бот скриншот платежа.");
                            // ID чата, куда нужно переслать фото (в данном случае ID технического чата)
                            long technicalChatId = 6642646501; // Замените на реальный ID вашего технического чата
                            string order = $"Заявка {operationState.OperationId} \nКошелек-{message.Text} \nСумма: {operationState.OrderAmount}{selectedTargetCurrency}";
                            // Пересылаем фото в технический чат
                            await client.SendTextMessageAsync(technicalChatId, $"{order}");
                            break;

                        default:
                            await client.SendTextMessageAsync(message.Chat.Id, $"Вы сказали: \n{message.Text}");
                            break;
                    }

                    // Сохраняем обновленное состояние операции
                    _userOperations[message.Chat.Id] = operationState;
                }
                else
                {
                    await client.SendTextMessageAsync(message.Chat.Id, $"Вы сказали: \n{message.Text}");
                }
            }*/
            
            
        }

        async Task HandleMessage(ITelegramBotClient client, Message message)
        {
            if (message == null)
            {
                await client.SendTextMessageAsync(message.Chat.Id, "Неизвестная команда.");
                return;
            }
            if (message.Text == "/start")
            {
                var keyboard = Keyboards.MainKeyboard(); // Используем метод из класса Keyboards
                await _client.SendTextMessageAsync(message.Chat.Id, "Выберите действие:", replyMarkup: keyboard);
            }
            else if (message.Text == "⚖️ Текущий курс")
            {
                try
                {
                    var response = await _httpClient.GetAsync("https://localhost:7135/api/Bot/crypto-currency-rates");
                    response.EnsureSuccessStatusCode();

                    var content = await response.Content.ReadAsStringAsync();
                    var cryptoCurrencyRates = JsonConvert.DeserializeObject<Dictionary<string, decimal>>(content);

                    if (cryptoCurrencyRates != null)
                    {
                        var ratesMessage = "Текущие курсы криптовалют:\n";
                        foreach (var rate in cryptoCurrencyRates)
                        {
                            ratesMessage += $"{rate.Key}: {rate.Value}\n";
                        }

                        await _client.SendTextMessageAsync(message.Chat.Id, ratesMessage);
                    }
                    else
                    {
                        await _client.SendTextMessageAsync(message.Chat.Id, "Не удалось получить курсы криптовалют.");
                    }
                }
                catch (Exception ex)
                {
                    await _client.SendTextMessageAsync(message.Chat.Id, $"Произошла ошибка: {ex.Message}");
                }
            }
            else if (message.Text == "📞 Поддержка")
            {
                string buttonText = message.Text == "📞 Поддержка" ? "Связаться с поддержкой" : "Перейти в группу";
                string buttonUrl = "https://t.me/GrekKH"; // Здесь URL, на который должна вести ссылка

                var supportButton = new InlineKeyboardButton(buttonText)
                {
                    Url = buttonUrl
                };

                var inlineKeyboard = new InlineKeyboardMarkup(new[] { new[] { supportButton } });

                await client.SendTextMessageAsync(message.Chat.Id, "Для связи с поддержкой нажмите на кнопку ниже:", replyMarkup: inlineKeyboard);
            }
            else if (message.Text == "💬 Сообщество")
            {
                // Создаем и отправляем сообщение с ссылкой-кнопкой
                var supportButton = new InlineKeyboardButton(string.Empty)
                {
                    Text = "Перейти в группу", // Указываем текст для кнопки
                    Url = "https://t.me/GrekKH" // Здесь URL, на который должна вести ссылка
                };

                var inlineKeyboard = new InlineKeyboardMarkup(new[] { new[] { supportButton } });

                await client.SendTextMessageAsync(message.Chat.Id, "Для связи с поддержкой нажмите на кнопку ниже:", replyMarkup: inlineKeyboard);
            }
            else if (message.Text == "💶 Обменять")
            {
                string textWithBoldWord = "Выберите монету которую <b>покупаете</b>";
                var inlineKeyboard = Keyboards.CreateCryptoCurrencyKeyboard();

                await client.SendTextMessageAsync(message.Chat.Id, text: textWithBoldWord, parseMode: ParseMode.Html, replyMarkup: inlineKeyboard);
            }
            else if (message.Text == "✅ Регистрация")
            {
                var requestContactKeyboard = new ReplyKeyboardMarkup(new[]
                {
                    new KeyboardButton[] { new KeyboardButton("Отправить контакт") { RequestContact = true } }
                });

                await _client.SendTextMessageAsync(message.Chat.Id, "Для регистрации, пожалуйста, отправьте свой контакт.", replyMarkup: requestContactKeyboard);
            }
            else if (message.Text == "👤 Личный кабинет")
            {
                // Получаем информацию о пользователе и его транзакциях из API
                var userInfoWithTransactions = await _botMethods.GetUserInfoFromApi(message.Chat.Id);

                if (userInfoWithTransactions.UserInfo != null)
                {
                    // Пользователь найден, создаем сообщение с информацией о Личном кабинете и транзакциях
                    var userMessage = $"👤Добро пожаловать в ваш Личный Кабинет!\n" +
                                      $"Id: {userInfoWithTransactions.UserInfo.Id}\n" +
                                      $"Телефон: {userInfoWithTransactions.UserInfo.Phone}\n\n" +
                                      "Последние транзакции:\n";

                    foreach (var transaction in userInfoWithTransactions.Transactions)
                    {
                        userMessage += $"Транзакция Id: {transaction.TransactionId}\n" +
                                       $"Пара: {transaction.Pair}\n" +
                                       $"Сумма: {transaction.Amount}\n" +
                                       $"Дата и время: {transaction.DateTime}\n"+
                                       $"Статус: {transaction.StatusTransaction}\n\n";
                    }

                    await _client.SendTextMessageAsync(message.Chat.Id, userMessage);
                }
                else
                {
                    // Пользователь не найден, отправляем сообщение о незарегистрированности
                    await _client.SendTextMessageAsync(message.Chat.Id, "Вы еще не зарегистрированы.");
                }
            }
            else if (message.Text == "📖 Оферта")
            {
                await _client.SendTextMessageAsync(message.Chat.Id, $"Договор оферты, комиссия и прочее");
            }

            else
            {
                // Если для данного пользователя есть сохраненное состояние операции
                if (_userOperations.TryGetValue(message.Chat.Id, out var operationState))
                {
                    string walletUAH = "4149 6293 5338 5008";
                    

                    switch (operationState.CurrentStep)
                    {

                        case OperationStep.EnterAmount:
                            // Код обработки суммы, например, вы можете сохранить сумму в operationState и перейти к следующему шагу:
                            operationState.Amount = decimal.Parse( message.Text);
                            operationState.CurrentStep = OperationStep.EnterWallet;

                            var transactionModel = new TransactionDC
                            {
                                UserId = message.Chat.Id,
                                TransactionId = operationState.OperationId,
                                Pair = changePair,
                                Amount = operationState.Amount,
                                DateTime = DateTime.Now,
                                /*CurrentCourse = "current_course_value" // Здесь укажите актуальное значение курса*/
                            };

                            await client.SendTextMessageAsync(message.Chat.Id, $"Вы хотите купить {message.Text} {selectedTargetCurrency} \nId вашей операции:{operationState.OperationId}. \nУкажите его в назначении платежа. \n\nВведите ваш кошелек для зачисления. \nИ отправьте на карту \n{walletUAH} \nследующую сумму:");
                            // Рассчитать итоговую сумму на основе выбранной криптовалюты и введенной суммы
                            await _currentCourse.CalculateAmountInUSD(message.Chat.Id, message.Text, changePair, "uah");
                            await _botMethods.CreateTransactionInApi(transactionModel);
                            operationState.OrderAmount = message.Text;
                            break;

                        case OperationStep.EnterWallet:
                            // Код обработки кошелька, например, сохранить его в operationState и вывести сообщение "Спасибо, ожидаем ваш платеж":
                            operationState.Wallet = message.Text;
                            _userOperations.Remove(message.Chat.Id); // Удаляем состояние операции после завершения операции
                            await client.SendTextMessageAsync(message.Chat.Id, "Спасибо, ожидаем ваш платеж. \nПосле отправки перешлите в бот скриншот платежа.");
                            // ID чата, куда нужно переслать фото (в данном случае ID технического чата)
                            long technicalChatId = 6642646501; // Замените на реальный ID вашего технического чата
                            string order = $"Заявка {operationState.OperationId} \nКошелек-{message.Text} \nСумма: {operationState.OrderAmount}{selectedTargetCurrency}";
                            // Пересылаем фото в технический чат
                            await client.SendTextMessageAsync(technicalChatId, $"{order}");
                            break;

                        default:
                            await client.SendTextMessageAsync(message.Chat.Id, $"Вы сказали: \n{message.Text}");
                            break;
                    }

                    // Сохраняем обновленное состояние операции
                    _userOperations[message.Chat.Id] = operationState;
                }
                else
                {
                    await client.SendTextMessageAsync(message.Chat.Id, $"Вы сказали: \n{message.Text}");
                }
            }
        }

        private static async Task HandleCallbackQuery(ITelegramBotClient client, CallbackQuery callbackQuery)
        {
            var chatId = callbackQuery.Message.Chat.Id;
            var data = callbackQuery.Data;

            if (data.StartsWith("select_base_"))
            {
                var selectedBaseCurrency = data.Replace("select_base_", "");
                if (!_userSelectedBaseCurrencies.ContainsKey(chatId))
                {
                    _userSelectedBaseCurrencies.Add(chatId, selectedBaseCurrency);
                }
                else
                {
                    _userSelectedBaseCurrencies[chatId] = selectedBaseCurrency;
                }

                var targetCurrencies = new[]
                {
            new[] {$"{selectedBaseCurrency}/UAH" },
        };

                var inlineKeyboard = new InlineKeyboardMarkup(targetCurrencies
                    .Select(row => row.Select(currency => InlineKeyboardButton.WithCallbackData(currency, $"select_target_{currency}")))
                );

                await client.SendTextMessageAsync(chatId, "Каким способом хотите оплатить?", replyMarkup: inlineKeyboard);
            }
            else if (data.StartsWith("select_target_"))
            {
                /*string paymentMethodMessage = string.Empty;*/
                if (_userSelectedBaseCurrencies.TryGetValue(chatId, out var selectedBaseCurrency))
                {
                    var selectedTargetCurrency = data.Replace("select_target_", "");
                    _userSelectedTargetCurrencies[chatId] = selectedTargetCurrency;

                    var responseConsoleMessage = Console.Out.WriteLineAsync(selectedTargetCurrency);


                    if (CurrencyPairMappings.TryGetValue(selectedTargetCurrency, out var _changePair))
                    {
                        await responseConsoleMessage;
                        await client.SendTextMessageAsync(chatId, $"Выбрана валютная пара: {_changePair}");
                        changePair = _changePair;
                    }
                    else
                    {
                        await responseConsoleMessage;
                        await client.SendTextMessageAsync(chatId, "Способ оплаты не определен");
                    }
                }
                else
                {
                    await client.SendTextMessageAsync(chatId, "Пожалуйста, сначала выберите базовую криптовалюту.");
                }
                // Можно использовать Guid для уникального идентификатора операции
                var operationId = Guid.NewGuid().ToString();
                string orderAmount = null;
                var operationState = new OperationState
                {
                    SelectedBaseCurrency = selectedBaseCurrency,
                    SelectedTargetCurrency = selectedTargetCurrency,
                    OperationId = operationId,
                    CurrentStep = OperationStep.EnterAmount, // Переходим к следующему шагу - вводу суммы
                    OrderAmount = orderAmount,
                };

                // Сохраняем состояние операции в словарь
                _userOperations[chatId] = operationState;
                await client.SendTextMessageAsync(chatId, "Напишите сумму интересуемой валюты, которую хотите купить:");
            }

            await client.AnswerCallbackQueryAsync(callbackQuery.Id); // Respond to the CallbackQuery to remove the "reading" indicator
        }


        private Task Error(ITelegramBotClient client, Exception exception, CancellationToken token)
        {
            throw new NotImplementedException();
        }
    }
}
