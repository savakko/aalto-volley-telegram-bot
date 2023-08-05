using HtmlAgilityPack;
using Newtonsoft.Json.Linq;
using System.Web;

namespace aalto_volley_bot.Services
{
    internal class NimenhuutoService
    {
        private readonly Uri BasePath;

        public NimenhuutoService()
        {
            BasePath = new("https://aalto-volley.nimenhuuto.com/");
        }

        public JArray ScrapeUpcomingEvents()
        {
            var doc = GetNimenhuutoPage("events");
            var events = doc.DocumentNode.SelectNodes("//div[@class='row event-detailed-container hover-menu-parent']");
            var eventsData = events.Select(node => ParseEventInfo(node));

            return JArray.FromObject(eventsData);
        }

        public JArray ScrapeUpcomingEvents(int count)
        {
            var doc = GetNimenhuutoPage("events");
            var events = doc.DocumentNode.SelectNodes("//div[@class='row event-detailed-container hover-menu-parent']");
            var eventsData = events.Take(count).Select(node => ParseEventInfo(node));

            return JArray.FromObject(eventsData);
        }

        //public JObject ScrapeCurrentEvent()
        //{
        //    var events = ScrapeUpcomingEvents();

        //    if (!events.Any() || )
        //}

        private HtmlDocument GetNimenhuutoPage(string path)
        {
            var uri = new Uri(BasePath, path);
            var web = new HtmlWeb();
            return web.Load(uri);
        }

        private static JObject ParseEventInfo(HtmlNode node)
        {
            var linkNode = node.SelectSingleNode(".//a[@class='event-title-link']");
            var link = linkNode.GetAttributeValue("href", string.Empty);
            var type = HttpUtility.HtmlDecode(linkNode.GetDirectInnerText().Trim());
            var name = HttpUtility.HtmlDecode(linkNode.SelectSingleNode(".//*[@class='event-information']").InnerText);
            var time = HttpUtility.HtmlDecode(node.SelectSingleNode(".//h4").InnerText).Split('(').First().Trim();

            return new JObject()
            {
                { "Type", type[..(type.Length - 2)] },
                { "Name", name },
                { "Time", time },
                { "Link", link },
            };
        }
    }
}
