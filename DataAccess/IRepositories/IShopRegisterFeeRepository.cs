using BusinessObject.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.IRepositories
{
	public interface IShopRegisterFeeRepository
	{
		void AddNewShopRegisterFee(long fee);
		List<ShopRegisterFee> GetFees(long businessFeeId, int maxFee, DateTime? fromDate, DateTime? toDate);
	}
}
