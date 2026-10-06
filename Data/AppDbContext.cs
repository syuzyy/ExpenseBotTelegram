using Microsoft.EntityFrameworkCore;

namespace ExpenseBot.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Chat> Chats => Set<Chat>();
    public DbSet<Spending> Spendings => Set<Spending>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Chat>().HasIndex(c => c.ChatId).IsUnique();
        modelBuilder.Entity<Chat>().HasIndex(c => c.ReportToken).IsUnique();
    }
}
