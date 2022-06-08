using Common.Logging;
using MicrosoftTeams.DataOperations;
using MicrosoftTeams.Interfaces;
using MicrosoftTeams.Models.OAuthApplicationModel;
using MicrosoftTeams.Token;
using Newtonsoft.Json;
using NSubstitute;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Terrasoft.Configuration.Tests;
using Terrasoft.Core.DB;

namespace MicrosoftTeams.Tests
{
	[TestFixture]
	[MockSettings(RequireMock.All)]
	internal class TokenHandlerTests : BaseConfigurationTestFixture
	{
		private TokenHandler _sut;
		private ILog _logger;
		private HttpClient httpClientMock;
		private IOAuthHandler ouathHandlerMock;

		protected override void SetUp()
		{
			base.SetUp();
			UserConnection.DBEngine = Substitute.ForPartsOf<DBEngine>();

			ouathHandlerMock = Substitute.For<IOAuthHandler>();
			ouathHandlerMock.GetOAuthApplicationModel().Returns(Consts.ApplicationSettings);

			var myArg = Arg.Any<Dto.Token>();
			ouathHandlerMock.TryGetTokenByUserId(out myArg)
				.Returns(c => {
					c[0] = Consts.Token;
					return true;
				});

			_logger = LogManager.GetLogger("");
			httpClientMock = new HttpClient();
			var factoryMock = Substitute.For<IHttpClientFactory>();
			factoryMock.CreateClient(Arg.Is<string>("OAuth")).Returns(httpClientMock);

			_sut = new TokenHandler(factoryMock, ouathHandlerMock, _logger);
		}

		[Test(Author = "Kirill Krylov", Description = "Checks that all request params are set correctly")]
		public void GetParameters_ShouldReturn() {

			//Arrange
			string expectedScopes = string.Join(" ", Consts.ApplicationSettings.Scopes);

			//Assert
			Assert.Multiple(() =>
			{
				var result  = _sut.GetParameters(Consts.Code);
				Assert.That(result, Has.Exactly(6).Items);
				Assert.That(result["client_id"], Is.EqualTo(Consts.ApplicationSettings.ClientId));
				Assert.That(result["grant_type"], Is.EqualTo("authorization_code"));
				Assert.That(result["scope"], Is.EqualTo(expectedScopes));
				Assert.That(result["redirect_uri"], Is.EqualTo(Consts.ApplicationSettings.RedirectUrl));
				Assert.That(result["client_secret"], Is.EqualTo(Consts.ApplicationSettings.ClientSecret));
				Assert.That(result["code"], Is.EqualTo(Consts.Code));
			});
		}

		[Test(Author = "Kirill Krylov", Description = "Checks that all request params are set correctly")]
		public void GetRefreshParameters_ShouldReturn()
		{

			//Arrange
			string expectedScopes = string.Join(" ", Consts.ApplicationSettings.Scopes);
	
			//Assert
			Assert.Multiple(() =>
			{
				var result = _sut.GetRefreshParameters();
				Assert.That(result, Has.Exactly(6).Items);
				Assert.That(result["client_id"], Is.EqualTo(Consts.ApplicationSettings.ClientId));
				Assert.That(result["grant_type"], Is.EqualTo("refresh_token"));
				Assert.That(result["scope"], Is.EqualTo(expectedScopes));
				Assert.That(result["refresh_token"], Is.EqualTo(Consts.Token.RefreshToken));
				Assert.That(result["redirect_uri"], Is.EqualTo(Consts.ApplicationSettings.RedirectUrl));
				Assert.That(result["client_secret"], Is.EqualTo(Consts.ApplicationSettings.ClientSecret));
			});
		}

