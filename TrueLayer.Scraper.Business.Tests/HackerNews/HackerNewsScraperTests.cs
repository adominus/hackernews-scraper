using AutoFixture;
using AutoFixture.AutoMoq;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TrueLayer.Scraper.Business.Exceptions;
using TrueLayer.Scraper.Business.HackerNews;
using TrueLayer.Scraper.Business.HttpClientServices;

namespace TrueLayer.Scraper.Business.Tests.HackerNews
{
	public class HackerNewsScraperTests
	{
		private IFixture _fixture;
		private Mock<IHttpClientService> _httpClientServiceMock;
		private Mock<IHackerNewsHtmlParser> _hackerNewsHtmlParserMock;
		private Mock<IHackerNewsPostValidator> _hackerNewsPostValidatorMock;

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
			_hackerNewsPostValidatorMock = _fixture.Freeze<Mock<IHackerNewsPostValidator>>();
		}

		[SetUp]
		public void SetUp()
		{
			_fixture.Register(() => _fixture.Build<HackerNewsPost>()
				.With(x => x.Href, _fixture.Create<Uri>().AbsoluteUri).Create());

			SetUpMockedPages();

			_hackerNewsPostValidatorMock.Setup(x => x.IsValid(It.IsAny<HackerNewsPost>()))
				.Returns(true);
		}

		private void SetUpMockedPages()
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
			_hackerNewsPostValidatorMock.Reset();
		}

		[Test]
		public async Task ShouldRequestHackerNewsContent()
		{
			// Arrange 
			var subject = _fixture.Create<HackerNewsScraper>();

			// Act 
			await subject.GetTopPostsAsync(1);

			// Assert
			_httpClientServiceMock.Verify(x => x.GetHtmlContentAsync(It.Is<string>(path => IsEqualToPage(path, 1))));
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
		public async Task ShouldValidateParsedPosts()
		{
			// Arrange 
			var subject = _fixture.Create<HackerNewsScraper>();

			// Act 
			await subject.GetTopPostsAsync(1);

			// Assert
			_hackerNewsPostValidatorMock.Verify(x => x.IsValid(It.IsAny<HackerNewsPost>()),
				Times.Exactly(_postsPage1.Count));

			foreach (var post in _postsPage1)
			{
				_hackerNewsPostValidatorMock.Verify(x => x.IsValid(post));
			}
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
		public async Task WhenHrefIsRelative_ShouldAssumeToHackerNews()
		{
			// Arrange 
			var expectedPost = _fixture.Create<HackerNewsPost>();
			expectedPost.Href = "foo";
			_postsPage1.RemoveAll(_ => true);
			_postsPage1.Add(expectedPost);

			var subject = _fixture.Create<HackerNewsScraper>();

			// Act 
			var result = await subject.GetTopPostsAsync(1);

			// Assert
			Assert.That(result.Single().Uri.AbsoluteUri, Is.EqualTo("https://news.ycombinator.com/foo"));
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
			_httpClientServiceMock.Verify(x => x.GetHtmlContentAsync(It.Is<string>(path => IsEqualToPage(path, 2))));
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
			_httpClientServiceMock.Verify(x => x.GetHtmlContentAsync(It.Is<string>(path => IsEqualToPage(path, 3))));
		}

		[Test]
		public async Task WhenDuplicatesFoundBetweenPages_ShouldReturnDeduplicatedPosts()
		{
			// Arrange 
			var expectedPost1 = _fixture.Create<HackerNewsPost>();
			expectedPost1.Rank = 1;
			var expectedPost2 = _fixture.Create<HackerNewsPost>();
			expectedPost2.Rank = 2;

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
		public async Task WhenPostsAreInvalid_ShouldSkipPost()
		{
			// Arrange 
			_hackerNewsPostValidatorMock.Setup(x => x.IsValid(_postsPage1[0]))
				.Returns(false);
			_hackerNewsPostValidatorMock.Setup(x => x.IsValid(_postsPage1[1]))
				.Returns(false);
			_hackerNewsPostValidatorMock.Setup(x => x.IsValid(_postsPage1[2]))
				.Returns(false);

			var expectedPost = _postsPage2[0];
			expectedPost.Rank = 1;
			_postsPage2[1].Rank = 2;
			_postsPage2[2].Rank = 3;

			var subject = _fixture.Create<HackerNewsScraper>();

			// Act 
			var result = await subject.GetTopPostsAsync(1);

			// Assert
			Assert.That(result.Count, Is.EqualTo(1));

			AssertPostsMatch(result.Single(), expectedPost);
		}

		[Test]
		public void WhenPostsNotFoundIn15Pages_ShouldThrowException()
		{
			// Arrange 
			var singlePost = _fixture.Create<HackerNewsPost>();

			for (var i = 1; i <= 15; i++)
			{
				var htmlContent = _fixture.Create<string>();
				_httpClientServiceMock.Setup(x => x.GetHtmlContentAsync(BuildPage(1)))
					.ReturnsAsync(htmlContent);

				_hackerNewsHtmlParserMock.Setup(x => x.ParsePosts(htmlContent))
					.Returns(new[] { singlePost });
			}

			var subject = _fixture.Create<HackerNewsScraper>();

			// Act 
			AsyncTestDelegate act = () => subject.GetTopPostsAsync(15);

			// Assert
			Assert.That(act, Throws.TypeOf<SearchDepthExceededException>());
		}

		private bool IsEqualToPage(string path, int page) => path == BuildPage(page);

		private string BuildPage(int page) => $"/news?p={page}";

		private void AssertPostsMatch(Domain.HackerNews.Post actual, HackerNewsPost expected)
		{
			Assert.That(actual.Author, Is.EqualTo(expected.Author));
			Assert.That(actual.Comments, Is.EqualTo(expected.Comments));
			Assert.That(actual.Points, Is.EqualTo(expected.Points));
			Assert.That(actual.Rank, Is.EqualTo(expected.Rank));
			Assert.That(actual.Title, Is.EqualTo(expected.Title));
			Assert.That(actual.Uri.AbsoluteUri, Is.EqualTo(expected.Href));
		}
	}
}
