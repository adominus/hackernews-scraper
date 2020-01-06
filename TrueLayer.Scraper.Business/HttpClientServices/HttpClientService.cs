using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace TrueLayer.Scraper.Business.HttpClientServices
{
	public class HttpClientService : IHttpClientService
	{
		private readonly HttpClient _httpClient;
		private readonly ILogger<HttpClientService> _logger;

		public HttpClientService(
			HttpClient httpClient,
			ILogger<HttpClientService> logger)
		{
			_httpClient = httpClient;
			_logger = logger;
		}

		public async Task<string> GetHtmlContentAsync(Uri uri)
		{
			try
			{
				var response = await _httpClient.GetAsync(uri);

				if (response.IsSuccessStatusCode &&
					string.Compare(response.Content.Headers.ContentType.MediaType, "text/html", true) == 0)
				{
					return await response.Content.ReadAsStringAsync();
				}
			}
			catch (Exception e)
			{
				_logger.LogError(e, $"Unable to get content of {uri?.AbsoluteUri}");
			}

			return null;
		}
	}
}
