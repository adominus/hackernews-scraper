using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Threading.Tasks;
using TrueLayer.Scraper.Configuration;
using TrueLayer.Scraper.Domain.HackerNews;

namespace TrueLayer.Scraper
{
    public class ScraperService
    {
        private readonly IHackerNewsScraper _hackerNewsScraper;
        private ScraperOptions _scraperOptions;

        public ScraperService(
            IHackerNewsScraper hackerNewsScraper,
            IOptions<ScraperOptions> scraperOptions)
        {
            _scraperOptions = scraperOptions?.Value ?? throw new ArgumentNullException(nameof(scraperOptions));
            _hackerNewsScraper = hackerNewsScraper;
        }

        public async Task ScrapeAsync()
        {
            if (_scraperOptions.Posts == null)
            {
                throw new ArgumentNullException("Please provide a value for posts");
            }

            var results = await _hackerNewsScraper.GetTopPostsAsync(_scraperOptions.Posts.Value);

            var serializerSettings = new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };

            Console.WriteLine(JsonConvert.SerializeObject(results, Formatting.Indented, serializerSettings));
        }
    }
}
