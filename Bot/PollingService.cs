using Microsoft.Extensions.Logging;
using TelegramQuiz.Bot.Abstract;

namespace TelegramQuiz.Bot
{
    public class PollingService(IServiceProvider serviceProvider, ILogger<PollingService> logger)
    : PollingServiceBase<ReceiverService>(serviceProvider, logger);
}
