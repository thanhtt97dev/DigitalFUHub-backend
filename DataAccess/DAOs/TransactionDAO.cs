using BusinessObject.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.DAOs
{
	internal class TransactionDAO
	{
		private static TransactionDAO? instance;
		private static readonly object instanceLock = new object();

		public static TransactionDAO Instance
		{
			get
			{
				lock (instanceLock)
				{
					if (instance == null)
					{
						instance = new TransactionDAO();
					}
				}
				return instance;
			}
		}

		internal void AddTransactionsForOrderConfirmed(Order order)
		{
			throw new NotImplementedException();
		}
	}
}
