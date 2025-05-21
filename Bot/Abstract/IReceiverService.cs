

namespace TelegramQuiz.Bot.Abstract
{
    public interface IReceiverService
    {
        Task ReceiveAsync(CancellationToken stoppingToken);
    }
}
