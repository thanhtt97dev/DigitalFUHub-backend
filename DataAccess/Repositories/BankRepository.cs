using BusinessObject;
using DataAccess.DAOs;
using DataAccess.IRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Repositories
{
	public class BankRepository : IBankRepository
	{
		public void AddUserBank(UserBank userBank) => BankDAO.Instance.AddUserBank(userBank);
		public List<Bank> GetAll() => BankDAO.Instance.GetAll();

		public List<UserBank> GetAllBankInfoUserLinked(int userId) => BankDAO.Instance.GetAllBankInfoUserLinked(userId);

		public int TotalUserLinkedBank(int userId) => BankDAO.Instance.TotalUserLinkedBank(userId);

	}
}
