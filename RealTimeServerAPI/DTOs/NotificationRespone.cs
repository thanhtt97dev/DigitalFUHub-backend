namespace RealTimeServerAPI.DTOs
{
	public class NotificationRespone
	{
		public long NotificationId { get; set; }
		public string? Title { get; set; } = null!;
		public string? Content { get; set; } = null!;
		public string? Link { get; set; }
		public DateTime DateCreated { get; set; }
		public bool IsReaded { get; set; }

	}
}
