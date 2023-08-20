using Newtonsoft.Json.Linq;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InlineQueryResults;
using Telegram.Bot.Types.ReplyMarkups;

namespace Telegram.Bot.Services;

public class UpdateHandlers
{
    private readonly ITelegramBotClient _botClient;
    private readonly ILogger<UpdateHandlers> _logger;

    public UpdateHandlers(ITelegramBotClient botClient, ILogger<UpdateHandlers> logger)
    {
        _botClient = botClient;
        _logger = logger;
    }

#pragma warning disable IDE0060 // Remove unused parameter
#pragma warning disable RCS1163 // Unused parameter.
    public Task HandleErrorAsync(Exception exception, CancellationToken cancellationToken)
#pragma warning restore RCS1163 // Unused parameter.
#pragma warning restore IDE0060 // Remove unused parameter
    {
        var ErrorMessage = exception switch
        {
            ApiRequestException apiRequestException => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
            _                                       => exception.ToString()
        };

        _logger.LogInformation("HandleError: {ErrorMessage}", ErrorMessage);
        return Task.CompletedTask;
    }

    public async Task HandleUpdateAsync(Update update, CancellationToken cancellationToken)
    {
        var handler = update switch
        {
            // UpdateType.Unknown:
            // UpdateType.ChannelPost:
            // UpdateType.EditedChannelPost:
            // UpdateType.ShippingQuery:
            // UpdateType.PreCheckoutQuery:
            // UpdateType.Poll:
            { Message: { } message }                       => BotOnMessageReceived(message, cancellationToken),
            { EditedMessage: { } message }                 => BotOnMessageReceived(message, cancellationToken),
            { CallbackQuery: { } callbackQuery }           => BotOnCallbackQueryReceived(callbackQuery, cancellationToken),
            { InlineQuery: { } inlineQuery }               => BotOnInlineQueryReceived(inlineQuery, cancellationToken),
            { ChosenInlineResult: { } chosenInlineResult } => BotOnChosenInlineResultReceived(chosenInlineResult, cancellationToken),
            _                                              => UnknownUpdateHandlerAsync(update, cancellationToken)
        };

        await handler;
    }

    private async Task BotOnMessageReceived(Message message, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Receive message type: {MessageType}", message.Type);
        if (message.Text is not { } messageText)
            return;

        var action = messageText.Split(' ')[0] switch
        {
            "/start" => SendInfoMessage(_botClient, message, cancellationToken),
            "/help" => SendInfoMessage(_botClient, message, cancellationToken),
            "/nimenhuuto" => SendNimenhuutoMainMenu(_botClient, message, cancellationToken),
            "/hbv" => SendHbvMainMenu(_botClient, message, cancellationToken),
            "/unpin" => RemoveKeyboard(_botClient, message, cancellationToken),
            _ => Usage(_botClient, message, cancellationToken)
        };
        Message sentMessage = await action;
        _logger.LogInformation("The message was sent with id: {SentMessageId}", sentMessage.MessageId);


        static async Task<Message> Usage(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
        {
            await botClient.SendChatActionAsync(
                chatId: message.Chat.Id,
                chatAction: ChatAction.Typing,
                cancellationToken: cancellationToken);

            var commands = await botClient.GetMyCommandsAsync(cancellationToken: cancellationToken);
            var response = "*Available commands:*\n" +
                string.Join("\n", commands.Select(command => $"/{command.Command} - {command.Description}"));

            return await botClient.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: response,
                parseMode: ParseMode.Markdown,
                cancellationToken: cancellationToken);
        }

        static async Task<Message> SendInfoMessage(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
        {
            await botClient.SendChatActionAsync(
                chatId: message.Chat.Id,
                chatAction: ChatAction.Typing,
                cancellationToken: cancellationToken);

            var commands = await botClient.GetMyCommandsAsync(cancellationToken: cancellationToken);
            var response = "Hello! 👋\n" +
                "I'm Aalto-Volley-Bot, and I aim to provide you with helpful info and tools about all things Aalto-Volley.\n\n" +
                "*Available commands:*\n" +
                string.Join("\n", commands.Select(command => $"/{command.Command} - {command.Description}")) +
                "\n\nFor any questions, comments, or bug reports, you may contact @Saulikaiseri. " +
                "If you want to become a contributor, you can find a link to the source code below.\n\n" +
                "*Notes:*\n" +
                "_-The current deployment in Google Cloud is left idle upon POST requests, meaning the bot " +
                "won't awaken automatically on new messages. You can awaken the bot manually by clicking the link below._";

            var markup = new InlineKeyboardMarkup(
                new[]
                {
                    new[]
                    {
                        InlineKeyboardButton.WithUrl(text: "Contribute", url: "https://github.com/savakko/aalto-volley-telegram-bot"),
                        InlineKeyboardButton.WithUrl(text: "Awaken the bot", url: "https://aalto-volley-bot-vhybjebhyq-lz.a.run.app"),
                    },
                });

            return await botClient.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: response,
                parseMode: ParseMode.Markdown,
                replyMarkup: markup,
                cancellationToken: cancellationToken);
        }

        static async Task<Message> RemoveKeyboard(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
        {
            await botClient.SendChatActionAsync(
                chatId: message.Chat.Id,
                chatAction: ChatAction.Typing,
                cancellationToken: cancellationToken);

            return await botClient.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: "Unpinned your keyboard!",
                replyMarkup: new ReplyKeyboardRemove(),
                cancellationToken: cancellationToken);
        }

        static async Task<Message> SendNimenhuutoMainMenu(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
        {
            await botClient.SendChatActionAsync(
                chatId: message.Chat.Id,
                chatAction: ChatAction.Typing,
                cancellationToken: cancellationToken);

            return await botClient.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: "Access Nimenhuuto directly in Telegram",
                replyMarkup: Nimenhuuto.GetMainMenuMarkup(),
                cancellationToken: cancellationToken);
        }

        static async Task<Message> SendHbvMainMenu(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
        {
            await botClient.SendChatActionAsync(
                chatId: message.Chat.Id,
                chatAction: ChatAction.Typing,
                cancellationToken: cancellationToken);

            return await botClient.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: "Get info about HBV events",
                replyMarkup: Hbv.GetMainMenuMarkup(),
                cancellationToken: cancellationToken);
        }
    }

