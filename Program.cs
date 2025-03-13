using Microsoft.Extensions.Configuration;
using Telegram.Bot;

var builder = new ConfigurationBuilder();
builder.SetBasePath(Directory.GetCurrentDirectory())
       .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

IConfiguration config = builder.Build();

var token = config["TelegramBotToken"];

Console.WriteLine($"Token:  {token}");


var bot = new TelegramBotClient(token);
var me = await bot.GetMe();
Console.WriteLine($"Hello, World! I am user {me.Id} and my name is {me.FirstName}.");