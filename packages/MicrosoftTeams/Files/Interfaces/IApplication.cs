namespace MicrosoftTeams.Interfaces
{
	/// <summary>
	/// Main Application
	/// </summary>
	public interface IApplication
	{
		/// <summary>
		/// Gets requested service from the application
		/// </summary>
		/// <typeparam name="T">Service to get from the application</typeparam>
		/// <returns></returns>
		T GetService<T>();
	}
}