		#region Check that it Throws
		[Test(Author = "Kirill Krylov")]
		public void GetParameters_ShouldThrow_OnMIssing_RedirectUrl()
		{
			//Arrange
			IOAuthApplicationModel ApplicationSettings = new OAuthApplicationModel
			{
				AuthorizeUrl = Consts.ApplicationSettings.AuthorizeUrl,
				LogoutUrl = Consts.ApplicationSettings.LogoutUrl,
				RevokeTokenUrl = Consts.ApplicationSettings.RevokeTokenUrl,
				TokenUrl = Consts.ApplicationSettings.TokenUrl,
				ClientId = Consts.ApplicationSettings.ClientId,
				ClientSecret = Consts.ApplicationSettings.ClientSecret,
				//RedirectUrl = Consts.ApplicationSettings.RedirectUrl,
				Scopes = Consts.ApplicationSettings.Scopes
			};

			var factoryMock = Substitute.For<IHttpClientFactory>();
			factoryMock.CreateClient(Arg.Is<string>("OAuth")).Returns(httpClientMock);

			var ouathHandlerMock1 = Substitute.For<IOAuthHandler>();
			ouathHandlerMock1.GetOAuthApplicationModel().Returns(ApplicationSettings);
			var sut = new TokenHandler(factoryMock, ouathHandlerMock1, _logger);

			string expectedMessage = "RedirectUrl cannot be empty";

			//Act
			var ex = Assert.Throws<KeyNotFoundException>(()=> {
				sut.GetParameters(Consts.Code);
			});

			Assert.That(ex.Message, Is.EqualTo(expectedMessage));

		}
		
		[Test(Author = "Kirill Krylov")]
		public void GetParameters_ShouldThrow_OnMIssing_ClientSecret()
		{
			//Arrange
			IOAuthApplicationModel ApplicationSettings = new OAuthApplicationModel
			{
				AuthorizeUrl = Consts.ApplicationSettings.AuthorizeUrl,
				LogoutUrl = Consts.ApplicationSettings.LogoutUrl,
				RevokeTokenUrl = Consts.ApplicationSettings.RevokeTokenUrl,
				TokenUrl = Consts.ApplicationSettings.TokenUrl,
				ClientId = Consts.ApplicationSettings.ClientId,
				//ClientSecret = Consts.ApplicationSettings.ClientSecret,
				RedirectUrl = Consts.ApplicationSettings.RedirectUrl,
				Scopes = Consts.ApplicationSettings.Scopes
			};


			var ouathHandlerMock1 = Substitute.For<IOAuthHandler>();
			ouathHandlerMock1.GetOAuthApplicationModel().Returns(ApplicationSettings);

			var factoryMock = Substitute.For<IHttpClientFactory>();
			factoryMock.CreateClient(Arg.Is<string>("OAuth")).Returns(httpClientMock);


			var sut = new TokenHandler(factoryMock, ouathHandlerMock1, _logger);

			string expectedMessage = "ClientSecret cannot be empty";

			//Act
			var ex = Assert.Throws<KeyNotFoundException>(()=> {
				sut.GetParameters(Consts.Code);
			});

			Assert.That(ex.Message, Is.EqualTo(expectedMessage));

		}
		
		[Test(Author = "Kirill Krylov")]
		public void GetParameters_ShouldThrow_OnMIssing_Code()
		{
			//Arrange
			IOAuthApplicationModel ApplicationSettings = new OAuthApplicationModel
			{
				AuthorizeUrl = Consts.ApplicationSettings.AuthorizeUrl,
				LogoutUrl = Consts.ApplicationSettings.LogoutUrl,
				RevokeTokenUrl = Consts.ApplicationSettings.RevokeTokenUrl,
				TokenUrl = Consts.ApplicationSettings.TokenUrl,
				ClientId = Consts.ApplicationSettings.ClientId,
				ClientSecret = Consts.ApplicationSettings.ClientSecret,
				RedirectUrl = Consts.ApplicationSettings.RedirectUrl,
				Scopes = Consts.ApplicationSettings.Scopes
			};

			var ouathHandlerMock1 = Substitute.For<IOAuthHandler>();
			ouathHandlerMock1.GetOAuthApplicationModel().Returns(ApplicationSettings);

			var factoryMock = Substitute.For<IHttpClientFactory>();
			factoryMock.CreateClient(Arg.Is<string>("OAuth")).Returns(httpClientMock);


			var sut = new TokenHandler(factoryMock, ouathHandlerMock1, _logger);

			string expectedMessage = "Code cannot be empty";

			//Act
			var ex = Assert.Throws<KeyNotFoundException>(()=> {
				sut.GetParameters(null);
			});

			Assert.That(ex.Message, Is.EqualTo(expectedMessage));

		}
		
