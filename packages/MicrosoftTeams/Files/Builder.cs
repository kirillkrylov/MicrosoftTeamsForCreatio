using Common.Logging;
using MicrosoftTeams.Interfaces;
using System;
using Terrasoft.Core;
using Terrasoft.Core.Factories;

namespace MicrosoftTeams
{
	/// <summary>
	/// DlbMsTeamsConnector builder
	/// </summary>
	[DefaultBinding(typeof(IBuilder<IApplication>))]
	public class Builder : IBuilder<IApplication>,
		IStage2<IApplication>, IStageBuild<IApplication>
	{
		private UserConnection _userConnection;
		private ILog _logger;

		
		/// <inheritdoc cref="IBuilder{TResult}{TResult}.ConfigureUserConnection(UserConnection)"/>
		public IStage2<IApplication> ConfigureUserConnection(UserConnection userConnection)
		{
			_userConnection = userConnection ?? throw new ArgumentNullException(nameof(userConnection));
			return this;
		}

		/// <inheritdoc cref="IStage2.ConfigureLogger(ILog)"/>
		public IStageBuild<IApplication> ConfigureLogger(ILog logger)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			return this;
		}

		/// <inheritdoc cref="IStageBuild.Build"/>
		public IApplication Build()
		{
			try
			{
				return new Application(_userConnection, _logger);
			}
			catch (Exception ex)
			{
				_logger.ErrorFormat("DlbMsTeamsConnector Error Building Application", ex.Message, ex.StackTrace);
				throw;
			}
		}
	}
}
