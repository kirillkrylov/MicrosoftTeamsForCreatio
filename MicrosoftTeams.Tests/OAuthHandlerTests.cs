using Common.Logging;
using MicrosftTeams.WebServices.OAuthHandler;
using MicrosoftTeams.DataOperations;
using MicrosoftTeams.Interfaces;
using NSubstitute;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Web.SessionState;
using Terrasoft.Common;
using Terrasoft.Configuration.Tests;
using Terrasoft.Core;
using Terrasoft.Core.DB;
using Terrasoft.Core.Entities;
using Terrasoft.Core.Factories;
using Terrasoft.TestFramework;
using Terrasoft.Web.Http.Abstractions;

namespace MicrosoftTeams.Tests
{

	[TestFixture]
	[MockSettings(RequireMock.All)]
	public class OAuthHandlerTests : BaseConfigurationTestFixture
	{
		
		private IOAuthHandler _sut;
		private IApplication mockApp;
		EntitySchema _mockOAuthTokenStorage;
		private ILog _logger;

		protected override void SetUp()
		{
			base.SetUp();
			UserConnection.DBEngine = Substitute.ForPartsOf<DBEngine>();
			_logger = LogManager.GetLogger("");

			var x = Substitute.For<IHttpContextAccessor>();
			x.GetInstance().Returns(callback => {
				var mc = new MyHttpContextAccessor();
				return mc.GetInstance();
			});

			ClassFactory.ReBind<IHttpContextAccessor, MyHttpContextAccessor>();

			mockApp = new Builder()
				.ConfigureUserConnection(UserConnection)
				.ConfigureLogger(LogManager.GetLogger("MsTeamsLogger"))
				.Build();

			EntitySchemaManager.AddCustomizedEntitySchema("OAuthApplications",
				new Dictionary<string, string>() {
					{ "AuthorizeUrl", "MediumText" },
					{ "ClientId", "Text" },
					{ "TokenUrl", "Text" },
					{ "SecretKey", "Text" },
					{ "RevokeTokenUrl", "Text" }
				}
			);

			EntitySchemaManager.AddCustomizedEntitySchema("SysAdminUnit",
				new Dictionary<string, string>() {
					{ "Name", "Text" }
				}
			);

			var _e2 = EntitySchemaManager.AddCustomizedEntitySchema("OAuthAppScope",
				new Dictionary<string, string>() {
					{ "Scope", "Text" }
				}
			);
			_e2.AddLookupColumn("OAuthApplications", "OAuth20App");


			_mockOAuthTokenStorage = EntitySchemaManager.AddCustomizedEntitySchema("OAuthTokenStorage",
				new Dictionary<string, string>() {
					{ "AccessToken", "Text" },
					{ "RefreshToken", "Text" },
					{ "ExpiresOn", "Integer" },
					{ "UserAppLogin", "Text" }
				}
			);
			_mockOAuthTokenStorage.AddLookupColumn("OAuthApplications", "OAuthApp");
			_mockOAuthTokenStorage.AddLookupColumn("SysAdminUnit", "SysUser");
		}

		[Test]
		public void GetLoginUrl_ShouldReturn()
		{
			//Arrange
			SetUpTestData("OAuthApplications", query => query.Has(Consts.ApplicationId), new Dictionary<string, object>() {
				{ "Id", Consts.ApplicationId },
				{ "Name", "Microsoft Graph"},
				{ "AuthorizeUrl", Consts.ApplicationSettings.AuthorizeUrl},
				{ "ClientId", Consts.ApplicationSettings.ClientId },
				{ "TokenUrl", Consts.ApplicationSettings.TokenUrl },
				{ "SecretKey", Consts.ApplicationSettings.ClientSecret },
				{ "RevokeTokenUrl", Consts.ApplicationSettings.RevokeTokenUrl }
			});
			
			SetUpTestData("OAuthAppScope", query => query.Has(Consts.ApplicationId), 
				new Dictionary<string, object>() {
					{ "Id", Guid.Parse("e5e128e9-9a48-45ae-9fa3-fbb66abc35cc") },
					{ "Scope", "Calendars.ReadWrite"}
				},
				new Dictionary<string, object>() {
					{ "Id", Guid.Parse("03f64937-974e-4a34-a72f-d2494a25e8f7") },
					{ "Scope", "OnlineMeetingArtifact.Read.All"}
				},
				new Dictionary<string, object>() {
					{ "Id", Guid.Parse("c6b56eeb-4870-4de7-a700-99572a381a9d") },
					{ "Scope", "OnlineMeetings.ReadWrite"}
				},
				new Dictionary<string, object>() {
					{ "Id", Guid.Parse("6c6126a1-9dcd-4436-8f4c-518dbfe5bef7") },
					{ "Scope", "offline_access"}
				},
				new Dictionary<string, object>() {
					{ "Id", Guid.Parse("7bbd115b-845c-432d-8115-abf77d978508") },
					{ "Scope", "User.Read"}
				}
			);

			//Act and Assert
			Assert.Multiple(() =>
			{
				_sut = mockApp.GetService<IOAuthHandler>();
				var result = _sut.GetLoginUrl();
				Assert.That(result, Is.EqualTo(Consts.ExpectedLoginUrl));
			});

			UserConnection.DBExecutor
				.Received(2)
				.ExecuteReader(
				sqlText: Arg.Any<string>(),
				queryParameters: ArgExt.ContainsQueryParameterByValue(Consts.ApplicationId)
			);
		}

