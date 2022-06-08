using Common.Logging;
using MicrosoftTeams.Models.OAuthApplicationModel;
using System;
using System.Collections.Generic;
using Terrasoft.Core;
using Terrasoft.Core.Entities;
using Terrasoft.Core.Factories;
using Terrasoft.Web.Http.Abstractions;

namespace MicrosoftTeams.DataOperations
{
	internal interface IOAuthHandler
	{
		/// <summary>
		/// Creates url for Authorization request
		/// </summary>
		/// <remarks>
		/// See MS <seealso href="https://docs.microsoft.com/en-us/graph/auth-v2-user#authorization-request">documentation</seealso>
		/// </remarks>
		/// <returns></returns>
		string GetLoginUrl();

		bool UpsertToken(Dto.Token token);
		IOAuthApplicationModel GetOAuthApplicationModel();

		bool TryGetTokenByUserId(out Dto.Token token);
	}

	internal class OAuthHandler : IOAuthHandler
	{
		private readonly UserConnection _userConnection;
		private readonly ILog _logger;
		private readonly Guid _oAuthApplicationId;
		internal readonly IOAuthApplicationModel _oAuthApplicationModel;
		
		//RFT - Move to SysSettings
		private readonly Guid _applicationId = Guid.Parse("f7f273d2-680a-45d1-b6e5-fbe2e55698aa");

		public OAuthHandler(UserConnection userconnection, ILog logger)
		{
			_userConnection = userconnection;
			_logger = logger;
			_oAuthApplicationId = Guid.Parse("f7f273d2-680a-45d1-b6e5-fbe2e55698aa");
			_oAuthApplicationModel = new OAuthApplicationModel();

			GetApplicationParams();
			GetScopes();
		}

		public string GetLoginUrl()
		{
			//RFT: Move to SysSettings
			string tenantId = "7b5ca2fb-a643-4385-8b39-36934270c716";

			HttpContext context = ClassFactory.Get<IHttpContextAccessor>().GetInstance();
			_oAuthApplicationModel.RedirectUrl = context.Request.BaseUrl;
			string result = $"{_oAuthApplicationModel.AuthorizeUrl}{tenantId}/oauth2/v2.0/authorize?" +
				$"client_id={_oAuthApplicationModel.ClientId}" +
				$"&response_type=code" +
				$"&redirect_uri={context.Request.BaseUrl}/rest/OAuthHandlerService/OAuthCallBack" +
				$"&scope={string.Join(" ", _oAuthApplicationModel.Scopes)}";

			return result;
		}

		internal void GetApplicationParams()
		{
			const string entitySchemaName = "OAuthApplications";
			var oAuthApplicationsEnt = _userConnection.EntitySchemaManager
				.GetInstanceByName(entitySchemaName).CreateEntity(_userConnection);

			if (oAuthApplicationsEnt.FetchFromDB(_oAuthApplicationId))
			{
				_oAuthApplicationModel.ClientId = oAuthApplicationsEnt.GetTypedColumnValue<string>("ClientId");
				_oAuthApplicationModel.ClientSecret = oAuthApplicationsEnt.GetTypedColumnValue<string>("SecretKey");
				_oAuthApplicationModel.AuthorizeUrl = new Uri(oAuthApplicationsEnt.GetTypedColumnValue<string>("AuthorizeUrl"));
				_oAuthApplicationModel.TokenUrl = new Uri(oAuthApplicationsEnt.GetTypedColumnValue<string>("TokenUrl"));
				_oAuthApplicationModel.RevokeTokenUrl = new Uri(oAuthApplicationsEnt.GetTypedColumnValue<string>("RevokeTokenUrl"));
				
				HttpContext context = ClassFactory.Get<IHttpContextAccessor>().GetInstance();

				if(context is object)
				{
					_oAuthApplicationModel.RedirectUrl = $"{context.Request.BaseUrl}/rest/OAuthHandlerService/OAuthCallBack";
				}
				else
				{
					var siteUrl = (string)Terrasoft.Core.Configuration.SysSettings.GetValue(_userConnection, "SiteUrl");
					_oAuthApplicationModel.RedirectUrl = $"{siteUrl}/rest/OAuthHandlerService/OAuthCallBack";
				}
			}
			else
			{
				string errorMessage = $"Could not find OAuth Application for Id: {_oAuthApplicationId}";
				_logger.Error(errorMessage);
				throw new KeyNotFoundException(errorMessage);
			}
		}

