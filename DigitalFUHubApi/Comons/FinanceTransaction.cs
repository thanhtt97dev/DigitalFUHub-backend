using static Comons.Constants;

namespace DigitalFUHubApi.Comons
{
	public class FinanceTransaction
	{
		public long UserId { get; set; }
		public FinanceTransactionType Type { get; set; }	
		public object? Data { get; set; }	
	}
}
