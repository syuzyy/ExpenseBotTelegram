namespace ExpenseBot.Data;

public class Chat
{
    public int Id { get; set; }
    public long ChatId { get; set; }
    public string ReportToken { get; set; } = "";
    public DateTime StartedAt { get; set; }

    public List<Spending> Spendings { get; set; } = new();
}
