using AutoFixture;
using AutoFixture.AutoMoq;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TrueLayer.Scraper.Business.HackerNews;
using TrueLayer.Scraper.Business.HttpClientServices;

namespace TrueLayer.Scraper.Business.Tests.HackerNews
{
	public class HackerNewsScraperTests
	{
		private IFixture _fixture;
		private Mock<IHttpClientService> _httpClientServiceMock;
		private Mock<IHackerNewsHtmlParser> _hackerNewsHtmlParserMock;

		private string _htmlContentPage1;
		private List<HackerNewsPost> _postsPage1;
		private List<HackerNewsPost> _postsPage2;
		private List<HackerNewsPost> _postsPage3;

		[OneTimeSetUp]
		public void OneTimeSetUp()
		{
			_fixture = new Fixture()
				.Customize(new AutoMoqCustomization());

			_httpClientServiceMock = _fixture.Freeze<Mock<IHttpClientService>>();
			_hackerNewsHtmlParserMock = _fixture.Freeze<Mock<IHackerNewsHtmlParser>>();
		}

		[SetUp]
		public void SetUp()
		{
			_htmlContentPage1 = _fixture.Create<string>();
			_httpClientServiceMock.Setup(x => x.GetHtmlContentAsync(BuildPage(1)))
				.ReturnsAsync(_htmlContentPage1);

			_postsPage1 = _fixture.CreateMany<HackerNewsPost>().ToList();
			_hackerNewsHtmlParserMock.Setup(x => x.ParsePosts(_htmlContentPage1))
				.Returns(() => _postsPage1);

			var htmlContentPage2 = _fixture.Create<string>();
			_httpClientServiceMock.Setup(x => x.GetHtmlContentAsync(BuildPage(2)))
				.ReturnsAsync(htmlContentPage2);

			_postsPage2 = _fixture.CreateMany<HackerNewsPost>().ToList();
			_hackerNewsHtmlParserMock.Setup(x => x.ParsePosts(htmlContentPage2))
				.Returns(() => _postsPage2);

			var htmlContentPage3 = _fixture.Create<string>();
			_httpClientServiceMock.Setup(x => x.GetHtmlContentAsync(BuildPage(3)))
				.ReturnsAsync(htmlContentPage3);

			_postsPage3 = _fixture.CreateMany<HackerNewsPost>().ToList();
			_hackerNewsHtmlParserMock.Setup(x => x.ParsePosts(htmlContentPage3))
				.Returns(() => _postsPage3);
		}

		[TearDown]
		public void TearDown()
		{
			_httpClientServiceMock.Reset();
			_hackerNewsHtmlParserMock.Reset();
		}

		[Test]
		public async Task ShouldRequestHackerNewsContent()
		{
			// Arrange 
			var subject = _fixture.Create<HackerNewsScraper>();

			// Act 
			await subject.GetTopPostsAsync(1);

			// Assert
			_httpClientServiceMock.Verify(x => x.GetHtmlContentAsync(It.Is<Uri>(uri => IsEqualToPage(uri, 1))));
		}

		[Test]
		public async Task ShouldRequestPostsFromHackerNewsContent()
		{
			// Arrange 
			var subject = _fixture.Create<HackerNewsScraper>();

			// Act 
			await subject.GetTopPostsAsync(1);

			// Assert
			_hackerNewsHtmlParserMock.Verify(x => x.ParsePosts(_htmlContentPage1));
		}

		[Test]
		[TestCase(0)]
		[TestCase(-1)]
		[TestCase(101)]
		[TestCase(102)]
		[TestCase(1000)]
		public void WhenPostsCountOutOfRange_ShouldThrowArgumentException(int postsCount)
		{
			// Arrange 
			var subject = _fixture.Create<HackerNewsScraper>();

			// Act 
			AsyncTestDelegate act = () => subject.GetTopPostsAsync(postsCount);

			// Assert
			Assert.That(act, Throws.TypeOf<ArgumentOutOfRangeException>());
		}