		[Test(Author = "Kirill Krylov", Description = "Check that update is called with correct parameters")]
		public void UpserToken_ShouldReturn_Update()
		{
			//Arrange
			Guid currentUserId = Guid.NewGuid();
			UserConnection.CurrentUser.Id = currentUserId;

			//Arrange
			SetUpTestData("OAuthApplications", query => query.Has(Consts.ApplicationId), new Dictionary<string, object>() {
				{ "Id", Consts.ApplicationId },
				{ "Name", "Microsoft Graph"},
				{ "AuthorizeUrl", Consts.ApplicationSettings.AuthorizeUrl},
				{ "ClientId", Consts.ApplicationSettings.ClientId },
				{ "TokenUrl", Consts.ApplicationSettings.TokenUrl },
				{ "SecretKey", Consts.ApplicationSettings.ClientSecret },
				{ "RevokeTokenUrl", Consts.ApplicationSettings.RevokeTokenUrl }
			});

			SetUpTestData("OAuthAppScope", query => query.Has(Consts.ApplicationId),
				new Dictionary<string, object>() {
					{ "Id", Guid.Parse("e5e128e9-9a48-45ae-9fa3-fbb66abc35cc") },
					{ "Scope", "Calendars.ReadWrite"}
				},
				new Dictionary<string, object>() {
					{ "Id", Guid.Parse("03f64937-974e-4a34-a72f-d2494a25e8f7") },
					{ "Scope", "OnlineMeetingArtifact.Read.All"}
				},
				new Dictionary<string, object>() {
					{ "Id", Guid.Parse("c6b56eeb-4870-4de7-a700-99572a381a9d") },
					{ "Scope", "OnlineMeetings.ReadWrite"}
				},
				new Dictionary<string, object>() {
					{ "Id", Guid.Parse("6c6126a1-9dcd-4436-8f4c-518dbfe5bef7") },
					{ "Scope", "offline_access"}
				},
				new Dictionary<string, object>() {
					{ "Id", Guid.Parse("7bbd115b-845c-432d-8115-abf77d978508") },
					{ "Scope", "User.Read"}
				}
			);


			Guid actual_recordId = Guid.NewGuid();

			SetUpTestData(
				schemaName: "OAuthTokenStorage",
				filterAction: query => query.Has(currentUserId).Has(Consts.ApplicationId),
				new Dictionary<string, object>() {
					{ "Id", actual_recordId },
				}
			);

			string sqlText = string.Empty;
			QueryParameterCollection qpc = default;
			UserConnection.DBExecutor.Execute(
					sqlText: Arg.Any<string>(),
					queryParameters: Arg.Any<QueryParameterCollection>()
			).Returns(callInfo =>
			{
				sqlText = (string)callInfo[0];
				qpc = (QueryParameterCollection)callInfo[1];
				return 1;
			});


			Assert.Multiple(() =>
			{
				_sut = mockApp.GetService<IOAuthHandler>();
				var result = _sut.UpsertToken(Consts.Token);
				Assert.That(result, Is.True);
				Dictionary<string, string> map = SqlParamFinder.GetParamsForUpdate(sqlText, "OAuthTokenStorage");
				Dictionary<string, string> filterMap = SqlParamFinder.GetFiltersForUpdate(sqlText, "OAuthTokenStorage");


				Assert.That(
					(Guid)qpc.FindByName(filterMap["Id"]).Value,
					Is.EqualTo(actual_recordId)
				);
				
				Assert.That(
					(string)qpc.FindByName(map["AccessToken"]).Value,
					Is.EqualTo(Consts.Token.AccessToken)
				);

				Assert.That(
					(string)qpc.FindByName(map["RefreshToken"]).Value,
					Is.EqualTo(Consts.Token.RefreshToken)
				);

				Assert.That(
					(string)qpc.FindByName(map["UserAppLogin"]).Value,
					Is.EqualTo("MicrosoftTeamsConnector")
				);


				long actualExpiresOn = (long)qpc.FindByName(map["ExpiresOn"]).Value;
				long approximateExpiresOn = DateTimeOffset.Now.ToUnixTimeSeconds() + Consts.Token.ExpiresIn;
				

				Assert.That(
					approximateExpiresOn- actualExpiresOn,
					Is.LessThan(3)
				);
			});
		}

