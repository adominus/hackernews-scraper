namespace TrueLayer.Scraper.Business.HackerNews
{
	public class HackerNewsPost
	{
		public string Id { get; set; }

		public string Title { get; set; }

		public string Href { get; set; }

		public string Author { get; set; }

		public int Points { get; set; }

		public int Comments { get; set; }

		public int Rank { get; set; }
	}
}
