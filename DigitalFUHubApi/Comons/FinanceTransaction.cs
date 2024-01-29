using static Comons.Enum;

namespace DigitalFUHubApi.Comons
{
	public class FinanceTransaction
	{
		public long UserId { get; set; }
		public FinanceTransactionType Type { get; set; }	
		public long Amount { get; set; }
		public long ForeignId { get; set; }	

	}
}
