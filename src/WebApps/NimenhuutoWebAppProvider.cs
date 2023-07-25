using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace aalto_volley_bot.src.WebApps
{
    internal class NimenhuutoWebAppProvider
    {
        private readonly Uri NimenhuutoBasePath;
        private readonly Dictionary<string, WebAppInfo> WebApps;

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

        public void AddNimenhuutoWebApp(string buttonText, string path)
        {
            if (WebApps.ContainsKey(buttonText))
            {
                Console.WriteLine($"Didn't add NimenhuutoWebApp with key {buttonText}, because it already exists");
                return;
            }

            WebApps.Add(buttonText, new() { Url = new Uri(NimenhuutoBasePath, path).ToString() });

            Console.WriteLine($"Added new NimenhuutoWebApp with key {buttonText}");
            return;
        }

        public void DeleteNimenhuutoWebApp(string buttonText)
        {
            if (!WebApps.ContainsKey(buttonText))
            {
                Console.WriteLine($"Didn't remove NimenhuutoWebApp with key {buttonText}, because it doesn't exist");
                return;
            }

            WebApps.Remove(buttonText);

            Console.WriteLine($"Removed the NimenhuutoWebApp with key {buttonText}");
            return;
        }

        public IReplyMarkup BuildMainMenu()
        {
            return new InlineKeyboardMarkup(new[]
            {
                new[]
                {
                    BuildWebAppButton("Front page"),
                    BuildWebAppButton("Upcoming events"),
                },
                new[]
                {
                    BuildWebAppButton("Archive"),
                    BuildWebAppButton("Statistics"),
                },
                new[]
                {
                    BuildWebAppButton("Login"),
                    BuildWebAppButton("Logout"),
                },
                new[]
                {
                    BuildWebAppButton("Register"),
                },
                new[]
                {
                    BuildMenuButton("Manager"),
                },
            });
        }

        public IReplyMarkup BuildManagerMenu()
        {
            return new InlineKeyboardMarkup(new[]
            {
                new[]
                {
                    BuildWebAppButton("Team settings"),
                },
                new[]
                {
                    BuildWebAppButton("Logs"),
                },
            });
        }

        // Builds a single-column-menu with all the WebApps
        public IReplyMarkup BuildWebAppsColumnMenu()
        {
            return new InlineKeyboardMarkup(WebApps.Select(pair =>
                new[] { InlineKeyboardButton.WithWebApp(text: pair.Key, webAppInfo: pair.Value) }));
        }

        private InlineKeyboardButton BuildWebAppButton(string key)
        {
            return InlineKeyboardButton.WithWebApp(text: key, webAppInfo: WebApps[key]);
        }

        private static InlineKeyboardButton BuildMenuButton(string command)
        {
            return InlineKeyboardButton.WithCallbackData(text: command, callbackData: "Nimenhuuto:" + command);
        }
    }
}
