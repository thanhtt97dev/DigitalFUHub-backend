using BusinessObject.Entities;
using DataAccess.DAOs;
using DataAccess.IRepositories;
using DTOs.Product;
using DTOs.Seller;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Repositories
{
	public class ProductRepository : IProductRepository
	{
		public void AddProduct(Product product) =>  ProductDAO.Instance.AddProduct(product);

		public List<SellerProductResponseDTO> GetAllProduct(int userId) => ProductDAO.Instance.GetAllProduct(userId);

        public List<AllProductResponseDTO> GetAllProduct() => ProductDAO.Instance.GetAllProduct();

        public List<ProductDetailVariantResponeDTO> GetProductVariants(int productId) => ProductDAO.Instance.GetProductVariants(productId);

        public ProductDetailResponseDTO GetProductById(long productId)
        {
            if (productId == 0) throw new ArgumentException("productId cannot eq 0 (at getProductById)");
            return ProductDAO.Instance.GetProductById(productId);
        }

		public void EditProduct(Product product, List<ProductVariant> productVariantsNew, List<ProductVariant> productVariantsUpdate, List<Tag> tags, List<ProductMedia> productMediaNew, List<string> productImagesOld)
		=> ProductDAO.Instance.EditProduct( product,  productVariantsNew, productVariantsUpdate, tags, productMediaNew, productImagesOld);

		public string GetProductThumbnail(long productId) => ProductDAO.Instance.GetProductThumbnail(productId);

		public List<ProductMedia> GetAllProductMediaById(long productId) => ProductDAO.Instance.GetAllProductMediaById(productId);
	}
}
