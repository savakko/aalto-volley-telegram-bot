using aalto_volley_bot.Services;
using aalto_volley_bot.src.WebApps;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

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

        public async Task SendManagerMenuAsync(CallbackQuery query, ITelegramBotClient botClient, CancellationToken cancellationToken)
        {
            await ControllerUtils.TryActionAsync(botClient.AnswerCallbackQueryAsync(
                callbackQueryId: query.Id,
                text: "Getting manager data...",
                cancellationToken: cancellationToken));

            await botClient.SendTextMessageAsync(
                chatId: query.Message != null ? query.Message.Chat.Id : query.From.Id,
                text: "Access the manager pages (requires manager privileges)",
                parseMode: ParseMode.Markdown,
                replyMarkup: _nimenhuutoWebApps.GetManagerMenu(),
                cancellationToken: cancellationToken);
            return;
        }

        public async Task SendUpcomingEventsMenuAsync(CallbackQuery query, ITelegramBotClient botClient, CancellationToken cancellationToken)
        {
            await ControllerUtils.TryActionAsync(botClient.AnswerCallbackQueryAsync(
                callbackQueryId: query.Id,
                text: "Getting upcoming events...",
                cancellationToken: cancellationToken));

            var events = _nimenhuutoService.ScrapeUpcomingEvents();

            await botClient.SendTextMessageAsync(
                chatId: query.Message != null ? query.Message.Chat.Id : query.From.Id,
                text: "Select event",
                replyMarkup: _nimenhuutoWebApps.BuildEventsColumnMenu(events),
                cancellationToken: cancellationToken);
            return;
        }
    }
}
