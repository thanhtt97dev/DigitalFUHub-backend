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
						var sellerId = order.ShopId;

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
							.Include(x => x.Shop)
							.Select(o => new Order
							{
								OrderId = o.OrderId,
								OrderDate = o.OrderDate,
								TotalPayment = o.TotalPayment,	
								User = new User 
								{
									UserId = o.User.UserId,
									Email = o.User.Email,
								},
								Shop = new Shop
								{
									UserId = o.ShopId,
									ShopName = o.Shop.ShopName
								},
								OrderStatusId = o.OrderStatusId
							})
							.Where(x =>
								fromDate <= x.OrderDate && toDate >= x.OrderDate &&
								x.User.Email.Contains(customerEmail) &&
								x.Shop.ShopName.Contains(shopName) &&
								(orderId == 0 ? true : x.OrderId == orderId) &&
								(status == 0 ? true : x.OrderStatusId == status)
							).OrderByDescending(x => x.OrderDate).ToList();
			
			}
			return orders;
		}

		#region Add order
		internal (string, string) AddOrder(long userId, List<ShopProductRequestAddOrderDTO> shopProducts, bool isUseCoin)
		{
			using (DatabaseContext context = new DatabaseContext())
			{
				var transaction = context.Database.BeginTransaction();
				try
				{
					// check valid quantity
					if (shopProducts.Any(x => x.Products.Any(p => p.Quantity <= 0))){
						return (Constants.RESPONSE_CODE_NOT_ACCEPT, "Invalid quantity order!");
					}

					//check customer existed
					var isCustomerExisted = context.User.Any(x => x.UserId == userId);
					if (!isCustomerExisted)
					{
						return (Constants.RESPONSE_CODE_DATA_NOT_FOUND, "Customer not existed!");
					}

					// get bussinsis fee
					var businessFeeDate = context.BusinessFee.Max(x => x.StartDate);
					var businessFee = context.BusinessFee.First(x => x.StartDate == businessFeeDate);
					long businessFeeId = businessFee.BusinessFeeId;
					long businessFeeValue = businessFee.Fee;

					foreach (var shopProduct in shopProducts)
					{
						// check ProductVariant existed
						var productVariantIdOrder = shopProduct.Products.Select(x => x.ProductVariantId).ToList();
						var productVariantOrder = context.ProductVariant
							.Include(x => x.Product)
							.Where(x => productVariantIdOrder.Contains(x.ProductVariantId)).ToList();

						bool isProductVariantExisted = productVariantIdOrder.All(id => productVariantOrder.Any(x => x.ProductVariantId == id));
						if (!isProductVariantExisted)
						{
							transaction.Rollback();
							return (Constants.RESPONSE_CODE_DATA_NOT_FOUND, "Product variant not existed!");
						}

						//check shop existed
						var shop = context.Shop.FirstOrDefault(x => x.UserId == shopProduct.ShopId);
						if (shop == null)
						{
							transaction.Rollback();
							return (Constants.RESPONSE_CODE_DATA_NOT_FOUND, "Shop not existed!");
						}

						//check ProductVariant of shop
						var isAllProductInShop = productVariantOrder
							.All(x => x.Product.ShopId == shopProduct.ShopId);

						if (!isAllProductInShop)
						{
							transaction.Rollback();
							return (Constants.RESPONSE_CODE_ORDER_PRODUCT_VARIANT_NOT_IN_SHOP, "A product variant not in shop!");
						}

						//get customer info
						var customer = context.User
							.FirstOrDefault(x => x.UserId == userId);
						if (customer == null)
						{
							transaction.Rollback();
							return (Constants.RESPONSE_CODE_DATA_NOT_FOUND, "Customer not found!");
						}
						long customerCoin = customer.Coin;

						//check customers buy their own products 
						var isCustomerBuyTheirOwnProducts = productVariantOrder
							.Any(x => x.Product.ShopId == userId);
						if (isCustomerBuyTheirOwnProducts)
						{
							transaction.Rollback();
							return (Constants.RESPONSE_CODE_ORDER_CUSTOMER_BUY_THEIR_OWN_PRODUCT, "Customers buy their own products !");
						}

						//create order
						Order order = new Order()
						{
							UserId = userId,
							ShopId = shopProduct.ShopId,
							BusinessFeeId = businessFeeId,
							OrderStatusId = Constants.ORDER_WAIT_CONFIRMATION,
							OrderDate = DateTime.Now
						};
						context.Order.Add(order);
						context.SaveChanges();

						//create order detail
						List<OrderDetail> orderDetails = new List<OrderDetail>();	
						foreach (var item in shopProduct.Products)
						{
							// check quantity
							var assetInformationRemaining = context.AssetInformation
								.Where(a => a.ProductVariantId == item.ProductVariantId && a.IsActive == true)
								.Take(item.Quantity);
							if(assetInformationRemaining.Count() < item.Quantity)
							{
								transaction.Rollback();
								return (Constants.RESPONSE_CODE_ORDER_NOT_ENOUGH_QUANTITY, "Buy more than available quantity!");
							}

							//get product productVariant info
							var productVariant =  context.ProductVariant
								.Include(x => x.Product)
								.ThenInclude(x => x.Shop)
								.Select(x => new ProductVariant
								{
									ProductVariantId = x.ProductVariantId,	
									ProductId = x.ProductId,
									Price = x.Price,
									isActivate = x.isActivate,
									Product = new Product
									{
										Discount = x.Product.Discount,
										ProductStatusId = x.Product.ProductStatusId,
										Shop = new Shop
										{
											IsActive = x.Product.Shop.IsActive
										}
									}
								})
								.First(x => x.ProductVariantId == item.ProductVariantId);

							if(!productVariant.isActivate || productVariant.Product.ProductStatusId == Constants.PRODUCT_BAN ||
								productVariant.Product.ProductStatusId == Constants.PRODUCT_HIDE || 
								!productVariant.Product.Shop.IsActive) 
							{
								transaction.Rollback();
								return (Constants.RESPONSE_CODE_ORDER_PRODUCT_HAS_BEEN_BANED, "Product has been baned");
							}

							OrderDetail orderDetail = new OrderDetail
							{
								OrderId = order.OrderId,
								ProductVariantId = item.ProductVariantId,	
								Quantity = item.Quantity,
								Price = productVariant.Price,
								Discount = productVariant.Product.Discount,	
								TotalAmount = productVariant.Price * item.Quantity * (100 - productVariant.Product.Discount) / 100,
								IsFeedback = false,
							};
							orderDetails.Add(orderDetail);

							// add order detail
							context.OrderDetail.Add(orderDetail);
							context.SaveChanges();

							// update asset information
							foreach (var asset in assetInformationRemaining)
							{
								asset.OrderDetailId = orderDetail.OrderDetailId;
								asset.IsActive = false;
							}
							context.AssetInformation.UpdateRange(assetInformationRemaining);
							context.SaveChanges();
						}


						// cacualte order info

						long totalAmount = orderDetails.Sum(x => x.TotalAmount);
						// check coupon
						long totalCouponDiscount = 0;
						if (!string.IsNullOrEmpty(shopProduct.Coupon))
						{
							var coupon = (from c in context.Coupon
									  where
									  shopProduct.Coupon == c.CouponCode &&
									  c.StartDate < DateTime.Now && c.EndDate > DateTime.Now &&
									  c.IsActive && c.Quantity > 0
									  select c).FirstOrDefault();
							if (coupon == null)
							{
								transaction.Rollback();
								return (Constants.RESPONSE_CODE_ORDER_COUPON_USED, "A coupon has been used!");
							}

							if (coupon.MinTotalOrderValue > totalAmount)
							{
								transaction.Rollback();
								return (Constants.RESPONSE_CODE_ORDER_NOT_ELIGIBLE, "Orders are not eligible to apply the coupons!");
							}
							totalCouponDiscount = coupon.PriceDiscount;

							// add orderCoupon and update coupon's quantity
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

						long totalPayment = totalAmount - totalCouponDiscount;

						if (totalPayment < 0)
						{
							totalPayment = 0;
						}

						// caculate total coin discount
						long totalCoinDiscount = 0;
						if (isUseCoin && customerCoin > 0 && totalPayment > 0)
						{
							if (totalPayment <= customerCoin)
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

						//update customer and admin account balance
						if (totalPayment > 0)
						{
							if (customer.AccountBalance < totalPayment)
							{
								transaction.Rollback();
								return (Constants.RESPONSE_CODE_ORDER_INSUFFICIENT_BALANCE, "Insufficient balance!");
							}
							customer.AccountBalance = customer.AccountBalance - totalPayment;
							if (totalCoinDiscount > 0)
							{
								customer.Coin = customer.Coin - totalCoinDiscount;
							}

							//update admin account balance
							var admin = context.User.First(x => x.UserId == Constants.ADMIN_USER_ID);
							admin.AccountBalance = admin.AccountBalance + totalPayment;

							context.User.UpdateRange(admin, customer);
							context.SaveChanges();
						}

						// update order
						order.TotalAmount = totalAmount;
						order.TotalCouponDiscount = totalCouponDiscount;
						order.TotalCoinDiscount = totalCoinDiscount;
						order.TotalPayment = totalPayment;

						context.Order.Update(order);
						context.SaveChanges();

						// add new transaction coin
						if (totalCoinDiscount > 0)
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
						if(totalPayment > 0)
						{
							TransactionInternal newTransaction = new TransactionInternal
							{
								UserId = order.UserId,
								TransactionInternalTypeId = Constants.TRANSACTION_TYPE_INTERNAL_PAYMENT,
								OrderId = order.OrderId,
								PaymentAmount = order.TotalPayment,
								Note = "Payment",
								DateCreate = DateTime.Now
							};
							context.TransactionInternal.Add(newTransaction);
							context.SaveChanges();
						}
					}
					transaction.Commit();
				}
				catch (Exception ex)
				{
					transaction.Rollback();
					throw new Exception(ex.Message);
				}
			}
			return (Constants.RESPONSE_CODE_SUCCESS, "Success!");
		}
		#endregion

		internal Order? GetOrder(long orderId)
		{
			return null;
			/*
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
									TotalCouponDiscount = o.TotalCouponDiscount,
									TotalAmount = o.TotalAmount,
									TotalCoinDiscount = o.TotalCoinDiscount,
									TotalPayment = o.TotalPayment,
									Note = o.Note,
									IsFeedback = o.IsFeedback,
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
								})
							   .FirstOrDefault();


				if (order != null && order.IsFeedback)
				{
					Feedback feedback = context.Feedback
						.Select(f => new Feedback()
						{
							FeedbackId = f.FeedbackId,
							Rate = f.Rate,
						})
						.First(x => x.OrderId == order.OrderId);
					order.Feedback = feedback;
				}

				return order;
			}
			*/
		}

		internal Order? GetSellerOrderDetail(long orderId)
		{
			return null;
			/*
			using (DatabaseContext context = new DatabaseContext())
			{
				return context.Order.Include(i => i.AssetInformations)
					.ThenInclude(ti => ti.ProductVariant).ThenInclude(x => x.Product).Include(x => x.User)
					.Where(x => x.OrderId == orderId).FirstOrDefault();
			}
			*/
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
									.First(x => x.OrderId == orderId);
					order.OrderStatusId = Constants.ORDER_REJECT_COMPLAINT;
					order.Note = note;
					context.Order.Update(order);

					var sellerId = order.ShopId;
					var adminProfit = order.TotalAmount * order.BusinessFee.Fee / 100;
					var sellerProfit = order.TotalAmount - adminProfit;

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
			return;
			/*
			using (DatabaseContext context = new DatabaseContext())
			{
				var order = context.Order.First(x => x.OrderId == orderId);
				order.OrderStatusId = status;
				order.Note = note;
				context.SaveChanges();
			}
			*/
		}

		internal Order? GetOrderForCheckingExisted(long orderId)
		{
			using (DatabaseContext context = new DatabaseContext())
			{
				var order = context.Order
					.FirstOrDefault(x => x.OrderId == orderId);
				return order;
			}
		}

		internal List<Order> GetAllOrderByUser(long userId, List<long> statusId, int limit, int offset)
		{

			using (DatabaseContext context = new DatabaseContext())
			{
				List<Order> orders = context.Order
					.Include(x => x.OrderCoupons)
					.Include(x => x.Shop)
					.Include(x => x.OrderDetails)
					.ThenInclude(x => x.ProductVariant)
					.ThenInclude(x => x.Product)
					.Where(x => x.UserId == userId
						&& (statusId.Count == 1 && statusId[0] == 0 ? true : statusId.Any(st => st == x.OrderStatusId)))
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
							var adminProfit = order.TotalAmount * fee.Fee / 100;
							var sellerProfit = order.TotalAmount - adminProfit;
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
									PaymentAmount = sellerProfit,
									TransactionInternalTypeId = Constants.TRANSACTION_TYPE_INTERNAL_RECEIVE_PAYMENT,
									UserId = shopId,
								},
								new TransactionInternal
								{
									DateCreate = DateTime.Now,
									Note = "Profit",
									OrderId = order.OrderId,
									PaymentAmount = adminProfit,
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
									join coupon in context.Coupon
										on orderCoupon.CouponId equals coupon.CouponId
									where orderCoupon.OrderId == orderId
									select new OrderCoupon
									{
										PriceDiscount = orderCoupon.PriceDiscount,
										Coupon = new Coupon { CouponName = coupon.CouponName },
									}).ToList();
				return orderCoupons;
			}
		}
	}
}

