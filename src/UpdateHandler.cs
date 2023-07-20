using aalto_volley_bot.Services;
using Newtonsoft.Json.Linq;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace aalto_volley_bot.src
{
    internal class UpdateHandler
    {
        private readonly HbvService hbvService;

        public UpdateHandler(TelegramBotClient botClient, CancellationToken cancellationToken)
        {
            hbvService = new HbvService();

            // StartReceiving does not block the caller thread. Receiving is done on the ThreadPool.
            ReceiverOptions receiverOptions = new()
            {
                AllowedUpdates = new[]
                {
                    UpdateType.Message,
                    UpdateType.CallbackQuery
                }
            };

            botClient.StartReceiving(
                updateHandler: HandleUpdateAsync,
                pollingErrorHandler: HandlePollingErrorAsync,
                receiverOptions: receiverOptions,
                cancellationToken: cancellationToken
            );
        }

        public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            switch (update.Type)
            {
                case UpdateType.Message:
                    if (update.Message is not { } message)
                        return;
                    if (message.Text is not { } messageText)
                        return;

                    Console.WriteLine($"Received {update.Type} with text '{messageText}' from {message.From}");

                    if (!messageText.StartsWith('/'))
                        return;

                    await RouteCommandAsync(message, botClient, cancellationToken);
                    return;

                case UpdateType.CallbackQuery:
                    if (update.CallbackQuery is not { } query)
                        return;

                    Console.WriteLine($"Received {update.Type} with data {query.Data} from {query.From}");

                    await RouteCallBackQueryAsync(query, botClient, cancellationToken);
                    return;

                default:
                    Console.WriteLine($"Received {update.Type} not handled");
                    return;
            }
        }

        public Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            var ErrorMessage = exception switch
            {
                ApiRequestException apiRequestException
                    => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                _ => exception.ToString()
            };

            Console.WriteLine(ErrorMessage);
            return Task.CompletedTask;
        }

        private async Task RouteCommandAsync(Message message, ITelegramBotClient botClient, CancellationToken cancellationToken)
        {
            switch (message.Text?[1..].Trim())
            {
                case "help":  // TODO
                    await botClient.SendTextMessageAsync(
                        chatId: message.Chat.Id,
                        text: "_Help message here_",
                        parseMode: ParseMode.MarkdownV2,
                        cancellationToken: cancellationToken);
                    return;

                case "keskarit":
                    InlineKeyboardMarkup inlineKeyboard = new(new[]
                    {
                        // first row
                        new []
                        {
                            InlineKeyboardButton.WithCallbackData(text: "Next men's weekly games", callbackData: "nextMensWeekly"),
                            InlineKeyboardButton.WithCallbackData(text: "Test CallbackQuery", callbackData: "testCallbackQuery"),
                        },
                        // second row
                        new []
                        {
                            InlineKeyboardButton.WithCallbackData(text: "2.1", callbackData: "21"),
                            InlineKeyboardButton.WithCallbackData(text: "2.2", callbackData: "22"),
                        },
                    });

                    await botClient.SendTextMessageAsync(
                        chatId: message.Chat.Id,
                        text: "A message with an inline keyboard markup",
                        replyMarkup: inlineKeyboard,
                        cancellationToken: cancellationToken);
                    return;

                case "song":  // TODO
                    await botClient.SendTextMessageAsync(
                        chatId: message.Chat.Id,
                        text: "Sure thing!",
                        replyMarkup: new InlineKeyboardMarkup(
                            InlineKeyboardButton.WithUrl(
                                text: "Click here for the song",
                                url: "https://open.spotify.com/track/0ngSk8aGEjWS6fsHIV9KKj?si=bd85cdc7d0f141b5")),
                        parseMode: ParseMode.Html,
                        cancellationToken: cancellationToken);
                    return;

                default:
                    await botClient.SendTextMessageAsync(
                        chatId: message.Chat.Id,
                        text: "Sorry, don't know what you mean 😕\nTry typing /help",
                        disableNotification: true,
                        replyToMessageId: message.MessageId,
                        cancellationToken: cancellationToken);
                    return;
            }
        }

        private async Task RouteCallBackQueryAsync(CallbackQuery query, ITelegramBotClient botClient, CancellationToken cancellationToken)
        {
            switch (query.Data)
            {
                case "nextMensWeekly":
                    var events = await hbvService.GetActiveEventsAsync();
                    var match = events.Where(ob => ob.Value<string>("name").ToLower().Contains("keskarit"));

                    if (!match.Any())
                    {
                        await botClient.AnswerCallbackQueryAsync(
                            callbackQueryId: query.Id,
                            text: "No active men's weekly games were found",
                            showAlert: true,
                            cancellationToken: cancellationToken);
                        return;
                    }

                    await botClient.AnswerCallbackQueryAsync(
                        callbackQueryId: query.Id,
                        text: "Posting the result in the chat...",
                        cancellationToken: cancellationToken);

                    var eventDetails = match.First();
                    var id = eventDetails.Value<string>("id");
                    var date = eventDetails.Value<string>("date");
                    var name = eventDetails.Value<string>("name");

                    await botClient.SendTextMessageAsync(
                        chatId: query.From.Id,
                        text: $"*Next men's weekly games*\nEvent id: {id}\nDate: {date}\nName: {name}",
                        cancellationToken: cancellationToken);
                    return;

                case "testCallbackQuery":
                    await botClient.AnswerCallbackQueryAsync(
                        callbackQueryId: query.Id,
                        text: "testing...",
                        cancellationToken: cancellationToken);
                    return;

                default:
                    await botClient.AnswerCallbackQueryAsync(
                        callbackQueryId: query.Id,
                        text: $"Response to query '{query.Data}' has not been implemented",
                        showAlert: true,
                        cancellationToken: cancellationToken);
                    return;
            }
        }
    }
}
