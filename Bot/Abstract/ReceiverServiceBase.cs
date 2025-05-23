﻿using Microsoft.Extensions.Logging;
using Telegram.Bot.Polling;
using Telegram.Bot;

namespace TelegramQuiz.Bot.Abstract
{
    public abstract class ReceiverServiceBase<TUpdateHandler>(ITelegramBotClient botClient, TUpdateHandler updateHandler, ILogger<ReceiverServiceBase<TUpdateHandler>> logger)
    : IReceiverService where TUpdateHandler : IUpdateHandler
    {
        /// <summary>Start to service Updates with provided Update Handler class</summary>
        public async Task ReceiveAsync(CancellationToken stoppingToken)
        {
            // ToDo: we can inject ReceiverOptions through IOptions container
            var receiverOptions = new ReceiverOptions() { DropPendingUpdates = true, AllowedUpdates = [] };

            var me = await botClient.GetMe(stoppingToken);
            logger.LogInformation("Start receiving updates for {BotName}", me.Username ?? "My Awesome Bot");

            // Start receiving updates
            await botClient.ReceiveAsync(updateHandler, receiverOptions, stoppingToken);
        }
    }
}