		[Test(Author = "Kirill Krylov", Description = "Check that insert is called with correct parameters")]
		public void UpserToken_ShouldReturn_Insert()
		{
			//Arrange
			Guid currentUserId = Guid.NewGuid();
			UserConnection.CurrentUser.Id = currentUserId;

			SetUpTestData("OAuthApplications", query => query.Has(Consts.ApplicationId), new Dictionary<string, object>() {
				{ "Id", Consts.ApplicationId },
				{ "Name", "Microsoft Graph"},
				{ "AuthorizeUrl", Consts.ApplicationSettings.AuthorizeUrl},
				{ "ClientId", Consts.ApplicationSettings.ClientId },
				{ "TokenUrl", Consts.ApplicationSettings.TokenUrl },
				{ "SecretKey", Consts.ApplicationSettings.ClientSecret },
				{ "RevokeTokenUrl", Consts.ApplicationSettings.RevokeTokenUrl }
			});

			SetUpTestData("OAuthAppScope", query => query.Has(Consts.ApplicationId),
				new Dictionary<string, object>() {
					{ "Id", Guid.Parse("e5e128e9-9a48-45ae-9fa3-fbb66abc35cc") },
					{ "Scope", "Calendars.ReadWrite"}
				},
				new Dictionary<string, object>() {
					{ "Id", Guid.Parse("03f64937-974e-4a34-a72f-d2494a25e8f7") },
					{ "Scope", "OnlineMeetingArtifact.Read.All"}
				},
				new Dictionary<string, object>() {
					{ "Id", Guid.Parse("c6b56eeb-4870-4de7-a700-99572a381a9d") },
					{ "Scope", "OnlineMeetings.ReadWrite"}
				},
				new Dictionary<string, object>() {
					{ "Id", Guid.Parse("6c6126a1-9dcd-4436-8f4c-518dbfe5bef7") },
					{ "Scope", "offline_access"}
				},
				new Dictionary<string, object>() {
					{ "Id", Guid.Parse("7bbd115b-845c-432d-8115-abf77d978508") },
					{ "Scope", "User.Read"}
				}
			);

			string sqlText = string.Empty;
			QueryParameterCollection qpc = default;
			UserConnection.DBExecutor.Execute(
					sqlText: Arg.Any<string>(),
					queryParameters: Arg.Any<QueryParameterCollection>()
			).Returns(callInfo =>
			{
				sqlText = (string)callInfo[0];
				qpc = (QueryParameterCollection)callInfo[1];
				return 1;
			});


			//Act and Assert
			Assert.Multiple(() =>
			{
				_sut = mockApp.GetService<IOAuthHandler>();
				var result = _sut.UpsertToken(Consts.Token);
				Assert.That(result, Is.True);
				Dictionary<string, string> map = SqlParamFinder.GetParamsForInsert(sqlText, "OAuthTokenStorage");

				Assert.That(
					Guid.TryParse(qpc.FindByName(map["Id"]).Value.ToString(), out Guid _),
					Is.True
				);
				
				Assert.That(
					(string)qpc.FindByName(map["AccessToken"]).Value,
					Is.EqualTo(Consts.Token.AccessToken)
				);

				Assert.That(
					(string)qpc.FindByName(map["RefreshToken"]).Value,
					Is.EqualTo(Consts.Token.RefreshToken)
				);

				Assert.That(
					(string)qpc.FindByName(map["UserAppLogin"]).Value,
					Is.EqualTo("MicrosoftTeamsConnector")
				);

				long actualExpiresOn = (long)qpc.FindByName(map["ExpiresOn"]).Value;
				long approximateExpiresOn = DateTimeOffset.Now.ToUnixTimeSeconds() + Consts.Token.ExpiresIn;
				
				Assert.That(
					approximateExpiresOn- actualExpiresOn,
					Is.LessThan(1)
				);
			});
		}


