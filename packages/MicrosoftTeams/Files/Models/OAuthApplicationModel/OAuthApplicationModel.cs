using System;
using System.Collections.Generic;

namespace MicrosoftTeams.Models.OAuthApplicationModel
{
	internal interface IOAuthApplicationModel
	{
		Uri AuthorizeUrl { get; set; }
		string ClientId { get; set; }
		string ClientSecret { get; set; }
		Uri LogoutUrl { get; set; }
		Uri RevokeTokenUrl { get; set; }
		IList<string> Scopes { get; }
		Uri TokenUrl { get; set; }
		string RedirectUrl { get; set; }
	}

	internal class OAuthApplicationModel : IOAuthApplicationModel
	{
		public Uri AuthorizeUrl { get; set; }
		public Uri TokenUrl { get; set; }
		public Uri RevokeTokenUrl { get; set; }
		public Uri LogoutUrl { get; set; }
		public string ClientId { get; set; }
		public string ClientSecret { get; set; }
		public IList<string> Scopes { get; set; } = new List<string>();
		public string RedirectUrl { get; set; }
	}
}
