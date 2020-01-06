using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;

namespace TrueLayer.Scraper.Business.HackerNews
{
	public class HackerNewsHtmlParser : IHackerNewsHtmlParser
	{
		public IEnumerable<HackerNewsPost> ParsePosts(string html)
		{
			if (string.IsNullOrWhiteSpace(html))
			{
				yield break;
			}

			var htmlDocument = new HtmlDocument();
			htmlDocument.LoadHtml(html);

			var postsTable = htmlDocument?.DocumentNode?.SelectSingleNode("//table[@class=\"itemlist\"]");
			var posts = postsTable?.SelectNodes(".//tr[@class=\"athing\"]");

			if (posts == null)
			{
				yield break;
			}

			foreach (var post in posts)
			{
				// //cc[preceding-sibling::bb[text()="zz"]]/text()
				var firstRow = ParseFirstRow(post);

				var second = post.SelectSingleNode($"following-sibling::tr");
				//var second = postsTable.SelectSingleNode($".//tr[preceding-sibling::tr[@id = \"{firstRow.Id}\"]]");
				yield return BuildPostFromRow(post, second);
			}

			//for (int i = 0; i < rows.Count; i += 2)
			//{
			//	if (i + 1 >= rows.Count)
			//	{
			//		throw new Exception("Unexpected list format");
			//	}

			//	yield return BuildPostFromRow(rows[i], rows[i + 1]);
			//}

			HackerNewsPost BuildPostFromRow(HtmlNode tr1, HtmlNode tr2)
			{
				var firstRow = ParseFirstRow(tr1);
				var secondRow = ParseSecondRow(tr2);

				return new HackerNewsPost
				{
					Id = firstRow.Id,
					Title = firstRow.Title,
					Href = firstRow.Href,
					Rank = firstRow.Rank,
					Author = secondRow.Author,
					Comments = secondRow.Comments,
					Points = secondRow.Points
				};
			}
		}

		private (string Id, string Title, string Href, int Rank) ParseFirstRow(HtmlNode tr)
		{
			var rankNode = GetRankNode(tr);
			var titleAnchor = GetTitleAnchor(tr);

			if (rankNode == null || titleAnchor == null )
			{
				throw new Exception("Unexpected rank and title format");
			}

			if (!int.TryParse(rankNode.InnerText.Replace(".", ""), out int rank))
			{
				throw new Exception("Unable to parse rank");
			}

			return (
				tr.GetAttributeValue("id", null),
				titleAnchor.InnerText?.Trim(),
				titleAnchor.GetAttributeValue("href", null),
				rank);

			HtmlNode GetRankNode(HtmlNode cell) => cell.SelectSingleNode(".//span[@class=\"rank\"]");
			HtmlNode GetTitleAnchor(HtmlNode cell) => cell.SelectSingleNode(".//a[@class=\"storylink\"]");
		}

		private (string Author, int Comments, int Points) ParseSecondRow(HtmlNode tr)
		{
			var scoreNode = GetScoreNode(tr);
			var authorAnchor = GetAuthorAnchor(tr);
			var commentsNode = GetCommentsNode(tr);

			int score = 0;
			if (scoreNode != null && !int.TryParse(scoreNode.InnerText.Replace("points", "").Replace("point", ""), out score))
			{
				throw new Exception("Unable to parse score");
			}

			int comments = 0;
			if (commentsNode != null && !int.TryParse(commentsNode.InnerText
					.Replace("comments", "")
					.Replace("comment", "")
					.Replace("&nbsp;", ""), out comments))
			{
				throw new Exception("Unable to parse comments");
			}

			return (
				authorAnchor?.InnerText.Trim(),
				comments,
				score);

			HtmlNode GetScoreNode(HtmlNode cell) => cell.SelectSingleNode(".//span[@class=\"score\"]");
			HtmlNode GetAuthorAnchor(HtmlNode cell) => cell.SelectSingleNode(".//a[@class=\"hnuser\"]");
			HtmlNode GetCommentsNode(HtmlNode cell) => cell.SelectNodes(".//a[not(@class=\"hnuser\")]")
				.Where(x => x.InnerText.Contains("comment"))
				.FirstOrDefault();
		}
	}
}
