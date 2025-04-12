using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Microsoft.Extensions.Configuration;

namespace Tenis_TelegramBot.App;

public class TelegramBotHandler
{
    private readonly IConfiguration _config;
    private readonly TelegramBotClient _botClient;

    public TelegramBotHandler(IConfiguration config)
    {
        _config = config;
        _botClient = new TelegramBotClient(_config["Telegram:BotToken"]);
    }

    public async Task StartAsync()
    {
        var cts = new CancellationTokenSource();

        var receiverOptions = new ReceiverOptions
        {
            AllowedUpdates = new[] { UpdateType.Message }
        };

        _botClient.StartReceiving(
            HandleUpdateAsync,
            HandleErrorAsync,
            receiverOptions,
            cancellationToken: cts.Token
        );

        var me = await _botClient.GetMeAsync();
        Console.WriteLine($"🤖 Bot başladı: {me.Username}");
    }

    private async Task HandleUpdateAsync(ITelegramBotClient bot, Update update, CancellationToken token)
    {
        if (update.Type != UpdateType.Message || update.Message?.Text == null)
            return;

        var message = update.Message;

        // ⏳ Bot yeniden başlarken geçmiş mesajları ignore et
        if (message.Date < DateTime.UtcNow.AddMinutes(-1))
        {
            Console.WriteLine("⏩ Eski mesaj atlandı: " + message.Text);
            return;
        }

        Console.WriteLine($"📩 Gelen mesaj: {message.Text}");

        if (message.Text.StartsWith("/randevu", StringComparison.OrdinalIgnoreCase))
        {
            await bot.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: "🔄 Randevu bilgileri alınıyor, lütfen bekleyin...",
                cancellationToken: token
            );

            var checker = new RandevuChecker(_config);
            var dailyResults = checker.GetAvailableSessions();

            if (dailyResults.Count == 1 && dailyResults[0].StartsWith("❌"))
            {
                await bot.SendTextMessageAsync(message.Chat.Id, text: dailyResults[0], cancellationToken: token);
                return;
            }

            string response = "📅 Bugünkü Randevu Durumu:\n\n";

            if (dailyResults.Count == 0)
                response += "Bugün için uygun randevu bulunamadı.";
            else
                response += string.Join("\n", dailyResults);

            await bot.SendTextMessageAsync(message.Chat.Id, response, cancellationToken: token);
        }
    }

    private Task HandleErrorAsync(ITelegramBotClient bot, Exception exception, CancellationToken token)
    {
        Console.WriteLine($"Hata: {exception.Message}");
        return Task.CompletedTask;
    }
}