using aalto_volley_bot.src.WebApps;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace aalto_volley_bot.src.Controllers
{
    internal class NimenhuutoController
    {
        private readonly NimenhuutoWebAppProvider _nimenhuutoWebApps = new();

        public IReplyMarkup GetNimenhuutoMainMenu()
        {
            return _nimenhuutoWebApps.BuildMainMenu();
        }

        public IReplyMarkup GetNimenhuutoManagerMenu()
        {
            return _nimenhuutoWebApps.BuildManagerMenu();
        }
    }
}
