using System;
using System.IO;
using System.Net;
using System.Net.Http;

namespace MicrosoftTeams.Tests
{
	abstract class MockBaseRouteHandler : DelegatingHandler
	{
		private protected Uri _requestUri;
		private protected string _filename;
		private protected HttpStatusCode _code;

		protected internal HttpRequestMessage HttpRequestMessage { get; set; }


		public virtual void SetRequestUri(Uri uri)
		{
			_requestUri = uri;
		}

		public virtual void SetRequestUri(string uri)
		{
			SetRequestUri(new Uri(uri));
		}


		public virtual void SetHttpStatusCode(HttpStatusCode code)
		{
			_code = code;
		}

		public virtual void SetFileName(string fileName)
		{
			_filename = fileName;
		}

		public MockBaseRouteHandler(string uri, string filename, HttpStatusCode code)
		{
			_requestUri = new Uri(uri);
			_filename = filename;
			_code = code;
		}
		internal virtual string GetJsonFromFile(string fileName)
		{
			string _dirpath = Path.GetDirectoryName(this.GetType().Assembly.CodeBase);
			string _file = Path.Combine(_dirpath, "mockResponse", fileName);
			string filePath = _file.Substring(6, _file.Length - 6);
			return File.ReadAllText(filePath);
		}
	}
}
