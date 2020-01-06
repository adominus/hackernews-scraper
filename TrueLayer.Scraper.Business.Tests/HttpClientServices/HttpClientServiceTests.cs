using AutoFixture;
using AutoFixture.AutoMoq;
using NUnit.Framework;
using RichardSzalay.MockHttp;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using TrueLayer.Scraper.Business.HttpClientServices;

namespace TrueLayer.Scraper.Business.Tests.HttpClientServices
{
	public class HttpClientServiceTests
	{
		private IFixture _fixture;

		private string _address;

		[OneTimeSetUp]
		public void OneTimeSetUp()
		{
			_fixture = new Fixture()
				.Customize(new AutoMoqCustomization());

			_address = "http://localhost";
		}

		[Test]
		public async Task ShouldMakeGetRequestToAddress()
		{
			// Arrange 
			var mockHttpMessageHandler = new MockHttpMessageHandler();
			mockHttpMessageHandler.Expect(HttpMethod.Get, _address);

			_fixture.Inject(mockHttpMessageHandler.ToHttpClient());

			var subject = _fixture.Create<HttpClientService>();

			//  Act 
			await subject.GetHtmlContentAsync(_address);

			// Assert
			mockHttpMessageHandler.VerifyNoOutstandingExpectation();
		}

		[Test]
		public async Task WhenContentTypeIsHtml_ShouldReturnContent()
		{
			// Arrange 
			var expectedResult = _fixture.Create<string>();

			var mockHttpMessageHandler = new MockHttpMessageHandler();
			mockHttpMessageHandler.When(HttpMethod.Get, _address)
				.Respond("text/html", expectedResult);

			_fixture.Inject(mockHttpMessageHandler.ToHttpClient());

			var subject = _fixture.Create<HttpClientService>();

			//  Act 
			var result = await subject.GetHtmlContentAsync(_address);

			// Assert
			Assert.That(result, Is.EqualTo(expectedResult));
		}

		[Test]
		public async Task WhenContentTypeIsNotHtml_ShouldReturnNull()
		{
			// Arrange 
			var mockHttpMessageHandler = new MockHttpMessageHandler();
			mockHttpMessageHandler.When(HttpMethod.Get, _address)
				.Respond("application/json", "{'foo':'bar'}");

			_fixture.Inject(mockHttpMessageHandler.ToHttpClient());

			var subject = _fixture.Create<HttpClientService>();

			//  Act 
			var result = await subject.GetHtmlContentAsync(_address);

			// Assert
			Assert.That(result, Is.Null);
		}

		[Test]
		public async Task WhenStatusCodeIsNotSuccessful_ShouldReturnNull()
		{
			// Arrange 
			var mockHttpMessageHandler = new MockHttpMessageHandler();
			mockHttpMessageHandler.When(HttpMethod.Get, _address)
				.Respond((HttpStatusCode)(_fixture.Create<int>() + 300), "text/html", _fixture.Create<string>());

			_fixture.Inject(mockHttpMessageHandler.ToHttpClient());

			var subject = _fixture.Create<HttpClientService>();

			//  Act 
			var result = await subject.GetHtmlContentAsync(_address);

			// Assert
			Assert.That(result, Is.Null);
		}
	}
}
