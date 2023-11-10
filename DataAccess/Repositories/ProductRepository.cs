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
		public void AddProduct(Product product) => ProductDAO.Instance.AddProduct(product);

		public List<SellerProductResponseDTO> GetAllProduct(long userId) => ProductDAO.Instance.GetAllProduct(userId);

		public List<AllProductResponseDTO> GetAllProduct() => ProductDAO.Instance.GetAllProduct();

		public List<ProductDetailVariantResponeDTO> GetProductVariants(int productId) => ProductDAO.Instance.GetProductVariants(productId);

		public ProductDetailResponseDTO? GetProductById(long productId) => ProductDAO.Instance.GetProductById(productId);

		public void EditProduct(Product product, List<ProductVariant> productVariantsNew, List<ProductVariant> productVariantsUpdate, List<Tag> tags, List<ProductMedia> productMediaNew, List<string> productImagesOld)
		=> ProductDAO.Instance.EditProduct(product, productVariantsNew, productVariantsUpdate, tags, productMediaNew, productImagesOld);

		public string GetProductThumbnail(long productId) => ProductDAO.Instance.GetProductThumbnail(productId);

		public List<ProductMedia> GetAllProductMediaById(long productId) => ProductDAO.Instance.GetAllProductMediaById(productId);

		public ProductVariant? GetProductVariant(long id) => ProductDAO.Instance.GetProductVariant(id);

		public Product? GetProductByShop(long userId, long productId) => ProductDAO.Instance.GetProductByShop(userId, productId);

		public bool IsExistProductByShop(long userId, long productId) => ProductDAO.Instance.IsExistProductByShop(userId, productId);

		public Product? CheckProductExist(long userId, long productId)
		=> ProductDAO.Instance.CheckProductExist(userId, productId);

		public (List<Product>, long) GetListProductOfSeller(long userId, string productId, string productName, int page)
		=> ProductDAO.Instance.GetListProductOfSeller(userId, productId, productName, page);

		public int GetNumberProductByConditions(long shopId, string shopName, long productId, string productName, int productCategory, int soldMin, int soldMax, int productStatusId) => ProductDAO.Instance.GetNumberProductByConditions(shopId, shopName, productId, productName, productCategory, soldMin, soldMax, productStatusId);

		public List<Product> GetProductsForAdmin(long shopId, string shopName, long productId, string productName, int productCategory, int soldMin, int soldMax, int productStatusId, int page) => ProductDAO.Instance.GetProductsForAdmin(shopId,shopName, productId, productName, productCategory, soldMin, soldMax, productStatusId, page);

		public Product? GetProduct(long id) => ProductDAO.Instance.GetProduct(id);

		public void UpdateProductStatusAdmin(long productId, int status, string note) => ProductDAO.Instance.UpdateProductStatusAdmin(productId, status, note);

		public List<Product> GetProductsOfSeller(long userId, long productId, string productName, int productCategory, int soldMin, int soldMax, int productStatusId, int page) => ProductDAO.Instance.GetProductsOfSeller(userId, productId, productName, productCategory, soldMin, soldMax, productStatusId, page);

	}
}
