using BusinessObject.Entities;
using Comons;
using DataAccess.IRepositories;
using DigitalFUHubApi.Comons;
using DTOs.Shop;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DigitalFUHubApi.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class ShopsController : ControllerBase
	{
		private readonly IShopRepository _shopRepository;

		public ShopsController(IShopRepository shopRepository)
		{
			_shopRepository = shopRepository;
		}
		//[Authorize]
		[HttpGet("CheckExistShopName/{shopName}")]
		public IActionResult CheckExistShopName(string shopName)
		{
			ResponseData response = new ResponseData();
			if (string.IsNullOrEmpty(shopName))
			{
				response.Status.Message = "Invalid";
				response.Status.Ok = false;
				response.Status.ResponseCode = Constants.RESPONSE_CODE_NOT_ACCEPT;
				return Ok(response);
			}
			bool result = _shopRepository.CheckShopNameExisted(shopName.Trim());
			response.Status.Message = !result ? "Success" : "Invalid";
			response.Status.Ok = !result;
			response.Status.ResponseCode = !result ? Constants.RESPONSE_CODE_SUCCESS : Constants.RESPONSE_CODE_FAILD;
			return Ok(response);
		}

	}
}
