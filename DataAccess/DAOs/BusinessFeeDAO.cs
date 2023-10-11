using BusinessObject;
using BusinessObject.Entities;
using DTOs.Admin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace DataAccess.DAOs
{
	internal class BusinessFeeDAO
	{
		private static BusinessFeeDAO? instance;
		private static readonly object instanceLock = new object();

		public static BusinessFeeDAO Instance
		{
			get
			{
				lock (instanceLock)
				{
					if (instance == null)
					{
						instance = new BusinessFeeDAO();
					}
				}
				return instance;
			}
		}

		internal List<BusinessFeeResponseDTO> GetBusinessFee(long businessFeeId, int maxFee, DateTime fromDate, DateTime toDate)
		{
			using (DatabaseContext context = new DatabaseContext())
			{
				var fees = (from businessFee in context.BusinessFee
							where (1 == 1) &&
							(businessFeeId != 0 ? businessFee.BusinessFeeId == businessFeeId : true) &&
							fromDate <= businessFee.StartDate && toDate >= businessFee.StartDate &&
							businessFee.Fee <= maxFee
							select new BusinessFeeResponseDTO 
							{
								BusinessFeeId = businessFee.BusinessFeeId,
								Fee = businessFee.Fee,	
								StartDate = businessFee.StartDate,
								EndDate = businessFee.EndDate,
								TotalOrderUsed = context.Order.Count(x => x.BusinessFeeId == businessFee.BusinessFeeId)
							})
							.OrderByDescending(x => x.BusinessFeeId)
							.ToList();
				return fees;
			}
		}

		internal void AddNewBusinessFee(long fee)
		{
			using (DatabaseContext context = new DatabaseContext())
			{
				var transaction = context.Database.BeginTransaction();
				try
				{
					var businessFeeDate = context.BusinessFee.Max(x => x.StartDate);
					var businessFeeOld = context.BusinessFee.First(x => x.StartDate == businessFeeDate);
					businessFeeOld.EndDate = DateTime.Now;

					var businessFee = new BusinessFee 
					{
						StartDate = DateTime.Now,
						Fee = fee,
					};
					context.BusinessFee.Add(businessFee);
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
	}
}
