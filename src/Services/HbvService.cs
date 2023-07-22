using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace aalto_volley_bot.Services
{
    internal class HbvService
    {
        public readonly Uri ApiBasePath = new("https://prod.hbv.fi/hbv-api/");

        public async Task<JArray> GetActiveEventsAsync()
        {
            var client = new HttpClient();
            var uri = new Uri(this.ApiBasePath, "events");
            var request = new HttpRequestMessage(HttpMethod.Get, uri);
            var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();

            return JArray.Parse(await response.Content.ReadAsStringAsync());
        }

        public async Task<JArray> GetAllEventsAsync()
        {
            var client = new HttpClient();
            var uri = new Uri(this.ApiBasePath, "allevents");
            var request = new HttpRequestMessage(HttpMethod.Get, uri);
            var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();

            return JArray.Parse(await response.Content.ReadAsStringAsync());
        }

        public async Task<JToken> GetEventByIdAsync(string eventId)
        {
            var client = new HttpClient();
            var uri = new Uri(this.ApiBasePath, $"events/{eventId}");
            var request = new HttpRequestMessage(HttpMethod.Get, uri);
            var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();

            return JToken.Parse(await response.Content.ReadAsStringAsync());
        }
        
        public async Task<JToken> GetGroupByEventIdAsync(string eventId)
        {
            var client = new HttpClient();
            var uri = new Uri(this.ApiBasePath, $"events/groups/{eventId}");
            var request = new HttpRequestMessage(HttpMethod.Get, uri);
            var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();

            return JToken.Parse(await response.Content.ReadAsStringAsync());
        }

        public async Task<JArray> GetParticipantsByGroupIdAsync(string groupId)
        {
            var client = new HttpClient();
            var uri = new Uri(this.ApiBasePath, $"events/participants/{groupId}");
            var request = new HttpRequestMessage(HttpMethod.Get, uri);
            var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();

            return JArray.Parse(await response.Content.ReadAsStringAsync());
        }

        public async Task<JToken> GetMemberByIdAsync(string memberId)
        {
            var client = new HttpClient();
            var uri = new Uri(this.ApiBasePath, $"membersearch?query=/{memberId}");
            var request = new HttpRequestMessage(HttpMethod.Get, uri);
            var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();

            return JToken.Parse(await response.Content.ReadAsStringAsync());
        }
    }
}
