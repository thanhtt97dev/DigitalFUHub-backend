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
	public class CategoryRepository : ICategoryRepository
	{
		public async Task<List<Category>> GetAllAsync() => await CategoryDAO.Instance.GetAllAsync();
	}
}
