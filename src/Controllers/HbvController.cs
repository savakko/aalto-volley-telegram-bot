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
            await ControllerUtils.RespondToPrivateChatAsync(message, botClient, cancellationToken,
                respondToChat: (chatId) => botClient.SendTextMessageAsync(
                    chatId: chatId,
                    text: "Get info about HBV events",
                    replyMarkup: GetMainMenu(),
                    cancellationToken: cancellationToken));
            return;
        }

        public async Task SendActiveEventsAsync(CallbackQuery query, ITelegramBotClient botClient, CancellationToken cancellationToken)
        {
            await botClient.AnswerCallbackQueryAsync(
                callbackQueryId: query.Id,
                text: "Fetching active events...",
                cancellationToken: cancellationToken);

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
                    { InlineKeyboardButton.WithUrl(
                        text: "Open in browser",
                        url: "https://prod.hbv.fi/tapahtumat/#/"),
                    }),
                cancellationToken: cancellationToken);
            return;
        }

        public async Task SendWeeklyGamesMenuAsync(CallbackQuery query, ITelegramBotClient botClient, CancellationToken cancellationToken)
        {
            if (query.Data == null)
            {
                await botClient.AnswerCallbackQueryAsync(
                    callbackQueryId: query.Id,
                    text: $"Unable to perform operation, because called CallbackQuery contained no data",
                    cancellationToken: cancellationToken);
                return;
            }

            var serie = query.Data.Split(':').Last();

            await botClient.AnswerCallbackQueryAsync(
                callbackQueryId: query.Id,
                text: $"Building menu for {serie}",
                cancellationToken: cancellationToken);

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
            if (query.Data == null)
            {
                await botClient.AnswerCallbackQueryAsync(
                    callbackQueryId: query.Id,
                    text: $"Unable to perform operation, because called CallbackQuery contained no data",
                    cancellationToken: cancellationToken);
                return;
            }

            var weeklyGameId = ControllerUtils.ParseQueryParams(query.Data)["id"];
            var serie = query.Data.Split(new[] { ':', '-' })[1];

            if (string.IsNullOrEmpty(weeklyGameId))
            {
                await botClient.AnswerCallbackQueryAsync(
                    callbackQueryId: query.Id,
                    text: $"No active {serie}",
                    showAlert: true,
                    cancellationToken: cancellationToken);
                return;
            }

            await botClient.AnswerCallbackQueryAsync(
                callbackQueryId: query.Id,
                text: $"Getting active {serie}",
                cancellationToken: cancellationToken);

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

        public async Task SendWeeklyGamesAsync(CallbackQuery query, ITelegramBotClient botClient, CancellationToken cancellationToken)
        {
            if (query.Data == null)
            {
                await botClient.AnswerCallbackQueryAsync(
                    callbackQueryId: query.Id,
                    text: $"Unable to perform operation, because called CallbackQuery contained no data",
                    cancellationToken: cancellationToken);
                return;
            }

            var queryData = query.Data.Split(':').Last().Split('|');
            var serie = queryData[0];  // Required!
            var season = queryData.Length > 1    // Defaults to current year if not given
                ? queryData[1]
                : DateTime.Now.Year.ToString();

            var events = await _hbvService.GetWeeklyGamesBySerieAndYearAsync(serie: serie.ToLower(), year: season);

            if (!events.Any())
            {
                await botClient.AnswerCallbackQueryAsync(
                    callbackQueryId: query.Id,
                    text: $"No {serie} events were found for {season}",
                    showAlert: true,
                    cancellationToken: cancellationToken);
                return;
            }

            await botClient.AnswerCallbackQueryAsync(
                callbackQueryId: query.Id,
                text: $"Getting all {serie} events for season {season}...",
                cancellationToken: cancellationToken);

            await botClient.SendTextMessageAsync(
                chatId: query.From.Id,
                text: $"{serie} season {season}",
                parseMode: ParseMode.Markdown,
                replyMarkup: new InlineKeyboardMarkup(events
                    .OrderByDescending(ev => ev.Value<string>("date"))
                    .Select(ev => new[] { InlineKeyboardButton.WithUrl(
                        text: serie + " " +
                            DateTime.Parse(
                                ev.Value<string>("date"),
                                CultureInfo.InvariantCulture)
                            .ToString("dd.MM"),
                        url: ev.Value<string>("event_link"))
                    })),
                cancellationToken: cancellationToken);
            return;
        }

        private async Task<JArray> GetActiveEventsByKeywordAsync(string keyword)
        {
            var events = await _hbvService.GetActiveEventsAsync();
            var result = events.Where(ev => ev.Value<string>("name").ToLower().Contains(keyword));

            return JArray.FromObject(result);
        }

        private async Task<JArray> GetAllEventsByKeywordAsync(string keyword)
        {
            var events = await _hbvService.GetAllEventsAsync();
            var result = events.Where(ev => ev.Value<string>("name").ToLower().Contains(keyword));

            return JArray.FromObject(result);
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
