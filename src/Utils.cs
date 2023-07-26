using Microsoft.Extensions.FileProviders;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace aalto_volley_bot.src
{
    static class Utils
    {
        public static async Task SendDefaultMessageAsync(Message message, ITelegramBotClient botClient, CancellationToken cancellationToken)
        {
            await botClient.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: "Sorry, don't know what you mean 😕\nTry typing /help",
                replyToMessageId: message.MessageId,
                cancellationToken: cancellationToken);
            return;
        }

        public static async Task SendHelpTextAsync(Message message, ITelegramBotClient botClient, CancellationToken cancellationToken)
        {
            await botClient.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: ReadFileContent("../../../files/helpmessage.txt"),  // TODO: move help message inside a database
                parseMode: ParseMode.Html,
                replyToMessageId: message.MessageId,
                cancellationToken: cancellationToken);
            return;
        }

        public static async Task RespondToPrivateChatAsync(Message message, ITelegramBotClient botClient, CancellationToken cancellationToken, Func<long, Task> respondToChat)
        {
            if (message.From == null)
            {
                await botClient.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: "Message sender was not found, try sending me a direct message",
                    cancellationToken: cancellationToken);
                return;
            }

            if (message.Chat.Type != ChatType.Private)
            {
                await botClient.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: "Going to answer that in your private chat",
                    replyToMessageId: message.MessageId,
                    cancellationToken: cancellationToken);
            }

            await respondToChat(message.From.Id);
            return;
        }

        public static async Task SendDefaultCallBackQueryAsync(CallbackQuery query, ITelegramBotClient botClient, CancellationToken cancellationToken)
        {
            await botClient.AnswerCallbackQueryAsync(
                callbackQueryId: query.Id,
                text: $"Response to query '{query.Data}' has not been implemented",
                showAlert: true,
                cancellationToken: cancellationToken);
            return;
        }

        private static string ReadFileContent(string path)
        {
            try
            {
                return System.IO.File.ReadAllText(path).Trim();
            }
            catch
            {
                return "Could not read file content at this time";
            }
        }
    }
}
