using Newtonsoft.Json.Linq;
using Telegram.Bot.Types.ReplyMarkups;

public class Hbv
{
    public static Uri GetBasePath()
    {
        return new("https://prod.hbv.fi/");
    }

    public static Uri GetApiBasePath()
    {
        return new(GetBasePath(), "hbv-api/");
    }

    public static async Task<JArray> GetActiveEventsAsync()
    {
        return (JArray)await GetJsonContent("events");
    }

    public static async Task<JArray> GetAllEventsAsync()
    {
        return (JArray)await GetJsonContent("allevents");
    }

    public static async Task<JObject> GetEventByIdAsync(string eventId)
    {
        return (JObject)await GetJsonContent($"events/{eventId}");
    }

    public static async Task<JArray> GetGroupsByEventIdAsync(string eventId)
    {
        return (JArray)await GetJsonContent($"events/groups/{eventId}");
    }

    public static async Task<JArray> GetParticipantsByGroupIdAsync(string groupId)
    {
        return (JArray)await GetJsonContent($"events/participants/{groupId}");
    }

    public static async Task<JObject> GetWeeklyGameByIdAsync(string weeklyGameId)
    {
        return (JObject)await GetJsonContent($"weekgames/{weeklyGameId}");
    }

    public static async Task<JArray> GetWeeklyGamesBySerieAndYearAsync(string serie, string year)
    {
        return (JArray)await GetJsonContent($"weekgames?serie={serie}&year={year}");
    }

    public static async Task<JArray> GetWeeklyGameResultsByIdAsync(string weeklyGameId)
    {
        return (JArray)await GetJsonContent($"weekgames/{weeklyGameId}/results");
    }

    public static async Task<JArray> GetWeeklyGameRankingsBySerieAndYearAsync(string serie, string year)
    {
        return (JArray)await GetJsonContent($"weekgameranking/points?serie={serie}&year={year}");
    }

    public static async Task<JArray> GetMembersByIdAsync(string memberIds)
    {
        return (JArray)await GetJsonContent($"membersearch?query=/{memberIds}");
    }

    public static async Task<JArray> GetResultsByMemberIdAsync(string memberId)
    {
        return (JArray)await GetJsonContent($"players/{memberId}");
    }


    public static InlineKeyboardMarkup GetMainMenuMarkup()
    {
        return new InlineKeyboardMarkup(new[]
            {
                new []
                {
                    InlineKeyboardButton.WithCallbackData(text: "All active events", callbackData: "Hbv:ActiveEvents"),
                },
                new []
                {
                    InlineKeyboardButton.WithCallbackData(text: "Tirsat", callbackData: "Hbv:Tirsat"),
                    InlineKeyboardButton.WithCallbackData(text: "Keskarit", callbackData: "Hbv:Keskarit"),
                },
            });
    }

    public static InlineKeyboardMarkup GetActiveEventsMenuMarkup()
    {
        return new InlineKeyboardMarkup(
            new[]
            {
                InlineKeyboardButton.WithUrl(
                    text: "Open in browser",
                    url: new Uri(GetBasePath(), "/tapahtumat/#/").ToString()),
            });
    }

    public static InlineKeyboardMarkup GetWeeklyGamesMenuMarkup(string serie, JArray upcoming)
    {
        return new InlineKeyboardMarkup(
            new[]
            {
                new[]
                {
                    ExtractUpcomingGame(serie, upcoming),
                },
                new[]
                {
                    InlineKeyboardButton.WithUrl(
                        text: "Rankings & Results",
                        url: new Uri(GetBasePath(), $"app/#/weekgames/{serie.ToLower()}").ToString()),
                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData(
                        text: "<-- Back",
                        callbackData: "Hbv:Main"),
                },
            });

        static InlineKeyboardButton ExtractUpcomingGame(string serie, JArray games)
        {
            var upcoming = games.Where(game => game.Value<int>("status") < 5);

            if (!upcoming.Any())
                return InlineKeyboardButton.WithCallbackData(
                    text: "Upcoming: None",
                    callbackData: $"Hbv:{serie}-Specific");

            var weeklyGame = (JObject)upcoming.OrderBy(game => game.Value<string>("date")).Last();
            var id = weeklyGame.Value<string>("id");
            var date = DateTime.TryParse(weeklyGame.Value<string>("date"), out DateTime dateTime)
                ? " " + dateTime.ToString("dd.MM")
                : "";

            return InlineKeyboardButton.WithCallbackData(
                text: $"Upcoming: {serie} {date}",
                callbackData: $"Hbv:{serie}-Specific?id={id}");
        }
    }

    public static InlineKeyboardMarkup GetSpecificWeeklyGameMenuMarkup(string serie, JObject weeklyGame, JArray groups)
    {
        var link = weeklyGame.Value<string>("event_link");
        var groupIds = string.Join(',', groups.Select(group => group.Value<string>("id")));

        return new InlineKeyboardMarkup(
            new[]
            {
                !string.IsNullOrEmpty(link)
                ? new[]
                {
                    InlineKeyboardButton.WithUrl(
                        text: "Sign up page",
                        url: link),
                }
                : new[]
                {
                    InlineKeyboardButton.WithCallbackData(
                        text: "Sign up page not found",
                        callbackData: "No event link found"),
                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData(
                        text: "Calculate pools",
                        callbackData: $"Hbv:{serie}-Pools?groups={groupIds}"),
                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData(
                        text: "<-- Back",
                        callbackData: $"Hbv:{serie}"),
                },
            });
    }

    private static async Task<JToken> GetJsonContent(string path)
    {
        var client = new HttpClient();
        var uri = new Uri(GetApiBasePath(), path);
        var request = new HttpRequestMessage(HttpMethod.Get, uri);
        var response = await client.SendAsync(request);

        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        return JToken.Parse(content);
    }
}
