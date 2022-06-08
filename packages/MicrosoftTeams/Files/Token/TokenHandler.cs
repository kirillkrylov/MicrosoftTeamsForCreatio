using Common.Logging;
using MicrosoftTeams.DataOperations;
using MicrosoftTeams.Models.OAuthApplicationModel;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace MicrosoftTeams.Token
{
	internal interface ITokenHandler
	{
		Task<string> OAuthCallBack(string code, string state);
		Task RefreshToken();
	}

	internal class TokenHandler : ITokenHandler
	{
		private readonly HttpClient _httpClient;
		private readonly IOAuthHandler _oAuthHandler;
		private readonly IOAuthApplicationModel _oAuthApplicationModel;
		private readonly ILog _logger;

		public TokenHandler(IHttpClientFactory httpClientFactory, IOAuthHandler oAuthHandler, ILog logger)
		{
			_httpClient = httpClientFactory.CreateClient("OAuth");
			_oAuthHandler = oAuthHandler;
			_oAuthApplicationModel = oAuthHandler.GetOAuthApplicationModel();
			_logger = logger;
		}
		public async Task<string> OAuthCallBack(string code, string state)
		{
			Dictionary<string, string> parameters = GetParameters(code);
			var message = await PrepareMessage(parameters).ConfigureAwait(false);

			var msg = await SendInternalAsync(message);
			
			//RFT: token may come back with error, need to throw;
			var token = JsonConvert.DeserializeObject<Dto.Token>(msg);

			if (_oAuthHandler.UpsertToken(token))
			{
				return $"Record Added";
			}
			else
			{
				return "Failed to save new token";
			}
		}


		/// <summary>
		/// Prepares params for token exchange
		/// </summary>
		/// <param name="code"></param>
		/// <remarks>
		/// See request  <seealso href="https://docs.microsoft.com/en-us/graph/auth-v2-user#token-request">documentation</seealso>
		/// </remarks>
		/// <returns></returns>
		/// <exception cref="KeyNotFoundException"></exception>
		internal Dictionary<string, string> GetParameters(string code = "")
		{
			string clientId = _oAuthApplicationModel.ClientId;
			if (string.IsNullOrEmpty(clientId))
			{
				const string errmsg = "ClientId cannot be empty";
				_logger.ErrorFormat("{0}", errmsg);
				throw new KeyNotFoundException(errmsg);
			}

			string redirectUri = _oAuthApplicationModel.RedirectUrl;
			if (string.IsNullOrEmpty(redirectUri))
			{
				const string errmsg = "RedirectUrl cannot be empty";
				_logger.ErrorFormat("{0}", errmsg);
				throw new KeyNotFoundException(errmsg);
			}

			string clientSecret = _oAuthApplicationModel.ClientSecret;
			if (string.IsNullOrEmpty(clientSecret))
			{
				const string errmsg = "ClientSecret cannot be empty";
				_logger.ErrorFormat("{0}", errmsg);
				throw new KeyNotFoundException(errmsg);
			}

			var result = new Dictionary<string, string>(){
				{ "client_id",clientId},
				{ "grant_type","authorization_code"},
				{ "scope",string.Join(" ",_oAuthApplicationModel.Scopes)},
				{ "redirect_uri",redirectUri},
				{ "client_secret",clientSecret},
			};

			if (string.IsNullOrEmpty(code))
			{
				const string errmsg = "Code cannot be empty";
				_logger.ErrorFormat("{0}", errmsg);
				throw new KeyNotFoundException(errmsg);
			}
			else
			{
				result.Add("code", code);
				return result;
			}
		}

		internal Dictionary<string, string> GetRefreshParameters()
		{
			var res = _oAuthHandler.TryGetTokenByUserId(out Dto.Token staleToken);
			Dictionary<string, string> parameters = new Dictionary<string, string>()
			{
				{ "client_id",_oAuthApplicationModel.ClientId},
				{ "grant_type","refresh_token"},
				{ "scope",string.Join(" ",_oAuthApplicationModel.Scopes)},
				{ "refresh_token",staleToken.RefreshToken},
				{ "redirect_uri",_oAuthApplicationModel.RedirectUrl},
				{ "client_secret",_oAuthApplicationModel.ClientSecret},
			};
			return parameters;
		}

		internal async Task<HttpRequestMessage> PrepareMessage(Dictionary<string, string> parameters)
		{
			FormUrlEncodedContent form = new FormUrlEncodedContent(parameters);
			HttpContent content = new StringContent(await form.ReadAsStringAsync());
			content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");

			//RFT - Move to SysSetting
			string tenantId = "7b5ca2fb-a643-4385-8b39-36934270c716";
			string url = $"/{tenantId}/oauth2/v2.0/token";
			HttpRequestMessage message = new HttpRequestMessage(HttpMethod.Post, url)
			{
				Content = content
			};
			return message;
		}

		internal async Task<string> SendInternalAsync(HttpRequestMessage message)
		{
			var response = await _httpClient.SendAsync(message).ConfigureAwait(false);
			return await response.Content.ReadAsStringAsync().ConfigureAwait(false);
		}

		public async Task RefreshToken()
		{
			var parameters = GetRefreshParameters();
			var message = await PrepareMessage(parameters);
			var msg = await SendInternalAsync(message).ConfigureAwait(false);

			//RFT: token may come back with error, need to throw;
			var token = JsonConvert.DeserializeObject<Dto.Token>(msg);
			var x = _oAuthHandler.UpsertToken(token);
		}
	}
}