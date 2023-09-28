using BusinessObject.Entities;
using DataAccess.DAOs;
using DataAccess.IRepositories;
using DTOs.Shop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Repositories
{
	public class ShopRepository : IShopRepository
	{
		public async Task CreateShopAsync(RegisterShopRequestDTO request) => await ShopDAO.Instance.CreateShopAsync(request);
	}
}
