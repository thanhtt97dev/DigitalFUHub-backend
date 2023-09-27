using BusinessObject.Entities;
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
		Task AddProductAsync(Product product);
		List<ProductResponeDTO> GetAllProduct(int userId);
		List<ProductVariantResponeDTO> GetProductVariants(int productId);	
	}
}