		[Test(Author = "Kirill Krylov")]
		public void GetParameters_ShouldThrow_OnMIssing_ClientId()
		{
			//Arrange
			IOAuthApplicationModel ApplicationSettings = new OAuthApplicationModel
			{
				AuthorizeUrl = new Uri("https://login.microsoftonline.com"),
				LogoutUrl = new Uri("https://login.microsoftonline.com"),
				RevokeTokenUrl = new Uri("https://login.microsoftonline.com"),
				TokenUrl = new Uri("https://login.microsoftonline.com"),
				
				ClientSecret = "",
				RedirectUrl = "",
				Scopes = new List<string>
				{
					{"Calendars.ReadWrite"},
					{"offline_access"},
					{"OnlineMeetingArtifact.Read.All"},
					{"OnlineMeetings.ReadWrite"},
					{"User.Read"}
				}
			};


			var ouathHandlerMock1 = Substitute.For<IOAuthHandler>();
			ouathHandlerMock1.GetOAuthApplicationModel().Returns(ApplicationSettings);

			var factoryMock = Substitute.For<IHttpClientFactory>();
			factoryMock.CreateClient(Arg.Is<string>("OAuth")).Returns(httpClientMock);


			var sut = new TokenHandler(factoryMock, ouathHandlerMock1, _logger);

			string expectedMessage = "ClientId cannot be empty";

			//Act
			var ex = Assert.Throws<KeyNotFoundException>(()=> {
				sut.GetParameters(Consts.Code);
			});

			Assert.That(ex.Message, Is.EqualTo(expectedMessage));

		}
		#endregion

		[Test(Author = "Kirill Krylov", Description = "Checks that httpMessage is configured correctly")]
		public async Task PrepareMessage_ShouldReturn()
		{
			//Arrage
			string expectedScopes = string.Join(" ", Consts.ApplicationSettings.Scopes);
			string expectedUrl = $"/{Consts.Tenantid}/oauth2/v2.0/token";
			var expectedContentTypeHeader = new MediaTypeHeaderValue("application/x-www-form-urlencoded");

			string expectedContent = $"" +
				$"client_id={Consts.ApplicationSettings.ClientId}" +
				$"&scope={string.Join(" ", Consts.ApplicationSettings.Scopes)}" +
				$"&code={Consts.Code}" +
				$"&redirect_uri={Consts.ApplicationSettings.RedirectUrl}" +
				$"&grant_type=authorization_code" +
				$"&client_secret={Consts.ApplicationSettings.ClientSecret}";

			Dictionary<string, string> parameters = new Dictionary<string, string>
			{
				{ "client_id", Consts.ApplicationSettings.ClientId },
				{ "scope", expectedScopes},
				{ "code", Consts.Code},
				{ "redirect_uri", Consts.ApplicationSettings.RedirectUrl},
				{ "grant_type", "authorization_code"},
				{ "client_secret", Consts.ApplicationSettings.ClientSecret},
			};

			Assert.Multiple(async () =>
			{
				var result = await _sut.PrepareMessage(parameters);
				var MediaTypeHeaderValuex = result.Content.Headers.ContentType;

				Assert.That(result.Content.Headers.ContentType, Is.EqualTo(expectedContentTypeHeader));
				Assert.That(result.Method, Is.EqualTo(HttpMethod.Post));
				Assert.That(result.RequestUri.ToString(), Is.EqualTo(expectedUrl));
				string content = await result.Content.ReadAsStringAsync();
				Assert.That(HttpUtility.UrlDecode(content), Is.EqualTo(expectedContent).IgnoreCase);
			});
		}


