﻿using System.Threading.Tasks;

namespace TrueLayer.Scraper.Business.HttpClientServices
{
	public interface IHttpClientService
	{
		Task<string> GetHtmlContentAsync(string path);
	}
}
