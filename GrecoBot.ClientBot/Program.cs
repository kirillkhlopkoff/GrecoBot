using GrecoBot.ClientBot;

class Program
{
    static void Main(string[] args)
    {
        using var httpClient = new HttpClient();
        var botHandler = new TelegramBotHandler("6358851439:AAEQksFeU-v4gkPjTfXjd384PMUL7d8cAcQ", httpClient);
        botHandler.RunBotAsync().GetAwaiter().GetResult();
    }
}
