using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TrueLayer.Scraper.Business.HttpClientServices;
using TrueLayer.Scraper.Domain.HackerNews;

namespace TrueLayer.Scraper.Business.HackerNews
{
	public class HackerNewsScraper : IHackerNewsScraper
	{
		private readonly IHttpClientService _httpClientService;
		private readonly IHackerNewsHtmlParser _hackerNewsHtmlParser;

		public HackerNewsScraper(
			IHttpClientService httpClientService,
			IHackerNewsHtmlParser hackerNewsHtmlParser)
		{
			_httpClientService = httpClientService;
			_hackerNewsHtmlParser = hackerNewsHtmlParser;
		}

		public async Task<IEnumerable<Post>> GetTopPostsAsync(int postsCount)
		{
			if (postsCount <= 0 || postsCount > 100)
			{
				throw new ArgumentOutOfRangeException(nameof(postsCount));
			}

			var posts = new List<HackerNewsPost>();
			var page = 1;

			while (posts.Select(x => x.Id).Distinct().Count() < postsCount)
			{
				foreach (var post in await GetPostsAsync(page))
				{
					if (!posts.Any(x => x.Id == post.Id))
					{
						posts.Add(post);
					}
				}

				page++;
			}

			return posts
				.OrderBy(x => x.Rank)
				.Take(postsCount)
				.Select(ToPostDomainModel);
		}

		private async Task<IEnumerable<HackerNewsPost>> GetPostsAsync(int page)
		{
			var htmlContent = await _httpClientService.GetHtmlContentAsync(BuildUriForPage(page));

			return _hackerNewsHtmlParser.ParsePosts(htmlContent);
		}

		private Uri BuildUriForPage(int page)
			=> new Uri($"https://news.ycombinator.com/news?p={page}");

		private Post ToPostDomainModel(HackerNewsPost post)
			=> new Post
			{
				Author = post.Author,
				Comments = post.Comments,
				Points = post.Points,
				Rank = post.Rank,
				Title = post.Title,
				Uri = post.Uri
			};
	}
}
