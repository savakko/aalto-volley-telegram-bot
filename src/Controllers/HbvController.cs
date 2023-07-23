using aalto_volley_bot.Services;
using Newtonsoft.Json.Linq;

namespace aalto_volley_bot.src.Controllers
{
    internal class HbvController
    {
        private readonly HbvService _hbvService = new();
        private readonly JObject _localMemory = new()  // TODO: Outsource local memory!
        {
            { "CacheTime", 600 }  // Amount of time in seconds that the data is cached
        };

        public async Task<JArray> GetActiveEventsAsync()
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

        public async Task<JArray> GetActiveEventsByKeywordAsync(string keyword)
        {
            var events = await this.GetActiveEventsAsync();
            var result = events.Where(ev => ev.Value<string>("name").ToLower().Contains(keyword));
            return JArray.FromObject(result);
        }

        public async Task<JObject> GetLatestEventByKeywordActiveAsync(string keyword)
        {
            var events = await this.GetActiveEventsByKeywordAsync(keyword);

            if (!events.Any())
                return new JObject();

            return (JObject)events.OrderByDescending(ev => ev.Value<string>("date")).First();
        }

        public async Task<JArray> GetAllEventsAsync()
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

        public async Task<JArray> GetAllEventsByKeywordAsync(string keyword)
        {
            var events = await this.GetAllEventsAsync();
            var result = events.Where(ev => ev.Value<string>("name").ToLower().Contains(keyword));
            return JArray.FromObject(result);
        }

        public async Task<JObject> GetLatestEventByKeywordAllAsync(string keyword)
        {
            var events = await this.GetAllEventsByKeywordAsync(keyword);

            if (!events.Any())
                return new JObject();

            return (JObject)events.OrderByDescending(ev => ev.Value<string>("date")).First();
        }

        public async Task<JObject> GetLatestEventByKeywordAsync(string keyword)
        {
            // If found, return the latest active event
            var ev = await this.GetLatestEventByKeywordActiveAsync(keyword);

            if (ev.HasValues)
                return ev;

            // Otherwise, return the latest event from all events
            return await this.GetLatestEventByKeywordAllAsync(keyword);
        }

        public async Task<JObject> GetLatestEventParticipantsByKeywordAsync(string keyword)
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
    }
}
