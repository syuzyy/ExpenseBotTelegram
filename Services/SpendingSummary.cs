using ExpenseBot.Data;

namespace ExpenseBot.Services;

public class CategoryTotal
{
    public string Category { get; set; } = "";
    public decimal Total { get; set; }
    public int Count { get; set; }
}

public class DayTotal
{
    public DateTime Date { get; set; }
    public decimal Total { get; set; }
}

public class SpendingSummary
{
    public decimal MonthTotal { get; set; }
    public int MonthCount { get; set; }
    public decimal Last7Days { get; set; }
    public decimal Previous7Days { get; set; }
    public int DaysElapsed { get; set; }
    public decimal TypicalDay { get; set; }
    public List<CategoryTotal> Categories { get; set; } = new();
    public List<DayTotal> Last14Days { get; set; } = new();

    public static SpendingSummary Build(List<Spending> spendings, DateTime now)
    {
        var today = now.Date;
        var monthStart = new DateTime(now.Year, now.Month, 1);

        var month = spendings.Where(s => s.SpentAt >= monthStart).ToList();

        var summary = new SpendingSummary();
        summary.MonthTotal = month.Sum(s => s.Amount);
        summary.MonthCount = month.Count;

        // [today-7, today) and [today-14, today-7)
        summary.Last7Days = spendings
            .Where(s => s.SpentAt >= today.AddDays(-7) && s.SpentAt < today)
            .Sum(s => s.Amount);
        summary.Previous7Days = spendings
            .Where(s => s.SpentAt >= today.AddDays(-14) && s.SpentAt < today.AddDays(-7))
            .Sum(s => s.Amount);

        summary.DaysElapsed = now.Day;
        summary.TypicalDay = Math.Round(summary.MonthTotal / now.Day, 2);

        summary.Categories = month
            .GroupBy(s => s.Category)
            .Select(g => new CategoryTotal { Category = g.Key, Total = g.Sum(s => s.Amount), Count = g.Count() })
            .OrderByDescending(c => c.Total)
            .ToList();

        for (int i = 0; i < 14; i++)
        {
            var day = today.AddDays(-i);
            summary.Last14Days.Add(new DayTotal
            {
                Date = day,
                Total = spendings.Where(s => s.SpentAt.Date == day).Sum(s => s.Amount)
            });
        }

        return summary;
    }
}
