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

			_fakeHackerNewsServer = FluentMockServer.Start();
		}

		[SetUp]
		public void SetUp()
		{
			_fakeHackerNewsServer.ResetMappings();

			_fixture.Register<IHackerNewsHtmlParser>(_fixture.Create<HackerNewsHtmlParser>);
			_fixture.Register<IHackerNewsPostValidator>(_fixture.Create<HackerNewsPostValidator>);

			var httpClient = new HttpClient
			{
				BaseAddress = new Uri($"http://localhost:{_fakeHackerNewsServer.Ports.First()}")
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
			SetupHackerNewsServerResponse("HackerNews-01.html", 1);

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
		public async Task WhenPostIsNotValid_ShouldNotIncludeItInResults()
		{
			// Arrange
			// Rank 25. does not meet validation requirements
			SetupHackerNewsServerResponse("HackerNews-01.html", 1);

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
			// HackerNews-01.html Contains posts: 
			// Rank 17. Billion-Dollar Weather and Climate Disasters: Time Series 
			// Rank 22. Managing product requests from customer-facing teams 
			// Rank 26. Credit Cards for Giving 
			// Rank 27. Ask HN: I've been slacking off at Google for 6 years. How can I stop this?
			SetupHackerNewsServerResponse("HackerNews-01.html", 1);

			// HackerNews-02.html Top 4 posts are: 
			// Rank 31.	Managing product requests from customer-facing teams 
			// Rank 32. Ask HN: I've been slacking off at Google for 6 years. How can I stop this?
			// Rank 33. Billion-Dollar Weather and Climate Disasters: Time Series 
			// Rank 34. Credit Cards for Giving 
			SetupHackerNewsServerResponse("HackerNews-02.html", 2);

			// Rank 35. in HackerNews-02.html: 
			var expectedLastPostTitle = "Airbnb analyses social media to root out people with 'narcissism or psychopathy'";

			// Act
			var results = (await _hackerNewsScraper.GetTopPostsAsync(30)).ToList();

			// Assert
			Assert.That(results.Count(), Is.EqualTo(30));

			Assert.That(results[28].Rank, Is.EqualTo(30));

			Assert.That(results[29].Rank, Is.EqualTo(35));
			Assert.That(results[29].Title, Is.EqualTo(expectedLastPostTitle));
		}

		private void SetupHackerNewsServerResponse(string fileName, int page)
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
