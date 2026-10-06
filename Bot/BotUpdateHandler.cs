using ExpenseBot.Parsing;
using ExpenseBot.Services;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace ExpenseBot.Bot;

public class BotUpdateHandler
{
    private readonly ITelegramBotClient _bot;
    private readonly SpendingService _service;
    private readonly MessageFormatter _formatter;

    public BotUpdateHandler(ITelegramBotClient bot, SpendingService service, MessageFormatter formatter)
    {
        _bot = bot;
        _service = service;
        _formatter = formatter;
    }

    public async Task HandleAsync(Update update)
    {
        if (update.Message == null || update.Message.Text == null)
            return;

        long chatId = update.Message.Chat.Id;
        string text = update.Message.Text.Trim();

        string reply = await GetReplyAsync(chatId, text);
        await _bot.SendMessage(chatId, reply);
    }

    private async Task<string> GetReplyAsync(long chatId, string text)
    {
        if (text == "/start")
        {
            await _service.StartChatAsync(chatId);
            return "Hi! Send me your spendings.\n" + MessageFormatter.Format;
        }

        var chat = await _service.FindChatAsync(chatId);
        if (chat == null)
            return "Send /start first.";

        if (text == "/today")
        {
            var today = await _service.GetTodayAsync(chat);
            return _formatter.Today(today);
        }

        if (text == "/month")
        {
            var summary = await _service.GetSummaryAsync(chat);
            return _formatter.Recap(summary, chat);
        }

        if (!SpendingParser.TryParse(text, out var parsed, out var error))
            return error + "\n" + MessageFormatter.Format;

        var spending = await _service.AddSpendingAsync(chat, parsed);
        return $"Saved {_formatter.Money(spending.Amount)} · {spending.Category}";
    }
}