		internal void GetScopes()
		{
			const string entitySchemaName = "OAuthAppScope";
			var oAuthAppScopeEnt = _userConnection.EntitySchemaManager
				.GetInstanceByName(entitySchemaName);

			EntitySchemaQuery esq = new EntitySchemaQuery(oAuthAppScopeEnt);
			esq.AddColumn("Scope");

			IEntitySchemaQueryFilterItem filterByParent = 
				esq.CreateFilterWithParameters(FilterComparisonType.Equal, "OAuth20App.Id", _oAuthApplicationId);
			
			esq.Filters.Add(filterByParent);
			var scopes = esq.GetEntityCollection(_userConnection);

			if (scopes.Count == 0)
			{
				string errorMessage = $"Could not find scopes for OAuth Application {_oAuthApplicationId}";
				_logger.Error(errorMessage);
				throw new KeyNotFoundException(errorMessage);
			}

			foreach (var scope in scopes)
			{
				_oAuthApplicationModel.Scopes.Add(scope.GetTypedColumnValue<string>("Scope"));
			}
		}

		public IOAuthApplicationModel GetOAuthApplicationModel()
		{
			return _oAuthApplicationModel;
		}

		public bool TryGetTokenByUserId(out Dto.Token token)
		{
			string schemaName = "OAuthTokenStorage";
			Entity tokenStorageEntity = _userConnection.EntitySchemaManager
				.GetInstanceByName(schemaName).CreateEntity(_userConnection);

			Guid sysUserId = _userConnection.CurrentUser.Id;
			Dictionary<string, object> conditions = new Dictionary<string, object>()
			{
				{"SysUser", sysUserId },				//Current User
				{"OAuthApp", _applicationId}			//Application Id
			};

			if(!tokenStorageEntity.FetchFromDB(conditions, false))
			{
				token = null;
				return false;
			}

			token = new Dto.Token
			{
				AccessToken = tokenStorageEntity.GetTypedColumnValue<string>("AccessToken"),
				RefreshToken = tokenStorageEntity.GetTypedColumnValue<string>("RefreshToken"),
				TokenType = "Bearer",
				ExpiresIn = (int)(tokenStorageEntity.GetTypedColumnValue<int>("ExpiresOn")- DateTimeOffset.Now.ToUnixTimeSeconds()),
			};
			

			return true;
		}

		public bool UpsertToken(Dto.Token token)
		{
			string schemaName = "OAuthTokenStorage";
			Entity tokenStorageEntity = _userConnection.EntitySchemaManager
				.GetInstanceByName(schemaName).CreateEntity(_userConnection);
			
			Guid sysUserId = _userConnection.CurrentUser.Id;
			Dictionary<string, object> conditions = new Dictionary<string, object>()
			{
				{"SysUser", sysUserId },				//Current User
				{"OAuthApp", _applicationId}			//Application Id
			};


			if(!tokenStorageEntity.FetchFromDB(conditions, false))
			{
				tokenStorageEntity.SetDefColumnValues();
				tokenStorageEntity.SetColumnValue("OAuthAppId", _applicationId);
				tokenStorageEntity.SetColumnValue("SysUserId", sysUserId);
			}

			tokenStorageEntity.SetColumnValue("AccessToken", token.AccessToken);
			tokenStorageEntity.SetColumnValue("RefreshToken", token.RefreshToken);
			tokenStorageEntity.SetColumnValue("UserAppLogin", "MicrosoftTeamsConnector"); 
			tokenStorageEntity.SetColumnValue("ExpiresOn", DateTimeOffset.UtcNow.ToUnixTimeSeconds() + token.ExpiresIn);

			bool result = tokenStorageEntity.Save();
			return result;
		}
	}
}