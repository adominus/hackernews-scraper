using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TrueLayer.Scraper.Domain.HackerNews;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;
using WireMock.Settings;

namespace TrueLayer.Scraper.AcceptanceTests
{
	public class HackerNewsScraperAcceptanceTests
	{
		private FluentMockServer _fakeHackerNewsServer;
		private IHackerNewsScraper _hackerNewsScraper;

		[OneTimeSetUp]
		public void OneTimeSetUp()
		{
			_fakeHackerNewsServer = FluentMockServer.Start(new FluentMockServerSettings
			{
				Port = 8000
			});
		}

		[SetUp]
		public void SetUp()
		{
			// TODO: 
			//_hackerNewsScraper 
		}

		[OneTimeTearDown]
		public void TearDown()
		{
			_fakeHackerNewsServer.Stop();
		}

		[Test]
		public async Task ShouldReturnTopPost()
		{
			// Arrange
			SetupHackerNewsServer("HackerNews-01.html", 1);

			// Act
			var result = await _hackerNewsScraper.GetTopPostsAsync(1);

			// Assert
			Assert.That(result.Count(), Is.EqualTo(1));

			var topPost = result.Single();
			Assert.That(topPost.Author, Is.EqualTo("zdw"));
			Assert.That(topPost.Comments, Is.EqualTo(42));
			Assert.That(topPost.Points, Is.EqualTo(92));
			Assert.That(topPost.Rank, Is.EqualTo(1));
			Assert.That(topPost.Title, Is.EqualTo("Always Review Your Dependencies, AGPL Edition"));
			Assert.That(topPost.Uri.AbsoluteUri, Is.EqualTo("https://www.agwa.name/blog/post/always_review_your_dependencies"));
		}

		private void SetupHackerNewsServer(string fileName, int page)
		{
			_fakeHackerNewsServer.ResetMappings();

			var fileContent = GetResourceFileContents(fileName);

			_fakeHackerNewsServer
				.Given(Request
					.Create()
					.WithPath($"/news?p={page}")
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
