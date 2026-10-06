using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace ExpenseBot.Bot;

public class TelegramPollingWorker : BackgroundService
{
    private readonly ITelegramBotClient _bot;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<TelegramPollingWorker> _logger;

    public TelegramPollingWorker(ITelegramBotClient bot, IServiceScopeFactory scopeFactory,
        ILogger<TelegramPollingWorker> logger)
    {
        _bot = bot;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var options = new ReceiverOptions
        {
            AllowedUpdates = new[] { UpdateType.Message }
        };

        _bot.StartReceiving(HandleUpdateAsync, HandleErrorAsync, options, stoppingToken);
        _logger.LogInformation("Bot started");

        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    private async Task HandleUpdateAsync(ITelegramBotClient bot, Update update, CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<BotUpdateHandler>();

        try
        {
            await handler.HandleAsync(update);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while handling message");
        }
    }

    private Task HandleErrorAsync(ITelegramBotClient bot, Exception ex, HandleErrorSource source, CancellationToken ct)
    {
        _logger.LogError(ex, "Telegram error");
        return Task.CompletedTask;
    }
}
