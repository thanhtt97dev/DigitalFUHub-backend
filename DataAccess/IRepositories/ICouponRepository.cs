using BusinessObject.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.IRepositories
{
    public interface ICouponRepository
    {
		void AddCoupon(Coupon coupon);
		List<Coupon> GetCouponPublic(long shopId);
        Coupon? GetCouponPrivate(string couponCode, long shopId);
		bool IsExistCouponCode(long shopId, string couponCode, char action);
        Coupon? GetCoupon(long couponId, long shopId);
		List<Coupon> GetListCouponsByShop(long userId, string couponCode, DateTime? startDate, DateTime? endDate, bool? isPublic);
		void UpdateStatusCoupon(Coupon coupon);
		void UpdateCoupon(Coupon coupon);
	}
}
