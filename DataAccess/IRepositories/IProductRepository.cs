using DTOs.Product;
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
		List<SellerProductResponeDTO> GetAllProduct(int userId);
		List<ProductDetailVariantResponeDTO> GetProductVariants(int productId);
        ProductDetailResponseDTO GetProductById(long productId);
    }
}