    private async Task BotOnCallbackQueryReceived(CallbackQuery callbackQuery, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Received inline keyboard callback from: {CallbackQueryId}", callbackQuery.Id);
        if (callbackQuery.Data is not { } queryData)
            return;
        
        var action = queryData.Split('?')[0] switch
        {
            "General:Unpin" => UnpinReplyMarkup(_botClient, callbackQuery, cancellationToken),
            "Nimenhuuto:Main" => SwitchToNimenhuutoMainMenu(_botClient, callbackQuery, cancellationToken),
            "Nimenhuuto:Manager" => SwitchToNimenhuutoManagerMenu(_botClient, callbackQuery, cancellationToken),
            "Nimenhuuto:Pin" => PinNimenhuutoMenu(_botClient, callbackQuery, cancellationToken),
            "Hbv:Main" => SwitchToHbvMainMenu(_botClient, callbackQuery, cancellationToken),
            "Hbv:ActiveEvents" => SendHbvActiveEvents(_botClient, callbackQuery, cancellationToken),
            "Hbv:Tirsat" => SwitchToHbvWeeklyGamesMenu(_botClient, callbackQuery, cancellationToken),
            "Hbv:Keskarit" => SwitchToHbvWeeklyGamesMenu(_botClient, callbackQuery, cancellationToken),
            "Hbv:Tirsat-Specific" => SwitchToSpecificHbvWeeklyGamesMenu(_botClient, callbackQuery, cancellationToken),
            "Hbv:Keskarit-Specific" => SwitchToSpecificHbvWeeklyGamesMenu(_botClient, callbackQuery, cancellationToken),
            "Hbv:Keskarit-Pools" => SendUpcomingWeeklyGameGroups(_botClient, callbackQuery, cancellationToken),
            _ => NotImplemented(_botClient, callbackQuery, cancellationToken)
        };
        await action;
        //_logger.LogInformation("The message was sent with id: {SentMessageId}", sentMessage.MessageId);


        static async Task NotImplemented(ITelegramBotClient botClient, CallbackQuery callbackQuery, CancellationToken cancellationToken)
        {
            await Utils.TryActionAsync(botClient.AnswerCallbackQueryAsync(
                callbackQueryId: callbackQuery.Id,
                text: $"Response to query '{callbackQuery.Data}' has not been implemented",
                showAlert: true,
                cancellationToken: cancellationToken));
        }

        static async Task UnpinReplyMarkup(ITelegramBotClient botClient, CallbackQuery callbackQuery, CancellationToken cancellationToken)
        {
            if (callbackQuery.Message is not { } message)
                return;

            await Utils.TryActionAsync(botClient.AnswerCallbackQueryAsync(
                callbackQueryId: callbackQuery.Id,
                text: $"Unpinning your current keyboard",
                cancellationToken: cancellationToken));

            await botClient.SendTextMessageAsync(
                message.Chat.Id,
                text: "Unpinned your keyboard!",
                replyMarkup: new ReplyKeyboardRemove(),
                cancellationToken: cancellationToken);
        }

        static async Task SwitchToNimenhuutoMainMenu(ITelegramBotClient botClient, CallbackQuery callbackQuery, CancellationToken cancellationToken)
        {
            if (callbackQuery.Message is not { } message)
                return;

            await Utils.TryActionAsync(botClient.AnswerCallbackQueryAsync(
                callbackQueryId: callbackQuery.Id,
                text: "Getting nimenhuuto menu",
                cancellationToken: cancellationToken));

            await botClient.EditMessageTextAsync(
                chatId: message.Chat.Id,
                messageId: message.MessageId,
                text: "Access Nimenhuuto directly in Telegram",
                replyMarkup: Nimenhuuto.GetMainMenuMarkup(),
                cancellationToken: cancellationToken);
        }

        static async Task SwitchToNimenhuutoManagerMenu(ITelegramBotClient botClient, CallbackQuery callbackQuery, CancellationToken cancellationToken)
        {
            if (callbackQuery.Message is not { } message)
                return;

            await Utils.TryActionAsync(botClient.AnswerCallbackQueryAsync(
                callbackQueryId: callbackQuery.Id,
                text: "Getting nimenhuuto manager options",
                cancellationToken: cancellationToken));

            await botClient.EditMessageTextAsync(
                chatId: message.Chat.Id,
                messageId: message.MessageId,
                text: "Access the manager pages (requires manager privileges)",
                replyMarkup: Nimenhuuto.GetManagerMenuMarkup(),
                cancellationToken: cancellationToken);
        }

        static async Task PinNimenhuutoMenu(ITelegramBotClient botClient, CallbackQuery callbackQuery, CancellationToken cancellationToken)
        {
            if (callbackQuery.Message is not { } message)
                return;

            await Utils.TryActionAsync(botClient.AnswerCallbackQueryAsync(
                callbackQueryId: callbackQuery.Id,
                text: "Pinning Nimenhuuto to your keyboard",
                cancellationToken: cancellationToken));

            await botClient.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: "Pinned upcoming events to your keyboard.\nType /unpin to remove it.",
                replyMarkup: Nimenhuuto.GetReplyKeyboardMarkup(),
                cancellationToken: cancellationToken);
        }

