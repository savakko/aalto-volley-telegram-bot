using aalto_volley_bot.src.Services;
using Newtonsoft.Json.Linq;

namespace aalto_volley_bot.Services
{
    internal class HbvService
    {
        public readonly Uri ApiBasePath = new("https://prod.hbv.fi/hbv-api/");

        public async Task<JArray> GetActiveEventsAsync()
        {
            var uri = new Uri(ApiBasePath, "events");
            var content = await ServiceUtils.GetContentAsStringAsync(uri);

            return JArray.Parse(content);
        }

        public async Task<JArray> GetAllEventsAsync()
        {
            var uri = new Uri(ApiBasePath, "allevents");
            var content = await ServiceUtils.GetContentAsStringAsync(uri);

            return JArray.Parse(content);
        }

        public async Task<JObject> GetEventByIdAsync(string eventId)
        {
            var uri = new Uri(ApiBasePath, $"events/{eventId}");
            var content = await ServiceUtils.GetContentAsStringAsync(uri);

            return JObject.Parse(content);
        }
        
        public async Task<JArray> GetGroupsByEventIdAsync(string eventId)
        {
            var uri = new Uri(ApiBasePath, $"events/groups/{eventId}");
            var content = await ServiceUtils.GetContentAsStringAsync(uri);

            return JArray.Parse(content);
        }

        public async Task<JArray> GetParticipantsByGroupIdAsync(string groupId)
        {
            var uri = new Uri(ApiBasePath, $"events/participants/{groupId}");
            var content = await ServiceUtils.GetContentAsStringAsync(uri);

            return JArray.Parse(content);
        }

        public async Task<JObject> GetWeeklyGameByIdAsync(string weeklyGameId)
        {
            var uri = new Uri(ApiBasePath, $"weekgames/{weeklyGameId}");
            var content = await ServiceUtils.GetContentAsStringAsync(uri);

            return JObject.Parse(content);
        }

        public async Task<JArray> GetWeeklyGamesBySerieAndYearAsync(string serie, string year)
        {
            var uri = new Uri(ApiBasePath, $"weekgames?serie={serie}&year={year}");
            var content = await ServiceUtils.GetContentAsStringAsync(uri);

            return JArray.Parse(content);
        }

        public async Task<JArray> GetWeeklyGameResultsByIdAsync(string weeklyGameId)
        {
            var uri = new Uri(ApiBasePath, $"weekgames/{weeklyGameId}/results");
            var content = await ServiceUtils.GetContentAsStringAsync(uri);

            return JArray.Parse(content);
        }

        public async Task<JArray> GetWeeklyGameRankingsBySerieAndYearAsync(string serie, string year)
        {
            var uri = new Uri(ApiBasePath, $"weekgameranking/points?serie={serie}&year={year}");
            var content = await ServiceUtils.GetContentAsStringAsync(uri);

            return JArray.Parse(content);
        }

        public async Task<JArray> GetMembersByIdAsync(string memberIds)
        {
            var uri = new Uri(ApiBasePath, $"membersearch?query=/{memberIds}");
            var content = await ServiceUtils.GetContentAsStringAsync(uri);

            return JArray.Parse(content);
        }

        public async Task<JArray> GetResultsByMemberIdAsync(string memberId)
        {
            var uri = new Uri(ApiBasePath, $"players/{memberId}");
            var content = await ServiceUtils.GetContentAsStringAsync(uri);

            return JArray.Parse(content);
        }
    }
}
