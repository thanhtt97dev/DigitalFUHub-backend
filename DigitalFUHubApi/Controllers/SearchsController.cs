using BusinessObject.Entities;
using Comons;
using DataAccess.IRepositories;
using DigitalFUHubApi.Comons;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Validations;

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
		#region get search hint
		[HttpGet("SearchHint")]
		public IActionResult GetSearchHint(string keyword) {
			if(string.IsNullOrWhiteSpace(keyword))
			{
				return Ok(new ResponseData(Constants.RESPONSE_CODE_NOT_ACCEPT, "Invalid data", true, new()));
			}
			List<string> listSearchHint = _productRepository.GetListProductNameForSearchHint(keyword);
			return Ok(new ResponseData(Constants.RESPONSE_CODE_SUCCESS, "Success", true, listSearchHint));
		}
		#endregion

	}
}
