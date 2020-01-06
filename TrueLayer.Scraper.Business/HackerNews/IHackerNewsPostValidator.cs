namespace TrueLayer.Scraper.Business.HackerNews
{
	public interface IHackerNewsPostValidator
	{
		bool IsValid(HackerNewsPost post);
	}
}
