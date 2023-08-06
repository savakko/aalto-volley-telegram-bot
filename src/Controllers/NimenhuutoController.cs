using aalto_volley_bot.Services;
using aalto_volley_bot.src.WebApps;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace aalto_volley_bot.src.Controllers
{
    internal class NimenhuutoController
    {
        private readonly NimenhuutoService _nimenhuutoService = new();
        private readonly NimenhuutoWebAppProvider _nimenhuutoWebApps = new();

        public async Task SendMainMenuAsync(Message message, ITelegramBotClient botClient, CancellationToken cancellationToken)
        {
            await ControllerUtils.RespondToPrivateChatAsync(
                respondToChat: (chatId) => botClient.SendTextMessageAsync(
                    chatId: chatId,
                    text: "Access Nimenhuuto directly in Telegram",
                    replyMarkup: _nimenhuutoWebApps.GetMainMenu(),
                    cancellationToken: cancellationToken),
                message, botClient, cancellationToken);
            return;
        }

        public async Task SendMainMenuAsync(CallbackQuery query, ITelegramBotClient botClient, CancellationToken cancellationToken)
        {
            if (query.Message == null)
            {
                await ControllerUtils.TryActionAsync(botClient.AnswerCallbackQueryAsync(
                    callbackQueryId: query.Id,
                    text: "The original message was not found. Try opening the menu again with the command /nimenhuuto",
                    showAlert: true,
                    cancellationToken: cancellationToken));
                return;
            }

            await ControllerUtils.TryActionAsync(botClient.AnswerCallbackQueryAsync(
                callbackQueryId: query.Id,
                text: "Getting Nimenhuuto main menu",
                cancellationToken: cancellationToken));

            await botClient.EditMessageTextAsync(
                chatId: query.Message.Chat.Id,
                messageId: query.Message.MessageId,
                text: "Access Nimenhuuto directly in Telegram",
                replyMarkup: (InlineKeyboardMarkup)_nimenhuutoWebApps.GetMainMenu(),
                cancellationToken: cancellationToken);
            return;
        }

        public async Task SendManagerMenuAsync(CallbackQuery query, ITelegramBotClient botClient, CancellationToken cancellationToken)
        {
            if (query.Message == null)
            {
                await ControllerUtils.TryActionAsync(botClient.AnswerCallbackQueryAsync(
                    callbackQueryId: query.Id,
                    text: "The original message was not found. Try opening the menu again with the command /nimenhuuto",
                    showAlert: true,
                    cancellationToken: cancellationToken));
                return;
            }

            await ControllerUtils.TryActionAsync(botClient.AnswerCallbackQueryAsync(
                callbackQueryId: query.Id,
                text: "Getting manager menu",
                cancellationToken: cancellationToken));

            await botClient.EditMessageTextAsync(
                chatId: query.Message.Chat.Id,
                messageId: query.Message.MessageId,
                text: "Access the manager pages (requires manager privileges)",
                replyMarkup: (InlineKeyboardMarkup)_nimenhuutoWebApps.GetManagerMenu(),
                cancellationToken: cancellationToken);
            return;
        }

        public async Task SendUpcomingEventsMenuAsync(CallbackQuery query, ITelegramBotClient botClient, CancellationToken cancellationToken)
        {
            if (query.Message == null)
            {
                await ControllerUtils.TryActionAsync(botClient.AnswerCallbackQueryAsync(
                    callbackQueryId: query.Id,
                    text: "The original message was not found. Try opening the menu again with the command /nimenhuuto",
                    showAlert: true,
                    cancellationToken: cancellationToken));
                return;
            }

            await ControllerUtils.TryActionAsync(botClient.AnswerCallbackQueryAsync(
                callbackQueryId: query.Id,
                text: "Getting upcoming events...",
                cancellationToken: cancellationToken));

            var events = _nimenhuutoService.ScrapeUpcomingEvents();

            await botClient.EditMessageTextAsync(
                chatId: query.Message.Chat.Id,
                messageId: query.Message.MessageId,
                text: "Select event",
                replyMarkup: (InlineKeyboardMarkup)_nimenhuutoWebApps.BuildEventsColumnMenu(events),
                cancellationToken: cancellationToken);
            return;
        }
    }
}
