using BusinessObject;
using BusinessObject.Entities;
using Comons;
using DTOs.Order;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.DAOs
{
	public class OrderDAO
	{
		private static OrderDAO? instance;
		private static readonly object instanceLock = new object();

		public static OrderDAO Instance
		{
			get
			{
				lock (instanceLock)
				{
					if (instance == null)
					{
						instance = new OrderDAO();
					}
				}
				return instance;
			}
		}

		internal List<Order> GetAllOrderWaitToConfirm(int days)
		{
			using (DatabaseContext context = new DatabaseContext())
			{
				DateTime timeAccept = DateTime.Now.AddDays(-days);
				var orders = context.Order
					.Where(x =>
						x.OrderStatusId == Constants.ORDER_WAIT_CONFIRMATION &&
						x.OrderDate < timeAccept
					)
					.ToList();
				return orders;
			}
		}

		internal void ConfirmOrdersWithWaitToConfirmStatus(List<Order> orders)
		{
			using (DatabaseContext context = new DatabaseContext())
			{
				var transaction = context.Database.BeginTransaction();
				try
				{
					foreach(var order in orders) 
					{
						// update order's status to confirmed
						var orderUpdate = context.Order.First(x => x.OrderId == order.OrderId);
						orderUpdate.OrderStatusId = Constants.ORDER_CONFIRMED;

						//get platform fee
						var fee = context.BusinessFee.First(x => x.BusinessFeeId == order.BusinessFeeId).Fee;
						//get sellerId
						var productId = context.ProductVariant.First(x => x.ProductVariantId == order.ProductVariantId).ProductId;
						var sellerId = context.Product.First(x => x.ProductId == productId).ShopId;

						//get profit
						var adminProfit = order.TotalAmount * fee / 100;
						var sellerProfit = order.TotalAmount - adminProfit;

						// update seller's balance
						var seller = context.User.First(x => x.UserId == sellerId);
						seller.AccountBalance = seller.AccountBalance + sellerProfit;

						// update admin's balance
						var admin = context.User.First(x => x.UserId == Constants.ADMIN_USER_ID);
						admin.AccountBalance = admin.AccountBalance + adminProfit;

						// add transaction for refund money to seller
						Transaction transactionSeller = new Transaction()
						{
							UserId = sellerId,
							TransactionTypeId = Constants.TRANSACTION_TYPE_INTERNAL_RECEIVE_PAYMENT,
							OrderId = order.OrderId,	
							PaymentAmount =  sellerProfit,
							Note="",
							DateCreate = DateTime.Now,
						};
						context.Transaction.Add(transactionSeller);
						// add transaction for get benefit
						Transaction transactionAdmin = new Transaction()
						{
							UserId = Constants.ADMIN_USER_ID,
							TransactionTypeId = Constants.TRANSACTION_TYPE_INTERNAL_RECEIVE_PROFIT,
							OrderId = order.OrderId,
							PaymentAmount = adminProfit,
							Note = "",
							DateCreate = DateTime.Now,
						};
						context.Transaction.Add(transactionAdmin);
					}
					context.SaveChanges();
					transaction.Commit();
				}
				catch (Exception ex)
				{
					transaction.Rollback();
					throw new Exception(ex.Message);
				}
			}
		}

		internal List<Order> GetOrders(long orderId, string customerEmail, string shopName, DateTime fromDate, DateTime toDate, int status)
		{
			List<Order> orders = new List<Order>();
			using (DatabaseContext context = new DatabaseContext())
			{
				orders = context.Order
							.Include(x => x.User)
							.Include(x => x.ProductVariant)
							.ThenInclude(x => x.Product)
							.ThenInclude(x => x.Shop)
							.Where(x => 
								fromDate <= x.OrderDate && toDate >= x.OrderDate &&
								x.User.Email.Contains(customerEmail) &&
								x.ProductVariant.Product.Shop.ShopName.Contains(shopName)
							)
							.OrderByDescending(x => x.OrderDate).ToList();
				if(orderId != 0)
				{
					orders = orders.Where(x => x.OrderId == orderId).ToList();	
				}
				
				if (status != 0)
				{
					orders = orders.Where(x => x.OrderStatusId == status).ToList();
				}

			}
			return orders;
		}

        internal void AddOrder(Order order)
        {
            using (DatabaseContext context = new DatabaseContext())
            {
                context.Order.Add(order);
				context.SaveChanges();


                // add new transaction
                Transaction newTransaction = new Transaction
				{
					UserId = order.UserId,
					TransactionTypeId = Constants.TRANSACTION_TYPE_INTERNAL_PAYMENT,
					OrderId = order.OrderId,
					PaymentAmount = order.TotalAmount,
					Note = "Thanh toan",
					DateCreate = new DateTime()
				};

				context.Transaction.Add(newTransaction);
				context.SaveChanges();

            }
        }
    }
}

