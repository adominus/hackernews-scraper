using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Threading.Tasks;
using TrueLayer.Scraper.Business.HackerNews;
using TrueLayer.Scraper.Business.HttpClientServices;

namespace TrueLayer.Scraper
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var host = Host.CreateDefaultBuilder(args)
                .ConfigureServices(ConfigureServices)
                .UseConsoleLifetime()
                .Build();

            using var serviceScope = host.Services.CreateScope();

            var services = serviceScope.ServiceProvider;

            var scraper = services.GetRequiredService<Domain.HackerNews.IHackerNewsScraper>();

            var result = await scraper.GetTopPostsAsync(5);

            var serializerSettings = new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };

            Console.WriteLine(JsonConvert.SerializeObject(result, Formatting.Indented, serializerSettings));
        }

        private static void ConfigureServices(HostBuilderContext hostBuilder, IServiceCollection services)
        {
            services.AddScoped<Domain.HackerNews.IHackerNewsScraper, HackerNewsScraper>();

            services.AddScoped<IHackerNewsHtmlParser, HackerNewsHtmlParser>();
            services.AddScoped<IHackerNewsPostValidator, HackerNewsPostValidator>();

            services.AddScoped<IHttpClientService, HttpClientService>();
            services.AddHttpClient<IHttpClientService, HttpClientService>(httpClientConfig =>
                httpClientConfig.BaseAddress = new Uri("https://news.ycombinator.com"));

            //services.Configure<CrawlerOptions>(hostBuilder.Configuration);
            //services.AddTransient<CrawlerService>();
        }
    }
}
