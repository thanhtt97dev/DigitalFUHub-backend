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
			var ordersWaitConfirm = orderRepository.GetAllOrderWaitToConfirm(Constants.NUMBER_DAYS_AUTO_CONFIRM_ORDER);
			if (ordersWaitConfirm.Count == 0) return Task.CompletedTask;

			//RULE: hanlde change order's status to "Confirmed" and refund money to seller and get benefit
			orderRepository.ConfirmOrdersWithWaitToConfirmStatus(ordersWaitConfirm);

			return Task.CompletedTask;	
		}
	}
}
