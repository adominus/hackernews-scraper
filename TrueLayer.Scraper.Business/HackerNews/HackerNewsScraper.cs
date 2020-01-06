using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TrueLayer.Scraper.Business.Exceptions;
using TrueLayer.Scraper.Business.HttpClientServices;
using TrueLayer.Scraper.Domain.HackerNews;

namespace TrueLayer.Scraper.Business.HackerNews
{
	public class HackerNewsScraper : IHackerNewsScraper
	{
		private readonly IHttpClientService _httpClientService;
		private readonly IHackerNewsHtmlParser _hackerNewsHtmlParser;
		private readonly IHackerNewsPostValidator _hackerNewsPostValidator;

		public HackerNewsScraper(
			IHttpClientService httpClientService,
			IHackerNewsHtmlParser hackerNewsHtmlParser,
			IHackerNewsPostValidator hackerNewsPostValidator)
		{
			_httpClientService = httpClientService;
			_hackerNewsHtmlParser = hackerNewsHtmlParser;
			_hackerNewsPostValidator = hackerNewsPostValidator;
		}

		public async Task<IEnumerable<Post>> GetTopPostsAsync(int postsCount)
		{
			if (postsCount <= 0 || postsCount > 100)
			{
				throw new ArgumentOutOfRangeException(nameof(postsCount));
			}

			var posts = await GetUniquePostsAsync(postsCount);

			return posts
				.OrderBy(x => x.Rank)
				.Take(postsCount)
				.Select(ToPostDomainModel);
		}

		private async Task<IEnumerable<HackerNewsPost>> GetUniquePostsAsync(int minimumRequiredPosts)
		{
			var posts = new List<HackerNewsPost>();
			var page = 1;

			while (!HasEnoughUniquePosts())
			{
				if (page > 15)
				{
					throw new SearchDepthExceededException();
				}

				foreach (var post in await GetPostsAsync(page))
				{
					if (_hackerNewsPostValidator.IsValid(post) && !posts.Any(x => x.Id == post.Id))
					{
						posts.Add(post);
					}
				}

				page++;
			}

			return posts;

			bool HasEnoughUniquePosts() => posts
				.Select(x => x.Id)
				.Distinct()
				.Count() >= minimumRequiredPosts;
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
				Uri = GetUriFromHref(post.Href)
			};

		private Uri GetUriFromHref(string href)
		{
			if (Uri.TryCreate(href, UriKind.Absolute, out Uri absoluteUri))
			{
				return absoluteUri;
			}

			return new Uri(new Uri("https://news.ycombinator.com"), href);
		}
	}
}
