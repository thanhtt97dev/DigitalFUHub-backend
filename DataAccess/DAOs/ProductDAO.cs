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
		internal List<SellerProductResponseDTO> GetAllProduct(long shopId)
		{
			List<SellerProductResponseDTO> result = new List<SellerProductResponseDTO>();
			using (DatabaseContext context = new DatabaseContext())
			{
				var products = context.Product.Where(x => x.ShopId == shopId && (x.ProductStatusId == Constants.PRODUCT_STATUS_ACTIVE || x.ProductStatusId == Constants.PRODUCT_STATUS_HIDE)).ToList();
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

				// product variant
				List<ProductDetailVariantResponeDTO> variants = new List<ProductDetailVariantResponeDTO>();
				foreach (var variant in productVariants)
				{
					ProductDetailVariantResponeDTO productVariantResp = new ProductDetailVariantResponeDTO()
					{
						ProductVariantId = variant.ProductVariantId,
						Name = variant.Name,
						Price = variant.Price,
						Discount = variant.Discount,
						Quantity = context.AssetInformation.Count(x => x.ProductVariantId == variant.ProductVariantId && x.IsActive == true)
					};

					variants.Add(productVariantResp);
					productQuantity += productVariantResp?.Quantity ?? 0;
				}

				// medias
				List<string> medias = new List<string>();
				foreach (var media in productMedias)
				{
					medias.Add(media.Url);
				}

				// tags
				List<TagResponseDTO> tags = new List<TagResponseDTO>();
				foreach (var tag in productTags)
				{
					tags.Add(new TagResponseDTO()
					{
						TagId = tag.TagId,
						TagName = tag.TagName,
					});
				}

				// number feedback and product
				long shopId = product.Shop.UserId;
				var productOfShop = context.Product.Where(x => x.ShopId == shopId).ToList();
				long productNumber = productOfShop.Count();
				long feedbachNumber = 0;
				foreach (Product item in productOfShop)
				{
					feedbachNumber += item.NumberFeedback;
				}

				var sellerInfo = context.User.First(x => x.UserId == shopId);

				ProductDetailShopResponseDTO shop = new ProductDetailShopResponseDTO
				{
					ShopId = product.Shop.UserId,
					Avatar = product.Shop.Avatar,
					ShopName = product.Shop.ShopName,
					DateCreate = product.Shop.DateCreate,
					FeedbackNumber = feedbachNumber,
					ProductNumber = productNumber,
					IsOnline = sellerInfo.IsOnline,
					LastTimeOnline = sellerInfo.LastTimeOnline
				};

				var feedbacks = (from feedback in context.Feedback
								 where
									feedback.ProductId == productId
								 select new Feedback
								 {
									 Rate = feedback.Rate,
									 Content = feedback.Content,
									 FeedbackMedias = context.FeedbackMedia.
															 Where(x => x.FeedbackId == feedback.FeedbackId)
															 .Select(x => new FeedbackMedia { })
															 .ToList(),
								 }
								).ToList();

				ProductDetailFeedBackGeneralInfo feedBackGeneralInfo = new ProductDetailFeedBackGeneralInfo
				{
					TotalRatingStar = product.TotalRatingStar,
					TotalFeedback = product.NumberFeedback,
					TotalFeedbackFiveStar = feedbacks.Count(x => x.Rate == 5),
					TotalFeedbackFourStar = feedbacks.Count(x => x.Rate == 4),
					TotalFeedbackThreeStar = feedbacks.Count(x => x.Rate == 3),
					TotalFeedbackTwoStar = feedbacks.Count(x => x.Rate == 2),
					TotalFeedbackOneStar = feedbacks.Count(x => x.Rate == 1),
					TotalFeedbackHaveComment = feedbacks.Count(x => !string.IsNullOrEmpty(x.Content)),
					TotalFeedbackHaveMedia = feedbacks.Count(x => x.FeedbackMedias.Count() > 0),
				};

				ProductDetailResponseDTO productDetailResponse = new ProductDetailResponseDTO()
				{
					ProductId = product.ProductId,
					ProductName = product.ProductName,
					Thumbnail = product.Thumbnail,
					Description = product.Description,
					CategoryId = product.CategoryId,
					SoldCount = product.SoldCount,
					ProductStatusId = product.ProductStatusId,
					TotalRatingStar = product.TotalRatingStar,
					NumberFeedback = product.NumberFeedback,
					Quantity = productQuantity,
					ProductVariants = variants,
					ProductMedias = medias,
					Tags = tags,
					Shop = shop,
					FeedBackGeneralInfo = feedBackGeneralInfo
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
						productE.DateUpdate = DateTime.Now;
						productE.CategoryId = product.CategoryId;
						productE.ProductName = product.ProductName;
						productE.Description = product.Description;
						//productE.Discount = product.Discount;
						productE.ProductStatusId = product.ProductStatusId;
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
								productVariant.Discount = item.Discount;
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
								Discount = item.Discount,
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
							&& (x.ProductStatusId == Constants.PRODUCT_STATUS_ACTIVE || x.ProductStatusId == Constants.PRODUCT_STATUS_HIDE))
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

		internal Product? CheckProductExist(long userId, long productId)
		{
			using (DatabaseContext context = new DatabaseContext())
			{
				Product? product = context.Product
					.Include(x => x.ProductMedias)
					.Include(x => x.ProductVariants)
					.Where(x => x.ProductId == productId && x.ShopId == userId
							&& (x.ProductStatusId == Constants.PRODUCT_STATUS_ACTIVE || x.ProductStatusId == Constants.PRODUCT_STATUS_HIDE))
						.FirstOrDefault();
				return product;
			}
		}

		internal bool CheckProductExist(List<long> productIds)
		{
			using (DatabaseContext context = new DatabaseContext())
			{
				bool isExist = context.Product.Any(x => productIds.Contains(x.ProductId));

				return isExist;

			}
		}

		internal (List<Product>, long) GetListProductOfSeller(long userId, string productId, string productName, int page)
		{
			using (DatabaseContext context = new DatabaseContext())
			{
				if (userId == 0) throw new Exception("INVALID DATA");
				var query = context.Product.Where(x => x.ShopId == userId
							&& (string.IsNullOrWhiteSpace(productId) ? true : productId.Trim() == x.ProductId.ToString())
							&& (string.IsNullOrWhiteSpace(productName) ? true : x.ProductName.ToLower().Contains(productName.ToLower().Trim())));
				var lsProduct = query
					.Skip((page - 1) * Constants.PAGE_SIZE).Take(Constants.PAGE_SIZE)
					.ToList();
				return (lsProduct, query.Count());
			}
		}

		internal int GetNumberProductByConditions(long shopId, string shopName, long productId, string productName, int productCategory, int soldMin, int soldMax, int productStatusId)
		{
			using (DatabaseContext context = new DatabaseContext())
			{
				return (from product in context.Product
						join shop in context.Shop
							 on product.ShopId equals shop.UserId
						where shop.ShopName.Contains(shopName.Trim()) &&
						product.ProductName.Contains(productName.Trim()) &&
						((shopId == 0) ? true : product.ShopId == shopId) &&
						((productId == 0) ? true : product.ProductId == productId) &&
						((productCategory == 0) ? true : product.CategoryId == productCategory) &&
						((productStatusId == 0) ? true : product.ProductStatusId == productStatusId) &&
						((soldMin == 0) ? true : product.SoldCount >= soldMin) &&
						((soldMax == 0) ? true : product.SoldCount <= soldMax)
						select new { }
						).Count();
			}
		}

		internal int GetNumberProductByConditions(long userId, string productName)
		{
			using (DatabaseContext context = new DatabaseContext())
			{
				return (from product in context.Product
						where product.ShopId == userId
								&&
								(product.ProductName.ToUpper().Trim().Contains(productName.ToUpper().Trim())
								&&
								(product.ProductStatusId == Constants.PRODUCT_STATUS_ACTIVE
								||
								product.ProductStatusId == Constants.PRODUCT_STATUS_BAN))
						select new { })
						.Count();
			}
		}


		internal List<Product> GetProductsForAdmin(long shopId, string shopName, long productId, string productName, int productCategory, int soldMin, int soldMax, int productStatusId, int page)
		{
			using (DatabaseContext context = new DatabaseContext())
			{
				var products = (from product in context.Product
								join shop in context.Shop
									 on product.ShopId equals shop.UserId
								where shop.ShopName.Contains(shopName.Trim()) &&
								product.ProductName.Contains(productName.Trim()) &&
								((shopId == 0) ? true : product.ShopId == shopId) &&
								((productId == 0) ? true : product.ProductId == productId) &&
								((productCategory == 0) ? true : product.CategoryId == productCategory) &&
								((productStatusId == 0) ? true : product.ProductStatusId == productStatusId) &&
								((soldMin == 0) ? true : product.SoldCount >= soldMin) &&
								((soldMax == 0) ? true : product.SoldCount <= soldMax)
								select new Product
								{
									ProductId = product.ProductId,
									ProductName = product.ProductName,
									Thumbnail = product.Thumbnail,
									ViewCount = product.ViewCount,
									LikeCount = product.LikeCount,
									SoldCount = product.SoldCount,
									ProductStatusId = product.ProductStatusId,
									Shop = new Shop
									{
										UserId = shop.UserId,
										ShopName = shop.ShopName,
									},
									ProductVariants = (from productVariant in context.ProductVariant
													   where productVariant.ProductId == product.ProductId && productVariant.isActivate == true
													   select new ProductVariant
													   {
														   ProductVariantId = productVariant.ProductId,
														   Name = productVariant.Name,
														   Price = productVariant.Price,
														   AssetInformations = (from assetInfomation in context.AssetInformation
																				where assetInfomation.ProductVariantId == productVariant.ProductVariantId && assetInfomation.IsActive
																				select new AssetInformation() { }
																			   ).ToList()
													   })
													   .OrderBy(x => x.Price)
													   .ToList(),
								}
							   )
							   .Skip((page - 1) * Constants.PAGE_SIZE)
							   .Take(Constants.PAGE_SIZE)
							   .ToList();
				return products;
			}
		}

		internal Product? GetProduct(long id)
		{
			using (DatabaseContext context = new DatabaseContext())
			{
				var productInfo = (from product in context.Product
								   join category in context.Category
										on product.CategoryId equals category.CategoryId
								   join shop in context.Shop
										on product.ShopId equals shop.UserId
								   where product.ProductId == id
								   select new Product
								   {
									   ProductId = id,
									   Category = new Category
									   {
										   CategoryId = category.CategoryId,
										   CategoryName = category.CategoryName
									   },
									   Shop = new Shop
									   {
										   UserId = shop.UserId,
										   ShopName = shop.ShopName,
										   Avatar = shop.Avatar,
										   User = (from seller in context.User
												   where seller.UserId == shop.UserId
												   select new User
												   {
													   Email = seller.Email
												   }).First()
									   },
									   ProductName = product.ProductName,
									   Description = product.Description,
									   Discount = product.Discount,
									   Thumbnail = product.Thumbnail,
									   DateCreate = product.DateCreate,
									   DateUpdate = product.DateUpdate,
									   TotalRatingStar = product.TotalRatingStar,
									   NumberFeedback = product.NumberFeedback,
									   ViewCount = product.ViewCount,
									   LikeCount = product.LikeCount,
									   SoldCount = product.SoldCount,
									   Note = product.Note,
									   ProductStatusId = product.ProductStatusId,
									   ProductVariants = (from productVariant in context.ProductVariant
														  where productVariant.ProductId == id && productVariant.isActivate
														  select new ProductVariant
														  {
															  Name = productVariant.Name,
															  Price = productVariant.Price,
															  Discount = productVariant.Discount,
														  }).OrderBy(x => x.Price).ToList(),
									   Tags = (from tag in context.Tag
											   where tag.ProductId == id
											   select new Tag
											   {
												   TagName = tag.TagName
											   }).ToList(),
									   ProductMedias = (from productMedia in context.ProductMedia
														where productMedia.ProductId == id
														select new ProductMedia
														{
															Url = productMedia.Url
														}).ToList(),
									   ReportProducts = (from reportProduct in context.ReportProduct
														 join reasonReportProduct in context.ReasonReportProduct
															 on reportProduct.ReasonReportProductId equals reasonReportProduct.ReasonReportProductId
														 join user in context.User
															 on reportProduct.UserId equals user.UserId
														 where reportProduct.ProductId == id
														 select new ReportProduct
														 {
															 ReportProductId = reportProduct.ReportProductId,
															 User = new User
															 {
																 UserId = user.UserId,
																 Email = user.Email,
																 Avatar = user.Avatar,
															 },
															 ReasonReportProduct = new ReasonReportProduct
															 {
																 ReasonReportProductId = reasonReportProduct.ReasonReportProductId,
																 ViName = reasonReportProduct.ViName,
																 ViExplanation = reasonReportProduct.ViExplanation
															 },
															 Description = reportProduct.Description,
															 DateCreate = reportProduct.DateCreate,
															 Note = reportProduct.Note,
															 ReportProductStatusId = reportProduct.ReportProductStatusId,
														 }).ToList()

								   })
								  .FirstOrDefault();
				return productInfo;
			}
		}

		internal void UpdateProductStatusAdmin(long productId, int status, string note)
		{
			using (DatabaseContext context = new DatabaseContext())
			{
				var product = context.Product.FirstOrDefault(x => x.ProductId == productId);
				if (product == null) throw new Exception("Data not found");
				product.ProductStatusId = status;
				product.Note = note;
				context.Product.Update(product);
				context.SaveChanges();
			}
		}

		internal List<Product> GetProductsOfSeller(long userId, long productId, string productName, int productCategory, int soldMin, int soldMax, int productStatusId, int page)
		{
			using (DatabaseContext context = new DatabaseContext())
			{
				var products = (from product in context.Product
								join shop in context.Shop
									 on product.ShopId equals shop.UserId
								where
								product.ProductName.Contains(productName.Trim()) &&
								product.ShopId == userId &&
								((productId == 0) ? true : product.ProductId == productId) &&
								((productCategory == 0) ? true : product.CategoryId == productCategory) &&
								((productStatusId == 0) ? true : product.ProductStatusId == productStatusId) &&
								((soldMin == 0) ? true : product.SoldCount >= soldMin) &&
								((soldMax == 0) ? true : product.SoldCount <= soldMax)
								select new Product
								{
									ProductId = product.ProductId,
									ProductName = product.ProductName,
									Thumbnail = product.Thumbnail,
									ViewCount = product.ViewCount,
									LikeCount = product.LikeCount,
									SoldCount = product.SoldCount,
									ProductStatusId = product.ProductStatusId,
									Shop = new Shop
									{
										UserId = shop.UserId,
										ShopName = shop.ShopName,
									},
									ProductVariants = (from productVariant in context.ProductVariant
													   where productVariant.ProductId == product.ProductId && productVariant.isActivate == true
													   select new ProductVariant
													   {
														   ProductVariantId = productVariant.ProductId,
														   Name = productVariant.Name,
														   Price = productVariant.Price,
														   AssetInformations = (from assetInfomation in context.AssetInformation
																				where assetInfomation.ProductVariantId == productVariant.ProductVariantId && assetInfomation.IsActive
																				select new AssetInformation() { }
																			   ).ToList()
													   })
													   .OrderBy(x => x.Price)
													   .ToList(),
								}
							   )
							   .Skip((page - 1) * Constants.PAGE_SIZE)
							   .Take(Constants.PAGE_SIZE)
							   .ToList();
				return products;
			}
		}

		internal Product? GetProductEntityById(long productId)
		{
			using (DatabaseContext context = new DatabaseContext())
			{
				var product = context.Product.FirstOrDefault(x => x.ProductId == productId);

				return product;
			}
		}

		internal List<Product> GetProductByUserId(long userId, int page, string productName)
		{
			using (DatabaseContext context = new DatabaseContext())
			{
				var products = (from product in context.Product
								where product.ShopId == userId
								&&
								(product.ProductName.ToUpper().Trim().Contains(productName.ToUpper().Trim())
								&&
								(product.ProductStatusId == Constants.PRODUCT_STATUS_ACTIVE
								||
								product.ProductStatusId == Constants.PRODUCT_STATUS_BAN))
								select new Product
								{
									ProductId = product.ProductId,
									ProductName = product.ProductName,
									Thumbnail = product.Thumbnail,
									TotalRatingStar = product.TotalRatingStar,
									NumberFeedback = product.NumberFeedback,
									SoldCount = product.SoldCount,
									ProductStatusId = product.ProductStatusId,
									ProductVariants = (from productVariant in context.ProductVariant
													   where productVariant.ProductId == product.ProductId
													   select new ProductVariant
													   {
														   ProductVariantId = productVariant.ProductId,
														   Discount = productVariant.Discount,
														   Price = productVariant.Price,
														   AssetInformations = (from assetInformation in context.AssetInformation
																				where assetInformation.ProductVariantId == productVariant.ProductVariantId
																				&&
																				assetInformation.IsActive
																				select new AssetInformation { }).ToList()
													   }).ToList()
								}
					).Skip((page - 1) * Constants.PAGE_SIZE_PRODUCT)
					 .Take(Constants.PAGE_SIZE_PRODUCT)
					 .ToList();

				return products;
			}
		}


		internal List<Product> GetProductForHomePageCustomer(int page, long categoryId, bool isOrderFeedback, bool isOrderSoldCount)
		{
			using (DatabaseContext context = new DatabaseContext())
			{
				var query = (from product in context.Product
							 where (categoryId == 0 ? true : product.CategoryId == categoryId)
								&&
                                (product.ProductStatusId == Constants.PRODUCT_STATUS_ACTIVE
                                ||
                                product.ProductStatusId == Constants.PRODUCT_STATUS_BAN)
                             select new Product
							 {
								 ProductId = product.ProductId,
								 ProductName = product.ProductName,
								 Thumbnail = product.Thumbnail,
								 TotalRatingStar = product.TotalRatingStar,
								 NumberFeedback = product.NumberFeedback,
								 SoldCount = product.SoldCount,
								 ProductStatusId = product.ProductStatusId,
								 ProductVariants = (from productVariant in context.ProductVariant
													where productVariant.ProductId == product.ProductId
													select new ProductVariant
													{
														ProductVariantId = productVariant.ProductId,
														Discount = productVariant.Discount,
														Price = productVariant.Price,
														AssetInformations = (from assetInformation in context.AssetInformation
																			 where assetInformation.ProductVariantId == productVariant.ProductVariantId
																			 &&
																			 assetInformation.IsActive
																			 select new AssetInformation { }).ToList()
													}).ToList()
							 }
					);

				// Sort descending by total star / number feedback
				if (isOrderFeedback)
				{
					query = query.OrderByDescending(x => x.NumberFeedback != 0 ? x.TotalRatingStar / x.NumberFeedback : 0);
				}

				// Sort descending by sold count
				if (isOrderSoldCount)
				{
					query = query.OrderByDescending(x => x.SoldCount);

				}

				return query.Skip((page - 1) * Constants.PAGE_SIZE_PRODUCT_HOME_PAGE)
					 .Take(Constants.PAGE_SIZE_PRODUCT_HOME_PAGE)
					 .ToList();
			}
		}

		internal int GetNumberProductByConditions(long categoryId)
		{
			using (DatabaseContext context = new DatabaseContext())
			{
				return (from product in context.Product
						where (categoryId == 0 ? true : product.CategoryId == categoryId)
								&&
								(product.ProductStatusId == Constants.PRODUCT_STATUS_ACTIVE
								||
								product.ProductStatusId == Constants.PRODUCT_STATUS_BAN)
						select new { })
						.Count();
			}
		}

		internal List<Product> GetListProductForSearchHint(string keyword)
		{
			using (DatabaseContext context = new DatabaseContext())
			{
				string keywordSearch = keyword.Trim().ToLower();
#pragma warning disable CS8604 // Possible null reference argument.
#pragma warning disable CS8602 // Dereference of a possibly null reference.
				return context.Product.Include(x => x.Tags)
					.Where(x => x.ProductName.ToLower().Contains(keywordSearch)
					|| x.Tags.Any(tag => tag.TagName.ToLower().Contains(keywordSearch)))
					.OrderByDescending(x => x.SoldCount)
					.Take(Constants.LIMIT_SEARCH_HINT)
					.ToList();
#pragma warning restore CS8602 // Dereference of a possibly null reference.
#pragma warning restore CS8604 // Possible null reference argument.
			}
		}

		internal (long totalItems, List<Product> productSearched) GetListProductSearched(string keyword, long categoryId,
			int rating, long? minPrice, long? maxPrice, long sort, int page)
		{
#pragma warning disable CS8604 // Possible null reference argument.
			using (DatabaseContext context = new DatabaseContext())
			{
				string keywordSearch = keyword.Trim().ToLower();
				var query = context.Product.Include(x => x.Tags).Include(x => x.ProductVariants)
					.Where(x => x.ProductName.ToLower().Contains(keywordSearch) && x.ProductStatusId == Constants.PRODUCT_STATUS_ACTIVE
						&& (categoryId == Constants.ALL_CATEGORY ? true : x.CategoryId == categoryId)
						&& (rating == Constants.FEEDBACK_TYPE_ALL ? true : (x.NumberFeedback != 0 && x.TotalRatingStar / x.NumberFeedback >= rating))
						&& (minPrice == null ? true : x.ProductVariants.Any(pv => (pv.Price - (pv.Price * pv.Discount / 100)) >= minPrice))
						&& (maxPrice == null ? true : x.ProductVariants.Any(pv => (pv.Price - (pv.Price * pv.Discount / 100)) <= maxPrice))
					);
				if (sort == Constants.SORTED_BY_DATETIME)
				{
					query = query.OrderByDescending(x => x.DateCreate);
				}
				else if (sort == Constants.SORTED_BY_PRICE_ASC)
				{
					query = query.OrderBy(x => x.ProductVariants.Min(pv => pv.Price - (pv.Price * pv.Discount / 100)));
				}
				else if (sort == Constants.SORTED_BY_PRICE_DESC)
				{
					query = query.OrderByDescending(x => x.ProductVariants.Min(pv => pv.Price - (pv.Price * pv.Discount / 100)));

				}
				else
				{
					query = query.OrderByDescending(x => x.SoldCount);
				}

				return (query.Count(), query.Skip((page - 1) * Constants.PAGE_SIZE_SEARCH_PRODUCT).Take(Constants.PAGE_SIZE_SEARCH_PRODUCT).ToList());
			}
#pragma warning restore CS8604 // Possible null reference argument.
		}
	}
}



