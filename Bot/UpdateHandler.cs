using Microsoft.Extensions.Logging;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot.Types;
using Telegram.Bot;

namespace TelegramQuiz.Bot
{
    public class UpdateHandler : IUpdateHandler
    {
        private readonly ITelegramBotClient _bot;
        private readonly Engine _engine;
        private ILogger<UpdateHandler> _logger;

        private bool _waitingForConfirm = false;

        public UpdateHandler(ITelegramBotClient botClient, ILogger<UpdateHandler> logger, Engine engine)
        {
            _bot = botClient;
            _engine = engine;
            _logger = logger;

            _engine.SendMessageAsync = async (chatId, text) =>
            {
                await SendMessage(chatId, text);
            };

            _engine.SendQuestionAsync = async (chatId, body, answers) =>
            {
                await SendQuestion(chatId, body, answers);
            };
        }

        public async Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, HandleErrorSource source, CancellationToken cancellationToken)
        {
            _logger.LogInformation("HandleError: {Exception}", exception);
            
            if (exception is RequestException)
                await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);
        }

        public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await (update switch
            {
                { Message: { } message } => OnMessage(message),
                { CallbackQuery: { } callbackQuery } => OnCallbackQuery(callbackQuery),
                // UpdateType.ChannelPost:
                // UpdateType.EditedChannelPost:
                // UpdateType.ShippingQuery:
                // UpdateType.PreCheckoutQuery:
                _ => UnknownUpdateHandlerAsync(update)
            });
        }

        private async Task OnMessage(Message msg)
        {
            _logger.LogInformation("Receive message type: {MessageType}", msg.Type);
            if (msg.Text is not { } messageText)
                return;

            var command = messageText.Split(' ')[0];

            if(_waitingForConfirm)
            {
                switch (command)
                {
                    case "Так":
                        await StartQuiz(msg);
                        break;
                    case "Ні":
                        await SendMessage(msg.Chat.Id, "Відміна");
                        break;
                    default:
                        await SendMessage(msg.Chat.Id, "Введіть Так або Ні");
                        await ConfirmStart(msg);
                        break;
                }
                _waitingForConfirm = false;
                return;
            }

            switch (command)
            {
                case "/start":
                    await ConfirmStart(msg);
                    break;
                case "/stop":
                    await StopQuiz(msg);
                    break;
                case "/c":
                    await GetQuestion(msg);
                    break;
                default:
                    await Usage(msg);
                    break;
            }
        }

        async Task<Message> Usage(Message msg)
        {
            const string usage = """
                <b><u>Bot menu</u></b>:
                /start          - пройти тест
                /stop           - завершити тест
                /c {i}          - відповісти на конкретне запитання
            """;
            return await _bot.SendMessage(msg.Chat, usage, parseMode: ParseMode.Html, replyMarkup: new ReplyKeyboardRemove());
        }

        async Task ConfirmStart(Message msg)
        {
            _waitingForConfirm = true;
            await _bot.SendMessage(msg.Chat, "Привіт! почнемо роботу");
            var replyMarkup = new ReplyKeyboardMarkup(true)
            .AddNewRow("Так")
            .AddNewRow("Ні");

            replyMarkup.ResizeKeyboard = true;
            replyMarkup.OneTimeKeyboard = true;

            await _bot.SendMessage(msg.Chat, "Нажми Так для старту", replyMarkup: replyMarkup);
        }

        async Task StartQuiz(Message msg)
        {
            _engine.UserName = msg.Chat.Username ??
                $"{msg.Chat.FirstName} {msg.Chat.LastName}";
            await _engine.Run(msg.Chat.Id);
        }

        async Task StopQuiz(Message msg)
        {
            await _engine.StopTest(msg.Chat.Id);
        }

        async Task GetQuestion(Message msg)
        {
            if(msg.Text!.Length < 4)
            {
                await _bot.SendMessage(msg.Chat, "Помилка в команді");
            }
                
            int number;
            bool success = Int32.TryParse(msg.Text.Split(' ')[1],out number);
            
            if (!success)
            {
                await _bot.SendMessage(msg.Chat, "Невірно введений номер");
            }

            await _engine.GetQuestion(msg.Chat.Id, number-1);
        }

        public async Task<Message> SendQuestion(long chatId,string body, string[] answers)
        {
            var buttons = answers
                 .Select(answer => new[] { InlineKeyboardButton.WithCallbackData(answer, answer[0].ToString()) })
                 .ToArray();

             var replyMarkup = new InlineKeyboardMarkup(buttons);

             return await _bot.SendMessage(chatId, body, replyMarkup: replyMarkup);
        }

        public async Task SendMessage(long chatId, string message)
        {
            await _bot.SendMessage(chatId, message);
        }

        private async Task OnCallbackQuery(CallbackQuery callbackQuery)
        {
            _logger.LogInformation("Received inline keyboard callback from: {CallbackQueryId}", callbackQuery.Id);

            await _engine.CheckAsync(callbackQuery.Message!.Chat.Id, callbackQuery.Data);
        }

        private Task UnknownUpdateHandlerAsync(Update update)
        {
            _logger.LogInformation("Unknown update type: {UpdateType}", update.Type);
            return Task.CompletedTask;
        }
    }
}
