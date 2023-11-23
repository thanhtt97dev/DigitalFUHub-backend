using BusinessObject.Entities;
using DataAccess.DAOs;
using DataAccess.IRepositories;
using DTOs.Admin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Repositories
{
	public class BusinessFeeRepositoty : IBusinessFeeRepository
	{
		public void AddNewBusinessFee(long fee) => BusinessFeeDAO.Instance.AddNewBusinessFee(fee);
		public List<BusinessFeeResponseDTO> GetBusinessFee(long businessFeeId, int maxFee, DateTime? fromDate, DateTime? toDate) => BusinessFeeDAO.Instance.GetBusinessFee(businessFeeId,maxFee,fromDate,toDate);
		
	}
}
