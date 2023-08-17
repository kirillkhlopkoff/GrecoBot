using GrecoBot.ClientBot.Keybords;
using GrecoBot.ClientBot.Methods;
using GrecoBot.Core;
using GrecoBot.DC;
using Newtonsoft.Json;
using System;
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
        public static TelegramBotClient _client;
        private readonly HttpClient _httpClient;
        private readonly BotMethods _botMethods;
        private readonly CurrencyRatesProvider _ratesProvider;

        public TelegramBotHandler(string apikey, HttpClient httpClient)
        {
            _client = new TelegramBotClient(apikey);
            _httpClient = httpClient;
            _botMethods = new BotMethods(_httpClient, _ratesProvider);
        }

        private Dictionary<long, UserState> _userStates = new Dictionary<long, UserState>();

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
            if (update.Type == UpdateType.Message && update.Message?.Text != null)
            {
                await Console.Out.WriteLineAsync($"{update.Message.Chat.FirstName} | {update.Message.Text}");

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
                    // Получаем информацию о пользователе из API
                    var userInfo = await _botMethods.GetUserInfoFromApi(message.Chat.Id);

                    // Отправляем сообщение пользователю с информацией о его Личном кабинете
                    var userMessage = $"👤Добро пожаловать в ваш Личный Кабинет!\n" +
                                      $"Id: {userInfo.Id}\n" +
                                      $"Телефон: {userInfo.Phone}";

                    await _client.SendTextMessageAsync(message.Chat.Id, userMessage);
                }

                else
                {
                    await client.SendTextMessageAsync(message.Chat.Id, $"Вы сказали: \n{message.Text}");
                }

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
        }



        private Task Error(ITelegramBotClient client, Exception exception, CancellationToken token)
        {
            throw new NotImplementedException();
        }
    }
}
