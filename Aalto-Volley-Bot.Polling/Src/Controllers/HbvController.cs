using aalto_volley_bot.Services;
using Newtonsoft.Json.Linq;
using System.Globalization;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace aalto_volley_bot.src.Controllers
{
    internal class HbvController
    {
        private readonly HbvService _hbvService = new();

        public async Task SendMainMenuAsync(Message message, ITelegramBotClient botClient, CancellationToken cancellationToken)
        {
            await ControllerUtils.RespondToPrivateChatAsync(
                respondToChat: (chatId) => botClient.SendTextMessageAsync(
                    chatId: chatId,
                    text: "Get info about HBV events",
                    replyMarkup: GetMainMenu(),
                    cancellationToken: cancellationToken),
                message, botClient, cancellationToken);
            return;
        }

        public async Task SendActiveEventsAsync(CallbackQuery query, ITelegramBotClient botClient, CancellationToken cancellationToken)
        {
            var (isValid, queryData, queryParams) = await ControllerUtils.ValidateAndParseCallbackQueryAsync(query, botClient, cancellationToken);
            if (!isValid)
                return;

            await ControllerUtils.TryActionAsync(botClient.AnswerCallbackQueryAsync(
                callbackQueryId: query.Id,
                text: "Fetching active events",
                cancellationToken: cancellationToken));

            var events = await _hbvService.GetActiveEventsAsync();
            var mapping = string.Join("\n\n", events.GroupBy(ev => ev.Value<string>("date"))
                .Select(group =>
                    "*" + group.Key + ":*\n" +
                    string.Join("\n", group.Select(ev =>
                        $"-{ev.Value<string>("name")}"))));

            await botClient.SendTextMessageAsync(
                chatId: query.From.Id,
                text: mapping,
                parseMode: ParseMode.Markdown,
                replyMarkup: new InlineKeyboardMarkup(new[]
                {
                    InlineKeyboardButton.WithUrl(
                        text: "Open in browser",
                        url: "https://prod.hbv.fi/tapahtumat/#/"),
                }),
                cancellationToken: cancellationToken);
            return;
        }

        public async Task SendWeeklyGamesMenuAsync(CallbackQuery query, ITelegramBotClient botClient, CancellationToken cancellationToken)
        {
            var (isValid, queryData, queryParams) = await ControllerUtils.ValidateAndParseCallbackQueryAsync(query, botClient, cancellationToken);
            if (!isValid)
                return;

            var serie = queryData.Split(':').Last();

            await ControllerUtils.TryActionAsync(botClient.AnswerCallbackQueryAsync(
                callbackQueryId: query.Id,
                text: $"Building {serie} menu",
                cancellationToken: cancellationToken));

            var weeklygames = await _hbvService.GetWeeklyGamesBySerieAndYearAsync(serie: serie, year: DateTime.Now.Year.ToString());
            var active = weeklygames.Where(game => game.Value<int>("status") < 5).FirstOrDefault(new JObject());

            await botClient.SendTextMessageAsync(
                chatId: query.From.Id,
                text: $"{serie} menu",
                replyMarkup: new InlineKeyboardMarkup(new[]
                {
                    new[]
                    {
                        InlineKeyboardButton.WithCallbackData(
                            text: "Upcoming: " + (active.Any() ? $"{serie} {DateTime.Parse(active.Value<string>("date")):dd.MM}" : "None"),
                            callbackData: $"Hbv:{serie}-Specific?id={active.Value<string>("id")}"),
                    },
                    new[]
                    {
                        InlineKeyboardButton.WithUrl(
                            text: "Rankings & Results",
                            url: $"https://prod.hbv.fi/app/#/weekgames/{serie.ToLower()}"),
                    }
                }),
                cancellationToken: cancellationToken);
        }

        public async Task SendSpecificWeeklyGameMenuAsync(CallbackQuery query, ITelegramBotClient botClient, CancellationToken cancellationToken)
        {
            var (isValid, queryData, queryParams) = await ControllerUtils.ValidateAndParseCallbackQueryAsync(query, botClient, cancellationToken);
            if (!isValid)
                return;

            var weeklyGameId = queryParams["id"];
            var serie = queryData.Split(new[] { ':', '-' })[1];

            if (string.IsNullOrEmpty(weeklyGameId))
            {
                await ControllerUtils.TryActionAsync(botClient.AnswerCallbackQueryAsync(
                    callbackQueryId: query.Id,
                    text: $"No active {serie}",
                    cancellationToken: cancellationToken));
                return;
            }

            await ControllerUtils.TryActionAsync(botClient.AnswerCallbackQueryAsync(
                callbackQueryId: query.Id,
                text: $"Getting active {serie}",
                cancellationToken: cancellationToken));

            var weeklyGame = await _hbvService.GetWeeklyGameByIdAsync(weeklyGameId);
            var eventInfo = await _hbvService.GetEventByIdAsync(weeklyGame.Value<string>("event_id"));

            await botClient.SendTextMessageAsync(
                chatId: query.From.Id,
                text: eventInfo.Value<string>("name"),
                replyMarkup: new InlineKeyboardMarkup(new[]
                {
                    new[]
                    {
                        InlineKeyboardButton.WithUrl(
                            text: "Sign up page",
                            url: weeklyGame.Value<string>("event_link")),
                    },
                }),
                cancellationToken: cancellationToken);
        }

        private static IReplyMarkup GetMainMenu()
        {
            return new InlineKeyboardMarkup(new[]
            {
                new []  // First row
                {
                    InlineKeyboardButton.WithCallbackData(text: "All active events", callbackData: "Hbv:ActiveEvents"),
                },
                new []  // Second row
                {
                    InlineKeyboardButton.WithCallbackData(text: "Tirsat", callbackData: "Hbv:Tirsat"),
                    InlineKeyboardButton.WithCallbackData(text: "Keskarit", callbackData: "Hbv:Keskarit"),
                },
            });
        }
    }
}
