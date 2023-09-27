using BusinessObject;
using BusinessObject.Entities;
using DTOs.MbBank;
using DTOs.Seller;
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

		internal async Task AddProductAsync(Product product)
		{
			using (DatabaseContext context = new DatabaseContext())
			{
				using (var transaction = await context.Database.BeginTransactionAsync())
				{
					try
					{
						context.Product.Add(product);
						await context.SaveChangesAsync();
						//foreach (var tag in product.Tags.ToList())
						//{
						//	Tag tagAdded = new Tag
						//	{
						//		ProductId = product.ProductId,
						//		TagName = tag.TagName
						//	};
						//	context.Tag.Add(tagAdded);
						//	await context.SaveChangesAsync();
						//}
						//foreach (var prdMedia in product.ProductMedias.ToList())
						//{
						//	ProductMedia productMediaAdded = new ProductMedia
						//	{
						//		ProductId = product.ProductId,
						//		Url = prdMedia.Url
						//	};
						//	context.ProductMedia.Add(productMediaAdded);
						//	await context.SaveChangesAsync();
						//}
						//foreach (var prdVar in product.ProductVariants.ToList())
						//{
						//	ProductVariant productVariantAdded = new ProductVariant
						//	{
						//		ProductId = product.ProductId,
						//		Price = prdVar.Price,
						//		isActivate = true,
						//		Name = prdVar.Name,
						//	};
						//	context.ProductVariant.Add(productVariantAdded);
						//	await context.SaveChangesAsync();
						//	foreach (var assetInfo in prdVar.AssetInformation.ToList())
						//	{
						//		AssetInformation assetInformation = new AssetInformation
						//		{
						//			ProductVariantId = productVariantAdded.ProductVariantId,
						//			CreateDate = assetInfo.CreateDate,
						//			Asset = assetInfo.Asset,
						//			IsActive = assetInfo.IsActive,
						//		};
						//		context.AssetInformation.Add(assetInformation);
						//		await context.SaveChangesAsync();
						//	}
						//}
						await transaction.CommitAsync();
					}
					catch (Exception e)
					{
						await transaction.RollbackAsync();
						throw new Exception(e.Message);
					}
				}
			}
		}
	}
}	

