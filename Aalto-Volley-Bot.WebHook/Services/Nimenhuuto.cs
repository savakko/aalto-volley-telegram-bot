using Telegram.Bot.Types.ReplyMarkups;

public class Nimenhuuto
{
    public static Uri GetBasePath()
    {
        return new("https://aalto-volley.nimenhuuto.com/");
    }

    public static InlineKeyboardMarkup GetMainMenuMarkup()
    {
        return new InlineKeyboardMarkup(
            new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithWebApp(
                        text: "Front page",
                        webAppInfo: new() { Url = new Uri(GetBasePath(), "?auto_redirect=false").ToString() }),
                    InlineKeyboardButton.WithWebApp(
                        text: "Upcoming events",
                        webAppInfo: new() { Url = new Uri(GetBasePath(), "events").ToString() }),
                },
                new[]
                {
                    InlineKeyboardButton.WithWebApp(
                        text: "Archive",
                        webAppInfo: new() { Url = new Uri(GetBasePath(), "events/archive").ToString() }),
                    InlineKeyboardButton.WithWebApp(
                        text: "Statistics",
                        webAppInfo: new() { Url = new Uri(GetBasePath(), "enrollments").ToString() }),
                },
                new[]
                {
                    InlineKeyboardButton.WithWebApp(
                        text: "Login",
                        webAppInfo: new() { Url = new Uri(GetBasePath(), "sessions/new").ToString() }),
                    InlineKeyboardButton.WithWebApp(
                        text: "Logout",
                        webAppInfo: new() { Url = new Uri(GetBasePath(), "sessions/logout").ToString() }),
                },
                new[]
                {
                    InlineKeyboardButton.WithWebApp(
                        text: "Register",
                        webAppInfo: new() { Url = new Uri(GetBasePath(), "public_join").ToString() }),
                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData(text: "Manager", callbackData: "Nimenhuuto:Manager"),
                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData(text: "Pin keyboard", callbackData: "Nimenhuuto:Pin"),
                    InlineKeyboardButton.WithCallbackData(text: "Unpin keyboard", callbackData: "General:Unpin"),
                },
            });
    }

    public static InlineKeyboardMarkup GetManagerMenuMarkup()
    {
        return new InlineKeyboardMarkup(
            new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithWebApp(
                        text: "Basic info",
                        webAppInfo: new() { Url = new Uri(GetBasePath(), "manager").ToString() }),
                },
                new[]
                {
                    InlineKeyboardButton.WithWebApp(
                        text: "Team settings",
                        webAppInfo: new() { Url = new Uri(GetBasePath(), "manager/settings").ToString() }),
                },
                new[]
                {
                    InlineKeyboardButton.WithWebApp(
                        text: "Audit logs",
                        webAppInfo: new() { Url = new Uri(GetBasePath(), "audit_logs").ToString() }),
                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData(text: "<-- Back", callbackData: "Nimenhuuto:Main"),
                },
            });
    }

    public static ReplyKeyboardMarkup GetReplyKeyboardMarkup()
    {
        return new ReplyKeyboardMarkup(
            new[]
            {
                new[]
                {
                    KeyboardButton.WithWebApp(
                        text: "Nimenhuuto: Upcoming events",
                        webAppInfo: new() { Url = new Uri(GetBasePath(), "events").ToString() }),
                },
                new[]
                {
                    KeyboardButton.WithWebApp(
                        text: "Awaken the bot",
                        webAppInfo: new() { Url = "https://aalto-volley-bot-vhybjebhyq-lz.a.run.app" }),
                },
            })
        {
            ResizeKeyboard = true,
        };
    }
}