		[Test(Author = "Kirill Krylov", Description = "Checks that httpMessage is configured correctly")]
		public async Task SendInternalAsync_ShouldReturn()
		{

			//Arrange 
			var ouathHandlerMock1 = Substitute.For<IOAuthHandler>();
			ouathHandlerMock1.GetOAuthApplicationModel().Returns(Consts.ApplicationSettings);
			string expectedContent = "random string";

			var routeHandler = new MockEmptyRouteHandler(HttpStatusCode.OK, expectedContent);
			HttpClient mockClient = new HttpClient(routeHandler)
			{
				BaseAddress = Consts.ApplicationSettings.TokenUrl
			};


			var factoryMock = Substitute.For<IHttpClientFactory>();
			factoryMock.CreateClient(Arg.Is<string>("OAuth")).Returns(mockClient);

			TokenHandler sut = new TokenHandler(factoryMock, ouathHandlerMock1, _logger);
			HttpRequestMessage message = new HttpRequestMessage();

			//Act
			var result = await sut.SendInternalAsync(message);
			Assert.That(result, Is.EqualTo(expectedContent));
		}


		[Test(Author = "Kirill Krylov", Description = "")]
		public async Task OAuthCallBack_ShouldReturnFailedMsg()
		{
			var ouathHandlerMock1 = Substitute.For<IOAuthHandler>();
			ouathHandlerMock1.GetOAuthApplicationModel().Returns(Consts.ApplicationSettings);
			string expectedContent = JsonConvert.SerializeObject(Consts.Token);

			var routeHandler = new MockEmptyRouteHandler(HttpStatusCode.OK, expectedContent);
			HttpClient mockClient = new HttpClient(routeHandler)
			{
				BaseAddress = Consts.ApplicationSettings.TokenUrl
			};


			var factoryMock = Substitute.For<IHttpClientFactory>();
			factoryMock.CreateClient(Arg.Is<string>("OAuth")).Returns(mockClient);

			TokenHandler sut = new TokenHandler(factoryMock, ouathHandlerMock1, _logger);

			var result = await sut.OAuthCallBack(Consts.Code, Consts.SessionState);

			Assert.That(result, Is.EqualTo("Failed to save new token"));
		}
		
		[Test(Author = "Kirill Krylov", Description = "")]
		public async Task OAuthCallBack_ShouldReturnSuccessMsg()
		{
			var ouathHandlerMock1 = Substitute.For<IOAuthHandler>();
			ouathHandlerMock1.GetOAuthApplicationModel().Returns(Consts.ApplicationSettings);
			ouathHandlerMock1.UpsertToken(Arg.Any<Dto.Token>()).Returns(true);

			string expectedContent = JsonConvert.SerializeObject(Consts.Token);

			var routeHandler = new MockEmptyRouteHandler(HttpStatusCode.OK, expectedContent);
			HttpClient mockClient = new HttpClient(routeHandler)
			{
				BaseAddress = Consts.ApplicationSettings.TokenUrl
			};

			var factoryMock = Substitute.For<IHttpClientFactory>();
			factoryMock.CreateClient(Arg.Is<string>("OAuth")).Returns(mockClient);

			TokenHandler sut = new TokenHandler(factoryMock, ouathHandlerMock1, _logger);
			var result = await sut.OAuthCallBack(Consts.Code, Consts.SessionState);

			Assert.That(result, Is.EqualTo("Record Added"));
		}

		[Test(Author = "Kirill Krylov", Description = "")]
		public async Task RefreshToken_ShouldSucceed()
		{
			string expectedContent = JsonConvert.SerializeObject(Consts.Token);
			var routeHandler = new MockEmptyRouteHandler(HttpStatusCode.OK, expectedContent);
			HttpClient mockClient = new HttpClient(routeHandler)
			{
				BaseAddress = Consts.ApplicationSettings.TokenUrl
			};

			var factoryMock = Substitute.For<IHttpClientFactory>();
			factoryMock.CreateClient(Arg.Is<string>("OAuth")).Returns(mockClient);
			ouathHandlerMock.UpsertToken(Arg.Any<Dto.Token>()).Returns(true);

			_sut = new TokenHandler(factoryMock, ouathHandlerMock, _logger);

			await _sut.RefreshToken();
			ouathHandlerMock.Received(1).UpsertToken(Arg.Any<Dto.Token>());
		}
	}
}
