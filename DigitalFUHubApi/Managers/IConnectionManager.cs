namespace DigitalFUHubApi.Managers
{
	public interface IConnectionManager
	{
		void AddConnection(long userId, string hubName, string connectionId);
		void RemoveConnection(string connectionId, string hubName);
		HashSet<string>? GetConnections(long userId, string hubName);
	}
}
