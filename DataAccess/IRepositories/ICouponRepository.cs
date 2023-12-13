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
		bool IsExistCouponCode(long shopId, string couponCode, string action);
		Coupon? GetCoupon(long couponId, long shopId);
		(long, List<Coupon>) GetListCouponsByShop(long userId, string couponCode, DateTime? startDate, DateTime? endDate,
			bool? isPublic, long status, int page);
		void UpdateStatusCoupon(Coupon coupon);
		void UpdateCoupon(Coupon coupon);
		void UpdateCouponFinish(long couponId, long v);
		Coupon? GetCouponDetailCustomer(long couponId);

    }
}
