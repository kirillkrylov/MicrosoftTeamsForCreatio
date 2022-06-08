using Newtonsoft.Json;

namespace MicrosoftTeams.Dto
{
	public class Token
	{

		/// <summary>
		/// Type of the access
		/// </summary>
		/// <value>Always Bearer</value>
		[JsonProperty("token_type")]
		public string TokenType { get; set; }

		[JsonProperty("scope")]
		public string Scope { get; set; }

		/// <summary>
		/// Number of seconds before token expires
		/// </summary>
		/// <value>Number of seconds before token expires</value>
		[JsonProperty("expires_in")]
		public int ExpiresIn { get; set; }


		/// <summary>
		/// Access token to use with all requests
		/// </summary>
		/// <value>Bearer token</value>
		[JsonProperty("access_token")]
		public string AccessToken { get; set; }


		/// <summary>
		/// Token to use when refreshing access token
		/// </summary>
		/// <value>Refresh token</value>
		[JsonProperty("refresh_token")]
		public string RefreshToken { get; set; }
	}
}