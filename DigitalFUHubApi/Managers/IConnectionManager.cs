namespace DigitalFUHubApi.Managers
{
	public interface IConnectionManager
	{
		void AddConnection(long userId, string hubName, string connectionId);
		void RemoveConnection(long userId, string connectionId, string hubName);
		HashSet<string>? GetConnections(long userId, string hubName);
		bool CheckUserConnected (long userId, string hubName);
	}
}
