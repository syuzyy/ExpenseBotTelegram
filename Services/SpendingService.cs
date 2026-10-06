using System.Security.Cryptography;
using ExpenseBot.Data;
using ExpenseBot.Parsing;
using Microsoft.EntityFrameworkCore;

namespace ExpenseBot.Services;

public class SpendingService
{
    private readonly AppDbContext _db;

    public SpendingService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<Chat?> FindChatAsync(long chatId)
    {
        return await _db.Chats.FirstOrDefaultAsync(c => c.ChatId == chatId);
    }

    public async Task<Chat?> FindChatByTokenAsync(string token)
    {
        return await _db.Chats.FirstOrDefaultAsync(c => c.ReportToken == token);
    }

    public async Task<Chat> StartChatAsync(long chatId)
    {
        var chat = await FindChatAsync(chatId);
        if (chat != null)
            return chat;

        chat = new Chat
        {
            ChatId = chatId,
            ReportToken = Convert.ToHexString(RandomNumberGenerator.GetBytes(16)),
            StartedAt = DateTime.UtcNow
        };
        _db.Chats.Add(chat);
        await _db.SaveChangesAsync();
        return chat;
    }

    public async Task<Spending> AddSpendingAsync(Chat chat, ParsedSpending parsed)
    {
        var spending = new Spending
        {
            ChatId = chat.Id,
            Amount = parsed.Amount,
            Category = parsed.Category,
            Note = parsed.Note,
            SpentAt = DateTime.UtcNow
        };
        _db.Spendings.Add(spending);
        await _db.SaveChangesAsync();
        return spending;
    }

    public async Task<List<Spending>> GetTodayAsync(Chat chat)
    {
        var today = DateTime.UtcNow.Date;
        return await _db.Spendings
            .Where(s => s.ChatId == chat.Id && s.SpentAt >= today)
            .ToListAsync();
    }

    public async Task<SpendingSummary> GetSummaryAsync(Chat chat)
    {
        var now = DateTime.UtcNow;
        var monthStart = new DateTime(now.Year, now.Month, 1);
        var twoWeeksAgo = now.Date.AddDays(-14);
        var from = monthStart < twoWeeksAgo ? monthStart : twoWeeksAgo;

        // SQLite can't sum decimals, so we load the rows and calculate in C#
        var spendings = await _db.Spendings
            .Where(s => s.ChatId == chat.Id && s.SpentAt >= from)
            .ToListAsync();

        return SpendingSummary.Build(spendings, now);
    }

    public async Task<List<Spending>> GetLastAsync(Chat chat, int count)
    {
        return await _db.Spendings
            .Where(s => s.ChatId == chat.Id)
            .OrderByDescending(s => s.Id)
            .Take(count)
            .ToListAsync();
    }

    public async Task<List<Chat>> GetChatsWithSpendingsThisMonthAsync()
    {
        var now = DateTime.UtcNow;
        var monthStart = new DateTime(now.Year, now.Month, 1);
        return await _db.Chats
            .Where(c => c.Spendings.Any(s => s.SpentAt >= monthStart))
            .ToListAsync();
    }
}
