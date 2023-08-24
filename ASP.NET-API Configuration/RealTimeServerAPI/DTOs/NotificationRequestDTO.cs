namespace RealTimeServerAPI.DTOs
{
	public class NotificationRequestDTO
	{
		public string ConnectionId { get; set; } = string.Empty;
		public string UserId { get; set; } = string.Empty;
		public string Message { get; set; } = string.Empty;
	}
}
