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
		bool CheckCouponCodeExist(string couponCode);
		List<Coupon> GetCouponPublic(long shopId);
        Coupon? GetCouponByCode(string couponCode);
        Coupon? GetCoupons(long couponId);
		List<Coupon> GetListCouponsByShop(long userId, string couponCode, DateTime? startDate, DateTime? endDate, bool? isPublic);
		void UpdateCoupon(Coupon coupon);
	}
}
