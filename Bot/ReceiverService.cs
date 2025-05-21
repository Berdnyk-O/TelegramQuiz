using Microsoft.Extensions.Logging;
using Telegram.Bot;
using TelegramQuiz.Bot.Abstract;

namespace TelegramQuiz.Bot
{
    public class ReceiverService(ITelegramBotClient botClient, UpdateHandler updateHandler, ILogger<ReceiverServiceBase<UpdateHandler>> logger)
    : ReceiverServiceBase<UpdateHandler>(botClient, updateHandler, logger);
}
