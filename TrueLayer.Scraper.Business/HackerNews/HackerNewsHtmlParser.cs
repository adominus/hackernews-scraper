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
			var postTableRows = postsTable?.SelectNodes(".//tr[@class=\"athing\"]");

			if (postTableRows == null)
			{
				yield break;
			}

			foreach (var postTableRow in postTableRows)
			{
				var postSiblingRow = postTableRow.SelectSingleNode($"following-sibling::tr");

				var (Id, Title, Href, Rank) = ParsePostRow(postTableRow);
				var (Author, Comments, Points) = ParsePostSiblingRow(postSiblingRow);

				yield return new HackerNewsPost
				{
					Id = Id,
					Title = Title,
					Href = Href,
					Rank = Rank,
					Author = Author,
					Comments = Comments,
					Points = Points
				};
			}
		}

		private (string Id, string Title, string Href, int Rank) ParsePostRow(HtmlNode tr)
		{
			var rankNode = GetRankNode(tr);
			var titleAnchor = GetTitleAnchor(tr);

			if (rankNode == null || titleAnchor == null)
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

		private (string Author, int Comments, int Points) ParsePostSiblingRow(HtmlNode tr)
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
