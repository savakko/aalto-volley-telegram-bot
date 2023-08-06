using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace aalto_volley_bot.src.Controllers
{
    static class ControllerUtils
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

        public static async Task RespondToPrivateChatAsync(Func<long, Task> respondToChat, Message message, ITelegramBotClient botClient, CancellationToken cancellationToken)
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

        public static async Task<(bool isValid, string queryData, Dictionary<string, string> queryParams)> ValidateAndParseCallbackQueryAsync(
            CallbackQuery query,
            ITelegramBotClient botClient,
            CancellationToken cancellationToken)
        {
            if (query.Data == null)
            {
                await TryActionAsync(botClient.AnswerCallbackQueryAsync(
                    callbackQueryId: query.Id,
                    text: $"Unable to perform operation, because called CallbackQuery contained no data",
                    cancellationToken: cancellationToken));
                return (false, string.Empty, new Dictionary<string, string>());
            }

            var queryData = query.Data;
            var queryParams = ParseQueryParams(queryData);

            return (true, queryData, queryParams);
        }

        public static async Task<bool> TryActionAsync(Task action)
        {
            try
            {
                await action;
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static Dictionary<string, string> ParseQueryParams(string queryData)
        {
            var result = new Dictionary<string, string>();
            var queryParams = queryData.Split('?').Last().Split('&');

            foreach (var query in queryParams)
            {
                var partition = query.Split('=');
                if (partition.Length == 2)
                    result.Add(partition[0], partition[1]);
            }

            return result;
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
