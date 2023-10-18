using System.Linq;

namespace DigitalFUHubApi.Managers
{
	public class ConnectionManager : IConnectionManager	
	{
		//Save Hub type with connectionIds of a user
		private static Dictionary<long, Dictionary<string, HashSet<string>>> data = new Dictionary<long, Dictionary<string, HashSet<string>>>();

		public void AddConnection(long userId,string hubName , string connectionId)
		{
			lock (data)
			{
				if(!data.ContainsKey(userId)) 
				{
					data[userId] = new Dictionary<string, HashSet<string>>();
					HashSet<string> connections = new HashSet<string> { connectionId };
					data[userId].Add(hubName, connections);
				}
				else
				{
					if (data[userId].ContainsKey(hubName))
					{
						if (!data[userId][hubName].Contains(connectionId))
						{
							data[userId][hubName].Add(connectionId);	
						}
					}
					else
					{
						data[userId].Add(hubName, new HashSet<string> { connectionId });
					}
				}
			}
		}

		public void RemoveConnection(string connectionId, string hubName)
		{
			lock (data)
			{
				try
				{
					foreach (var userId in data.Keys)
					{
						if (data[userId][hubName].Contains(connectionId))
						{
							data[userId][hubName].Remove(connectionId);
						}
						if (data[userId][hubName].Count() == 0)
						{
							data.Remove(userId);
						}
					}
				}catch { }
			}
		}

		public HashSet<string>? GetConnections(long userId, string hubName)
		{
			var connections = new HashSet<string>();
			try
			{
				lock (data)
				{
					connections = data[userId][hubName];
				}
			}
			catch
			{
				connections = null;
			}
			return connections;
		}

		public bool CheckUserConnectd(long userId)
		{
			return data.ContainsKey(userId);
		}
	}
}
