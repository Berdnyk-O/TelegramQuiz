using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Telegram.Bot;
using TelegramQuiz;
using TelegramQuiz.Bot;
using TelegramQuiz.Database;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .WriteTo.File(Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, @$"..\..\..\Logs\log-.txt")), rollingInterval: RollingInterval.Day)
    .CreateLogger();

try
{
    IHost host = Host.CreateDefaultBuilder(args)
        .UseSerilog()
        .ConfigureServices((context, services) =>
        {
            services.AddScoped<FileWriter>(provider =>
                  new FileWriter("/Answers", "UserAnswers", ".txt"));
            services.AddScoped<Statistics>(provider => new Statistics(0, 0, provider.GetRequiredService<FileWriter>()));
            services.AddScoped<QuestionData>();
            services.AddDbContext<QuizDbContext>(options =>
                options.UseNpgsql(context.Configuration.GetConnectionString("DefaultConnection")));

            services.AddScoped<Engine>();

            services.AddHttpClient("telegram_bot_client").RemoveAllLoggers()
                .AddTypedClient<ITelegramBotClient>((httpClient, sp) =>
                {
                    string? token = context.Configuration.GetValue<string>("TelegramBotToken");
                    ArgumentNullException.ThrowIfNull(token);
                    TelegramBotClientOptions options = new(token);
                    return new TelegramBotClient(options, httpClient);
                });

            services.AddScoped<UpdateHandler>();
            services.AddScoped<ReceiverService>();
            services.AddHostedService<PollingService>();
        })
        .Build();

    await host.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Застосунок аварійно завершив роботу");
}
finally
{
    Log.CloseAndFlush();
}