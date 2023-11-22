using BusinessObject.Entities;
using Comons;
using DataAccess.IRepositories;
using DigitalFUHubApi.Comons;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DigitalFUHubApi.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class SearchsController : ControllerBase
	{
		private readonly IShopRepository _shopRepository;
		private readonly IProductRepository _productRepository;

		public SearchsController(IShopRepository shopRepository, IProductRepository productRepository)
		{
			_shopRepository = shopRepository;
			_productRepository = productRepository;
		}

		[HttpGet("SearchHint")]
		public IActionResult GetSearchHint(string keyword) {
			if(string.IsNullOrWhiteSpace(keyword))
			{
				return Ok(new ResponseData(Constants.RESPONSE_CODE_NOT_ACCEPT, "Invalid data", true, new()));
			}
			List<Product> products = _productRepository.GetListProductForSearchHint(keyword);
			List<string> listSearchHint = products.Select(x => x.ProductName).ToList();
			return Ok(new ResponseData(Constants.RESPONSE_CODE_SUCCESS, "Success", true, listSearchHint));
		}
		
	}
}
