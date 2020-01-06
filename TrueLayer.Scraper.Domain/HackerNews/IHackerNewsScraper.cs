using System.Collections.Generic;
using System.Threading.Tasks;

namespace TrueLayer.Scraper.Domain.HackerNews
{
	public interface IHackerNewsScraper
	{
		Task<IEnumerable<Post>> GetTopPostsAsync(int postsCount);
	}
}
