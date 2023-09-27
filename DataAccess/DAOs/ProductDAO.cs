using BusinessObject;
using BusinessObject.Entities;
using DTOs.MbBank;
using DTOs.Seller;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.DAOs
{
	internal class ProductDAO
	{
		private static ProductDAO? instance;
		private static readonly object instanceLock = new object();

		public static ProductDAO Instance
		{
			get
			{
				lock (instanceLock)
				{
					if (instance == null)
					{
						instance = new ProductDAO();
					}
				}
				return instance;
			}
		}
		internal List<ProductResponeDTO> GetAllProduct(int shopId)
		{
			List<ProductResponeDTO> result = new List<ProductResponeDTO>();
			using (DatabaseContext context = new DatabaseContext())
			{
				var products = context.Product.Where(x => x.ShopId == shopId).ToList();
				foreach (var product in products)
				{
					var productVariants = context.ProductVariant.Where(x => x.ProductId == product.ProductId).ToList();
					List<ProductVariantResponeDTO> variants = new List<ProductVariantResponeDTO>();
					foreach (var variant in productVariants)
					{
						variants.Add(new ProductVariantResponeDTO()
						{
							ProductVariantId = variant.ProductVariantId,
							Name = variant.Name,
							Price = variant.Price,
							Quantity = context.AssetInformation.Count(x => x.ProductVariantId == variant.ProductVariantId)
						});
					}
					ProductResponeDTO productResponeDTO = new ProductResponeDTO()
					{
						ProductId = product.ProductId,
						ProductName = product.ProductName,
						Thumbnail = product.Thumbnail,
						ProductVariants = variants
					};
					result.Add(productResponeDTO);
				}
				return result;
			}
		}

		internal List<ProductVariantResponeDTO> GetProductVariants(int productId)
		{
			List<ProductVariantResponeDTO> result = new List<ProductVariantResponeDTO>();
			using (DatabaseContext context = new DatabaseContext())
			{
				var variants = context.ProductVariant.Where(x => x.ProductId == productId && x.isActivate).ToList();
				foreach (var variant in variants)
				{
					result.Add(new ProductVariantResponeDTO()
					{
						ProductVariantId = variant.ProductVariantId,	
						Name = variant.Name,
						Price = variant.Price,
						Quantity = context.AssetInformation.Count(x => x.ProductVariantId == variant.ProductVariantId)
					});
				}
				return result;
			}
		}

		internal ProductResponeDTO GetProductById (long productId)
		{
            using (DatabaseContext context = new DatabaseContext())
            {
                var product = context.Product
						.Include(_ => _.ProductStatus)
                        .Include(_ => _.Shop)
                        .Include(_ => _.Category)
                        .FirstOrDefault(x => x.ProductId == productId);
				if (product == null) throw new ArgumentException("Not found Product hav id = " + productId);
                var productVariants = context.ProductVariant.Where(x => x.ProductId == product.ProductId).ToList();
                    List<ProductVariantResponeDTO> variants = new List<ProductVariantResponeDTO>();
                    foreach (var variant in productVariants)
                    {
                        variants.Add(new ProductVariantResponeDTO()
                        {
                            ProductVariantId = variant.ProductVariantId,
                            Name = variant.Name,
                            Price = variant.Price,
                            Quantity = context.AssetInformation.Count(x => x.ProductVariantId == variant.ProductVariantId)
                        });
                    }

                    ProductResponeDTO productResponeDTO = new ProductResponeDTO()
                    {
                        ProductId = product.ProductId,
                        ProductName = product.ProductName,
                        Thumbnail = product.Thumbnail,
                        Description = product.Description,
                        Discount = product.Discount,
						ProductStatus = product.ProductStatus,
						Category = product.Category,
						Shop = product.Shop,
                        ProductVariants = variants
                    };

                return productResponeDTO;
            }
		}
	}
}
