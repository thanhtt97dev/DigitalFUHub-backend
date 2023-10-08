using BusinessObject.Entities;
using DataAccess.DAOs;
using DataAccess.IRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Repositories
{
    public class CouponRepository : ICouponRepository
    {
        public List<Coupon> GetByShopId(long shopId) => CouponDAO.Instance.GetByShopId(shopId);
    }
}
