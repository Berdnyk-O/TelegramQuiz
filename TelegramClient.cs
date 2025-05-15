using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace TelegramQuiz
{
    class TelegramClient
    {
        public Action OnQuizSelected;
        Func<string, Task> OnInlineButtonPressed;
        public TelegramBotClient Client { get; private set; }
        public User User { get; private set; }
        public string UserName { get; private set; }

        private long chatId;

        public TelegramClient(string token)
        {
            Client = new TelegramBotClient(token);

            GetMe();

            Client.OnMessage += OnMessage;
            Client.OnUpdate += OnUpdate;
        }

        private async void GetMe()
        {
            User = await Client.GetMe();
        }

        private async Task OnMessage(Message msg, UpdateType type)
        {
            chatId = msg.Chat.Id;
            
            string? username = msg.From?.Username;

            if (!string.IsNullOrEmpty(username))
            {
                UserName = username;
            }
            else
            {
                UserName = msg.From?.FirstName + " " + msg.From?.LastName;
            }

            if (msg.Text is not { } text)
                Console.WriteLine($"Received a message of type {msg.Type}");
            else if (text.StartsWith('/'))
            {
                var space = text.IndexOf(' ');
                if (space < 0) space = text.Length;
                var command = text[..space].ToLower();
                if (command.LastIndexOf('@') is > 0 and int at)
                    if (command[(at + 1)..].Equals(User.Username, StringComparison.OrdinalIgnoreCase))
                        command = command[..at];
                    else
                        return;
                await OnCommand(command, text[space..].TrimStart(), msg);
            }
            else
                await OnTextMessage(msg);
        }

        private async Task OnTextMessage(Message msg)
        {
            Console.WriteLine($"Received text '{msg.Text}' in {msg.Chat}");
            await OnCommand("/start", "", msg);
        }

        private async Task OnCommand(string command, string args, Message msg)
        {
            Console.WriteLine($"Received command: {command} {args}");
            switch (command)
            {
                case "/start":
                    await Client.SendMessage(msg.Chat, """
                        <b><u>Bot menu</u></b>:
                        /quiz       -  pass the test
                        /statistic       -  pass the test
                        """, parseMode: ParseMode.Html, linkPreviewOptions: true,
                        replyMarkup: new ReplyKeyboardRemove());
                    break;
                case "/quiz":
                    OnQuizSelected?.Invoke();
                    break;
            }
        }

        private async Task OnUpdate(Update update)
        {
            switch (update)
            {
                case { CallbackQuery: { } callbackQuery }: await OnCallbackQuery(callbackQuery); break;
                default: Console.WriteLine($"Received unhandled update {update.Type}"); break;
            }
        }

        private async Task OnCallbackQuery(CallbackQuery callbackQuery)
        {
            await Client.AnswerCallbackQuery(callbackQuery.Id, $"You selected {callbackQuery.Data}");
            await Client.SendMessage(callbackQuery.Message!.Chat, $"Received callback from inline button {callbackQuery.Data}");
            OnInlineButtonPressed?.Invoke(callbackQuery.Data);

        }

        public async Task SendQuestion(string body, string[] answers, Func<string, Task> onUserAnswer)
        {
            var buttons = answers
                .Select(answer => new[] { InlineKeyboardButton.WithCallbackData(answer, answer[0].ToString()) })
                .ToArray();

            var replyMarkup = new InlineKeyboardMarkup(buttons);

            if (OnInlineButtonPressed == null)
                OnInlineButtonPressed = onUserAnswer;

            await Client.SendMessage(chatId, body, replyMarkup: replyMarkup);
        }

        public async Task SendMessage(string message)
        {
            await Client.SendMessage(chatId, message);
        }

    }
}
