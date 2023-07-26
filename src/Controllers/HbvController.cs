using aalto_volley_bot.Services;
using Newtonsoft.Json.Linq;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace aalto_volley_bot.src.Controllers
{
    internal class HbvController
    {
        private readonly HbvService _hbvService = new();
        private readonly JObject _localMemory = new()  // TODO: Outsource local memory!
        {
            { "CacheTime", 600 }  // Amount of time in seconds that the data is cached
        };

        public async Task SendMainMenuAsync(Message message, ITelegramBotClient botClient, CancellationToken cancellationToken)
        {
            await Utils.RespondToPrivateChatAsync(message, botClient, cancellationToken,
                respondToChat: (chatId) => botClient.SendTextMessageAsync(
                    chatId: chatId,
                    text: "Get info about HBV events",
                    replyMarkup: GetMainMenu(),
                    cancellationToken: cancellationToken));
            return;
        }

        public async Task SendActiveEventsAsync(CallbackQuery query, ITelegramBotClient botClient, CancellationToken cancellationToken)
        {
            // Give the caller an answer before completing the query
            await botClient.AnswerCallbackQueryAsync(
                callbackQueryId: query.Id,
                text: "Fetching active events...",
                cancellationToken: cancellationToken);

            var events = await GetActiveEventsAsync();
            var mapping = string.Join("\n\n", events.GroupBy(ev => ev.Value<string>("date"))
                .Select(group =>
                    "*" + group.Key + ":*\n" +
                    string.Join("\n", group.Select(ev =>
                        $"-{ev.Value<string>("name")}, (id: {ev.Value<string>("id")})"))));

            await botClient.SendTextMessageAsync(
                chatId: query.From.Id,
                text: mapping,
                parseMode: ParseMode.Markdown,
                cancellationToken: cancellationToken);
            return;
        }

        public async Task SendLatestMensWeeklyAsync(CallbackQuery query, ITelegramBotClient botClient, CancellationToken cancellationToken)
        {
            await botClient.AnswerCallbackQueryAsync(
                callbackQueryId: query.Id,
                text: "Fetching the latest men's weekly games...",
                cancellationToken: cancellationToken);

            var ev = await GetLatestEventParticipantsByKeywordAsync("keskarit");

            if (!ev.HasValues)
            {
                await botClient.AnswerCallbackQueryAsync(
                    callbackQueryId: query.Id,
                    text: "No men's weekly games were found",
                    showAlert: true,
                    cancellationToken: cancellationToken);
                return;
            }

            var mapping =
                "*" + ev.Value<string>("name") + "*" +
                "\nId: " + ev.Value<string>("id") +
                "\nDate: " + ev.Value<string>("date") +
                "\nParticipants: " + string.Join(
                    ", ",
                    ev.Value<JArray>("participants")
                        .Select(participant => participant.Value<string>("name1")));

            await botClient.SendTextMessageAsync(
                chatId: query.From.Id,
                text: mapping,
                parseMode: ParseMode.Markdown,
                cancellationToken: cancellationToken);
            return;
        }

        public async Task SendLatestWomensWeeklyAsync(CallbackQuery query, ITelegramBotClient botClient, CancellationToken cancellationToken)
        {
            await botClient.AnswerCallbackQueryAsync(
                callbackQueryId: query.Id,
                text: "Fetching the latest women's weekly games...",
                cancellationToken: cancellationToken);

            var ev = await GetLatestEventParticipantsByKeywordAsync("tirsat");

            if (!ev.HasValues)
            {
                await botClient.AnswerCallbackQueryAsync(
                    callbackQueryId: query.Id,
                    text: "No women's weekly games were found",
                    showAlert: true,
                    cancellationToken: cancellationToken);
                return;
            }

            var mapping =
                "*" + ev.Value<string>("name") + "*" +
                "\nId: " + ev.Value<string>("id") +
                "\nDate: " + ev.Value<string>("date") +
                "\nParticipants: " + string.Join(
                    ", ",
                    ev.Value<JArray>("participants")
                        .Select(participant => participant.Value<string>("name1")));

            await botClient.SendTextMessageAsync(
                chatId: query.From.Id,
                text: mapping,
                parseMode: ParseMode.Markdown,
                cancellationToken: cancellationToken);
            return;
        }

        private async Task<JArray> GetActiveEventsAsync()
        {
            var key = "ActiveEvents";
            var timestamp = DateTime.Now;

            // Use previous data if cache time is still active
            if (_localMemory.ContainsKey(key) &&
                ((DateTime)_localMemory.SelectToken($"{key}.Timestamp")) > timestamp.AddSeconds(-(int)_localMemory.SelectToken("CacheTime")))
            {
                return (JArray)_localMemory.SelectToken($"{key}.Value");
            }

            // Otherwise, fetch new and replace cached data
            Console.WriteLine("Fetching and caching active HBV events");
            var events = await _hbvService.GetActiveEventsAsync();

            _localMemory[key] = new JObject()
            {
                { "Value", events },
                { "Timestamp", timestamp }
            };

            return events;
        }

        private async Task<JArray> GetActiveEventsByKeywordAsync(string keyword)
        {
            var events = await this.GetActiveEventsAsync();
            var result = events.Where(ev => ev.Value<string>("name").ToLower().Contains(keyword));
            return JArray.FromObject(result);
        }

        private async Task<JObject> GetLatestEventByKeywordActiveAsync(string keyword)
        {
            var events = await this.GetActiveEventsByKeywordAsync(keyword);

            if (!events.Any())
                return new JObject();

            return (JObject)events.OrderByDescending(ev => ev.Value<string>("date")).First();
        }

        private async Task<JArray> GetAllEventsAsync()
        {
            var key = "AllEvents";
            var timestamp = DateTime.Now;

            // Use previous data if cache time is still active
            if (_localMemory.ContainsKey(key) &&
                ((DateTime)_localMemory.SelectToken($"{key}.Timestamp")) > timestamp.AddSeconds(-(int)_localMemory.SelectToken("CacheTime")))
            {
                return (JArray)_localMemory.SelectToken($"{key}.Value");
            }

            // Otherwise, fetch new and replace cached data
            Console.WriteLine("Fetching and caching all HBV events");
            var events = await _hbvService.GetAllEventsAsync();

            _localMemory[key] = new JObject()
            {
                { "Value", events },
                { "Timestamp", timestamp }
            };

            return events;
        }

        private async Task<JArray> GetAllEventsByKeywordAsync(string keyword)
        {
            var events = await this.GetAllEventsAsync();
            var result = events.Where(ev => ev.Value<string>("name").ToLower().Contains(keyword));
            return JArray.FromObject(result);
        }

        private async Task<JObject> GetLatestEventByKeywordAllAsync(string keyword)
        {
            var events = await this.GetAllEventsByKeywordAsync(keyword);

            if (!events.Any())
                return new JObject();

            return (JObject)events.OrderByDescending(ev => ev.Value<string>("date")).First();
        }

        private async Task<JObject> GetLatestEventByKeywordAsync(string keyword)
        {
            // If found, return the latest active event
            var ev = await this.GetLatestEventByKeywordActiveAsync(keyword);

            if (ev.HasValues)
                return ev;

            // Otherwise, return the latest event from all events
            return await this.GetLatestEventByKeywordAllAsync(keyword);
        }

        private async Task<JObject> GetLatestEventParticipantsByKeywordAsync(string keyword)
        {
            var ev = await this.GetLatestEventByKeywordAsync(keyword);

            if (!ev.ContainsKey("id"))
                return ev;

            var groups = await _hbvService.GetGroupByEventIdAsync(ev.Value<string>("id"));

            if (groups.Count() != 1 || !((JObject)groups.First()).ContainsKey("id"))
                return ev;

            var participants = await _hbvService.GetParticipantsByGroupIdAsync(groups.First().Value<string>("id"));
            ev.Add("participants", participants);

            return ev;
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
                    InlineKeyboardButton.WithCallbackData(text: "Tirsat", callbackData: "Hbv:LatestWomensWeekly"),
                    InlineKeyboardButton.WithCallbackData(text: "Keskarit", callbackData: "Hbv:LatestMensWeekly"),
                },
            });
        }
    }
}
