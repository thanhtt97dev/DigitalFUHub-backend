namespace DigitalFUHubApi.Managers
{
	public interface IConnectionManager
	{
		void AddConnection(long userId, string hubName, string connectionId);
		void RemoveConnection(long userId, string hubName, string connectionId);
		HashSet<string>? GetConnections(long userId, string hubName);
		bool CheckUserConnected (long userId, string hubName);
	}
}
