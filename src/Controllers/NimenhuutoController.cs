using aalto_volley_bot.src.WebApps;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace aalto_volley_bot.src.Controllers
{
    internal class NimenhuutoController
    {
        private readonly NimenhuutoWebAppProvider _nimenhuutoWebApps = new();

        public async Task SendMainMenuAsync(Message message, ITelegramBotClient botClient, CancellationToken cancellationToken)
        {
            await Utils.RespondToPrivateChatAsync(message, botClient, cancellationToken,
                respondToChat: (chatId) => botClient.SendTextMessageAsync(
                    chatId: chatId,
                    text: "Access Nimenhuuto directly in Telegram",
                    replyMarkup: _nimenhuutoWebApps.GetMainMenu(),
                    cancellationToken: cancellationToken));
            return;
        }

        public async Task SendManagerMenuAsync(CallbackQuery query, ITelegramBotClient botClient, CancellationToken cancellationToken)
        {
            await botClient.AnswerCallbackQueryAsync(
                callbackQueryId: query.Id,
                text: "Getting manager data...",
                cancellationToken: cancellationToken);

            // Edit the queried message
            //await botClient.EditMessageReplyMarkupAsync(
            //    chatId: query.Message.Chat.Id,
            //    messageId: query.Message.MessageId,
            //    replyMarkup: (InlineKeyboardMarkup)_nimenhuutoWebApps.GetManagerMenu(),
            //    cancellationToken: cancellationToken);

            //Send a new message below the queried one
            await botClient.SendTextMessageAsync(
                chatId: query.Message != null ? query.Message.Chat.Id : query.From.Id,
                text: "Access the manager pages (requires manager privileges)",
                parseMode: ParseMode.Markdown,
                replyMarkup: _nimenhuutoWebApps.GetManagerMenu(),
                cancellationToken: cancellationToken);
            return;
        }
    }
}
