﻿namespace aalto_volley_bot.src.Services
{
    static class ServiceUtils
    {
        public static async Task<string> GetContentAsStringAsync(Uri uri)
        {
            var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Get, uri);
            var response = await client.SendAsync(request);

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStringAsync();
        }
    }
}
