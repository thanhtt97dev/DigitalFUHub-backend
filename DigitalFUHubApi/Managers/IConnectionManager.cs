namespace DigitalFUHubApi.Managers
{
	public interface IConnectionManager
	{
		void AddConnection(int userId, string hubName, string connectionId);
		void RemoveConnection(string connectionId, string hubName);
		HashSet<string>? GetConnections(int userId, string hubName);
	}
}
