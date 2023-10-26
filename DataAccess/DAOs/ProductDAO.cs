using BusinessObject;
using BusinessObject.Entities;
using Comons;
using DTOs.MbBank;
using DTOs.Product;
using DTOs.Seller;
using DTOs.Tag;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
		internal List<SellerProductResponseDTO> GetAllProduct(int shopId)
		{
			List<SellerProductResponseDTO> result = new List<SellerProductResponseDTO>();
			using (DatabaseContext context = new DatabaseContext())
			{
				var products = context.Product.Where(x => x.ShopId == shopId && x.ProductStatusId == Constants.PRODUCT_ACTIVE).ToList();
				foreach (var product in products)
				{
					var productVariants = context.ProductVariant.Where(x => x.ProductId == product.ProductId).ToList();
					List<ProductDetailVariantResponeDTO> variants = new List<ProductDetailVariantResponeDTO>();
					foreach (var variant in productVariants)
					{
						variants.Add(new ProductDetailVariantResponeDTO()
						{
							ProductVariantId = variant.ProductVariantId,
							Name = variant.Name,
							Price = variant.Price,
							Quantity = context.AssetInformation.Count(x => x.ProductVariantId == variant.ProductVariantId && x.IsActive)
						});
					}
					SellerProductResponseDTO productResponeDTO = new SellerProductResponseDTO()
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

		internal List<AllProductResponseDTO> GetAllProduct()
		{
			List<AllProductResponseDTO> result = new List<AllProductResponseDTO>();
			using (DatabaseContext context = new DatabaseContext())
			{
				var products = context.Product.ToList();
				foreach (var product in products)
				{
					var productVariants = context.ProductVariant.Where(x => x.ProductId == product.ProductId).ToList();
					List<ProductDetailVariantResponeDTO> variants = new List<ProductDetailVariantResponeDTO>();
					foreach (var variant in productVariants)
					{
						variants.Add(new ProductDetailVariantResponeDTO()
						{
							ProductVariantId = variant.ProductVariantId,
							Price = variant.Price
						});
					}
					AllProductResponseDTO productResponeDTO = new AllProductResponseDTO()
					{
						ProductId = product.ProductId,
						ProductName = product.ProductName,
						Discount = product.Discount,
						Thumbnail = product.Thumbnail,
						ProductVariants = variants
					};
					result.Add(productResponeDTO);
				}
				return result;
			}
		}

		internal List<ProductDetailVariantResponeDTO> GetProductVariants(int productId)
		{
			List<ProductDetailVariantResponeDTO> result = new List<ProductDetailVariantResponeDTO>();
			using (DatabaseContext context = new DatabaseContext())
			{
				var variants = context.ProductVariant.Where(x => x.ProductId == productId && x.isActivate).ToList();
				foreach (var variant in variants)
				{
					result.Add(new ProductDetailVariantResponeDTO()
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


		internal ProductDetailResponseDTO? GetProductById(long productId)
		{
			using (DatabaseContext context = new DatabaseContext())
			{
				var product = context.Product.Include(_ => _.Shop).FirstOrDefault(x => x.ProductId == productId);
				if (product == null) return null;
				long productQuantity = 0;
				List<ProductVariant> productVariants = context.ProductVariant.Where(x => x.ProductId == product.ProductId).ToList() ?? new List<ProductVariant>();
				List<ProductMedia> productMedias = context.ProductMedia.Where(x => x.ProductId == product.ProductId).ToList() ?? new List<ProductMedia>();
				List<Tag> productTags = context.Tag.Where(x => x.ProductId == product.ProductId).ToList() ?? new List<Tag>();
				List<ProductDetailVariantResponeDTO> variants = new List<ProductDetailVariantResponeDTO>();
				foreach (var variant in productVariants)
				{
					ProductDetailVariantResponeDTO productVariantResp = new ProductDetailVariantResponeDTO()
					{
						ProductVariantId = variant.ProductVariantId,
						Name = variant.Name,
						Price = variant.Price,
						Quantity = context.AssetInformation.Count(x => x.ProductVariantId == variant.ProductVariantId && x.IsActive == true)
					};

					variants.Add(productVariantResp);
					productQuantity += productVariantResp?.Quantity ?? 0;
				}

				List<string> medias = new List<string>();
				foreach (var media in productMedias)
				{
					medias.Add(media.Url);
				}

				List<TagResponseDTO> tags = new List<TagResponseDTO>();
				foreach (var tag in productTags)
				{
					tags.Add(new TagResponseDTO()
					{
						TagId = tag.TagId,
						TagName = tag.TagName,
					});
				}

				long shopId = product.Shop.UserId;
				var productOfShop = context.Product.Where(x => x.ShopId == shopId);
				long productNumber = productOfShop.Count();
				long feedbachNumber = 0;
				foreach (Product item in productOfShop)
				{
					feedbachNumber += context.Feedback.Where(x => x.ProductId == item.ProductId).Count();
				}

				var sellerInfo = context.User.First(x => x.UserId == shopId);

				ProductDetailShopResponseDTO shop = new ProductDetailShopResponseDTO
				{
					ShopId = product.Shop.UserId,
					Avatar = "",
					ShopName = product.Shop.ShopName,
					DateCreate = product.Shop.DateCreate,
					FeedbackNumber = feedbachNumber,
					ProductNumber = productNumber,
					IsOnline = sellerInfo.IsOnline,
					LastTimeOnline = sellerInfo.LastTimeOnline
				};

				ProductDetailResponseDTO productDetailResponse = new ProductDetailResponseDTO()
				{
					ProductId = product.ProductId,
					ProductName = product.ProductName,
					Thumbnail = product.Thumbnail,
					Description = product.Description,
					Discount = product.Discount,
					CategoryId = product.CategoryId,
					Quantity = productQuantity,
					ProductVariants = variants,
					ProductMedias = medias,
					Tags = tags,
					Shop = shop
				};

				return productDetailResponse;
			}
		}

		internal void AddProduct(Product product)
		{
			using (DatabaseContext context = new DatabaseContext())
			{
				using (var transaction = context.Database.BeginTransaction())
				{
					try
					{
						context.Product.Add(product);
						context.SaveChanges();
						transaction.Commit();
					}
					catch (Exception e)
					{
						transaction.Rollback();
						throw new Exception(e.Message);
					}
				}
			}
		}

		internal void EditProduct(Product product, List<ProductVariant> productVariantsNew, List<ProductVariant> productVariantsUpdate, List<Tag> tags, List<ProductMedia> productMediaNew, List<string> productMediaOld)
		{
			using (DatabaseContext context = new DatabaseContext())
			{
				using (var transaction = context.Database.BeginTransaction())
				{
					try
					{
						Product? productE = context.Product.Where(p => p.ProductId == product.ProductId).FirstOrDefault();
						if (productE == null) throw new Exception("Product not found");
						productE.UpdateDate = DateTime.Now;
						productE.CategoryId = product.CategoryId;
						productE.ProductName = product.ProductName;
						productE.Description = product.Description;
						productE.Discount = product.Discount;
						if (product.Thumbnail != null)
						{
							productE.Thumbnail = product.Thumbnail;
						}
						context.Product.Update(productE);
						//context.SaveChanges();

						// remove media
						IQueryable<ProductMedia> productMedias = context.ProductMedia.Where(x => x.ProductId == productE.ProductId && !productMediaOld.Any(pm => pm == x.Url));
						context.ProductMedia.RemoveRange(productMedias);

						// add new product media
						foreach (var item in productMediaNew)
						{
							context.ProductMedia.Add(new ProductMedia
							{
								ProductId = product.ProductId,
								Url = item.Url,
							});
						}
						//context.SaveChanges();

						List<ProductVariant> productVariantsDelete = context.ProductVariant.Where(x => x.ProductId == productE.ProductId && !productVariantsUpdate.Select(pv => pv.ProductVariantId).ToList().Any(pvd => pvd == x.ProductVariantId)).ToList();
						foreach (var item in productVariantsDelete)
						{
							item.isActivate = false;
						}
						context.ProductVariant.UpdateRange(productVariantsDelete);
						//context.SaveChanges();

						foreach (var item in productVariantsUpdate)
						{
							ProductVariant? productVariant = context.ProductVariant.FirstOrDefault(x => x.ProductId == productE.ProductId && x.ProductVariantId == item.ProductVariantId);
							if (productVariant != null)
							{
								// delete old data and add new data
								if (item.AssetInformations != null || item.AssetInformations?.Count > 0)
								{
									IQueryable<AssetInformation> assetInformations = context.AssetInformation.Where(x => x.IsActive == true && x.ProductVariantId == item.ProductVariantId);
									context.AssetInformation.RemoveRange(assetInformations);

									//foreach (var asset in item.AssetInformation)
									//{
									//	context.AssetInformation.Add(new AssetInformation
									//	{
									//		ProductVariantId = item.ProductVariantId,
									//		CreateDate = DateTime.Now,
									//		Asset = asset.Asset,
									//		IsActive = true
									//	});
									//}
									productVariant.AssetInformations = item.AssetInformations;
								}
								productVariant.Name = item.Name;
								productVariant.Price = item.Price;
								context.ProductVariant.Update(productVariant);
							}
						}
						foreach (var item in productVariantsNew)
						{
							context.ProductVariant.Add(new ProductVariant
							{
								isActivate = true,
								AssetInformations = item.AssetInformations,
								Price = item.Price,
								Name = item.Name,
								ProductId = productE.ProductId,
							});
						}
						// delete old tag
						List<Tag> tagsOld = context.Tag.Where(x => x.ProductId == productE.ProductId).ToList();
						context.Tag.RemoveRange(tagsOld);

						// add new tag
						foreach (var item in tags)
						{
							context.Tag.Add(new Tag
							{
								ProductId = productE.ProductId,
								TagName = item.TagName
							});
						}

						context.SaveChanges();
						transaction.Commit();
					}
					catch (Exception)
					{
						transaction.Rollback();
						throw new Exception();
					}
				}
			}
		}

		internal string GetProductThumbnail(long productId)
		{
			using (DatabaseContext context = new DatabaseContext())
			{
				return context.Product.FirstOrDefault(x => x.ProductId == productId)?.Thumbnail ?? "";
			}
		}

		internal List<ProductMedia> GetAllProductMediaById(long productId)
		{
			using (DatabaseContext context = new DatabaseContext())
			{
				return context.ProductMedia.Where(x => x.ProductId == productId).ToList();
			}
		}

		internal ProductVariant? GetProductVariant(long id)
		{
			using (DatabaseContext context = new DatabaseContext())
			{
				return context.ProductVariant.FirstOrDefault(x => x.ProductVariantId == id);
			}
		}

		internal Product? GetProductByShop(long userId, long productId)
		{
			using (DatabaseContext context = new DatabaseContext())
			{
				Product? product = context.Product
					.Where(x => x.ProductId == productId && x.ShopId == userId
							&& x.ProductStatusId != Constants.PRODUCT_BAN && x.ProductStatusId != Constants.PRODUCT_HIDE)
						.Select(x => new Product
						{
							ProductId = productId,
							CategoryId = x.CategoryId,
							Description = x.Description,
							Discount = x.Discount,
							ProductMedias = x.ProductMedias,
							ProductName = x.ProductName,
							ProductStatusId = x.ProductStatusId,
							Tags = x.Tags,
							Thumbnail = x.Thumbnail,
							ProductVariants = context.ProductVariant.Include(i => i.AssetInformations.Where(x => x.IsActive == true)).Where(x => x.ProductId == productId && x.isActivate == true).ToList(),
						}).FirstOrDefault();
				return product;
			}
		}

		internal bool IsExistProductByShop(long userId, long productId)
		{
			using (DatabaseContext context = new DatabaseContext())
			{
				return context.Shop.Include(i => i.Products)
					.Any(x => x.UserId == userId && x.Products.Any(x => x.ProductId == productId));
			}
		}
	}
}



