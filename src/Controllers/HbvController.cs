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
                        $"-{ev.Value<string>("name")}, (id: {ev.Value<string>("id")})"))));

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
