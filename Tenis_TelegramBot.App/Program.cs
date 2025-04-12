using Microsoft.Extensions.Configuration;
using Tenis_TelegramBot.App;

var builder = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

var config = builder.Build();

var telegramHandler = new TelegramBotHandler(config);
await telegramHandler.StartAsync();

//Console.WriteLine("🤖 Bot dinlemeye başladı. Çıkmak için Enter'a bas.");
Console.ReadLine();

await Task.Delay(-1);