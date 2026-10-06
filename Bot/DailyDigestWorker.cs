using ExpenseBot.Options;
using ExpenseBot.Services;
using Microsoft.Extensions.Options;
using Telegram.Bot;

namespace ExpenseBot.Bot;

public class DailyDigestWorker : BackgroundService
{
    private readonly ITelegramBotClient _bot;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly int _hour;
    private readonly ILogger<DailyDigestWorker> _logger;

    public DailyDigestWorker(ITelegramBotClient bot, IServiceScopeFactory scopeFactory,
        IOptions<TelegramOptions> options, ILogger<DailyDigestWorker> logger)
    {
        _bot = bot;
        _scopeFactory = scopeFactory;
        _hour = options.Value.DigestHourUtc;
        _logger = logger;
    }

    public static DateTime GetNextRun(DateTime now, int hour)
    {
        var next = now.Date.AddHours(hour);
        if (next <= now)
            next = next.AddDays(1);
        return next;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var now = DateTime.UtcNow;
            var next = GetNextRun(now, _hour);
            _logger.LogInformation("Next daily message at {Next} UTC", next);

            await Task.Delay(next - now, stoppingToken);
            await SendToAllAsync();

            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        }
    }

    private async Task SendToAllAsync()
    {
        using var scope = _scopeFactory.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<SpendingService>();
        var formatter = scope.ServiceProvider.GetRequiredService<MessageFormatter>();

        var chats = await service.GetChatsWithSpendingsThisMonthAsync();
        foreach (var chat in chats)
        {
            try
            {
                var summary = await service.GetSummaryAsync(chat);
                await _bot.SendMessage(chat.ChatId, formatter.Recap(summary, chat));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Could not send daily message to {ChatId}", chat.ChatId);
            }
        }
    }
}
