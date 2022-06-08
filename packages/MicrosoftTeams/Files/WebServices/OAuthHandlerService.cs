using Common.Logging;
using MicrosoftTeams.DataOperations;
using MicrosoftTeams.Interfaces;
using MicrosoftTeams.Token;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.Web;
using System.Threading.Tasks;
using System.Web.SessionState;
using Terrasoft.Core.Factories;
using Terrasoft.Nui.ServiceModel.DataContract;
using Terrasoft.Web.Common;

namespace MicrosftTeams.WebServices.OAuthHandler
{
	[ServiceContract]
	[AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Required)]
	public class OAuthHandlerService : BaseService, IReadOnlySessionState
	{
		#region Methods : REST
		[OperationContract]
		[WebInvoke(Method = "POST", RequestFormat = WebMessageFormat.Json, 
			BodyStyle = WebMessageBodyStyle.Bare, ResponseFormat = WebMessageFormat.Json)]
		public TeamsOAuthResponse GetLoginUrl()
		{
			var logger = LogManager.GetLogger("MicrosoftTeamsConnectorLogger");
			var app = ClassFactory.Get<IBuilder<IApplication>>()
				.ConfigureUserConnection(UserConnection)
				.ConfigureLogger(logger)
				.Build();

			var response = new TeamsOAuthResponse()
			{
				Success = true,
				LoginUrl = app.GetService<IOAuthHandler>().GetLoginUrl()
			};
			return response;
		}


		/// <summary>
		/// Exchange code for Token
		/// </summary>
		/// <param name="code">Code from MS Identity on success OAuth</param>
		/// <param name="session_state"></param>
		/// <returns></returns>
		[OperationContract]
		[WebInvoke(Method = "GET", RequestFormat = WebMessageFormat.Json,
			BodyStyle = WebMessageBodyStyle.Bare, ResponseFormat = WebMessageFormat.Json)]
		public string OauthCallback(string code, string session_state)
		{
			var logger = LogManager.GetLogger("MicrosoftTeamsConnectorLogger");
			
			var app = ClassFactory.Get<IBuilder<IApplication>>()
				.ConfigureUserConnection(UserConnection)
				.ConfigureLogger(logger)
				.Build();

			ITokenHandler tokenHandler = app.GetService<ITokenHandler>();

			string result = string.Empty;
			Task.Run(async () =>
			{
				result = await tokenHandler.OAuthCallBack(code, session_state).ConfigureAwait(false);

			}).Wait();

			//RFT: Return decent looking page.
			return result;
		}
		#endregion

		#region Methods : Private

		#endregion
	}


	[DataContract]
	public class TeamsOAuthResponse : BaseResponse
	{
		[DataMember(Name = "LoginUrl")]
		public string LoginUrl { get; set; }
	}
}