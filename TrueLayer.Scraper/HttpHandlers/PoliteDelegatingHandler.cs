using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace TrueLayer.Scraper.HttpHandlers
{
    public class PoliteDelegatingHandler : DelegatingHandler
    {
        protected async override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            await Task.Delay(250);

            return await base.SendAsync(request, cancellationToken);
        }
    }
}
