using BusinessObject.Entities;
using DataAccess.DAOs;
using DataAccess.IRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace DataAccess.Repositories
{
	public class ShopRegisterFeeRepository : IShopRegisterFeeRepository
	{
		public void AddNewShopRegisterFee(long fee) => ShopRegisterFeeDAO.Instance.AddNewShopRegisterFee(fee);
		public long GetCurrentFee() => ShopRegisterFeeDAO.Instance.GetCurrentFee();
		public List<ShopRegisterFee> GetFees(long businessFeeId, int maxFee, DateTime? fromDate, DateTime? toDate) => ShopRegisterFeeDAO.Instance.GetFees(businessFeeId, maxFee, fromDate, toDate);

	}
}
