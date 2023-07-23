using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace aalto_volley_bot.src.WebApps
{
    internal class NimenhuutoWebAppProvider
    {
        public readonly Uri NimenhuutoBasePath;
        public readonly Dictionary<string, WebAppInfo> WebApps;

        public NimenhuutoWebAppProvider()
        {
            NimenhuutoBasePath = new("https://aalto-volley.nimenhuuto.com/");
            WebApps = new()
            {
                { "Front page", new() { Url = new Uri(NimenhuutoBasePath, "?auto_redirect=false").ToString() } },
                { "Events", new() { Url = new Uri(NimenhuutoBasePath, "events").ToString() } },
                { "Login", new() { Url = new Uri(NimenhuutoBasePath, "sessions/new").ToString() } },
                { "Logout", new() { Url = new Uri(NimenhuutoBasePath, "sessions/logout").ToString() } },
                { "Register", new() { Url = new Uri(NimenhuutoBasePath, "public_join").ToString() } },
            };
        }
    }
}
