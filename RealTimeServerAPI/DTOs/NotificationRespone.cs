namespace RealTimeServerAPI.DTOs
{
	public class NotificationRespone
	{
		public string Title { get; set; } = null!;
		public string Message { get; set; } = null!;
		public DateTime Date { get; set; } = DateTime.Now;
	}
}
