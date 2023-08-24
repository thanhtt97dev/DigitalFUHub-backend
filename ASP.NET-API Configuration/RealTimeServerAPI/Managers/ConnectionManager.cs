namespace RealTimeServerAPI.Managers
{
	public class ConnectionManager : IConnectionManager
	{
		//Save connectionIds with a identifier user
		private static Dictionary<string, HashSet<string>> useMap = new Dictionary<string, HashSet<string>>();	

		public void AddConnection(string userId, string connectionId)
		{
			lock(useMap) 
			{
				if(!useMap.ContainsKey(userId)) 
				{
					useMap[userId] = new HashSet<string>();
				}
				useMap[userId].Add(connectionId);	
			}
		}

		public void RemoveConnection(string connectionId)
		{
			lock (useMap)
			{
				foreach (var userId in useMap.Keys)
				{
					if (useMap[userId].Contains(connectionId))
					{
						useMap[userId].Remove(connectionId);
					}
					if (useMap[userId].Count() == 0)
					{
						useMap.Remove(userId);	
					}
				}
			}
		}

		public HashSet<string>? GetConnections(string connectionId)
		{
			var connections = new HashSet<string>();
			try
			{
				lock (useMap)
				{
					connections = useMap[connectionId];	
				}
			}catch
			{
				connections = null;
			}
			return connections;
		}

		public IEnumerable<string> OnlineUsers { get { return useMap.Keys; } }	
	}
}
