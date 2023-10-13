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

		internal void UpdateStatusOrderToConfirm(List<Order> orders)
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
						TransactionInternal transactionSeller = new TransactionInternal()
						{
							UserId = sellerId,
							TransactionInternalTypeId = Constants.TRANSACTION_TYPE_INTERNAL_RECEIVE_PAYMENT,
							OrderId = order.OrderId,
							PaymentAmount = sellerProfit,
							Note = "",
							DateCreate = DateTime.Now,
						};
						context.TransactionInternal.Add(transactionSeller);
						// add transaction for get benefit
						TransactionInternal transactionAdmin = new TransactionInternal()
						{
							UserId = Constants.ADMIN_USER_ID,
							TransactionInternalTypeId = Constants.TRANSACTION_TYPE_INTERNAL_RECEIVE_PROFIT,
							OrderId = order.OrderId,
							PaymentAmount = adminProfit,
							Note = "",
							DateCreate = DateTime.Now,
						};
						context.TransactionInternal.Add(transactionAdmin);
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

		internal List<Order> GetAllOrderComplaint(int days)
		{
			using (DatabaseContext context = new DatabaseContext())
			{
				DateTime timeAccept = DateTime.Now.AddDays(-days);
				var orders = context.Order
					.Where(x =>
						x.OrderStatusId == Constants.ORDER_COMPLAINT &&
						x.OrderDate < timeAccept)
					.ToList();
				return orders;
			}
		}

		internal void UpdateStatusOrderToSellerRefunded(List<Order> orders)
		{
			using (DatabaseContext context = new DatabaseContext())
			{
				var transaction = context.Database.BeginTransaction();
				try
				{
					foreach (var item in orders)
					{
						//update order status
						var order = context.Order.First(x => x.OrderId == item.OrderId);
						order.OrderStatusId = Constants.ORDER_SELLER_REFUNDED;
						context.SaveChanges();

						var customerId = order.UserId;

						// add transaction internal
						var transactionInternal = new TransactionInternal()
						{
							UserId = customerId,
							TransactionInternalTypeId = Constants.TRANSACTION_TYPE_INTERNAL_RECEIVE_REFUND,
							OrderId = order.OrderId,
							PaymentAmount = order.TotalPayment,
							Note = "Seller refund money",
							DateCreate = DateTime.Now,	
						};
						context.TransactionInternal.Add(transactionInternal);
						context.SaveChanges();

						// update customer balance
						var customer = context.User.First(x => x.UserId == customerId);
						customer.AccountBalance = customer.AccountBalance + order.TotalPayment;
						context.SaveChanges();
						//update admin banance
						var admin = context.User.First(x => x.UserId == Constants.ADMIN_USER_ID);
						admin.AccountBalance = admin.AccountBalance - order.TotalPayment;
						context.SaveChanges();
					}
					transaction.Commit();	
				}
				catch(Exception ex) 
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
								x.ProductVariant.Product.Shop.ShopName.Contains(shopName) &&
								(orderId == 0 ? true : x.OrderId == orderId) &&
								(status ==0 ? true : x.OrderStatusId == status)
							).OrderByDescending(x => x.OrderDate).ToList();
			}
			return orders;
		}

		internal (string, string) AddOrder(long userId, List<ProductRequestAddOrderDTO> orders , bool isUseCoin)
		{
			using (DatabaseContext context = new DatabaseContext())
			{
				var transaction = context.Database.BeginTransaction();
				try
				{
					// get bussinsis fee
					var businessFeeDate = context.BusinessFee.Max(x => x.StartDate);
					var businessFee = context.BusinessFee.FirstOrDefault(x => x.StartDate == businessFeeDate);
					if (businessFee == null)
					{
						return (Constants.RESPONSE_CODE_DATA_NOT_FOUND, "Business fee not found!");
					}
					long businessFeeId = businessFee.BusinessFeeId;
					long businessFeeValue = businessFee.Fee;



					foreach (var data in orders)
					{
						//get customer info
						var customerInfo = context.User
							.Select(x => new User { UserId = x.UserId, Coin = x.Coin })
							.FirstOrDefault(x => x.UserId == userId);
						if (customerInfo == null)
						{
							return (Constants.RESPONSE_CODE_DATA_NOT_FOUND, "Customer not found!");
						}
						long customerCoin = customerInfo.Coin;

						// get productVariant
						ProductVariant? productVariant = context.ProductVariant
							.FirstOrDefault(x => x.ProductVariantId == data.ProductVariantId && x.isActivate);
						if (productVariant == null)
						{
							transaction.Rollback();
							return (Constants.RESPONSE_CODE_DATA_NOT_FOUND, "Product variant not found!");
						}
						//get product 
						Product? product = context.Product
							.FirstOrDefault(x => x.ProductId == productVariant.ProductId && x.ProductStatusId == Constants.PRODUCT_ACTIVE);
						if (product == null)
						{
							transaction.Rollback();
							return (Constants.RESPONSE_CODE_DATA_NOT_FOUND, "Product not found!");
						}

						//check customers buy their own products 
						if(userId == product.ShopId)
						{
							transaction.Rollback();
							return (Constants.RESPONSE_CODE_NOT_ACCEPT, "Customers buy their own products !");
						}

						//get coupons
						long totalCouponsDiscount = 0;

						// get list coupon
						List<Coupon> coupons = new List<Coupon>();

						if (data.Coupons != null && data.Coupons.Count != 0)
						{
							//get coupon
							coupons = (from coupon in context.Coupon
									   where data.Coupons.Distinct().Contains(coupon.CouponCode) &&
									   coupon.StartDate < DateTime.Now && coupon.EndDate > DateTime.Now &&
									   coupon.IsActive && coupon.Quantity > 0
									   select coupon).ToList();

							if (coupons.Count != data.Coupons.Count)
							{
								transaction.Rollback();
                                return (Constants.RESPONSE_CODE_ORDER_COUPON_USED, "A coupon has been used!");
                            }
                            totalCouponsDiscount = coupons.Sum(x => x.PriceDiscount);
						}
						long totalAmount = productVariant.Price * data.Quantity * (100 - product.Discount) / 100;
						long totalPayment = totalAmount - totalCouponsDiscount;

						// set total coin discount
						long totalCoinDiscount = 0;

						if (isUseCoin && customerCoin > 0 && totalPayment > 0) 
						{
							if(totalPayment <= customerCoin)
							{
								totalCoinDiscount = totalPayment;
								totalPayment = 0;
							}
							else
							{
								totalCoinDiscount = customerCoin;
								totalPayment = totalPayment - customerCoin;
							}
						}

						if (totalPayment < 0)
						{
							totalPayment = 0;
						}

						var order = new Order
						{
							UserId = userId,
							ProductVariantId = data.ProductVariantId,
							BusinessFeeId = businessFeeId,
							OrderStatusId = Constants.ORDER_WAIT_CONFIRMATION,
							Quantity = data.Quantity,
							Price = productVariant.Price,
							Discount = product.Discount,
							TotalAmount = totalAmount,
							TotalCouponDiscount = totalCouponsDiscount,
							TotalCoinDiscount = totalCoinDiscount,
							TotalPayment = totalPayment,
							OrderDate = DateTime.Now,
						};
						context.Order.Add(order);
						context.SaveChanges();

						// update asset info
						var assetInformations = context.AssetInformation.Where(a => a.ProductVariantId == order.ProductVariantId && a.IsActive == true).Take(order.Quantity).ToList();
						if (assetInformations.Count != order.Quantity)
						{
							transaction.Rollback();
							return (Constants.RESPONSE_CODE_ORDER_NOT_ENOUGH_QUANTITY, "Buy more than available quantity!");
						}

						foreach (var asset in assetInformations)
						{
							asset.OrderId = order.OrderId;
							asset.IsActive = false;
						}
						context.AssetInformation.UpdateRange(assetInformations);
						context.SaveChanges();

						// update customer coin 
						if(totalCoinDiscount > 0)
						{
							var customer = context.User.FirstOrDefault(x => x.UserId == order.UserId);
							if (customer == null)
							{
								transaction.Rollback();
								return (Constants.RESPONSE_CODE_DATA_NOT_FOUND, "Customer not found!");
							}
							customer.Coin = customer.Coin - totalCoinDiscount;
							context.User.Update(customer);
							context.SaveChanges();
						}

						//update customer and admin account balance
						if (totalPayment > 0)
						{
							
							var customer = context.User.FirstOrDefault(x => x.UserId == order.UserId);
							if (customer == null)
							{
								transaction.Rollback();
								return (Constants.RESPONSE_CODE_DATA_NOT_FOUND, "Customer not found!");
							}
							if (customer.AccountBalance < totalPayment)
							{
								transaction.Rollback();
								return (Constants.RESPONSE_CODE_ORDER_INSUFFICIENT_BALANCE, "Insufficient balance!");
							}
							customer.AccountBalance = customer.AccountBalance - totalPayment;

							//update admin account balance
							var admin = context.User.First(x => x.UserId == Constants.ADMIN_USER_ID);
							admin.AccountBalance = admin.AccountBalance + totalPayment;

							context.User.UpdateRange(admin, customer);
							context.SaveChanges();
						}

						// add orderCoupon and update coupon's status
						if (coupons.Count != 0)
						{
							foreach (var coupon in coupons)
							{
								OrderCoupon orderCoupon = new OrderCoupon()
								{
									OrderId = order.OrderId,
									CouponId = coupon.CouponId,
									PriceDiscount = coupon.PriceDiscount,
									UseDate = DateTime.Now
								};
								context.OrderCoupon.Add(orderCoupon);
								context.SaveChanges();

								coupon.Quantity = coupon.Quantity - 1;
								context.Coupon.Update(coupon);
								context.SaveChanges();
							}
						}


						// add new transaction coin
						if(totalCoinDiscount > 0)
						{
							TransactionCoin transactionCoin = new TransactionCoin
							{
								UserId = userId,
								OrderId = order.OrderId,
								TransactionCoinTypeId = Constants.TRANSACTION_COIN_TYPE_USE,
								Amount = totalCoinDiscount,
								DateCreate = DateTime.Now,
							};
							context.TransactionCoin.Add(transactionCoin); 
							context.SaveChanges();
						}

						// add new transaction internal
						TransactionInternal newTransaction = new TransactionInternal
						{
							UserId = order.UserId,
							TransactionInternalTypeId = Constants.TRANSACTION_TYPE_INTERNAL_PAYMENT,
							OrderId = order.OrderId,
							PaymentAmount = order.TotalAmount,
							Note = "Payment",
							DateCreate = DateTime.Now
						};
						context.TransactionInternal.Add(newTransaction);
						context.SaveChanges();

					}
					context.SaveChanges();
					transaction.Commit();

				}
				catch (Exception ex)
				{
					transaction.Rollback();
					throw new Exception(ex.Message);
				}
				return (Constants.RESPONSE_CODE_SUCCESS, "Success!");
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
									TotalAmount = o.TotalAmount,
									TotalCouponDiscount = o.TotalCouponDiscount,
									TotalCoinDiscount = o.TotalCoinDiscount,
									TotalPayment = o.TotalPayment,
									Note = o.Note,
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
															 where product.ProductId == productMedia.ProductId
															 select new ProductMedia { Url = productMedia.Url }
															).ToList()
										}
									},
									BusinessFee = new BusinessFee
									{
										BusinessFeeId = businessFee.BusinessFeeId,
										Fee = businessFee.Fee,
									},
									/*
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
									*/
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

		internal void UpdateOrderStatusSellerViolates(long orderId, string? note)
		{
			using (DatabaseContext context = new DatabaseContext())
			{
				var transaction = context.Database.BeginTransaction();
				try
				{
					var order = context.Order.First(x => x.OrderId == orderId);
					order.OrderStatusId = Constants.ORDER_SELLER_VIOLATES;
					order.Note = note;
					context.SaveChanges();

					var customerId = order.UserId;

					// add transaction for refund money to customer
					TransactionInternal transactionInternal = new TransactionInternal()
					{
						UserId = customerId,
						OrderId = orderId,
						TransactionInternalTypeId = Constants.TRANSACTION_TYPE_INTERNAL_RECEIVE_REFUND,
						PaymentAmount = order.TotalPayment,
						DateCreate = DateTime.Now,
					};
					context.TransactionInternal.Add(transactionInternal);
					context.SaveChanges();

					// update customer account balance
					var customer = context.User.First(x => x.UserId == customerId);
					customer.AccountBalance = customer.AccountBalance + order.TotalPayment;
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

		internal void UpdateOrderStatusRejectComplaint(long orderId, string? note)
		{
			using (DatabaseContext context = new DatabaseContext())
			{
				var transaction = context.Database.BeginTransaction();
				try
				{
					var order = context.Order
									.Include(x => x.BusinessFee)
									.Include(x => x.ProductVariant)
									.ThenInclude(x => x.Product)
									.First(x => x.OrderId == orderId);
					order.OrderStatusId = Constants.ORDER_REJECT_COMPLAINT;
					order.Note = note;

					var sellerId = order.ProductVariant.Product.ShopId;
					var adminProfit = order.TotalPayment * order.BusinessFee.Fee / 100;
					var sellerProfit = order.TotalPayment - adminProfit;

					// add transaction for refund money to seller
					TransactionInternal transactionInternalSeller = new TransactionInternal()
					{
						UserId = sellerId,
						OrderId = orderId,
						TransactionInternalTypeId = Constants.TRANSACTION_TYPE_INTERNAL_RECEIVE_PAYMENT,
						PaymentAmount = sellerProfit,
						DateCreate = DateTime.Now,
					};
					context.TransactionInternal.Add(transactionInternalSeller);

					// add transaction for get profit admin
					TransactionInternal transactionInternalAdmin = new TransactionInternal()
					{
						UserId = Constants.ADMIN_USER_ID,
						OrderId = orderId,
						TransactionInternalTypeId = Constants.TRANSACTION_TYPE_INTERNAL_RECEIVE_PROFIT,
						PaymentAmount = adminProfit,
						DateCreate = DateTime.Now,
					};
					context.TransactionInternal.Add(transactionInternalAdmin);

					// update seller account balance
					var seller = context.User.First(x => x.UserId == sellerId);
					seller.AccountBalance = seller.AccountBalance + sellerProfit;

					//update admin profit account balance
					var admin = context.User.First(x => x.UserId == Constants.ADMIN_USER_ID);
					admin.AccountBalance = admin.AccountBalance + adminProfit;
					
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
		internal void UpdateOrderStatusAdmin(long orderId, int status, string? note)
		{
			using (DatabaseContext context = new DatabaseContext())
			{
				var order = context.Order.First(x => x.OrderId == orderId);
				order.OrderStatusId = status;
				order.Note = note;
				context.SaveChanges();
			}
		}

		internal Order? GetOrderForCheckingExisted(long orderId)
		{
			using (DatabaseContext context = new DatabaseContext())
			{
				var order = context.Order
					.Include(x => x.ProductVariant)
					.ThenInclude(x => x.Product)
					.FirstOrDefault(x => x.OrderId == orderId);
				return order;
			}
		}

		internal List<Order> GetAllOrderByUser(long userId, List<long> statusId, int limit, int offset)
		{
			using (DatabaseContext context = new DatabaseContext())
			{
				List<Order> orders = context.Order
					.Include(x => x.Feedback)
					.Include(x => x.AssetInformations)
					.Include(x => x.ProductVariant)
					.ThenInclude(x => x.Product)
					.ThenInclude(x => x.Shop)
					.Where(x => x.UserId == userId
						&&
						(statusId.Count == 1 && statusId[0] == 0
						? true :
						statusId.Any(st => st == x.OrderStatusId)))
					.OrderByDescending(x => x.OrderDate)
					.Skip(offset)
					.Take(limit)
					.ToList();
				return orders;
			}
		}

		internal void UpdateOrderStatusCustomer(long orderId, long shopId, int status)
		{
			using (DatabaseContext context = new DatabaseContext())
			{
				using (var transaction = context.Database.BeginTransaction())
				{
					try
					{
						// update order status
						Order order = context.Order.First(x => x.OrderId == orderId);
						order.OrderStatusId = status;
						if (status == Constants.ORDER_CONFIRMED)
						{
							BusinessFee fee = context.BusinessFee.First(x => x.BusinessFeeId == order.BusinessFeeId);
							var adminProfit = order.TotalPayment * fee.Fee / 100;
							var sellerProfit = order.TotalPayment - adminProfit;
							// update blance of seller
							var seller = context.User.First(x => x.UserId == shopId);
							seller.AccountBalance = seller.AccountBalance + sellerProfit;
							// add transaction receive payment and profit
							List<TransactionInternal> trans = new List<TransactionInternal>
							{
								new TransactionInternal
								{
									DateCreate = DateTime.Now,
									Note = "Receive Payment",
									OrderId = order.OrderId,
									PaymentAmount = order.TotalPayment - (order.TotalPayment * fee.Fee / 100),
									TransactionInternalTypeId = Constants.TRANSACTION_TYPE_INTERNAL_RECEIVE_PAYMENT,
									UserId = shopId,
								},
								new TransactionInternal
								{
									DateCreate = DateTime.Now,
									Note = "Profit",
									OrderId = order.OrderId,
									PaymentAmount = order.TotalPayment * fee.Fee / 100,
									TransactionInternalTypeId = Constants.TRANSACTION_TYPE_INTERNAL_RECEIVE_PROFIT,
									UserId = Constants.ADMIN_USER_ID,
								},
							};
							context.TransactionInternal.AddRange(trans);
							// update profit for admin
							User admin = context.User.First(x => x.UserId == Constants.ADMIN_USER_ID);
							admin.AccountBalance += adminProfit;
						}
						context.SaveChanges();
						transaction.Commit();
					}
					catch (Exception e)
					{
						transaction.Rollback();
						throw new Exception(e.Message);
					}
				}
			}
		}

		internal List<OrderCoupon> GetCouponsInOrder(long orderId)
		{
			using (DatabaseContext context = new DatabaseContext())
			{
				var orderCoupons = (from orderCoupon in context.OrderCoupon
									join  coupon in context.Coupon
										on orderCoupon.CouponId equals coupon.CouponId
									where orderCoupon.OrderId == orderId
									select new OrderCoupon 
									{ 
										PriceDiscount =orderCoupon.PriceDiscount,
										Coupon = new Coupon { CouponName  = coupon.CouponName },
									}).ToList();
				return orderCoupons;
			}
		}
	}
}