        static async Task SwitchToHbvMainMenu(ITelegramBotClient botClient, CallbackQuery callbackQuery, CancellationToken cancellationToken)
        {
            if (callbackQuery.Message is not { } message)
                return;

            await Utils.TryActionAsync(botClient.AnswerCallbackQueryAsync(
                callbackQueryId: callbackQuery.Id,
                text: "Getting Hbv menu",
                cancellationToken: cancellationToken));

            await botClient.EditMessageTextAsync(
                chatId: message.Chat.Id,
                messageId: message.MessageId,
                text: "Get info about HBV events",
                replyMarkup: Hbv.GetMainMenuMarkup(),
                cancellationToken: cancellationToken);
        }

        static async Task SendHbvActiveEvents(ITelegramBotClient botClient, CallbackQuery callbackQuery, CancellationToken cancellationToken)
        {
            if (callbackQuery.Message is not { } message)
                return;

            await Utils.TryActionAsync(botClient.AnswerCallbackQueryAsync(
                callbackQueryId: callbackQuery.Id,
                text: "Getting active events",
                cancellationToken: cancellationToken));

            var events = await Hbv.GetActiveEventsAsync();
            var response = string.Join("\n\n", events.GroupBy(ev => ev.Value<string>("date"))
                .Select(group => "*" + group.Key + ":*\n" + string.Join("\n", group.Select(ev => $"-{ev.Value<string>("name")}"))));

            await botClient.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: response,
                parseMode: ParseMode.Markdown,
                replyMarkup: Hbv.GetActiveEventsMenuMarkup(),
                cancellationToken: cancellationToken);
        }

        static async Task SwitchToHbvWeeklyGamesMenu(ITelegramBotClient botClient, CallbackQuery callbackQuery, CancellationToken cancellationToken)
        {
            if (callbackQuery.Message is not { } message)
                return;
            if (callbackQuery.Data is not { } data)
                return;

            var serie = data.Split(':').Last();

            await Utils.TryActionAsync(botClient.AnswerCallbackQueryAsync(
                callbackQueryId: callbackQuery.Id,
                text: $"Getting menu options for {serie}",
                cancellationToken: cancellationToken));

            var weeklyGames = await Hbv.GetWeeklyGamesBySerieAndYearAsync(serie: serie, year: DateTime.Now.Year.ToString());

            await botClient.EditMessageTextAsync(
                chatId: message.Chat.Id,
                messageId: message.MessageId,
                text: $"{serie} menu",
                replyMarkup: Hbv.GetWeeklyGamesMenuMarkup(serie, weeklyGames),
                cancellationToken: cancellationToken);
        }

        static async Task SwitchToSpecificHbvWeeklyGamesMenu(ITelegramBotClient botClient, CallbackQuery callbackQuery, CancellationToken cancellationToken)
        {
            if (callbackQuery.Message is not { } message)
                return;

            var (path, queryParams) = Utils.ParseCallbackQuery(callbackQuery.Data);
            var serie = path.Split(new[] { ':', '-' })[1];
            var weeklyGameId = queryParams.GetValueOrDefault("id", "");

            if (string.IsNullOrEmpty(weeklyGameId))
            {
                await Utils.TryActionAsync(botClient.AnswerCallbackQueryAsync(
                    callbackQueryId: callbackQuery.Id,
                    text: $"No upcoming {serie}",
                    showAlert: true,
                    cancellationToken: cancellationToken));
                return;
            }

            await Utils.TryActionAsync(botClient.AnswerCallbackQueryAsync(
                callbackQueryId: callbackQuery.Id,
                text: $"Getting menu options for upcoming {serie}",
                cancellationToken: cancellationToken));

            var weeklyGame = await Hbv.GetWeeklyGameByIdAsync(weeklyGameId);
            var eventId = weeklyGame.Value<string>("event_id");
            var link = weeklyGame.Value<string>("event_link");

            if (string.IsNullOrEmpty(eventId) || string.IsNullOrEmpty(link))
            {
                await Utils.TryActionAsync(botClient.AnswerCallbackQueryAsync(
                    callbackQueryId: callbackQuery.Id,
                    text: $"Something went wrong getting event for query {callbackQuery.Data}",
                    showAlert: true,
                    cancellationToken: cancellationToken));
                return;
            }

            var groups = await Hbv.GetGroupsByEventIdAsync(eventId);

            await botClient.EditMessageTextAsync(
                chatId: message.Chat.Id,
                messageId: message.MessageId,
                text: $"Upcoming {serie}",
                replyMarkup: Hbv.GetSpecificWeeklyGameMenuMarkup(serie, weeklyGame, groups),
                cancellationToken: cancellationToken);
        }

        static async Task SendUpcomingWeeklyGameGroups(ITelegramBotClient botClient, CallbackQuery callbackQuery, CancellationToken cancellationToken)
        {
            if (callbackQuery.Message is not { } message)
                return;

            var (path, queryParams) = Utils.ParseCallbackQuery(callbackQuery.Data);
            var serie = path.Split(new[] { ':', '-' })[1];
            var groupId = queryParams.GetValueOrDefault("groups", "");

            if (string.IsNullOrEmpty(groupId))
            {
                await Utils.TryActionAsync(botClient.AnswerCallbackQueryAsync(
                    callbackQueryId: callbackQuery.Id,
                    text: $"Malformatted query {callbackQuery.Data}",
                    showAlert: true,
                    cancellationToken: cancellationToken));
                return;
            }

            await botClient.SendChatActionAsync(
                message.Chat.Id,
                chatAction: ChatAction.Typing,
                cancellationToken: cancellationToken);
            await Utils.TryActionAsync(botClient.AnswerCallbackQueryAsync(
                callbackQueryId: callbackQuery.Id,
                text: $"Calculating groups upcoming {serie}",
                cancellationToken: cancellationToken));

            var participants = await Hbv.GetParticipantsByGroupIdAsync(groupId);
            var rankings = await Hbv.GetWeeklyGameRankingsBySerieAndYearAsync(serie: serie.ToLower(), year: DateTime.Now.Year.ToString());
            var joined = participants.Join(
                rankings,
                participant => participant.Value<string>("member1_id"),
                ranking => ranking.Value<string>("id"),
                (participant, ranking) => new JObject()
                {
                    { "Id", participant["member1_id"]},
                    { "Name", participant["name1"] },
                    {
                        "Ranking",
                        ranking.Value<string>("allpoints")
                            .Split(',')
                            .TakeLast(2)
                            .Select(value => double.Parse(value.Replace('.', ',')))
                            .Average()
                    },
                });
            var ordered = joined.OrderByDescending(player => player.Value<double>("Ranking")).ToArray();

            var length = ordered.Length;
            var i = 0;
            while (i < length)
            {
                var player = ordered[i];
                var position = i + 1;
                var groupNumber = i / 4 + 1;
                var groupPosition = i % 4 + 1;
                //var basePoints  = 14.75 - 0.5 * group;  // 14.75 == 15.5 - 0.5 * (1 - sijoituksen jakauma)
                var basePoints = 15.5 - 0.5 * (groupPosition - 1) - 0.5 * groupNumber;
                var bonusPoints = 0.25 * (50 - Math.Min(groupNumber - 1, 7) * 2);  // 0.25 == omat pisteet / kaikki pisteet odotusarvo
                var totalPoints = basePoints + bonusPoints;
                var difference = totalPoints - player.Value<double>("Ranking");

                player.Add("Position", position);
                player.Add("Group", groupNumber);
                player.Add("GroupPosition", groupPosition);
                player.Add("BasePoints", basePoints);
                player.Add("BonusPoints", bonusPoints);
                player.Add("TotalPoints", totalPoints);
                player.Add("Difference", difference);
                i++;
            }

            var groups = ordered.GroupBy(participant => participant.Value<int>("Group"));
            var groupMappings = groups.Select(group =>
                $"*Group {group.Key}:*\n" +
                string.Join("\n", group.Select(participant =>
                    $"{participant.Value<int>("Position")}. " +
                    $"{participant.Value<string>("Name")}: " +
                    $"{participant.Value<double>("Ranking"):0.##} vs. " +
                    $"{participant.Value<double>("TotalPoints"):0.##} = " +
                    $"{participant.Value<double>("Difference"):+0.##;-0.##}")));

            var response = "";
            length = groupMappings.Count();
            i = 0;
            while (i < length)
            {
                response += groupMappings.ElementAt(i) + "\n\n";

                // Send message for every 10 groups
                if ((i + 1) % 10 == 0 || i == length - 1)
                {
                    await botClient.SendTextMessageAsync(
                        chatId: message.Chat.Id,
                        text: response,
                        parseMode: ParseMode.Markdown,
                        cancellationToken: cancellationToken);
                    response = "";
                }

                i++;
            }
        }
    }

    #region Inline Mode

    private async Task BotOnInlineQueryReceived(InlineQuery inlineQuery, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Received inline query from: {InlineQueryFromId}", inlineQuery.From.Id);

        InlineQueryResult[] results = {
            // displayed result
            new InlineQueryResultArticle(
                id: "1",
                title: "TgBots",
                inputMessageContent: new InputTextMessageContent("hello"))
        };

        await _botClient.AnswerInlineQueryAsync(
            inlineQueryId: inlineQuery.Id,
            results: results,
            cacheTime: 0,
            isPersonal: true,
            cancellationToken: cancellationToken);
    }

    private async Task BotOnChosenInlineResultReceived(ChosenInlineResult chosenInlineResult, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Received inline result: {ChosenInlineResultId}", chosenInlineResult.ResultId);

        await _botClient.SendTextMessageAsync(
            chatId: chosenInlineResult.From.Id,
            text: $"You chose result with Id: {chosenInlineResult.ResultId}",
            cancellationToken: cancellationToken);
    }

    #endregion

#pragma warning disable IDE0060 // Remove unused parameter
#pragma warning disable RCS1163 // Unused parameter.
    private Task UnknownUpdateHandlerAsync(Update update, CancellationToken cancellationToken)
#pragma warning restore RCS1163 // Unused parameter.
#pragma warning restore IDE0060 // Remove unused parameter
    {
        _logger.LogInformation("Unknown update type: {UpdateType}", update.Type);
        return Task.CompletedTask;
    }
}
