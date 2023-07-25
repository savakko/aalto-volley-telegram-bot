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
                { "Upcoming events", new() { Url = new Uri(NimenhuutoBasePath, "events").ToString() } },
                { "Archive", new() { Url = new Uri(NimenhuutoBasePath, "events/archive").ToString() } },
                { "Statistics", new() { Url = new Uri(NimenhuutoBasePath, "enrollments").ToString() } },
                { "Login", new() { Url = new Uri(NimenhuutoBasePath, "sessions/new").ToString() } },
                { "Logout", new() { Url = new Uri(NimenhuutoBasePath, "sessions/logout").ToString() } },
                { "Register", new() { Url = new Uri(NimenhuutoBasePath, "public_join").ToString() } },
                { "Team settings", new() { Url = new Uri(NimenhuutoBasePath, "manager").ToString() } },
                { "Logs", new() { Url = new Uri(NimenhuutoBasePath, "audit_logs").ToString() } },
            };
        }
    }
}
