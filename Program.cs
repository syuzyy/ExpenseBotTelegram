using ExpenseBot.Bot;
using ExpenseBot.Data;
using ExpenseBot.Options;
using ExpenseBot.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Telegram.Bot;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<TelegramOptions>(builder.Configuration.GetSection(TelegramOptions.SectionName));

builder.Services.AddDbContext<AppDbContext>(o => o.UseSqlite("Data Source=expenses.db"));

builder.Services.AddSingleton<ITelegramBotClient>(sp =>
{
    var options = sp.GetRequiredService<IOptions<TelegramOptions>>().Value;
    return new TelegramBotClient(options.BotToken);
});

builder.Services.AddSingleton<MessageFormatter>();
builder.Services.AddScoped<SpendingService>();
builder.Services.AddScoped<BotUpdateHandler>();

builder.Services.AddHostedService<TelegramPollingWorker>();
builder.Services.AddHostedService<DailyDigestWorker>();

builder.Services.AddControllers();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
}

app.MapGet("/", () => "ExpenseBot is running");
app.MapControllers();

app.Run();
