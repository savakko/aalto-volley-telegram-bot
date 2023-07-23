using aalto_volley_bot.src.WebApps;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
