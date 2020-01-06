using AutoFixture;
using NUnit.Framework;
using TrueLayer.Scraper.Business.HackerNews;

namespace TrueLayer.Scraper.Business.Tests.HackerNews
{
	public class HackerNewsPostValidatorTests
	{
		private IFixture _fixture;

		private HackerNewsPostValidator _subject;

		private HackerNewsPost _post;

		[OneTimeSetUp]
		public void OneTimeSetUp()
		{
			_fixture = new Fixture();
		}

		[SetUp]
		public void SetUp()
		{
			_subject = _fixture.Create<HackerNewsPostValidator>();

			_post = new HackerNewsPost
			{
				Author = "foo",
				Comments = 1,
				Href = "http://localhost/bar",
				Id = "baz",
				Points = 2,
				Rank = 3,
				Title = "qux"
			};
		}

		[Test]
		public void WhenPostIsValid_ShouldReturnTrue()
		{
			// Arrange, Act
			var result = _subject.IsValid(_post);

			// Assert
			Assert.That(result, Is.True);
		}

		[Test]
		[TestCase("")]
		[TestCase(" ")]
		[TestCase(null)]
		public void WhenTitleIsNullOrWhitespace_ShouldReturnFalse(string title)
		{
			// Arrange
			_post.Title = title;

			// Act
			var result = _subject.IsValid(_post);

			// Assert
			Assert.That(result, Is.False);
		}

		[Test]
		[TestCase("257-zzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzz")]
		[TestCase("258-zzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzz")]
		public void WhenTitleIsOver256Characters_ShouldReturnFalse(string title)
		{
			// Arrange
			_post.Title = title;

			// Act
			var result = _subject.IsValid(_post);

			// Assert
			Assert.That(result, Is.False);
		}

		[Test]
		[TestCase("")]
		[TestCase(" ")]
		[TestCase(null)]
		public void WhenAuthorIsNullOrWhitespace_ShouldReturnFalse(string author)
		{
			// Arrange
			_post.Author = author;

			// Act
			var result = _subject.IsValid(_post);

			// Assert
			Assert.That(result, Is.False);
		}

		[Test]
		[TestCase("257-zzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzz")]
		[TestCase("258-zzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzz")]
		public void WhenAuthorIsOver256Characters_ShouldReturnFalse(string author)
		{
			// Arrange
			_post.Author = author;

			// Act
			var result = _subject.IsValid(_post);

			// Assert
			Assert.That(result, Is.False);
		}

		[Test]
		[TestCase(-1)]
		[TestCase(-12)]
		[TestCase(-123)]
		public void WhenPointsIsANegativeInteger_ShouldReturnFalse(int points)
		{
			// Arrange
			_post.Points = points;

			// Act
			var result = _subject.IsValid(_post);

			// Assert
			Assert.That(result, Is.False);
		}

		[Test]
		[TestCase(-1)]
		[TestCase(-12)]
		[TestCase(-123)]
		public void WhenCommentsIsANegativeInteger_ShouldReturnFalse(int comments)
		{
			// Arrange
			_post.Comments = comments;

			// Act
			var result = _subject.IsValid(_post);

			// Assert
			Assert.That(result, Is.False);
		}

		[Test]
		[TestCase(-1)]
		[TestCase(-12)]
		[TestCase(-123)]
		public void WhenRankIsANegativeInteger_ShouldReturnFalse(int rank)
		{
			// Arrange
			_post.Rank = rank;

			// Act
			var result = _subject.IsValid(_post);

			// Assert
			Assert.That(result, Is.False);
		}

		[Test]
		public void WhenHrefIsNotValid_ShouldReturnFalse()
		{
			// Arrange
			_post.Href = _fixture.Create<string>();

			// Act
			var result = _subject.IsValid(_post);

			// Assert
			Assert.That(result, Is.False);
		}
	}
}