		[Test(Author = "Kirill Krylov", Description = "Find Access token for a current user")]
		public void TryGetTokenByUserId_ShouldFindToken()
		{
			//Arrange
			Guid currentUserId = Guid.NewGuid();
			UserConnection.CurrentUser.Id = currentUserId;
			
			//Arrange
			SetUpTestData("OAuthApplications", query => query.Has(Consts.ApplicationId), new Dictionary<string, object>() {
				{ "Id", Consts.ApplicationId },
				{ "Name", "Microsoft Graph"},
				{ "AuthorizeUrl", Consts.ApplicationSettings.AuthorizeUrl},
				{ "ClientId", Consts.ApplicationSettings.ClientId },
				{ "TokenUrl", Consts.ApplicationSettings.TokenUrl },
				{ "SecretKey", Consts.ApplicationSettings.ClientSecret },
				{ "RevokeTokenUrl", Consts.ApplicationSettings.RevokeTokenUrl }
			});

			SetUpTestData("OAuthAppScope", query => query.Has(Consts.ApplicationId),
				new Dictionary<string, object>() {
					{ "Id", Guid.Parse("e5e128e9-9a48-45ae-9fa3-fbb66abc35cc") },
					{ "Scope", "Calendars.ReadWrite"}
				},
				new Dictionary<string, object>() {
					{ "Id", Guid.Parse("03f64937-974e-4a34-a72f-d2494a25e8f7") },
					{ "Scope", "OnlineMeetingArtifact.Read.All"}
				},
				new Dictionary<string, object>() {
					{ "Id", Guid.Parse("c6b56eeb-4870-4de7-a700-99572a381a9d") },
					{ "Scope", "OnlineMeetings.ReadWrite"}
				},
				new Dictionary<string, object>() {
					{ "Id", Guid.Parse("6c6126a1-9dcd-4436-8f4c-518dbfe5bef7") },
					{ "Scope", "offline_access"}
				},
				new Dictionary<string, object>() {
					{ "Id", Guid.Parse("7bbd115b-845c-432d-8115-abf77d978508") },
					{ "Scope", "User.Read"}
				}
			);

			Guid actual_recordId = Guid.NewGuid();

			SetUpTestData(
				schemaName: "OAuthTokenStorage",
				filterAction: query => query.Has(currentUserId).Has(Consts.ApplicationId),
				new Dictionary<string, object>() {
					{ "Id", actual_recordId },
					{ "AccessToken", Consts.Token.AccessToken },
					{ "RefreshToken", Consts.Token.RefreshToken },
					{ "ExpiresOn", (int)DateTimeOffset.Now.ToUnixTimeSeconds() }
				}
			);

			Assert.Multiple(() =>
			{
				_sut = mockApp.GetService<IOAuthHandler>();
				_sut.TryGetTokenByUserId(out Dto.Token token);
				var result = _sut.UpsertToken(Consts.Token);
				
				Assert.That(result, Is.True);
				Assert.That(token.AccessToken, Is.EqualTo(Consts.Token.AccessToken));
				Assert.That(token.RefreshToken, Is.EqualTo(Consts.Token.RefreshToken));
				Assert.That(token.ExpiresIn, Is.InRange(-1, 0));
			});
		}

