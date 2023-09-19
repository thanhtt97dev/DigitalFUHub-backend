namespace DigitalFUHubApi.Managers
{
	public interface IConnectionManager
	{
		void AddConnection(int userId, string connectionId);
		void RemoveConnection(string connectionId);
		HashSet<string>? GetConnections(int userId);
		IEnumerable<int> OnlineUsers { get; }
	}
}
