using System.Net;
using System.Text;
using ExpenseBot.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ExpenseBot.Controllers;

[ApiController]
[AllowAnonymous]
[Route("report")]
public class ReportController : ControllerBase
{
    private readonly SpendingService _service;
    private readonly MessageFormatter _formatter;

    public ReportController(SpendingService service, MessageFormatter formatter)
    {
        _service = service;
        _formatter = formatter;
    }

    [HttpGet("{token}")]
    public async Task<IActionResult> Get(string token)
    {
        var chat = await _service.FindChatByTokenAsync(token);
        if (chat == null)
            return NotFound();

        var summary = await _service.GetSummaryAsync(chat);
        var last = await _service.GetLastAsync(chat, 20);

        var html = new StringBuilder();
        html.Append("<html><head><meta charset='utf-8'><title>Report</title></head><body>");
        html.Append("<h1>Spending report</h1>");

        html.Append($"<p>This month: {_formatter.Money(summary.MonthTotal)} ({summary.MonthCount} entries)</p>");
        html.Append($"<p>Last 7 days: {_formatter.Money(summary.Last7Days)}</p>");
        html.Append($"<p>Previous 7 days: {_formatter.Money(summary.Previous7Days)}</p>");
        html.Append($"<p>Typical day: {_formatter.Money(summary.TypicalDay)}</p>");

        html.Append("<h2>Categories</h2><table border='1'><tr><th>Category</th><th>Count</th><th>Total</th></tr>");
        foreach (var c in summary.Categories)
            html.Append($"<tr><td>{c.Category}</td><td>{c.Count}</td><td>{_formatter.Money(c.Total)}</td></tr>");
        html.Append("</table>");

        html.Append("<h2>Last 14 days</h2><table border='1'><tr><th>Date</th><th>Total</th></tr>");
        foreach (var d in summary.Last14Days)
            html.Append($"<tr><td>{d.Date:yyyy-MM-dd}</td><td>{_formatter.Money(d.Total)}</td></tr>");
        html.Append("</table>");

        html.Append("<h2>Last 20 spendings</h2><table border='1'><tr><th>Date</th><th>Amount</th><th>Category</th><th>Note</th></tr>");
        foreach (var s in last)
        {
            var note = WebUtility.HtmlEncode(s.Note ?? "");
            html.Append($"<tr><td>{s.SpentAt:yyyy-MM-dd HH:mm}</td><td>{_formatter.Money(s.Amount)}</td><td>{s.Category}</td><td>{note}</td></tr>");
        }
        html.Append("</table>");

        html.Append("</body></html>");
        return Content(html.ToString(), "text/html");
    }
}