		[Test]
		public async Task ShouldReturnPosts()
		{
			// Arrange 
			var expectedPost = _fixture.Create<HackerNewsPost>();
			_postsPage1.RemoveAll(_ => true);
			_postsPage1.Add(expectedPost);

			var subject = _fixture.Create<HackerNewsScraper>();

			// Act 
			var result = await subject.GetTopPostsAsync(1);

			// Assert
			Assert.That(result.Count, Is.EqualTo(1));

			var post = result.Single();
			AssertPostsMatch(post, expectedPost);
		}

		[Test]
		public async Task WhenMorePostsParsedThanRequested_ShouldReturnOnlyRequestedNumberOfPosts()
		{
			// Arrange 
			var expectedPost = _postsPage1[2];
			expectedPost.Rank = 1;

			var subject = _fixture.Create<HackerNewsScraper>();

			// Act 
			var result = await subject.GetTopPostsAsync(1);

			// Assert
			Assert.That(result.Count, Is.EqualTo(1));

			var post = result.Single();
			AssertPostsMatch(post, expectedPost);
		}

		[Test]
		public async Task WhenLessPostsParsedThanRequested_ShouldRequestNextPage()
		{
			// Arrange 
			var subject = _fixture.Create<HackerNewsScraper>();

			// Act 
			await subject.GetTopPostsAsync(_postsPage1.Count + 1);

			// Assert
			_httpClientServiceMock.Verify(x => x.GetHtmlContentAsync(It.Is<Uri>(uri => IsEqualToPage(uri, 2))));
		}

		[Test]
		public async Task WhenDuplicatesFoundBetweenPages_ShouldGetMorePosts()
		{
			// Arrange 
			_postsPage2[2].Id = _postsPage1[0].Id;

			var subject = _fixture.Create<HackerNewsScraper>();

			// Act 
			await subject.GetTopPostsAsync(_postsPage1.Count + _postsPage2.Count);

			// Assert
			_httpClientServiceMock.Verify(x => x.GetHtmlContentAsync(It.Is<Uri>(uri => IsEqualToPage(uri, 3))));
		}

		[Test]
		public async Task WhenDuplicatesFoundBetweenPages_ShouldReturnDeduplicatedPosts()
		{
			// Arrange 
			var expectedPost1 = _fixture.Build<HackerNewsPost>()
				.With(x => x.Rank, 1).Create();
			var expectedPost2 = _fixture.Build<HackerNewsPost>()
				.With(x => x.Rank, 2).Create();

			_postsPage1.RemoveAll(_ => true);
			_postsPage2.RemoveAll(_ => true);
			_postsPage3.RemoveAll(_ => true);

			_postsPage1.Add(expectedPost1);
			_postsPage2.Add(expectedPost1);
			_postsPage3.Add(expectedPost2);

			var subject = _fixture.Create<HackerNewsScraper>();

			// Act 
			var result = await subject.GetTopPostsAsync(2);

			// Assert
			Assert.That(result.Count, Is.EqualTo(2));

			AssertPostsMatch(result.First(), expectedPost1);
			AssertPostsMatch(result.Skip(1).First(), expectedPost2);
		}

		[Test]
		public async Task WhenPostsNotFoundIn15Pages_ShouldThrowException()
		{
			// Arrange 
			Assert.Fail();
			//var subject = _fixture.Create<HackerNewsScraper>();

			//// Act 
			//await subject.GetTopPostsAsync(_postsPage1.Count + 1);

			//// Assert
			//_httpClientServiceMock.Verify(x => x.GetHtmlContentAsync(It.Is<Uri>(uri => IsEqualToPage(uri, 2))));
		}

		private bool IsEqualToPage(Uri uri, int page)
			=> uri.AbsoluteUri == BuildPage(page).AbsoluteUri;

		private Uri BuildPage(int page)
			=> new Uri($"https://news.ycombinator.com/news?p={page}");

		private void AssertPostsMatch(Domain.HackerNews.Post actual, HackerNewsPost expected)
		{
			Assert.That(actual.Author, Is.EqualTo(expected.Author));
			Assert.That(actual.Comments, Is.EqualTo(expected.Comments));
			Assert.That(actual.Points, Is.EqualTo(expected.Points));
			Assert.That(actual.Rank, Is.EqualTo(expected.Rank));
			Assert.That(actual.Title, Is.EqualTo(expected.Title));
			Assert.That(actual.Uri, Is.EqualTo(expected.Uri));
		}
	}
}
