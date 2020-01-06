using AutoFixture;
using AutoFixture.AutoMoq;
using NUnit.Framework;
using RichardSzalay.MockHttp;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using TrueLayer.Scraper.Business.HttpClientServices;

namespace TrueLayer.Scraper.Business.Tests.HttpClientServices
{
	public class HttpClientServiceTests
	{
		private IFixture _fixture;

		private Uri _address;

		[OneTimeSetUp]
		public void OneTimeSetUp()
		{
			_fixture = new Fixture()
				.Customize(new AutoMoqCustomization());

			_address = _fixture.Create<Uri>();
		}

		[Test]
		public async Task ShouldMakeGetRequestToAddress()
		{
			// Arrange 
			var mockHttpMessageHandler = new MockHttpMessageHandler();
			mockHttpMessageHandler.Expect(HttpMethod.Get, _address.AbsoluteUri);

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
			mockHttpMessageHandler.When(HttpMethod.Get, _address.AbsoluteUri)
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
			mockHttpMessageHandler.When(HttpMethod.Get, _address.AbsoluteUri)
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
			mockHttpMessageHandler.When(HttpMethod.Get, _address.AbsoluteUri)
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
