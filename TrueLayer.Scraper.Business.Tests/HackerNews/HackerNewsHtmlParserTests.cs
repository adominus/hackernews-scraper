using AutoFixture;
using NUnit.Framework;
using System;
using System.Linq;
using System.Reflection;
using TrueLayer.Scraper.Business.HackerNews;

namespace TrueLayer.Scraper.Business.Tests.HackerNews
{
	public class HackerNewsHtmlParserTests
	{
		private IFixture _fixture;

		private HackerNewsHtmlParser _subject;

		[OneTimeSetUp]
		public void OneTimeSetUp()
		{
			_fixture = new Fixture();
		}

		[SetUp]
		public void SetUp()
		{
			_subject = _fixture.Create<HackerNewsHtmlParser>();
		}

		[Test]
		public void WhenSinglePost_ShouldReturn1Post()
		{
			// Arrange 
			var html = GetResourceFileContents("SinglePost.html");

			// Act 
			var result = _subject.ParsePosts(html);

			// Assert
			Assert.That(result.Count, Is.EqualTo(1));

			var singlePost = result.Single();

			Assert.That(singlePost.Id, Is.EqualTo("PostId"));
			Assert.That(singlePost.Title, Is.EqualTo("Post Title"));
			Assert.That(singlePost.Href, Is.EqualTo("https://localhost/uri"));
			Assert.That(singlePost.Rank, Is.EqualTo(1));
			Assert.That(singlePost.Author, Is.EqualTo("author name"));
			Assert.That(singlePost.Comments, Is.EqualTo(1));
			Assert.That(singlePost.Points, Is.EqualTo(1));
		}

		[Test]
		public void WhenNoComments_ShouldReturnPostWithZeroComments()
		{
			// Arrange 
			var html = GetResourceFileContents("NoComments.html");

			// Act 
			var result = _subject.ParsePosts(html);

			// Assert
			Assert.That(result.Count, Is.EqualTo(1));

			var singlePost = result.Single();

			Assert.That(singlePost.Id, Is.EqualTo("21970184"));
			Assert.That(singlePost.Title, Is.EqualTo("I Do (Hopefully) Fair Performance Reviews for Software Developers"));
			Assert.That(singlePost.Href, Is.EqualTo("https://blog.pragmaticengineer.com/performance-reviews-for-software-engineers/"));
			Assert.That(singlePost.Rank, Is.EqualTo(7));
			Assert.That(singlePost.Author, Is.EqualTo("2bluesc"));
			Assert.That(singlePost.Comments, Is.EqualTo(0));
			Assert.That(singlePost.Points, Is.EqualTo(9));
		}

		[Test]
		public void WhenNoCommentsAuthorOrPoints_ShouldReturnPostWithZeroCommentsNullAuthorAndNoPoints()
		{
			// Arrange 
			var html = GetResourceFileContents("NoCommentsAuthorOrPoints.html");

			// Act 
			var result = _subject.ParsePosts(html);

			// Assert
			Assert.That(result.Count, Is.EqualTo(1));

			var singlePost = result.Single();

			Assert.That(singlePost.Id, Is.EqualTo("21969089"));
			Assert.That(singlePost.Title, Is.EqualTo("SafetyWing (YC W18) is hiring a head of sales"));
			Assert.That(singlePost.Href, Is.EqualTo("https://workew.com/job/head-of-sales-safetywing/"));
			Assert.That(singlePost.Rank, Is.EqualTo(25));
			Assert.That(singlePost.Author, Is.Null);
			Assert.That(singlePost.Comments, Is.Zero);
			Assert.That(singlePost.Points, Is.Zero);
		}

		[Test]
		public void WhenMultiplePosts_ShouldReturnExpectedPosts()
		{
			// Arrange 
			var html = GetResourceFileContents("MultiplePosts.html");

			// Act 
			var results = _subject.ParsePosts(html).ToList();

			// Assert
			Assert.That(results.Count, Is.EqualTo(3));

			Assert.That(results[0].Id, Is.EqualTo("PostId"));
			Assert.That(results[0].Title, Is.EqualTo("Post Title"));
			Assert.That(results[0].Href, Is.EqualTo("https://localhost/uri"));
			Assert.That(results[0].Rank, Is.EqualTo(1));
			Assert.That(results[0].Author, Is.EqualTo("author name"));
			Assert.That(results[0].Comments, Is.EqualTo(1));
			Assert.That(results[0].Points, Is.EqualTo(1));
		
			Assert.That(results[1].Id, Is.EqualTo("PostId2"));
			Assert.That(results[1].Title, Is.EqualTo("2nd Post Title"));
			Assert.That(results[1].Href, Is.EqualTo("https://localhost/uri-2"));
			Assert.That(results[1].Rank, Is.EqualTo(2));
			Assert.That(results[1].Author, Is.EqualTo("2nd author name"));
			Assert.That(results[1].Comments, Is.EqualTo(2));
			Assert.That(results[1].Points, Is.EqualTo(2));
		
			Assert.That(results[2].Id, Is.EqualTo("PostId3"));
			Assert.That(results[2].Title, Is.EqualTo("3rd Post Title"));
			Assert.That(results[2].Href, Is.EqualTo("https://localhost/uri-3"));
			Assert.That(results[2].Rank, Is.EqualTo(3));
			Assert.That(results[2].Author, Is.EqualTo("3rd author name"));
			Assert.That(results[2].Comments, Is.EqualTo(3));
			Assert.That(results[2].Points, Is.EqualTo(3));
		}

		[Test]
		public void WhenMalformedItemList_ShouldThrowException()
		{
			// Arrange 
			var html = GetResourceFileContents("MalformedItemList.html");

			// Act 
			TestDelegate act = () => _subject.ParsePosts(html).ToList();

			// Assert
			Assert.That(act, Throws.TypeOf<Exception>());
		}

		private static string GetResourceFileContents(string fileName)
		{
			var currentDirectory = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			var filePath = System.IO.Path.Combine(currentDirectory, @"HackerNews\Resources", fileName);

			return System.IO.File.ReadAllText(filePath);
		}
	}
}
