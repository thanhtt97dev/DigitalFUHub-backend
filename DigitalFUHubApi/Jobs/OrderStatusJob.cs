using Comons;
using DataAccess.IRepositories;
using DigitalFUHubApi.Services;
using Quartz;

namespace DigitalFUHubApi.Jobs
{
	public class OrderStatusJob : IJob
	{
		private readonly IOrderRepository orderRepository;

		public OrderStatusJob(IOrderRepository orderRepository)
		{
			this.orderRepository = orderRepository;
		}

		public Task Execute(IJobExecutionContext context)
		{
			//RULE: handle change order's status to "Confirmed" if order status still "wait confirm" in a range times
			orderRepository.UpdateStatusOrderFromWaitConfirmationToConfirmInPreviousDays(Constants.NUMBER_DAYS_AUTO_UPDATE_STAUTS_CONFIRM_ORDER);

			//RULE: handle change order's status to "seller refunded " if order status still "complaint" in a range times
			orderRepository.UpdateStatusOrderFromComplaintToSellerRefundedInPreviousDays(Constants.NUMBER_DAYS_AUTO_UPDATE_STATUS_SELLER_REFUNDED_ORDER);

			return Task.CompletedTask;	
		}
	}
}
