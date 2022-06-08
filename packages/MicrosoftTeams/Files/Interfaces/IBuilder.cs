using Common.Logging;
using System;
using Terrasoft.Core;

namespace MicrosoftTeams.Interfaces
{
	/// <summary>
	/// Abstraction for application configuration
	/// </summary>
	/// <typeparam name="TResult">Type of application to build</typeparam>
	public interface IBuilder<out TResult>
	{
		/// <summary>Stage: Configure UserConnection</summary>
		/// <param name="userConnection">UserConnection</param>
		/// <returns>Next Stage</returns>
		/// <exception cref="ArgumentNullException">
		/// Thrown when <paramref name="userConnection"/> is <see langword="null"/>
		/// </exception>
		/// <remarks>
		/// <see cref="ArgumentNullException"/> thrown when <paramref name="userConnection"/> is <see langword="null"/>
		/// </remarks>
		IStage2<TResult> ConfigureUserConnection(UserConnection userConnection);
	}

	/// <summary>
	/// Stage: Build Application
	/// </summary>
	/// <typeparam name="TResult">Application to build</typeparam>
	public interface IStage2<out TResult>
	{
		/// <summary>
		/// Logger to be associated with the application
		/// </summary>
		/// <param name="logger">ILogger</param>
		/// <returns>Next Stage</returns>
		/// <exception cref="ArgumentNullException">
		/// Thrown when <paramref name="logger"/> is <see langword="null"/>
		/// </exception>
		/// <remarks>
		/// <see cref="ArgumentNullException"/> thrown when <paramref name="logger"/> is <see langword="null"/>
		/// </remarks>
		IStageBuild<TResult> ConfigureLogger(ILog logger);
	}

	/// <summary>
	/// Stage: Build Application
	/// </summary>
	/// <typeparam name="TResult">Application to build</typeparam>
	public interface IStageBuild<out TResult>
	{
		/// <summary>
		/// Build Application
		/// </summary>
		/// <returns>Configured application</returns>
		TResult Build();
	}
}
