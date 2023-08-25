namespace RealTimeServerAPI.DTOs
{
	public class NotificationRespone
	{
		public string Title { get; set; } = null!;
		public string Message { get; set; } = null!;
		public DateTime Date { get; set; }

		public NotificationRespone(string title, string message)
		{
			Title = title;
			Message = message;
			Date = DateTime.Now;
		}
	}
}
