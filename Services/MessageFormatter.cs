using System.Text;
using ExpenseBot.Data;
using ExpenseBot.Options;
using Microsoft.Extensions.Options;

namespace ExpenseBot.Services;

public class MessageFormatter
{
    public const string Format = "Format: <amount> <category> [note]\nExample: 4.50 coffee";

    private readonly string _currency;
    private readonly string _baseUrl;

    public MessageFormatter(IOptions<TelegramOptions> options, IConfiguration config)
    {
        _currency = options.Value.Currency;
        _baseUrl = config["PublicBaseUrl"] ?? "http://localhost:5080";
    }

    public string Money(decimal amount)
    {
        return $"{amount:0.##} {_currency}";
    }

    public string ReportLink(Chat chat)
    {
        return $"{_baseUrl}/report/{chat.ReportToken}";
    }

    public string Today(List<Spending> spendings)
    {
        if (spendings.Count == 0)
            return "No spendings today.";

        var sb = new StringBuilder();
        sb.AppendLine($"Today: {Money(spendings.Sum(s => s.Amount))} ({spendings.Count})");
        foreach (var s in spendings)
            sb.AppendLine($"- {Money(s.Amount)} {s.Category} {s.Note}");
        return sb.ToString();
    }

    public string Recap(SpendingSummary summary, Chat chat)
    {
        if (summary.MonthCount == 0)
            return "No spendings this month.";

        var sb = new StringBuilder();
        sb.AppendLine($"This month: {Money(summary.MonthTotal)} ({summary.MonthCount} entries)");
        sb.AppendLine($"Last 7 days: {Money(summary.Last7Days)}");
        sb.AppendLine($"Previous 7 days: {Money(summary.Previous7Days)}");
        sb.AppendLine($"Typical day: {Money(summary.TypicalDay)}");
        sb.AppendLine("Top categories:");
        foreach (var c in summary.Categories.Take(3))
            sb.AppendLine($"- {c.Category}: {Money(c.Total)}");
        sb.AppendLine($"Report: {ReportLink(chat)}");
        return sb.ToString();
    }
}
