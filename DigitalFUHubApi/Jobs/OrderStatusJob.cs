using Comons;
using DataAccess.IRepositories;
using DigitalFUHubApi.Services;
using Quartz;

namespace DigitalFUHubApi.Jobs
{
	public class OrderStatusJob : IJob
	{
		//private readonly IConfiguration configuration;
		private readonly IOrderRepository orderRepository;

		public OrderStatusJob(IOrderRepository orderRepository)
		{
			this.orderRepository = orderRepository;
		}

		public Task Execute(IJobExecutionContext context)
		{
			//RULE: handle change order's status to "Confirmed" if order status still "wait confirm" in a range times
			var ordersWaitConfirm = orderRepository.GetAllOrderWaitToConfirm(Constants.NUMBER_DAYS_AUTO_UPDATE_STAUTS_CONFIRM_ORDER);
			if (ordersWaitConfirm.Count != 0)
			{
				orderRepository.UpdateStatusOrderToConfirm(ordersWaitConfirm);
			}

			//RULE: handle change order's status to "seller refunded " if order status still "complaint" in a range times
			var ordersComplaint = orderRepository.GetAllOrderComplaint(Constants.NUMBER_DAYS_AUTO_UPDATE_STATUS_SELLER_REFUNDED_ORDER);
			if(ordersComplaint.Count != 0)
			{
				orderRepository.UpdateStatusOrderToSellerRefunded(ordersComplaint);
			}

			return Task.CompletedTask;	
		}
	}
}
