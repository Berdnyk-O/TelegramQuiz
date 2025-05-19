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
        public Action<long> OnQuizSelected;
        Func<long, string, Task> OnInlineButtonPressed;

        private static readonly InputPollOption[] PollOptions = ["Hello", "World!"];

        private readonly ITelegramBotClient _bot;
        private readonly Engine _engine;
        private ILogger<UpdateHandler> _logger;

        public UpdateHandler(ITelegramBotClient botClient, ILogger<UpdateHandler> logger, Engine engine)
        {
            _bot = botClient;
            _engine = engine;
            _logger = logger;

            _engine.SendMessageAsync = async (chatId, text) =>
            {
                await SendMessage(chatId, text);
            };

            _engine.SendQuestionAsync = async (chatId, body, answers, onAnswerCallback) =>
            {
                await SendQuestion(chatId, body, answers, onAnswerCallback);
            };
        }

        public async Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, HandleErrorSource source, CancellationToken cancellationToken)
        {
            _logger.LogInformation("HandleError: {Exception}", exception);
            // Cooldown in case of network connection error
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

            var t = messageText.Split(' ')[0];

            Message sentMessage = await (messageText.Split(' ')[0] switch
            {
                "/start" => StartQuiz(msg),
                "/stop" => StopQuiz(msg),
                "/c" => GetQuestion(msg),
                _ => Usage(msg)
            });
            _logger.LogInformation("The message was sent with id: {SentMessageId}", sentMessage.Id);
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

        async Task<Message> StartQuiz(Message msg)
        {
            _engine.UserName = msg.Chat.Username;
            await _engine.Run(msg.Chat.Id);
            return await _bot.SendMessage(msg.Chat, "Start");
        }

        async Task<Message> StopQuiz(Message msg)
        {
            await _engine.StopTest(msg.Chat.Id);
            return await _bot.SendMessage(msg.Chat, "Stop");
        }

        async Task<Message> GetQuestion(Message msg)
        {
            if(msg.Text.Length<4) return await _bot.SendMessage(msg.Chat, "Помилка в команді");
            int number;

            bool success = Int32.TryParse(msg.Text.Split(' ')[1],out number);
            if (!success)
            {
                return await _bot.SendMessage(msg.Chat, "Невірно введений номер");
            }

            await _engine.GetQuestion(msg.Chat.Id, number-1);
            return await _bot.SendMessage(msg.Chat, "GetQuestion");
        }

        public async Task<Message> SendQuestion(long chatId,string body, string[] answers, Func<long, string, Task> onUserAnswer)
        {
            /* var keyboardButtons = answers
                 .Select(answer => new KeyboardButton(answer))
                 .ToArray();

             var replyKeyboard = new ReplyKeyboardMarkup(
             keyboardButtons.Select(b => new[] { b }))
             {
                 ResizeKeyboard = true,
                 OneTimeKeyboard = true
             };*/

            var buttons = answers
                .Select(answer => new[] { InlineKeyboardButton.WithCallbackData(answer, answer[0].ToString()) })
                .ToArray();

            var replyMarkup = new InlineKeyboardMarkup(buttons);

            if (OnInlineButtonPressed == null)
                OnInlineButtonPressed = onUserAnswer;

            return await _bot.SendMessage(chatId, body, replyMarkup: replyMarkup);
        }

        public async Task SendMessage(long chatId, string message)
        {
            await _bot.SendMessage(chatId, message);
        }

        private async Task OnCallbackQuery(CallbackQuery callbackQuery)
        {
            _logger.LogInformation("Received inline keyboard callback from: {CallbackQueryId}", callbackQuery.Id);
            _engine.CheckAsync(callbackQuery.Message!.Chat.Id, callbackQuery.Data);
        }

        private Task UnknownUpdateHandlerAsync(Update update)
        {
            _logger.LogInformation("Unknown update type: {UpdateType}", update.Type);
            return Task.CompletedTask;
        }
    }
}
