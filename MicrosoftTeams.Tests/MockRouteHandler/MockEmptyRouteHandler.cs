using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace MicrosoftTeams.Tests
{
	internal class MockEmptyRouteHandler : DelegatingHandler
	{
		private readonly HttpStatusCode _code;
		private readonly string _responseContent;

		public MockEmptyRouteHandler(HttpStatusCode code, string responseContent) 
		{
			_code = code;
			_responseContent = responseContent;
		}

		protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
		{
			HttpResponseMessage response = new HttpResponseMessage()
			{
				StatusCode = _code,
				Content = new StringContent(_responseContent)
			};
			return response;
		}
	}
}
