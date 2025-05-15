using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using TelegramQuiz;
using TelegramQuiz.Database;

var builder = new ConfigurationBuilder();
builder.SetBasePath(Directory.GetCurrentDirectory())
       .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

IConfiguration config = builder.Build();

var token = config["TelegramBotToken"];
var connectionString = config["ConnectionStrings:DefaultConnection"];

var options = new DbContextOptionsBuilder<QuizDbContext>()
            .UseNpgsql(connectionString)
            .Options;

QuizDbContext quizDbContext = new QuizDbContext(options);

FileWriter fileWriter = new FileWriter("/Answers", "UserAnswers", ".txt");
Statistics statistics = new Statistics(0, 0, fileWriter);
QuestionData questionData = new QuestionData();

TelegramClient client = new TelegramClient(token);
Engine engine = new Engine(questionData,
    client,
    fileWriter,
    statistics);

while (Console.ReadKey(true).Key != ConsoleKey.Escape) ;
    Environment.Exit(0);

/*IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        services.AddScoped<FileWriter>(provider =>
            new FileWriter("/Answers", "UserAnswers", ".txt"));
        services.AddScoped<Statistics>(provider => new Statistics(0,0, provider.GetRequiredService<FileWriter>()));
        services.AddScoped<QuestionData>();
        services.AddDbContext<QuizDbContext>(options =>
            options.UseNpgsql(context.Configuration.GetConnectionString("DefaultConnection")));

        services.AddHttpClient("telegram_bot_client").RemoveAllLoggers()
                .AddTypedClient<ITelegramBotClient>((httpClient, sp) =>
                {
                    string token = context.Configuration.GetValue<string>("TelegramBotToken");
                    TelegramBotClientOptions options = new(token);
                    return new TelegramBotClient(options, httpClient);
                });

        services.AddScoped<TelegramClient>(provider => new TelegramClient(provider.GetRequiredService<ITelegramBotClient>()));

        services.AddScoped<Engine>();
    })
    .Build();

await host.RunAsync();*/


/*public class QuizDbContextFactory : IDesignTimeDbContextFactory<QuizDbContext>
{
    public QuizDbContext CreateDbContext(string[] args)
    {
        IConfigurationRoot configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory()) // важливо для dotnet ef
            .AddJsonFile("appsettings.json")
            .Build();

        var connectionString = configuration.GetConnectionString("DefaultConnection");

        var optionsBuilder = new DbContextOptionsBuilder<QuizDbContext>();
        optionsBuilder.UseNpgsql(connectionString);

        return new QuizDbContext(optionsBuilder.Options);
    }
}*/