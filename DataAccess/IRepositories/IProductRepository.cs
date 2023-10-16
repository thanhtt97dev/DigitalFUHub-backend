using DTOs.Product;
using BusinessObject.Entities;
using DTOs.Seller;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataAccess.DAOs;

namespace DataAccess.IRepositories
{
	public interface IProductRepository
	{
		void AddProduct(Product product);
		List<SellerProductResponseDTO> GetAllProduct(int userId);
		List<ProductDetailVariantResponeDTO> GetProductVariants(int productId);
        ProductDetailResponseDTO? GetProductById(long productId);
		void EditProduct(Product product, List<ProductVariant> productVariantsNew, List<ProductVariant> productVariantsUpdate, List<Tag> tags, List<ProductMedia> productMediaNew, List<string> productImagesOld);
		string GetProductThumbnail(long productId);
		List<ProductMedia> GetAllProductMediaById(long productId);
        List<AllProductResponseDTO> GetAllProduct();
		ProductVariant? GetProductVariant(long id);
	}
}
