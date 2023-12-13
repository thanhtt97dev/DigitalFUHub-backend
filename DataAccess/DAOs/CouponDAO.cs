using BusinessObject;
using BusinessObject.Entities;
using Comons;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.DAOs
{
	public class CouponDAO
	{

		private static CouponDAO? instance;
		private static readonly object instanceLock = new object();

		public static CouponDAO Instance
		{
			get
			{
				lock (instanceLock)
				{
					if (instance == null)
					{
						instance = new CouponDAO();
					}
				}
				return instance;
			}
		}

		internal List<Coupon> GetCouponPublic(long shopId)
		{
			using (DatabaseContext context = new DatabaseContext())
			{
                DateTime now = DateTime.Now;
                var coupons = (from coupon in context.Coupon
                              where
                              coupon.ShopId == shopId
                              &&
                              coupon.IsActive == true
                              &&
                              coupon.IsPublic == true
                              &&
                              coupon.StartDate <= now && now <= coupon.EndDate
                              select new Coupon
                              {
                                  CouponId = coupon.CouponId,
                                  CouponTypeId = coupon.CouponTypeId,
                                  CouponCode = coupon.CouponCode,
                                  CouponName = coupon.CouponName,
                                  PriceDiscount = coupon.PriceDiscount,
                                  Quantity = coupon.Quantity,
                                  StartDate = coupon.StartDate,
                                  EndDate = coupon.EndDate,
                                  MinTotalOrderValue = coupon.MinTotalOrderValue,
                                  CouponProducts = context.CouponProduct.Where(x => x.CouponId == coupon.CouponId)
                                  .Select(x => new CouponProduct { ProductId = x.ProductId }).ToList()
                              }).ToList();

				return coupons;
			}
		}

		internal Coupon? GetCouponPrivate(string couponCode, long shopId)
		{
			using (DatabaseContext context = new DatabaseContext())
			{
                DateTime now = DateTime.Now;
				var coupon = (from coupons in context.Coupon
							  where coupons.CouponCode.Trim().ToLower().Equals(couponCode.Trim().ToLower())
							  &&
							  coupons.ShopId == shopId
							  &&
							  coupons.IsActive == true
							  &&
							  coupons.IsPublic == false
							  &&
							  coupons.StartDate <= now && now <= coupons.EndDate
							  select new Coupon
							  {
                                  CouponId = coupons.CouponId,
                                  CouponTypeId = coupons.CouponTypeId,
                                  CouponCode = coupons.CouponCode,
                                  CouponName = coupons.CouponName,
                                  PriceDiscount = coupons.PriceDiscount,
                                  Quantity = coupons.Quantity,
                                  StartDate = coupons.StartDate,
                                  EndDate = coupons.EndDate,
                                  MinTotalOrderValue = coupons.MinTotalOrderValue,
								  CouponProducts = context.CouponProduct.Where(x => x.CouponId == coupons.CouponId)
								  .Select(x => new CouponProduct { ProductId = x.ProductId }).ToList()
                              }).FirstOrDefault();

				return coupon;
			}
		}

        internal Coupon? GetCouponDetailCustomer(long couponId)
        {
            using (DatabaseContext context = new DatabaseContext())
            {
                DateTime now = DateTime.Now;
                var coupon = (from coupons in context.Coupon
                              where coupons.CouponId == couponId
                              select new Coupon
                              {
                                  CouponId = coupons.CouponId,
                                  CouponTypeId = coupons.CouponTypeId,
                                  CouponCode = coupons.CouponCode,
                                  CouponName = coupons.CouponName,
                                  PriceDiscount = coupons.PriceDiscount,
                                  StartDate = coupons.StartDate,
                                  EndDate = coupons.EndDate,
                                  MinTotalOrderValue = coupons.MinTotalOrderValue,
                              }).FirstOrDefault();

                return coupon;
            }
        }

        internal (long, List<Coupon>) GetListCouponsByShop(long userId, string couponCode, DateTime? startDate, DateTime? endDate,
			bool? isPublic, long status , int page)
		{
			using (DatabaseContext context = new DatabaseContext())
			{
#pragma warning disable CS8620 // Argument cannot be used for parameter due to differences in the nullability of reference types.
				var query = context.Coupon
					.Include(x => x.CouponProducts)
					.ThenInclude(x => x.Product)
					.Where(c => c.ShopId == userId
							&& c.IsActive == true
							&& (status == Constants.COUPON_STATUS_ALL ?
								true
								:
								(
									status == Constants.COUPON_STATUS_COMING_SOON ? c.StartDate > DateTime.Now
									:
									status == Constants.COUPON_STATUS_ONGOING ? c.StartDate <= DateTime.Now && DateTime.Now <= c.EndDate
									:
									status == Constants.COUPON_STATUS_FINISHED ? c.EndDate < DateTime.Now : true
								)
							)
							&& c.CouponCode.ToLower().Contains(couponCode.ToLower())
							&& (startDate == null ? true : c.StartDate.Date >= startDate.Value.Date)
							&& (endDate == null ? true : c.EndDate.Date <= endDate.Value.Date)
							&& (isPublic == null ? true : c.IsPublic == isPublic)
							)
					.OrderByDescending(x => x.CouponId);
				List<Coupon> coupons = query.Skip((page - 1) * Constants.PAGE_SIZE).Take(Constants.PAGE_SIZE).ToList();
#pragma warning restore CS8620 // Argument cannot be used for parameter due to differences in the nullability of reference types.

				return (query.Count(), coupons);
			}
		}

		internal void AddCoupon(Coupon coupon)
		{
			using (DatabaseContext context = new DatabaseContext())
			{
				try
				{
					Shop? shop = context.Shop.FirstOrDefault(x => x.UserId == coupon.ShopId);
					Coupon? coup = context.Coupon.FirstOrDefault(x => x.CouponCode.ToLower() == coupon.CouponCode.ToLower().Trim());

					if (shop == null || coup != null || coupon.StartDate >= coupon.EndDate)
					{
						throw new Exception("Invalid coupon");
					}
					if (coupon.CouponTypeId == Constants.COUPON_TYPE_SPECIFIC_PRODUCTS)
					{
#pragma warning disable CS8604 // Possible null reference argument.
						bool isExist = coupon.CouponProducts.Any(cp => !context.Product.Any(x => x.ShopId == coupon.ShopId
											&& x.ProductStatusId == Constants.PRODUCT_STATUS_ACTIVE && cp.ProductId == x.ProductId));
#pragma warning restore CS8604 // Possible null reference argument.
						if (isExist)
						{
							throw new Exception("Invalid coupon");
						}
					}
					context.Coupon.Add(coupon);
					context.SaveChanges();
				}
				catch (Exception e)
				{
					throw new Exception(e.Message);
				}
			}
		}

		internal Coupon? GetCoupon(long couponId, long shopId)
		{
			using (DatabaseContext context = new DatabaseContext())
			{
				return context.Coupon.Include(x => x.CouponProducts).ThenInclude(x => x.Product)
					.FirstOrDefault(x => x.CouponId == couponId && x.ShopId == shopId && x.IsActive == true);
			}
		}

		internal void UpdateStatusCoupon(Coupon coupon)
		{
			using (DatabaseContext context = new DatabaseContext())
			{
				try
				{
					context.Coupon.Update(coupon);
					context.SaveChanges();
				}
				catch (Exception e)
				{
					throw new Exception(e.Message);
				}
			}
		}

		internal bool IsExistCouponCode(long shopId, string couponCode, string action)
		{
			using (DatabaseContext context = new DatabaseContext())
			{
				return context.Coupon.Any(x => x.CouponCode.ToLower() == couponCode.ToLower()
				&& (action.ToLower() == "u" ? x.ShopId != shopId : true));
			}
		}

		internal void UpdateCoupon(Coupon coupon)
		{
			using (DatabaseContext context = new DatabaseContext())
			{
				try
				{
					Coupon? coup = context.Coupon
						.FirstOrDefault(x => x.CouponId == coupon.CouponId && x.ShopId == coupon.ShopId
						&& x.IsActive == true);
					if (coup == null || coup.StartDate <= DateTime.Now||coupon.StartDate >= coupon.EndDate) throw new Exception("INVALID");
					coup.StartDate = coupon.StartDate;
					coup.EndDate = coupon.EndDate;
					coup.CouponCode = coupon.CouponCode;
					coup.CouponName = coupon.CouponName;
					coup.IsPublic = coupon.IsPublic;
					coup.PriceDiscount = coupon.PriceDiscount;
					coup.MinTotalOrderValue = coupon.MinTotalOrderValue;
					coup.Quantity = coupon.Quantity;

					if (coupon.CouponTypeId == Constants.COUPON_TYPE_SPECIFIC_PRODUCTS)
					{
						context.CouponProduct.RemoveRange(context.CouponProduct.Where(x => x.CouponId == coup.CouponId));
#pragma warning disable CS8604 // Possible null reference argument.
						context.CouponProduct.AddRange(coupon.CouponProducts);
#pragma warning restore CS8604 // Possible null reference argument.
					}

					context.Coupon.Update(coup);
					context.SaveChanges();
				}
				catch (Exception e)
				{
					throw new Exception(e.Message);
				}
			}
		}

		internal void UpdateCouponFinish(long couponId, long userId)
		{
			using (DatabaseContext context = new DatabaseContext())
			{
				try
				{
					Coupon? coupon = GetCoupon(couponId, userId);
					if (coupon == null)
					{
						throw new Exception("Not Found");
					}
					DateTime now = DateTime.Now;
					if (coupon.StartDate <= now && now <= coupon.EndDate)
					{
						coupon.EndDate = now;
						context.Coupon.Update(coupon);
						context.SaveChanges();
					}
					else
					{
						throw new Exception("Exceed time can't end");
					}
				}
				catch (Exception e)
				{
					throw new Exception(e.Message);
				}
			}
		}
	}
}
