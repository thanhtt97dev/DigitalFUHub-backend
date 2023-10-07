using BusinessObject;
using BusinessObject.Entities;
using Comons;
using DTOs.Order;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml.Style;

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
					foreach (var order in orders)
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
							PaymentAmount = sellerProfit,
							Note = "",
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
				if (orderId != 0)
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

		internal void AddOrder(List<Order> orders)
		{
			using (DatabaseContext context = new DatabaseContext())
			{
				var transaction = context.Database.BeginTransaction();
				try
				{
					// get bussinsis fee
					var businessFeeDate = context.BusinessFee.Max(x => x.StartDate);
					var businessFee = context.BusinessFee.FirstOrDefault(x => x.StartDate == businessFeeDate);
					if (businessFee == null) throw new Exception();
					long businessFeeId = businessFee.BusinessFeeId;
					long businessFeeValue = businessFee.Fee;

					foreach (var data in orders)
					{
						// get productVariant
						ProductVariant? productVariant = context.ProductVariant.FirstOrDefault(x => x.ProductVariantId == data.ProductVariantId);
						if (productVariant == null) throw new Exception();
						//get product 
						Product? product = context.Product.FirstOrDefault(x => x.ProductId == productVariant.ProductId);
						if (product == null) throw new Exception();
						//get coupons
						long totalCouponsDiscount = 0;
						if (data.OrderCoupons != null)
						{
							totalCouponsDiscount = context.Coupon.Where(x => data.OrderCoupons.Any(o => o.CouponId == x.CouponId)).Sum(x => x.PriceDiscount);
						}

						long totalDiscount = data.Quantity * (productVariant.Price * product.Discount / 100) + totalCouponsDiscount;

						var order = new Order
						{
							UserId = data.UserId,
							ProductVariantId = data.ProductVariantId,
							BusinessFeeId = businessFeeId,
							OrderStatusId = Constants.ORDER_WAIT_CONFIRMATION,
							Quantity = data.Quantity,
							Price = productVariant.Price,
							Discount = product.Discount,
							TotalDiscount = totalDiscount,
							TotalAmount = productVariant.Price * data.Quantity - totalCouponsDiscount,
							OrderDate = DateTime.Now,
						};
						context.Order.Add(order);
						context.SaveChanges();

						// update asset info
						var assetInformations = context.AssetInformation.Where(a => a.ProductVariantId == order.ProductVariantId && a.IsActive == true).Take(order.Quantity).ToList();
						if (assetInformations.Count < order.Quantity) throw new Exception();

						foreach (var asset in assetInformations)
						{
							asset.OrderId = order.OrderId;
							asset.IsActive = false;
						}
						context.AssetInformation.UpdateRange(assetInformations);

						//update customer account balance
						var customer = context.User.FirstOrDefault(x => x.UserId == order.UserId);
						if (customer == null) throw new NullReferenceException();
						if (customer.AccountBalance < order.TotalAmount) throw new Exception();
						customer.AccountBalance = customer.AccountBalance - order.TotalAmount;

						//update admin account balance
						var admin = context.User.First(x => x.UserId == Constants.ADMIN_USER_ID);
						admin.AccountBalance = admin.AccountBalance + order.TotalAmount;

						// add new transaction
						Transaction newTransaction = new Transaction
						{
							UserId = order.UserId,
							TransactionTypeId = Constants.TRANSACTION_TYPE_INTERNAL_PAYMENT,
							OrderId = order.OrderId,
							PaymentAmount = order.TotalAmount,
							Note = "Thanh toan",
							DateCreate = DateTime.Now
						};

						context.Transaction.Add(newTransaction);
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

		internal Order? GetOrder(long orderId)
		{
			using (DatabaseContext context = new DatabaseContext())
			{
				Order? order = (from o in context.Order
								join user in context.User
									on o.UserId equals user.UserId
								join businessFee in context.BusinessFee
									on o.BusinessFeeId equals businessFee.BusinessFeeId
								join productVariant in context.ProductVariant
									on o.ProductVariantId equals productVariant.ProductVariantId
								join product in context.Product
									on productVariant.ProductId equals product.ProductId
								join shop in context.Shop
									on product.ShopId equals shop.UserId
								join category in context.Category
									on product.CategoryId equals category.CategoryId
								where o.OrderId == orderId
								select new Order
								{
									OrderId = orderId,
									UserId = o.UserId,
									ProductVariantId = o.ProductVariantId,
									BusinessFeeId = o.BusinessFeeId,
									Quantity = o.Quantity,
									Price = o.Price,
									Discount = o.Discount,	
									OrderDate = o.OrderDate,
									TotalDiscount = o.TotalDiscount,
									TotalAmount = o.TotalAmount,
									FeedbackId = o.FeedbackId,
									OrderStatusId = o.OrderStatusId,
									User = new User
									{
										UserId = o.UserId,
										Email = user.Email,
									},
									ProductVariant = new ProductVariant
									{
										ProductVariantId = productVariant.ProductVariantId,
										Name = productVariant.Name,
										ProductId = productVariant.ProductId,
										Product = new Product
										{
											ProductId = product.ProductId,
											ProductName = product.ProductName,
											Thumbnail = product.Thumbnail,
											Category = new Category
											{
												CategoryId = category.CategoryId,
												CategoryName = category.CategoryName
											},
											Shop = new Shop
											{
												UserId = shop.UserId,
												ShopName = shop.ShopName,
											},
											ProductMedias = (from productMedia in context.ProductMedia
															 where productMedia.ProductId == productMedia.ProductId
															 select new ProductMedia { Url = productMedia.Url }
															).ToList()
										}
									},
									BusinessFee = new BusinessFee
									{
										BusinessFeeId = businessFee.BusinessFeeId,
										Fee = businessFee.Fee,
									},
									AssetInformations = (from assetInformation in context.AssetInformation
														 where assetInformation.OrderId == orderId
														 select new AssetInformation { Asset = assetInformation.Asset }
														).ToList(),
									OrderCoupons = (from orderCoupon in context.OrderCoupon
													join coupon in context.Coupon
														on orderCoupon.CouponId equals coupon.CouponId
													where orderCoupon.OrderId == orderId
													select new OrderCoupon
													{
														PriceDiscount = orderCoupon.PriceDiscount,
													}
													).ToList(),
								})
							   .FirstOrDefault();


				if (order != null && order.FeedbackId != null)
				{
					Feedback feedback = context.Feedback
						.Select(f => new Feedback()
						{
							FeedbackId = f.FeedbackId,
							Rate = f.Rate,
						})
						.First(x => x.FeedbackId == order.FeedbackId);
					order.Feedback = feedback;
				}

				return order;
			}
		}

		internal Order? GetSellerOrderDetail(long orderId)
		{
			using (DatabaseContext context = new DatabaseContext())
			{
				return context.Order.Include(i => i.AssetInformations)
					.ThenInclude(ti => ti.ProductVariant).ThenInclude(x => x.Product).Include(x => x.User)
					.Where(x => x.OrderId == orderId).FirstOrDefault();
			}
		}
	}
}

