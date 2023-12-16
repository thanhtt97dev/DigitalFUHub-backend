using BusinessObject;
using BusinessObject.Entities;
using Comons;
using DTOs.Order;
using DTOs.Statistic;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml.Style;
using System;
using System.Diagnostics;
using System.Drawing;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

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

		#region Update List Order's status in range time (WaitConfirmation => Confirm)
		internal void UpdateStatusOrderFromWaitConfirmationToConfirmInPreviousDays(int days)
		{
			using (DatabaseContext context = new DatabaseContext())
			{
				var transaction = context.Database.BeginTransaction();
				try
				{
					DateTime timeAccept = DateTime.Now.AddDays(-days);
					var orders = context.Order
						.Where(x =>
							x.OrderStatusId == Constants.ORDER_STATUS_WAIT_CONFIRMATION &&
							x.OrderDate < timeAccept
						)
						.ToList();
					if (orders.Count() == 0) return;

					foreach (var order in orders)
					{
						// update order status
						order.OrderStatusId = Constants.ORDER_STATUS_CONFIRMED;

						//get platform fee
						var fee = context.BusinessFee.First(x => x.BusinessFeeId == order.BusinessFeeId).Fee;

						//get sellerId
						var sellerId = order.ShopId;

						//get profit
						var businessFee = order.TotalAmount * fee / 100;
						var adminProfit = order.TotalCoinDiscount > businessFee ? 0 : businessFee - order.TotalCoinDiscount;
						var sellerProfit = order.TotalAmount - order.TotalCouponDiscount - businessFee;

						// update sold count number of product
						var orderDetails = context.OrderDetail.Where(x => x.OrderId == order.OrderId).ToList();
						foreach (var orderDetail in orderDetails)
						{
							var productVariant = context.ProductVariant.First(x => x.ProductVariantId == orderDetail.ProductVariantId);
							var product = context.Product.First(x => x.ProductId == productVariant.ProductId);
							product.SoldCount += orderDetail.Quantity;
							context.Product.Update(product);
						}

						if (sellerProfit > 0)
						{
							// update seller's balance
							var seller = context.User.First(x => x.UserId == sellerId);
							seller.AccountBalance = seller.AccountBalance + sellerProfit;

							// add transaction for refund money to seller
							TransactionInternal transactionSeller = new TransactionInternal()
							{
								UserId = sellerId,
								TransactionInternalTypeId = Constants.TRANSACTION_INTERNAL_TYPE_RECEIVE_PAYMENT,
								OrderId = order.OrderId,
								PaymentAmount = sellerProfit,
								Note = "",
								DateCreate = DateTime.Now,
							};
							context.TransactionInternal.Add(transactionSeller);
						}

						if (adminProfit > 0)
						{
							// update admin's balance
							var admin = context.User.First(x => x.UserId == Constants.ADMIN_USER_ID);
							admin.AccountBalance = admin.AccountBalance + adminProfit;

							// add transaction for get benefit
							TransactionInternal transactionAdmin = new TransactionInternal()
							{
								UserId = Constants.ADMIN_USER_ID,
								TransactionInternalTypeId = Constants.TRANSACTION_INTERNAL_TYPE_RECEIVE_PROFIT,
								OrderId = order.OrderId,
								PaymentAmount = adminProfit,
								Note = "",
								DateCreate = DateTime.Now,
							};
							context.TransactionInternal.Add(transactionAdmin);
						}

						// add history order status
						HistoryOrderStatus historyOrderStatus = new HistoryOrderStatus
						{
							OrderId = order.OrderId,
							OrderStatusId = Constants.ORDER_STATUS_CONFIRMED,
							DateCreate = DateTime.Now,
						};
						context.HistoryOrderStatus.Add(historyOrderStatus);
					}
					context.Order.UpdateRange(orders);
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
		#endregion

		#region Update List Order's status in range time (Complaint => SellerRefunded)
		internal void UpdateStatusOrderFromComplaintToSellerRefundedInPreviousDays(int days)
		{
			using (DatabaseContext context = new DatabaseContext())
			{
				var transaction = context.Database.BeginTransaction();
				try
				{
					DateTime timeAccept = DateTime.Now.AddDays(-days);
					var orders = context.Order
						.Where(x =>
							x.OrderStatusId == Constants.ORDER_STATUS_COMPLAINT &&
							x.OrderDate < timeAccept)
						.ToList();

					foreach (var order in orders)
					{
						//update order status
						order.OrderStatusId = Constants.ORDER_STATUS_SELLER_REFUNDED;

						var customerId = order.UserId;
						var customer = context.User.First(x => x.UserId == customerId);

						if (order.TotalPayment > 0)
						{
							// update customer balance
							customer.AccountBalance = customer.AccountBalance + order.TotalPayment;

							// add transaction internal
							var transactionInternal = new TransactionInternal()
							{
								UserId = customerId,
								TransactionInternalTypeId = Constants.TRANSACTION_INTERNAL_TYPE_RECEIVE_REFUND,
								OrderId = order.OrderId,
								PaymentAmount = order.TotalPayment,
								Note = "Seller refund money",
								DateCreate = DateTime.Now,
							};
							context.TransactionInternal.Add(transactionInternal);

						}
						if (order.TotalCoinDiscount > 0)
						{
							//refund coin of customer
							customer.Coin = customer.Coin + order.TotalCoinDiscount;
							var transactionCoin = new TransactionCoin
							{
								UserId = customerId,
								TransactionCoinTypeId = Constants.TRANSACTION_COIN_TYPE_REFUND,
								OrderId = order.OrderId,
								Amount = order.TotalCoinDiscount,
								DateCreate = DateTime.Now
							};
							context.TransactionCoin.Add(transactionCoin);
						}

						// add history order status
						HistoryOrderStatus historyOrderStatus = new HistoryOrderStatus
						{
							OrderId = order.OrderId,
							OrderStatusId = Constants.ORDER_STATUS_SELLER_REFUNDED,
							DateCreate = DateTime.Now,
						};
						context.HistoryOrderStatus.Add(historyOrderStatus);
					}
					context.Order.UpdateRange(orders);
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
		#endregion

		#region Get total record with condition
		internal int GetNumberOrders(long orderId, string customerEmail, long shopId, string shopName, DateTime? fromDate, DateTime? toDate, int status)
		{
			using (DatabaseContext context = new DatabaseContext())
			{
				var orders = context.Order
							.Include(x => x.User)
							.Include(x => x.Shop)
							.Where
							(x =>
								((fromDate != null && toDate != null) ? fromDate <= x.OrderDate && toDate >= x.OrderDate : true) &&
								x.User.Email.Contains(customerEmail) &&
								((shopId == 0) ? true : x.Shop.UserId == shopId) &&
								x.Shop.ShopName.Contains(shopName) &&
								(orderId == 0 ? true : x.OrderId == orderId) &&
								(status == 0 ? true : x.OrderStatusId == status)
							)
							.Count();
				return orders;
			}
		}
		#endregion

		#region Get orders with conditions
		internal List<Order> GetOrders(long orderId, string customerEmail, long shopId, string shopName, DateTime? fromDate, DateTime? toDate, int status, int page)
		{
			List<Order> orders = new List<Order>();
			using (DatabaseContext context = new DatabaseContext())
			{
				orders = context.Order
							.Include(x => x.User)
							.Include(x => x.Shop)
							.Include(x => x.BusinessFee)
							.Select(o => new Order
							{
								OrderId = o.OrderId,
								OrderDate = o.OrderDate,
								TotalPayment = o.TotalPayment,
								TotalAmount = o.TotalAmount,
								TotalCouponDiscount = o.TotalCouponDiscount,
								TotalCoinDiscount = o.TotalCoinDiscount,
								BusinessFee = new BusinessFee
								{
									Fee = o.BusinessFee.Fee
								},
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
								((fromDate != null && toDate != null) ? fromDate <= x.OrderDate && toDate >= x.OrderDate : true) &&
								x.User.Email.Contains(customerEmail) &&
								((shopId == 0) ? true : x.Shop.UserId == shopId) &&
								x.Shop.ShopName.Contains(shopName) &&
								(orderId == 0 ? true : x.OrderId == orderId) &&
								(status == 0 ? true : x.OrderStatusId == status)
							)
							.OrderByDescending(x => x.OrderDate)
							.Skip((page - 1) * Constants.PAGE_SIZE)
							.Take(Constants.PAGE_SIZE)
							.ToList();

			}
			return orders;
		}
		#endregion

		#region Add order
		internal (string, string, int, Order) AddOrder(long userId, List<ShopProductRequestAddOrderDTO> shopProducts, bool isUseCoin)
		{
			using (DatabaseContext context = new DatabaseContext())
			{
				var transaction = context.Database.BeginTransaction();
				try
				{
					int numberQuantityAvailable = 0;
					Order orderResult = new Order();

					#region Check customer buy with quantity < 0
					if (shopProducts.Any(x => x.Products.Any(p => p.Quantity <= 0)))
					{
						return (Constants.RESPONSE_CODE_NOT_ACCEPT, "Invalid quantity order!", numberQuantityAvailable, orderResult);
					}
					#endregion

					#region Check customer existed
					var isCustomerExisted = context.User.Any(x => x.UserId == userId && x.Status == true);
					if (!isCustomerExisted)
					{
						return (Constants.RESPONSE_CODE_DATA_NOT_FOUND, "Customer not existed!", numberQuantityAvailable, orderResult);
					}

					#endregion

					#region Get bussinsis fee
					var businessFeeDate = context.BusinessFee.Max(x => x.StartDate);
					var businessFee = context.BusinessFee.First(x => x.StartDate == businessFeeDate);
					long businessFeeId = businessFee.BusinessFeeId;
					long businessFeeValue = businessFee.Fee;
					#endregion

					#region Make order
					foreach (var shopProduct in shopProducts)
					{
						#region Check ProductVariant existed
						var productVariantIds = shopProduct.Products.Select(x => x.ProductVariantId).ToList();
						var productVariants = context.ProductVariant
							.Include(x => x.Product)
							.Where(x => productVariantIds.Contains(x.ProductVariantId) &&
								   x.isActivate &&
								   x.Product.ProductStatusId == Constants.PRODUCT_STATUS_ACTIVE
								   )
							.ToList();

						bool isProductVariantsExisted = productVariantIds.All(id => productVariants.Any(x => x.ProductVariantId == id));
						if (!isProductVariantsExisted)
						{
							transaction.Rollback();
							return (Constants.RESPONSE_CODE_DATA_NOT_FOUND, "Product variant not existed!", numberQuantityAvailable, orderResult);
						}
						#endregion

						#region Check shop existed
						var shop = context.Shop.FirstOrDefault(x => x.UserId == shopProduct.ShopId && x.IsActive);
						if (shop == null)
						{
							transaction.Rollback();
							return (Constants.RESPONSE_CODE_DATA_NOT_FOUND, "Shop not existed!", numberQuantityAvailable, orderResult);
						}
						#endregion

						#region Check all product variant order in shop
						var isAllProductVariantInShop = productVariants
							.All(x => x.Product.ShopId == shopProduct.ShopId);

						if (!isAllProductVariantInShop)
						{
							transaction.Rollback();
							return (Constants.RESPONSE_CODE_ORDER_PRODUCT_VARIANT_NOT_IN_SHOP, "A product variant not in shop!", numberQuantityAvailable, orderResult);
						}
						#endregion

						#region Check customer existed
						var customer = context.User
							.FirstOrDefault(x => x.UserId == userId && x.Status == true);
						if (customer == null)
						{
							transaction.Rollback();
							return (Constants.RESPONSE_CODE_DATA_NOT_FOUND, "Customer not found!", numberQuantityAvailable, orderResult);
						}
						var customerCoin = customer.Coin;
						#endregion

						#region Check customer buy their own products 
						var isCustomerBuyTheirOwnProducts = productVariants
							.Any(x => x.Product.ShopId == userId);
						if (isCustomerBuyTheirOwnProducts)
						{
							transaction.Rollback();
							return (Constants.RESPONSE_CODE_ORDER_CUSTOMER_BUY_THEIR_OWN_PRODUCT, "Customers buy their own products !", numberQuantityAvailable, orderResult);
						}
						#endregion

						#region Create order
						Order order = new Order()
						{
							UserId = userId,
							ShopId = shopProduct.ShopId,
							BusinessFeeId = businessFeeId,
							OrderStatusId = Constants.ORDER_STATUS_WAIT_CONFIRMATION,
							OrderDate = DateTime.Now
						};
						context.Order.Add(order);
						context.SaveChanges();
						#endregion

						#region Create list order detail
						List<OrderDetail> orderDetails = new List<OrderDetail>();
						foreach (var item in shopProduct.Products)
						{
							// check quantity
							var assetInformationRemaining = context.AssetInformation
								.Where(a => a.ProductVariantId == item.ProductVariantId && a.IsActive == true);
							if (assetInformationRemaining.Count() < item.Quantity)
							{
								transaction.Rollback();
								numberQuantityAvailable = assetInformationRemaining.Count();
								return (Constants.RESPONSE_CODE_ORDER_NOT_ENOUGH_QUANTITY, "Buy more than available quantity!", numberQuantityAvailable, orderResult);
							}
							assetInformationRemaining = assetInformationRemaining.Take(item.Quantity);

							//get product productVariant info
							var productVariant = context.ProductVariant
								.Include(x => x.Product)
								.ThenInclude(x => x.Shop)
								.Select(x => new ProductVariant
								{
									ProductVariantId = x.ProductVariantId,
									ProductId = x.ProductId,
									Price = x.Price,
									Discount = x.Discount,
									isActivate = x.isActivate,
									Product = new Product
									{
										ProductStatusId = x.Product.ProductStatusId,
										Shop = new Shop
										{
											IsActive = x.Product.Shop.IsActive
										}
									}
								})
								.First(x => x.ProductVariantId == item.ProductVariantId);

							if (!productVariant.isActivate || productVariant.Product.ProductStatusId == Constants.PRODUCT_STATUS_BAN ||
								productVariant.Product.ProductStatusId == Constants.PRODUCT_STATUS_REMOVE ||
								!productVariant.Product.Shop.IsActive)
							{
								transaction.Rollback();
								return (Constants.RESPONSE_CODE_ORDER_PRODUCT_HAS_BEEN_BANED, "Product has been baned", numberQuantityAvailable, orderResult);
							}

							OrderDetail orderDetail = new OrderDetail
							{
								OrderId = order.OrderId,
								ProductVariantId = item.ProductVariantId,
								Quantity = item.Quantity,
								Price = productVariant.Price,
								Discount = productVariant.Discount,
								TotalAmount = productVariant.Price * item.Quantity * (100 - productVariant.Discount) / 100,
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
						#endregion

						#region Cacualte order's info

						long totalAmount = orderDetails.Sum(x => x.TotalAmount);
						long totalCouponDiscount = 0;
						long totalCoinDiscount = 0;
						long totalPayment = 0;

						#region Check coupon
						if (!string.IsNullOrEmpty(shopProduct.Coupon))
						{
							var now = DateTime.Now;
							var coupon = (from c in context.Coupon
										  where
										  shopProduct.Coupon == c.CouponCode &&
										  now > c.StartDate &&
										  now < c.EndDate &&
										  c.IsActive && c.Quantity > 0
										  select c).FirstOrDefault();
							if (coupon == null)
							{
								transaction.Rollback();
								return (Constants.RESPONSE_CODE_ORDER_COUPON_NOT_EXISTED, "Coupon not existed!", numberQuantityAvailable, orderResult);
							}

							if (coupon.CouponTypeId == Constants.COUPON_TYPE_ALL_PRODUCTS)
							{

							}
							else if (coupon.CouponTypeId == Constants.COUPON_TYPE_ALL_PRODUCTS_OF_SHOP)
							{
								var couponOfShopExisted = context.Coupon.Any(x => x.ShopId == shopProduct.ShopId && x.CouponId == coupon.CouponId);
								if (!couponOfShopExisted)
								{
									return (Constants.RESPONSE_CODE_ORDER_COUPON_NOT_EXISTED, "Coupon not existed!", numberQuantityAvailable, orderResult);
								}
							}
							else if (coupon.CouponTypeId == Constants.COUPON_TYPE_SPECIFIC_PRODUCTS)
							{
								var listProductIdsOrder = productVariants.Select(x => x.ProductId).ToList();
								var couponForSpecificProductOfShopExisted = context.CouponProduct
																				.Any(x => x.CouponId == coupon.CouponId &&
																					listProductIdsOrder.Contains(x.ProductId)
																				);
								if (!couponForSpecificProductOfShopExisted)
								{
									return (Constants.RESPONSE_CODE_ORDER_COUPON_INVALID_PRODUCT_APPLY, "Invalid product apply!", numberQuantityAvailable, orderResult);
								}
							}

							if (coupon.MinTotalOrderValue > totalAmount)
							{
								transaction.Rollback();
								return (Constants.RESPONSE_CODE_ORDER_NOT_ELIGIBLE, "Orders are not eligible to apply the coupons!", numberQuantityAvailable, orderResult);
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
						#endregion

						// caculate total payment
						totalPayment = totalAmount - totalCouponDiscount;
						if (totalPayment < 0)
						{
							totalPayment = 0;
						}

						// caculate total coin discount
						if (isUseCoin && customerCoin > 0 && totalPayment > 0)
						{
							if (totalPayment <= customerCoin)
							{
								totalCoinDiscount = totalPayment;
								totalPayment = 0;
								customerCoin -= totalCoinDiscount;
							}
							else
							{
								totalCoinDiscount = customerCoin;
								totalPayment = totalPayment - customerCoin;
								customerCoin = 0;
							}
						}
						#endregion

						#region Update data in database
						//update customer and admin account balance
						if (totalPayment >= 0)
						{
							//Check customer is a seller of another shop and total payment > seller's account balance require
							var shopOfBuyer = context.Shop.FirstOrDefault(x => x.UserId == userId);
							if (shopOfBuyer != null)
							{
								if (customer.AccountBalance - totalPayment < Constants.ACCOUNT_BALANCE_REQUIRED_FOR_SELLER)
								{
									transaction.Rollback();
									return (Constants.RESPONSE_CODE_ORDER_SELLER_LOCK_TRANSACTION, "Seller lock transaction!", numberQuantityAvailable, orderResult);
								}
							}

							if (customer.AccountBalance < totalPayment)
							{
								transaction.Rollback();
								return (Constants.RESPONSE_CODE_ORDER_INSUFFICIENT_BALANCE, "Insufficient balance!", numberQuantityAvailable, orderResult);
							}
							customer.AccountBalance = customer.AccountBalance - totalPayment;
							if (totalCoinDiscount > 0)
							{
								customer.Coin = customer.Coin - totalCoinDiscount;
							}

							//update admin account balance
							var admin = context.User.First(x => x.UserId == Constants.ADMIN_USER_ID);
							admin.AccountBalance += totalPayment == 0 ? totalCoinDiscount : totalPayment;

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
						if (totalPayment > 0)
						{
							TransactionInternal newTransaction = new TransactionInternal
							{
								UserId = order.UserId,
								TransactionInternalTypeId = Constants.TRANSACTION_INTERNAL_TYPE_PAYMENT,
								OrderId = order.OrderId,
								PaymentAmount = order.TotalPayment,
								Note = "Payment",
								DateCreate = DateTime.Now
							};
							context.TransactionInternal.Add(newTransaction);
							context.SaveChanges();
						}

						// add history order status
						HistoryOrderStatus historyOrderStatus = new HistoryOrderStatus
						{
							OrderId = order.OrderId,
							OrderStatusId = Constants.ORDER_STATUS_WAIT_CONFIRMATION,
							DateCreate = order.OrderDate,
						};
						context.HistoryOrderStatus.Add(historyOrderStatus);
						context.SaveChanges();
						#endregion

						orderResult = order;
					}
					#endregion

					transaction.Commit();
					return (Constants.RESPONSE_CODE_SUCCESS, "Success!", numberQuantityAvailable, orderResult);
				}
				catch (Exception ex)
				{
					transaction.Rollback();
					throw new Exception(ex.Message);
				}
			}

		}
		#endregion

		#region Get order detail for admin
		internal Order? GetOrderInfoAdmin(long orderId)
		{
			using (DatabaseContext context = new DatabaseContext())
			{
				Order? orderInfo = (from order in context.Order
									join user in context.User
										on order.UserId equals user.UserId
									join shop in context.Shop
										on order.ShopId equals shop.UserId
									join businessFee in context.BusinessFee
										on order.BusinessFeeId equals businessFee.BusinessFeeId
									where order.OrderId == orderId
									select new Order
									{
										OrderId = orderId,
										User = new User
										{
											UserId = user.UserId,
											Email = user.Email,
											Avatar = user.Avatar,
										},
										Shop = new Shop
										{
											UserId = shop.UserId,
											ShopName = shop.ShopName,
											Avatar = shop.Avatar
										},
										BusinessFee = new BusinessFee
										{
											BusinessFeeId = businessFee.BusinessFeeId,
											Fee = businessFee.Fee
										},
										ConversationId = order.ConversationId,
										OrderStatusId = order.OrderStatusId,
										OrderDate = order.OrderDate,
										Note = order.Note,
										TotalAmount = order.TotalAmount,
										TotalCouponDiscount = order.TotalCouponDiscount,
										TotalCoinDiscount = order.TotalCoinDiscount,
										TotalPayment = order.TotalPayment,
										OrderDetails = (from orderDetail in context.OrderDetail
														join productVariant in context.ProductVariant
															 on orderDetail.ProductVariantId equals productVariant.ProductVariantId
														join product in context.Product
															 on productVariant.ProductId equals product.ProductId
														where orderDetail.OrderId == orderId
														select new OrderDetail
														{
															OrderDetailId = orderDetail.OrderDetailId,
															ProductVariant = new ProductVariant
															{
																ProductVariantId = productVariant.ProductVariantId,
																Name = productVariant.Name,
																Product = new Product
																{
																	ProductId = product.ProductId,
																	ProductName = product.ProductName,
																	Thumbnail = product.Thumbnail
																}
															},
															Quantity = orderDetail.Quantity,
															Price = orderDetail.Price,
															Discount = orderDetail.Discount,
															TotalAmount = orderDetail.TotalAmount,
															IsFeedback = orderDetail.IsFeedback,
															Feedback = (orderDetail.IsFeedback ?
																		 (from feedBack in context.Feedback
																		  join feedbackBenefit in context.FeedbackBenefit
																			 on feedBack.FeedbackBenefitId equals feedbackBenefit.FeedbackBenefitId
																		  where feedBack.OrderDetailId == orderDetail.OrderDetailId
																		  select new Feedback
																		  {
																			  Rate = feedBack.Rate,
																			  FeedbackBenefit = new FeedbackBenefit
																			  {
																				  Coin = feedbackBenefit.Coin
																			  }
																		  }).FirstOrDefault()
																		 :
																		 null),
															AssetInformations = (from assetInfomation in context.AssetInformation
																				 where assetInfomation.OrderDetailId == orderDetail.OrderDetailId
																				 select new AssetInformation
																				 {
																					 Asset = assetInfomation.Asset
																				 }).ToList()

														}).ToList(),
										TransactionInternals = (from transactionInternal in context.TransactionInternal
																where transactionInternal.OrderId == orderId
																select new TransactionInternal
																{
																	TransactionInternalTypeId = transactionInternal.TransactionInternalTypeId,
																	PaymentAmount = transactionInternal.PaymentAmount,
																	DateCreate = transactionInternal.DateCreate
																}).ToList(),
										TransactionCoins = (from transactionCoin in context.TransactionCoin
															where transactionCoin.OrderId == orderId
															select new TransactionCoin
															{
																TransactionCoinTypeId = transactionCoin.TransactionCoinTypeId,
																Amount = transactionCoin.Amount,
																DateCreate = transactionCoin.DateCreate
															}).ToList(),
										HistoryOrderStatus = (from historyOrderStatus in context.HistoryOrderStatus
															  where historyOrderStatus.OrderId == orderId
															  select new HistoryOrderStatus
															  {
																  OrderStatusId = historyOrderStatus.OrderStatusId,
																  DateCreate = historyOrderStatus.DateCreate,
																  Note = historyOrderStatus.Note
															  }).ToList()
									}
									).FirstOrDefault();
				return orderInfo;
			}
		}
		#endregion

		#region Get order detail seller
		internal Order? GetOrderDetailSeller(long userId, long orderId)
		{
			using (DatabaseContext context = new DatabaseContext())
			{
				return context.Order
					.Include(x => x.HistoryOrderStatus)
					.Include(x => x.OrderCoupons)
					.ThenInclude(x => x.Coupon)
					.Include(x => x.Shop)
					.Include(x => x.User)
					.Include(x => x.OrderDetails)
					.ThenInclude(x => x.ProductVariant)
					.ThenInclude(x => x.Product)
					.Include(x => x.BusinessFee)
					.Include(x => x.OrderDetails)
					.ThenInclude(x => x.Feedback)
					.Include(x => x.OrderDetails)
					.ThenInclude(x => x.AssetInformations)
					.FirstOrDefault(x => x.OrderId == orderId && x.ShopId == userId);
			}
		}
		#endregion

		#region Update order status (Seller violate)
		internal void UpdateOrderStatusSellerViolates(long orderId, string note)
		{
			using (DatabaseContext context = new DatabaseContext())
			{
				var transaction = context.Database.BeginTransaction();
				try
				{
					var order = context.Order.First(x => x.OrderId == orderId);
					order.OrderStatusId = Constants.ORDER_STATUS_SELLER_VIOLATES;
					order.Note = note;
					context.SaveChanges();

					var customerId = order.UserId;

					// update customer account balance
					var customer = context.User.First(x => x.UserId == customerId);

					if (order.TotalPayment > 0)
					{
						// update customer's account balance
						customer.AccountBalance = customer.AccountBalance + order.TotalPayment;

						// add transaction for refund money to customer
						TransactionInternal transactionInternal = new TransactionInternal()
						{
							UserId = customerId,
							OrderId = orderId,
							TransactionInternalTypeId = Constants.TRANSACTION_INTERNAL_TYPE_RECEIVE_REFUND,
							PaymentAmount = order.TotalPayment,
							DateCreate = DateTime.Now,
						};
						context.TransactionInternal.Add(transactionInternal);
						context.SaveChanges();
					}


					if (order.TotalCoinDiscount > 0)
					{
						// update customer's coin
						customer.Coin = customer.Coin + order.TotalCoinDiscount;

						// add transaction coin for refund coin to customer
						TransactionCoin transactionCoin = new TransactionCoin
						{
							UserId = customer.UserId,
							TransactionCoinTypeId = Constants.TRANSACTION_COIN_TYPE_REFUND,
							OrderId = orderId,
							FeedbackId = null,
							Note = "",
							Amount = order.TotalCoinDiscount,
							DateCreate = DateTime.Now
						};
						context.TransactionCoin.Add(transactionCoin);
						context.SaveChanges();
					}

					// add history order status
					HistoryOrderStatus historyOrderStatus = new HistoryOrderStatus
					{
						OrderId = order.OrderId,
						OrderStatusId = Constants.ORDER_STATUS_SELLER_VIOLATES,
						DateCreate = DateTime.Now,
						Note = note
					};
					context.HistoryOrderStatus.Add(historyOrderStatus);
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
		#endregion

		#region Update order status (RejectComplaint)
		internal void UpdateOrderStatusRejectComplaint(long orderId, string note)
		{
			using (DatabaseContext context = new DatabaseContext())
			{
				var transaction = context.Database.BeginTransaction();
				try
				{
					var order = context.Order
									.Include(x => x.BusinessFee)
									.First(x => x.OrderId == orderId);
					order.OrderStatusId = Constants.ORDER_STATUS_REJECT_COMPLAINT;
					order.Note = note;
					context.Order.Update(order);

					var sellerId = order.ShopId;
					long fee = context.BusinessFee.First(x => x.BusinessFeeId == order.BusinessFeeId).Fee;
					var businessFee = order.TotalAmount * fee / 100;
					var adminProfit = order.TotalCoinDiscount > businessFee ? 0 : businessFee - order.TotalCoinDiscount;
					var sellerProfit = order.TotalAmount - order.TotalCouponDiscount - businessFee;

					// update sold count number of product
					var orderDetails = context.OrderDetail.Where(x => x.OrderId == order.OrderId).ToList();
					foreach (var orderDetail in orderDetails)
					{
						var productVariant = context.ProductVariant.First(x => x.ProductVariantId == orderDetail.ProductVariantId);
						var product = context.Product.First(x => x.ProductId == productVariant.ProductId);
						product.SoldCount += orderDetail.Quantity;
						context.Product.Update(product);
					}

					if (sellerProfit > 0)
					{
						// update seller account balance
						var seller = context.User.First(x => x.UserId == sellerId);
						seller.AccountBalance = seller.AccountBalance + sellerProfit;

						// add transaction for refund money to seller
						TransactionInternal transactionInternalSeller = new TransactionInternal()
						{
							UserId = sellerId,
							OrderId = orderId,
							TransactionInternalTypeId = Constants.TRANSACTION_INTERNAL_TYPE_RECEIVE_PAYMENT,
							PaymentAmount = sellerProfit,
							DateCreate = DateTime.Now,
						};
						context.TransactionInternal.Add(transactionInternalSeller);
					}

					if (adminProfit > 0)
					{
						//update admin profit account balance
						var admin = context.User.First(x => x.UserId == Constants.ADMIN_USER_ID);
						admin.AccountBalance = admin.AccountBalance + adminProfit;

						// add transaction for get profit admin
						TransactionInternal transactionInternalAdmin = new TransactionInternal()
						{
							UserId = Constants.ADMIN_USER_ID,
							OrderId = orderId,
							TransactionInternalTypeId = Constants.TRANSACTION_INTERNAL_TYPE_RECEIVE_PROFIT,
							PaymentAmount = adminProfit,
							DateCreate = DateTime.Now,
						};
						context.TransactionInternal.Add(transactionInternalAdmin);
					}
					// add history order status
					HistoryOrderStatus historyOrderStatus = new HistoryOrderStatus
					{
						OrderId = order.OrderId,
						OrderStatusId = Constants.ORDER_STATUS_REJECT_COMPLAINT,
						DateCreate = DateTime.Now,
						Note = note
					};
					context.HistoryOrderStatus.Add(historyOrderStatus);
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
		#endregion


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

		#region Check order exised
		internal Order? GetOrderForCheckingExisted(long orderId)
		{
			using (DatabaseContext context = new DatabaseContext())
			{
				var order = context.Order
					.FirstOrDefault(x => x.OrderId == orderId);
				return order;
			}
		}
		#endregion

		#region Get order detail for user
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
		#endregion

		#region Update order status for customer
		internal void UpdateOrderStatusCustomer(long orderId, long shopId, int status, string note)
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

						HistoryOrderStatus historyOrderStatus = new HistoryOrderStatus
						{
							OrderId = order.OrderId,
							DateCreate = DateTime.Now,
							Note = note
						};
						if (status == Constants.ORDER_STATUS_CONFIRMED)
						{
							long fee = context.BusinessFee.First(x => x.BusinessFeeId == order.BusinessFeeId).Fee;
							var businessFee = order.TotalAmount * fee / 100;
							var adminProfit = order.TotalCoinDiscount > businessFee ? 0 : businessFee - order.TotalCoinDiscount;
							var sellerProfit = order.TotalAmount - order.TotalCouponDiscount - businessFee;

							// update sold count number of product
							var orderDetails = context.OrderDetail.Where(x => x.OrderId == order.OrderId).ToList();
							foreach (var orderDetail in orderDetails)
							{
								var productVariant = context.ProductVariant.First(x => x.ProductVariantId == orderDetail.ProductVariantId);
								var product = context.Product.First(x => x.ProductId == productVariant.ProductId);
								product.SoldCount += orderDetail.Quantity;
								context.Product.Update(product);
							}

							// add transaction receive payment and profit
							List<TransactionInternal> transactionInternals = new List<TransactionInternal>();

							// update blance of seller
							if (sellerProfit > 0)
							{
								var seller = context.User.First(x => x.UserId == shopId);
								seller.AccountBalance = seller.AccountBalance + sellerProfit;
								TransactionInternal transactionInternal = new TransactionInternal
								{
									DateCreate = DateTime.Now,
									Note = "Receive Payment",
									OrderId = order.OrderId,
									PaymentAmount = sellerProfit,
									TransactionInternalTypeId = Constants.TRANSACTION_INTERNAL_TYPE_RECEIVE_PAYMENT,
									UserId = shopId,
								};
								transactionInternals.Add(transactionInternal);
							}

							if (adminProfit > 0)
							{
								User admin = context.User.First(x => x.UserId == Constants.ADMIN_USER_ID);
								admin.AccountBalance += adminProfit;
								TransactionInternal transactionInternal = new TransactionInternal
								{
									DateCreate = DateTime.Now,
									Note = "Profit",
									OrderId = order.OrderId,
									PaymentAmount = adminProfit,
									TransactionInternalTypeId = Constants.TRANSACTION_INTERNAL_TYPE_RECEIVE_PROFIT,
									UserId = Constants.ADMIN_USER_ID,
								};
								transactionInternals.Add(transactionInternal);
							}

							if (transactionInternals.Count > 0)
							{
								context.TransactionInternal.AddRange(transactionInternals);
							}
							historyOrderStatus.OrderStatusId = Constants.ORDER_STATUS_CONFIRMED;
						}
						else if (status == Constants.ORDER_STATUS_COMPLAINT)
						{
							historyOrderStatus.OrderStatusId = Constants.ORDER_STATUS_COMPLAINT;
						}
						context.HistoryOrderStatus.Add(historyOrderStatus);
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
		#endregion

		internal Order? GetOrderCustomer(long orderId, long customerId, long shopId)
		{
			using (DatabaseContext context = new DatabaseContext())
			{
				return context.Order
					.Include(x => x.User)
					.Include(x => x.OrderCoupons)
					.Include(x => x.Shop)
					.ThenInclude(x => x.User)
					.Include(x => x.OrderDetails)
					.ThenInclude(x => x.Feedback)
					.Include(x => x.OrderDetails)
					.ThenInclude(x => x.AssetInformations)
					.ThenInclude(x => x.ProductVariant)
					.ThenInclude(x => x.Product)
					.FirstOrDefault(x => x.OrderId == orderId && x.UserId == customerId && x.ShopId == shopId);
					//&& x.OrderStatusId != Constants.ORDER_STATUS_CONFIRMED);
			}
		}

		internal Order? GetOrderCustomer(long orderId, long customerId)
		{
			using (DatabaseContext context = new DatabaseContext())
			{
				return context.Order
					.Include(x => x.OrderCoupons)
					.Include(x => x.HistoryOrderStatus)
					.Include(x => x.Shop)
					.Include(x => x.OrderDetails)
					.ThenInclude(x => x.Feedback)
					.Include(x => x.OrderDetails)
					.ThenInclude(x => x.AssetInformations)
					.ThenInclude(x => x.ProductVariant)
					.ThenInclude(x => x.Product)
					.FirstOrDefault(x => x.OrderId == orderId && x.UserId == customerId);
			}
		}

		internal (long, List<Order>) GetListOrderSeller(long userId, string orderId, string username, DateTime? fromDate,
			DateTime? toDate, int status, int page)
		{
			using (DatabaseContext context = new DatabaseContext())
			{
				var query = context.Order
					.Include(x => x.User)
					.Include(x => x.BusinessFee)
					.Include(x => x.OrderCoupons)
					.ThenInclude(x => x.Coupon)
					.Include(x => x.OrderDetails)
					.ThenInclude(x => x.ProductVariant)
					.ThenInclude(x => x.Product)
					.Where(x => x.ShopId == userId && x.User.Username.ToLower().Contains(username.ToLower())
							&& (fromDate != null && toDate != null ? x.OrderDate.Date >= fromDate.Value.Date
							&& x.OrderDate.Date <= toDate.Value.Date : true)
							&& (status == 0 ? true : x.OrderStatusId == status)
							&& (string.IsNullOrWhiteSpace(orderId) ? true : x.OrderId.ToString() == orderId.Trim()))
					.OrderByDescending(x => x.OrderDate);
				return (query.Count(), query.Skip((page - 1) * Constants.PAGE_SIZE).Take(Constants.PAGE_SIZE).ToList());


			}
		}

		internal void UpdateStatusOrderDispute(long sellerId, long customerId, long orderId, string note)
		{
			using (DatabaseContext context = new DatabaseContext())
			{
				using (var transaction = context.Database.BeginTransaction())
				{
					try
					{
						Order? order = context.Order.FirstOrDefault(x => x.ShopId == sellerId && x.UserId == customerId
								&& x.OrderId == orderId);
						if (order == null) throw new Exception("Not found");
						if (order.OrderStatusId != Constants.ORDER_STATUS_COMPLAINT) throw new Exception("Invalid order");
						Conversation conversation = new Conversation
						{
							ConversationName = $"Nhóm chat: Tranh chấp đơn hàng mã #{orderId}",
							DateCreate = DateTime.Now,
							IsActivate = true,
							IsGroup = true,
							Messages = new List<Message>
							{
								new Message
								{
									UserId = Constants.ADMIN_USER_ID,
									Content = $"Tranh chấp đơn hàng mã #{orderId}.",
									MessageType = "0",
									DateCreate = DateTime.Now,
									IsDelete = false,
								}
							},
							UserConversations = new List<UserConversation> {
								new UserConversation
								{
									UserId = sellerId,
									IsRead = Constants.USER_CONVERSATION_TYPE_IS_READ,
								},
								new UserConversation
								{
									UserId = customerId,
									IsRead = Constants.USER_CONVERSATION_TYPE_UN_READ,
								},
								new UserConversation
								{
									UserId = Constants.ADMIN_USER_ID,
									IsRead = Constants.USER_CONVERSATION_TYPE_UN_READ,
								}
							}
						};
						context.Conversations.Add(conversation);
						context.SaveChanges();

						order.OrderStatusId = Constants.ORDER_STATUS_DISPUTE;
						order.ConversationId = conversation.ConversationId;
						context.Order.Update(order);

						// add history order status
						HistoryOrderStatus historyOrderStatus = new HistoryOrderStatus
						{
							OrderId = order.OrderId,
							OrderStatusId = Constants.ORDER_STATUS_DISPUTE,
							DateCreate = DateTime.Now,
							Note = note
						};
						context.HistoryOrderStatus.Add(historyOrderStatus);

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

		internal void UpdateStatusOrderRefund(long sellerId, long orderId, string note)
		{
			using (DatabaseContext context = new DatabaseContext())
			{
				using (var transaction = context.Database.BeginTransaction())
				{
					try
					{
						Order? order = context.Order.FirstOrDefault(x => x.ShopId == sellerId && x.OrderId == orderId);
						if (order == null) throw new Exception("Not found");

						order.OrderStatusId = Constants.ORDER_STATUS_SELLER_REFUNDED;
						order.Note = note;

						User customer = context.User.First(x => x.UserId == order.UserId);
						if (order.TotalPayment > 0)
						{
							// update customer's account balance and coin
							customer.AccountBalance = customer.AccountBalance + order.TotalPayment;
							// add transaction for refund money to customer
							TransactionInternal transactionInternal = new TransactionInternal()
							{
								UserId = customer.UserId,
								OrderId = orderId,
								TransactionInternalTypeId = Constants.TRANSACTION_INTERNAL_TYPE_RECEIVE_REFUND,
								PaymentAmount = order.TotalPayment,
								DateCreate = DateTime.Now,
							};
							context.TransactionInternal.Add(transactionInternal);
							context.SaveChanges();
						}


						if (order.TotalCoinDiscount > 0)
						{
							// update customer's coin
							customer.Coin = customer.Coin + order.TotalCoinDiscount;

							// add transaction coin for refund coin to customer
							TransactionCoin transactionCoin = new TransactionCoin
							{
								UserId = customer.UserId,
								TransactionCoinTypeId = Constants.TRANSACTION_COIN_TYPE_REFUND,
								OrderId = orderId,
								FeedbackId = null,
								Note = "",
								Amount = order.TotalCoinDiscount,
								DateCreate = DateTime.Now
							};
							context.TransactionCoin.Add(transactionCoin);
							context.SaveChanges();
						}

						// add history order status
						HistoryOrderStatus historyOrderStatus = new HistoryOrderStatus
						{
							OrderId = order.OrderId,
							OrderStatusId = Constants.ORDER_STATUS_SELLER_REFUNDED,
							DateCreate = DateTime.Now,
							Note = note
						};
						context.HistoryOrderStatus.Add(historyOrderStatus);

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

		internal Order? GetOrder(long orderId)
		{
			using (DatabaseContext context = new DatabaseContext())
			{
				return context.Order
							.Include(x => x.User)
							.Include(x => x.Shop)
							.ThenInclude(x => x.User)
							.FirstOrDefault(x => x.OrderId == orderId);
			}
		}

		internal OrderDetail? GetOrderDetail(long orderDetailId)
		{
			using (DatabaseContext context = new DatabaseContext())
			{
				return context.OrderDetail.Include(x => x.Order).FirstOrDefault(x => x.OrderDetailId == orderDetailId);
			}
		}

		internal (long totalItem, List<Order> orders) GetListOrderByCoupon(long userId, long couponId, int page)
		{
			using (DatabaseContext context = new DatabaseContext())
			{
				var query = context.Order.Include(x => x.User).
					Where(x => x.ShopId == userId && x.OrderCoupons.Any(x => x.CouponId == couponId));

				return (query.Count(), query.Skip((page - 1) * Constants.PAGE_SIZE).Take(Constants.PAGE_SIZE).ToList());
			}
		}

		internal List<Order> GetListOrderSeller(long userId, string orderId, string username, DateTime? fromDate, DateTime? toDate, int status)
		{
			using (DatabaseContext context = new DatabaseContext())
			{
				return context.Order
					.Include(x => x.User)
					.Include(x => x.BusinessFee)
					.Include(x => x.OrderDetails)
					.ThenInclude(x => x.ProductVariant)
					.ThenInclude(x => x.Product)
					.Where(x => x.ShopId == userId && x.User.Username.ToLower().Contains(username.ToLower())
							&& (fromDate != null && toDate != null ? x.OrderDate.Date >= fromDate.Value.Date
							&& x.OrderDate.Date <= toDate.Value.Date : true)
							&& (status == 0 ? true : x.OrderStatusId == status)
							&& (string.IsNullOrWhiteSpace(orderId) ? true : x.OrderId.ToString() == orderId.Trim()))
					.OrderByDescending(x => x.OrderDate)
					.ToList();
			}
		}

		internal List<Order> GetListOrderOfShop(long userId, int month, int year, int statusOrder)
		{
			using (DatabaseContext context = new DatabaseContext())
			{
				return context.Order
					.Include(x => x.BusinessFee)
					.Include(x => x.OrderDetails)
					.ThenInclude(x => x.ProductVariant)
					.ThenInclude(x => x.Product)
					.Where(x => x.ShopId == userId && (statusOrder == Constants.ORDER_ALL ? true : x.OrderStatusId == statusOrder)
						&& (month == 0 ? true : x.OrderDate.Month == month)
						&& year == x.OrderDate.Year)
					.OrderBy(x => x.OrderDate)
					.ToList();
			}
		}

		internal List<Order> GetListOrderOfCurrentMonth(long userId)
		{
			using (DatabaseContext context = new DatabaseContext())
			{
				DateTime now = DateTime.Now;
				return context.Order
					.Include(x => x.BusinessFee)
					.Include(x => x.OrderDetails)
					.ThenInclude(x => x.ProductVariant)
					.ThenInclude(x => x.Product)
					.Where(x => x.ShopId == userId && x.OrderDate.Month == now.Month && x.OrderDate.Year == now.Year)
					.ToList();
			}
		}

		internal List<Order> GetListOrderByStatus(long userId)
		{
			using (DatabaseContext context = new DatabaseContext())
			{
				return context.Order.Where(x => x.ShopId == userId && (x.OrderStatusId == Constants.ORDER_STATUS_WAIT_CONFIRMATION
							|| x.OrderStatusId == Constants.ORDER_STATUS_DISPUTE
							|| x.OrderStatusId == Constants.ORDER_STATUS_COMPLAINT))
						.ToList();
			}
		}

		internal List<Order> GetOrdersForReport(long orderId, string customerEmail, long shopId, string shopName, DateTime? fromDate, DateTime? toDate, int status)
		{
			using (DatabaseContext context = new DatabaseContext())
			{
				var orders = context.Order
							.Include(x => x.User)
							.Include(x => x.Shop)
							.Include(x => x.BusinessFee)
							.Select(o => new Order
							{
								OrderId = o.OrderId,
								OrderDate = o.OrderDate,
								TotalPayment = o.TotalPayment,
								TotalAmount = o.TotalAmount,
								TotalCouponDiscount = o.TotalCouponDiscount,
								TotalCoinDiscount = o.TotalCoinDiscount,
								BusinessFee = new BusinessFee
								{
									Fee = o.BusinessFee.Fee
								},
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
								Note = o.Note,
								OrderStatusId = o.OrderStatusId
							})
							.Where(x =>
								((fromDate != null && toDate != null) ? fromDate <= x.OrderDate && toDate >= x.OrderDate : true) &&
								x.User.Email.Contains(customerEmail) &&
								((shopId == 0) ? true : x.Shop.UserId == shopId) &&
								x.Shop.ShopName.Contains(shopName) &&
								(orderId == 0 ? true : x.OrderId == orderId) &&
								(status == 0 ? true : x.OrderStatusId == status)
							)
							.OrderByDescending(x => x.OrderDate)
							.ToList();
				return orders;
			}
		}

		internal int GetTotalNumberOrderSellerViolates(long shopId)
		{
			using (DatabaseContext context = new DatabaseContext())
			{
				return context.Order
						.Where(x =>
							x.OrderStatusId == Constants.ORDER_STATUS_SELLER_VIOLATES &&
							x.ShopId == shopId
						).Count();
			}
		}

		internal List<Order> GetListOrderAllShop(int month, int year, int statusOrder)
		{
			using (DatabaseContext context = new DatabaseContext())
			{
				return context.Order
					.Include(x => x.BusinessFee)
					.Include(x => x.OrderDetails)
					.ThenInclude(x => x.ProductVariant)
					.ThenInclude(x => x.Product)
					.Where(x => (statusOrder == Constants.ORDER_ALL ? true : x.OrderStatusId == statusOrder)
						&& (month == 0 ? true : x.OrderDate.Month == month)
						&& year == x.OrderDate.Year)
					.OrderBy(x => x.OrderDate)
					.ToList();
			}
		}

		internal long GetNumberOrdersDispute()
		{
			using (DatabaseContext context = new DatabaseContext())
			{
				return context.Order.LongCount(x => x.OrderStatusId == Constants.ORDER_STATUS_DISPUTE);
			}
		}

		internal List<Order> GetListOrderOfCurrentMonthAllShop()
		{
			using (DatabaseContext context = new DatabaseContext())
			{
				DateTime now = DateTime.Now;
				return context.Order
					.Include(x => x.BusinessFee)
					.Include(x => x.OrderDetails)
					.ThenInclude(x => x.ProductVariant)
					.ThenInclude(x => x.Product)
					.Where(x => x.OrderDate.Month == now.Month && x.OrderDate.Year == now.Year)
					.ToList();
			}
		}
	}
}

