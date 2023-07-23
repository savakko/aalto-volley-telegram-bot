using aalto_volley_bot.src.WebApps;
using Telegram.Bot.Types;

namespace aalto_volley_bot.src.Controllers
{
    internal class NimenhuutoController
    {
        private readonly NimenhuutoWebAppProvider _nimenhuutoWebApps = new();

        public Dictionary<string, WebAppInfo> GetAllWebApps()
        {
            return _nimenhuutoWebApps.WebApps;
        }
    }
}
