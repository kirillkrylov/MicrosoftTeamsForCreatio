using System;
using System.Collections.Specialized;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace MicrosoftTeams.Tests
{
	internal class MockRouteHandler : MockBaseRouteHandler
	{
		public NameValueCollection QueryParameters { get; set; }
		public string RequestMessageContent { get; set; }


		public MockRouteHandler(string uri, string filename, HttpStatusCode code) : base(uri, filename, code) { }

		protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
		{
			HttpRequestMessage = request;

			if(request.Content is object)
			{
				RequestMessageContent = await request.Content.ReadAsStringAsync().ConfigureAwait(false);
			}
			


			HttpResponseMessage response = new HttpResponseMessage()
			{
				StatusCode = _code,
				Content = new StringContent(string.Empty)
			};
			response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

			if (request.RequestUri == _requestUri && string.IsNullOrEmpty(request.RequestUri.Query))
			{
				response.Content = new StringContent(GetJsonFromFile(_filename));
				return response;
			}

			if (request.RequestUri.Query.StartsWith("?per_page=100&page="))
			{
				var nvc = GetParams(request.RequestUri.Query);
				var perPage = int.Parse(nvc["per_page"]);
				var pageNum = int.Parse(nvc["page"]);

				var fileSegment = _filename.Split('.');
				string newFileName = $"{fileSegment[0]}_p{pageNum}.{fileSegment[1]}";


				response.Content = new StringContent(GetJsonFromFile(newFileName));
				return response;
			}
			throw new NotImplementedException();
		}
		private NameValueCollection GetParams(string query)
		{
			string  querystring = query.Substring(query.IndexOf('?'));
			QueryParameters = HttpUtility.ParseQueryString(querystring);
			return QueryParameters;
		}
	}
}
