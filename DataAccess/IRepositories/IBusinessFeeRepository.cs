using BusinessObject.Entities;
using DTOs.Admin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.IRepositories
{
	public interface IBusinessFeeRepository
	{
		List<BusinessFeeResponseDTO> GetBusinessFee(long businessFeeId, int maxFee, DateTime? fromDate, DateTime? toDate);
		void AddNewBusinessFee(long fee);
		long GetBusinessFeeCurrent();
	}
}
