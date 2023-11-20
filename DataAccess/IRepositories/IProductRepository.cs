﻿using DTOs.Product;
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
		List<SellerProductResponseDTO> GetAllProduct(long userId);
		List<ProductDetailVariantResponeDTO> GetProductVariants(int productId);
		ProductDetailResponseDTO? GetProductById(long productId);
		void EditProduct(Product product, List<ProductVariant> productVariantsNew, List<ProductVariant> productVariantsUpdate, List<Tag> tags, List<ProductMedia> productMediaNew, List<string> productImagesOld);
		string GetProductThumbnail(long productId);
		List<ProductMedia> GetAllProductMediaById(long productId);
		List<AllProductResponseDTO> GetAllProduct();
		ProductVariant? GetProductVariant(long id);
		Product? GetProductByShop(long userId, long productId);
		bool IsExistProductByShop(long userId, long productId);
		Product? CheckProductExist(long userId, long productId);
		Product? GetProductEntityById(long productId);
		bool CheckProductExist(List<long> productIds);
		(List<Product>, long) GetListProductOfSeller(long userId, string productId, string productName, int page);
		int GetNumberProductByConditions(long shopId, string shopName, long productId, string productName, int productCategory, int soldMin, int soldMax, int productStatusId);
		List<Product> GetProductsForAdmin(long shopId, string shopName, long productId, string productName, int productCategory, int soldMin, int soldMax, int productStatusId, int page);
		Product? GetProduct(long id);
		void UpdateProductStatusAdmin(long productId, int status, string note);
		int GetNumberProductByConditions(long userId, string productName);
		int GetNumberProductByConditions(long categoryId);
        List<Product> GetProductByUserId(long userId, int page, string productName);
		List<Product> GetProductForHomePageCustomer(int page, long categoryId, bool isOrderFeedback, bool isOrderSoldCount);
        List<Product> GetProductsOfSeller(long userId, long productId, string productName, int productCategory, int soldMin, int soldMax, int productStatusId, int page);
    }
}
