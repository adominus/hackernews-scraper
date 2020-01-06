using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading.Tasks;
using TrueLayer.Scraper.Business.HackerNews;
using TrueLayer.Scraper.Business.HttpClientServices;
using TrueLayer.Scraper.Configuration;
using TrueLayer.Scraper.HttpHandlers;

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

            await services.GetRequiredService<ScraperService>()
                .ScrapeAsync();
        }

        private static void ConfigureServices(HostBuilderContext hostBuilder, IServiceCollection services)
        {
            // Scraper
            services.AddTransient<PoliteDelegatingHandler>();
            services.Configure<ScraperOptions>(hostBuilder.Configuration);
            services.AddTransient<ScraperService>();

            // Scraper.Domain
            services.AddScoped<Domain.HackerNews.IHackerNewsScraper, HackerNewsScraper>();

            // Scraper.Business
            services.AddScoped<IHackerNewsHtmlParser, HackerNewsHtmlParser>();
            services.AddScoped<IHackerNewsPostValidator, HackerNewsPostValidator>();

            services.AddScoped<IHttpClientService, HttpClientService>();
            services.AddHttpClient<IHttpClientService, HttpClientService>(httpClientConfig =>
                httpClientConfig.BaseAddress = new Uri("https://news.ycombinator.com"))
                .AddHttpMessageHandler<PoliteDelegatingHandler>();
        }
    }
}
