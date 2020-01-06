using AutoFixture;
using NUnit.Framework;
using System;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using TrueLayer.Scraper.Business.HackerNews;
using TrueLayer.Scraper.Business.HttpClientServices;
using TrueLayer.Scraper.Domain.HackerNews;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;
using WireMock.Settings;

namespace TrueLayer.Scraper.AcceptanceTests
{
	public class HackerNewsScraperAcceptanceTests
	{
		private IFixture _fixture;
		private FluentMockServer _fakeHackerNewsServer;
		private IHackerNewsScraper _hackerNewsScraper;

		[OneTimeSetUp]
		public void OneTimeSetUp()
		{
			_fixture = new Fixture();

			_fakeHackerNewsServer = FluentMockServer.Start(new FluentMockServerSettings
			{
				Port = 8000
			});
		}

		[SetUp]
		public void SetUp()
		{
			_fakeHackerNewsServer.ResetMappings();

			_fixture.Register<IHackerNewsHtmlParser>(_fixture.Create<HackerNewsHtmlParser>);
			_fixture.Register<IHackerNewsPostValidator>(_fixture.Create<HackerNewsPostValidator>);

			var httpClient = new HttpClient
			{
				BaseAddress = new Uri("http://localhost:8000")
			};
			_fixture.Inject<IHttpClientService>(new HttpClientService(httpClient, null));

			_hackerNewsScraper = _fixture.Create<HackerNewsScraper>();
		}

		[OneTimeTearDown]
		public void TearDown()
		{
			_fakeHackerNewsServer.Stop();
		}

		[Test]
		public async Task WhenRequesting1Post_ShouldReturnTopPost()
		{
			// Arrange
			SetupHackerNewsServer("HackerNews-01.html", 1);

			// Act
			var result = await _hackerNewsScraper.GetTopPostsAsync(1);

			// Assert
			Assert.That(result.Count(), Is.EqualTo(1));

			var topPost = result.Single();
			Assert.That(topPost.Author, Is.EqualTo("mindgam3"));
			Assert.That(topPost.Comments, Is.EqualTo(42));
			Assert.That(topPost.Points, Is.EqualTo(120));
			Assert.That(topPost.Rank, Is.EqualTo(1));
			Assert.That(topPost.Title, Is.EqualTo("New Cambridge Analytica Leaks"));
			Assert.That(topPost.Uri.AbsoluteUri, Is.EqualTo("https://techcrunch.com/2020/01/06/facebook-data-misuse-and-voter-manipulation-back-in-the-frame-with-latest-cambridge-analytica-leaks/"));
		}

		[Test]
		public async Task WhenPostDoesNotGetValidated_ShouldNotIncludeItInResults()
		{
			// Arrange
			// 25. does not meet validation requirements
			SetupHackerNewsServer("HackerNews-01.html", 1);

			// Act
			var results = (await _hackerNewsScraper.GetTopPostsAsync(25)).ToList();

			// Assert
			Assert.That(results.Count(), Is.EqualTo(25));

			Assert.That(results[23].Rank, Is.EqualTo(24));
			Assert.That(results[24].Rank, Is.EqualTo(26));
		}

		[Test]
		public async Task WhenPostsDuplicateAcrossPages_ShouldReturnNextMostRankedPosts()
		{
			// Arrange
			// Contains posts: 
			// 17. Billion-Dollar Weather and Climate Disasters: Time Series 
			// 22. Managing product requests from customer-facing teams (brettcvz.com)
			// 26. Credit Cards for Giving 
			// 27. Ask HN: I've been slacking off at Google for 6 years. How can I stop this?
			SetupHackerNewsServer("HackerNews-01.html", 1);

			// Top 4 posts are: 
			// 31. 	Managing product requests from customer-facing teams (brettcvz.com)
			// 32. Ask HN: I've been slacking off at Google for 6 years. How can I stop this?
			// 33. Billion-Dollar Weather and Climate Disasters: Time Series 
			// 34. Credit Cards for Giving 
			SetupHackerNewsServer("HackerNews-02.html", 2);

			var expected31stPostTitle = "Airbnb analyses social media to root out people with 'narcissism or psychopathy'";

			// Act
			var results = (await _hackerNewsScraper.GetTopPostsAsync(30)).ToList();

			// Assert
			Assert.That(results.Count(), Is.EqualTo(30));

			Assert.That(results.Last().Title, Is.EqualTo(expected31stPostTitle));
		}

		private void SetupHackerNewsServer(string fileName, int page)
		{
			var fileContent = GetResourceFileContents(fileName);

			_fakeHackerNewsServer
				.Given(Request
					.Create()
					.WithPath($"/news")
					.WithParam("p", page.ToString())
					.UsingGet())
				.RespondWith(Response
					.Create()
					.WithStatusCode(200)
					.WithHeader("Content-Type", "text/html")
					.WithBody(fileContent));
		}

		private static string GetResourceFileContents(string fileName)
		{
			var currentDirectory = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			var filePath = System.IO.Path.Combine(currentDirectory, @"Resources", fileName);

			return System.IO.File.ReadAllText(filePath);
		}
	}
}
