using DTOs.Seller;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.IRepositories
{
	public interface IProductRepository
	{
		List<ProductResponeDTO> GetAllProduct(int userId);
	}
}
