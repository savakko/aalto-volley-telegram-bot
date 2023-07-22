﻿using aalto_volley_bot.Services;
using aalto_volley_bot.src.Controllers;
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
        private readonly HbvController _hbvController;

        public UpdateHandler(TelegramBotClient botClient, CancellationToken cancellationToken)
        {
            _hbvController = new HbvController();

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
                case var help when (new[] { "help", "start" }).Contains(help):
                    await botClient.SendTextMessageAsync(
                        chatId: message.Chat.Id,
                        text: "*Current commands:*\n/help | /start\n/hello | /hi\n/hbv\n/song",
                        parseMode: ParseMode.Markdown,
                        cancellationToken: cancellationToken);
                    return;

                case var greeting when (new[] { "hello", "hi" }).Contains(greeting):
                    await botClient.SendTextMessageAsync(
                        chatId: message.Chat.Id,
                        text: $"Hi {message.From?.FirstName} 🤗\nType /help to get started",
                        cancellationToken: cancellationToken);
                    return;

                case "hbv":
                    InlineKeyboardMarkup inlineKeyboard = new(new[]
                    {
                        new []  // First row
                        {
                            InlineKeyboardButton.WithCallbackData(text: "All active events", callbackData: "Hbv:ActiveEvents"),
                            //InlineKeyboardButton.WithCallbackData(text: "Test CallbackQuery", callbackData: "Test:CallbackQuery"),
                        },
                        new []  // Second row
                        {
                            InlineKeyboardButton.WithCallbackData(text: "Latest men's weekly games", callbackData: "Hbv:LatestMensWeekly"),
                            InlineKeyboardButton.WithCallbackData(text: "Latest women's weekly games", callbackData: "Hbv:LatestWomensWeekly"),
                        },
                    });

                    await botClient.SendTextMessageAsync(
                        chatId: message.Chat.Id,
                        text: "What would you like me to check",
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
            JArray events;
            JObject singleEvent;
            string mapping;

            switch (query.Data)
            {
                case "Hbv:ActiveEvents":
                    events = await _hbvController.GetActiveEventsAsync();
                    var temp = events.GroupBy(ev => ev.Value<string>("date"))
                        .Select(group => $"*{group.Key}:*\n" +
                            string.Join("\n", group.Select(ev => $"-{ev["name"]}, (id: {ev["id"]})")));
                    mapping = string.Join("\n\n", temp);

                    await botClient.AnswerCallbackQueryAsync(
                        callbackQueryId: query.Id,
                        text: "Posting the result in chat...",
                        cancellationToken: cancellationToken);

                    await botClient.SendTextMessageAsync(
                        chatId: query.From.Id,
                        text: mapping,
                        parseMode: ParseMode.Markdown,
                        cancellationToken: cancellationToken);
                    return;

                case "Hbv:LatestMensWeekly":
                    singleEvent = await _hbvController.GetLatestEventParticipantsByKeywordAsync("keskarit");

                    if (!singleEvent.HasValues)
                    {
                        await botClient.AnswerCallbackQueryAsync(
                            callbackQueryId: query.Id,
                            text: "No men's weekly games were found",
                            showAlert: true,
                            cancellationToken: cancellationToken);
                        return;
                    }

                    await botClient.AnswerCallbackQueryAsync(
                        callbackQueryId: query.Id,
                        text: "Posting the result in chat...",
                        cancellationToken: cancellationToken);

                    mapping =
                        "*" + singleEvent.Value<string>("name") + "*" +
                        "\nId: " + singleEvent.Value<string>("id") +
                        "\nDate: " + singleEvent.Value<string>("date") +
                        "\nParticipants: " + string.Join(
                            ", ",
                            singleEvent.Value<JArray>("participants")
                                .Select(participant => participant.Value<string>("name1")));

                    await botClient.SendTextMessageAsync(
                        chatId: query.From.Id,
                        text: mapping,
                        parseMode: ParseMode.Markdown,
                        cancellationToken: cancellationToken);
                    return;

                case "Hbv:LatestWomensWeekly":
                    singleEvent = await _hbvController.GetLatestEventParticipantsByKeywordAsync("tirsat");

                    if (!singleEvent.HasValues)
                    {
                        await botClient.AnswerCallbackQueryAsync(
                            callbackQueryId: query.Id,
                            text: "No women's weekly games were found",
                            showAlert: true,
                            cancellationToken: cancellationToken);
                        return;
                    }

                    await botClient.AnswerCallbackQueryAsync(
                        callbackQueryId: query.Id,
                        text: "Posting the result in chat...",
                        cancellationToken: cancellationToken);

                    mapping =
                        "*" + singleEvent.Value<string>("name") + "*" +
                        "\nId: " + singleEvent.Value<string>("id") +
                        "\nDate: " + singleEvent.Value<string>("date") +
                        "\nParticipants: " + string.Join(
                            ", ",
                            singleEvent.Value<JArray>("participants")
                                .Select(participant => participant.Value<string>("name1")));

                    await botClient.SendTextMessageAsync(
                        chatId: query.From.Id,
                        text: mapping,
                        parseMode: ParseMode.Markdown,
                        cancellationToken: cancellationToken);
                    return;

                case "Test:CallbackQuery":
                    await botClient.AnswerCallbackQueryAsync(
                        callbackQueryId: query.Id,
                        text: "Testing...",
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
