using Common.Logging;
using MicrosftTeams.WebServices.OAuthHandler;
using MicrosoftTeams.DataOperations;
using MicrosoftTeams.Interfaces;
using MicrosoftTeams.Token;
using NSubstitute;
using NUnit.Framework;
using Terrasoft.Configuration.Tests;
using Terrasoft.Core;
using Terrasoft.Core.Factories;

namespace MicrosoftTeams.Tests
{
	[TestFixture]
	[MockSettings(RequireMock.All)]
	public class OAuthHandlerServiceTests : BaseConfigurationTestFixture
	{
		private OAuthHandlerService _sut;

		protected override void SetUp()
		{
			base.SetUp();
			_sut = new OAuthHandlerService()
			{
				HttpContextAccessor = SetupHttpContextAccessor(UserConnection)
			};
		}

		[Test(Author = "Kirill Krylov")]
		public void GetLoginUrl_ShouldReturn()
		{
			//Arrange
			var OAuthHandlerMock = Substitute.For<IOAuthHandler>();
			OAuthHandlerMock.GetLoginUrl().Returns(Consts.ExpectedLoginUrl);

			var applicationMock = Substitute.For<IApplication>();
			applicationMock.GetService<IOAuthHandler>().Returns(OAuthHandlerMock);

			var stageBuildMock = Substitute.For<IStageBuild<IApplication>>();
			stageBuildMock.Build().Returns(applicationMock);

			var stage2 = Substitute.For<IStage2<IApplication>>();
			stage2.ConfigureLogger(Arg.Any<ILog>()).Returns(stageBuildMock);

			var builderMock = Substitute.For<IBuilder<IApplication>>();
			builderMock.ConfigureUserConnection(Arg.Any<UserConnection>()).Returns(stage2);

			ClassFactory.RebindWithFactoryMethod(() => builderMock);

			//Act
			var result = _sut.GetLoginUrl();

			//Assert
			Assert.That(result.LoginUrl, Is.EqualTo(Consts.ExpectedLoginUrl));
			Assert.That(result.Success, Is.True);
		}

		[Test(Author = "Kirill Krylov")]
		public void OauthCallback_ShouldReturn()
		{
			//Arrange
			var OAuthHandlerMock = Substitute.For<IOAuthHandler>();
			OAuthHandlerMock.GetLoginUrl().Returns(Consts.ExpectedLoginUrl);
			OAuthHandlerMock.GetOAuthApplicationModel().Returns(Consts.ApplicationSettings);

			var applicationMock = Substitute.For<IApplication>();
			applicationMock.GetService<IOAuthHandler>().Returns(OAuthHandlerMock);


			string expectedResult = "Added";
			var tokenHandlerMock = Substitute.For<ITokenHandler>();
			tokenHandlerMock.OAuthCallBack(
				code: Arg.Is(Consts.Code),
				state: Arg.Is(Consts.SessionState))
				.Returns(expectedResult);
			applicationMock.GetService<ITokenHandler>().Returns(tokenHandlerMock);

			var stageBuildMock = Substitute.For<IStageBuild<IApplication>>();
			stageBuildMock.Build().Returns(applicationMock);

			var stage2 = Substitute.For<IStage2<IApplication>>();
			stage2.ConfigureLogger(Arg.Any<ILog>()).Returns(stageBuildMock);

			var builderMock = Substitute.For<IBuilder<IApplication>>();
			builderMock.ConfigureUserConnection(Arg.Any<UserConnection>()).Returns(stage2);

			ClassFactory.RebindWithFactoryMethod(() => builderMock);


			//Act and Assert
			Assert.Multiple(() =>
			{
				var result = _sut.OauthCallback(Consts.Code, Consts.SessionState);
				Assert.That(result, Is.EqualTo(expectedResult));
			});

			tokenHandlerMock.Received(1).OAuthCallBack(Consts.Code, Consts.SessionState);
		}
	}
}
