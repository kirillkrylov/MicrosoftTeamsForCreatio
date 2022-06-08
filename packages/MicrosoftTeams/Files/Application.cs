using Common.Logging;
using MicrosoftTeams.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using System;
using Terrasoft.Core;
using MicrosoftTeams.Models.OAuthApplicationModel;
using MicrosoftTeams.DataOperations;
using MicrosoftTeams.Token;
using System.Net.Http;
using MicrosoftTeams.MsGraph;

namespace MicrosoftTeams
{
	internal sealed class Application : IApplication
	{
		private readonly IServiceScope _scope;
		private readonly ILog _logger;

		internal Application(UserConnection userConnection, ILog logger)
		{
			ServiceCollection services = new ServiceCollection();

			//ADD YOUR SERVICES HERE
			services.AddSingleton<UserConnection>(userConnection);
			services.AddSingleton<ILog>(logger);

			services.AddHttpClient("OAuth", client =>
			{
				client.BaseAddress = new Uri("https://login.microsoftonline.com");
			});

			services.AddSingleton<IOAuthHandler, OAuthHandler>();
			services.AddSingleton<IOAuthApplicationModel, OAuthApplicationModel>();
			services.AddSingleton<ITokenHandler, TokenHandler>();
			services.AddSingleton<IMisc, Misc>();
	
			services.AddSingleton<IGClient, GClient>();
			ServiceProvider container = services.BuildServiceProvider(true);
			_scope = container.CreateScope();
			_logger = logger;
		}
		public T GetService<T>()
		{
			try
			{
				return _scope.ServiceProvider.GetService<T>();
			}
			catch (Exception ex)
			{
				_logger.ErrorFormat("Error {0} while resolving service {3}\n{1}\n{2}", ex.GetType(), ex.Message, ex.StackTrace, typeof(T).FullName);
				throw;
			}

		}
	}
}