		[Test(Author = "Kirill Krylov", Description = "Will NOT find Access token for a current user")]
		public void TryGetTokenByUserId_ShouldNotFindToken()
		{
			//Arrange
			Guid currentUserId = Guid.NewGuid();
			UserConnection.CurrentUser.Id = currentUserId;
			
			//Arrange
			SetUpTestData("OAuthApplications", query => query.Has(Consts.ApplicationId), new Dictionary<string, object>() {
				{ "Id", Consts.ApplicationId },
				{ "Name", "Microsoft Graph"},
				{ "AuthorizeUrl", Consts.ApplicationSettings.AuthorizeUrl},
				{ "ClientId", Consts.ApplicationSettings.ClientId },
				{ "TokenUrl", Consts.ApplicationSettings.TokenUrl },
				{ "SecretKey", Consts.ApplicationSettings.ClientSecret },
				{ "RevokeTokenUrl", Consts.ApplicationSettings.RevokeTokenUrl }
			});

			SetUpTestData("OAuthAppScope", query => query.Has(Consts.ApplicationId),
				new Dictionary<string, object>() {
					{ "Id", Guid.Parse("e5e128e9-9a48-45ae-9fa3-fbb66abc35cc") },
					{ "Scope", "Calendars.ReadWrite"}
				},
				new Dictionary<string, object>() {
					{ "Id", Guid.Parse("03f64937-974e-4a34-a72f-d2494a25e8f7") },
					{ "Scope", "OnlineMeetingArtifact.Read.All"}
				},
				new Dictionary<string, object>() {
					{ "Id", Guid.Parse("c6b56eeb-4870-4de7-a700-99572a381a9d") },
					{ "Scope", "OnlineMeetings.ReadWrite"}
				},
				new Dictionary<string, object>() {
					{ "Id", Guid.Parse("6c6126a1-9dcd-4436-8f4c-518dbfe5bef7") },
					{ "Scope", "offline_access"}
				},
				new Dictionary<string, object>() {
					{ "Id", Guid.Parse("7bbd115b-845c-432d-8115-abf77d978508") },
					{ "Scope", "User.Read"}
				}
			);

			Guid actual_recordId = Guid.NewGuid();

			

			Assert.Multiple(() =>
			{
				_sut = mockApp.GetService<IOAuthHandler>();
				_sut.TryGetTokenByUserId(out Dto.Token token);
				var result = _sut.UpsertToken(Consts.Token);
				
				Assert.That(result, Is.False);
				Assert.That(token, Is.Null);
				
			});
		}


		[Test(Author = "Kirill Krylov")]
		public void GetApplicationParams_ShouldThrow()
		{
			string expectedMessage = $"Could not find OAuth Application for Id: {Consts.ApplicationId}";
			var logMock = Substitute.For<ILog>();

			var ex = Assert.Throws<KeyNotFoundException>(() => {
				new OAuthHandler(UserConnection, logMock);
			});
			logMock.Received(1).Error(Arg.Is(expectedMessage));	
			Assert.That(ex.Message, Is.EqualTo(expectedMessage));
		}


		



		#region Private
		protected void SetUpTestData(string schemaName, params Dictionary<string, object>[] items)
		{
			var selectData = new SelectData(UserConnection, schemaName);
			items.ForEach(values => selectData.AddRow(values));
			selectData.MockUp();
		}

		protected void SetUpTestData(string schemaName, Action<SelectData> filterAction, params Dictionary<string, object>[] items)
		{
			var selectData = new SelectData(UserConnection, schemaName);
			items.ForEach(values => selectData.AddRow(values));
			filterAction.Invoke(selectData);
			selectData.MockUp();
		}
		#endregion
	}

	public class MyHttpContextAccessor : IHttpContextAccessor
	{
		public MyHttpContextAccessor(){}

		public static IHttpContextAccessor SetupHttpContextAccessor(UserConnection userConnection = null)
		{
			HttpContext httpContext = Substitute.For<HttpContext>(Array.Empty<object>());
			HttpRequest httpRequest = Substitute.For<HttpRequest>();
			
			if (userConnection != null)
			{
				httpContext.Session["UserConnection"] = userConnection;
			}

			if (userConnection?.AppConnection != null)
			{
				httpContext.Application["AppConnection"] = userConnection.AppConnection;
			}

			httpRequest.BaseUrl.Returns(Consts.BaseUrl);
			httpContext.Request.Returns(httpRequest);


			IHttpContextAccessor httpContextAccessor = Substitute.For<IHttpContextAccessor>(Array.Empty<object>());
			httpContextAccessor.GetInstance().Returns(httpContext);
			return httpContextAccessor;
		}

		public HttpContext GetInstance()
		{
			return SetupHttpContextAccessor().GetInstance();
		}
	}
}
