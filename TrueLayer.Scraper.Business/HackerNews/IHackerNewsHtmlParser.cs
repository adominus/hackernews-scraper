using System.Collections.Generic;

namespace TrueLayer.Scraper.Business.HackerNews
{
	public interface IHackerNewsHtmlParser
	{
		IEnumerable<HackerNewsPost> ParsePosts(string html);
	}
}
