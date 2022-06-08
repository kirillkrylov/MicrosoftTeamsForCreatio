using Microsoft.Graph;
using MicrosoftTeams.DataOperations;
using MicrosoftTeams.Token;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Terrasoft.Core;

namespace MicrosoftTeams.MsGraph
{
	internal interface IGClient
	{
		Task<GraphServiceClient> GetGraphServiceClient();
	}

	internal class GClient : IGClient
	{
		private readonly UserConnection _userConnection;
		private readonly ITokenHandler _tokenHandler;
		private readonly IOAuthHandler _oAuthHandler;
		private string _accessToken = string.Empty;

		public GClient(UserConnection userConnection, ITokenHandler tokenHandler, IOAuthHandler oAuthHandler)
		{
			_userConnection = userConnection;
			_tokenHandler = tokenHandler;
			_oAuthHandler = oAuthHandler;
		}

		public async Task<GraphServiceClient> GetGraphServiceClient()
		{
			bool tokenNeedsRefresh = false;
			if (_oAuthHandler.TryGetTokenByUserId(out Dto.Token token))
			{
				_accessToken = token.AccessToken;
				if (token.ExpiresIn <= 30)
				{
					tokenNeedsRefresh = true;
				}
			};


			if (tokenNeedsRefresh)
			{
				await _tokenHandler.RefreshToken();
				if (_oAuthHandler.TryGetTokenByUserId(out Dto.Token freshToken))
				{
					_accessToken = freshToken.AccessToken;
				}
			}

			DelegateAuthenticationProvider provider = new DelegateAuthenticationProvider(async (requestMessage) =>
			{
				requestMessage.Headers.Authorization = new AuthenticationHeaderValue("bearer", _accessToken);
			});
			return new GraphServiceClient(provider);
		}
	}
}
