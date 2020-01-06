using System;

namespace TrueLayer.Scraper.Business.HackerNews
{
	public class HackerNewsPostValidator : IHackerNewsPostValidator
	{
		public bool IsValid(HackerNewsPost post)
		{
			if (post == null)
			{
				return false;
			}

			if (string.IsNullOrWhiteSpace(post.Author))
			{
				return false;
			}

			if (post.Author.Length > 256)
			{
				return false;
			}

			if (string.IsNullOrWhiteSpace(post.Title))
			{
				return false;
			}

			if (post.Title.Length > 256)
			{
				return false;
			}

			if (!Uri.TryCreate(post.Href, UriKind.Absolute, out _) &&
				!Uri.TryCreate(post.Href, UriKind.Relative, out _))
			{
				return false;
			}

			if (post.Comments < 0)
			{
				return false;
			}

			if (post.Rank < 0)
			{
				return false;
			}

			if (post.Points < 0)
			{
				return false;
			}

			return true;
		}
	}
}
