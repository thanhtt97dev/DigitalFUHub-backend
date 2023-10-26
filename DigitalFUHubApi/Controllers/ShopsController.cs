﻿using BusinessObject.Entities;
using Comons;
using DataAccess.IRepositories;
using DataAccess.Repositories;
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
		private readonly IUserRepository _userRepository;

		public ShopsController(IShopRepository shopRepository, IUserRepository userRepository)
		{
			_shopRepository = shopRepository;
			_userRepository = userRepository;
		}


		[Authorize]
		[HttpGet("IsExistShopName/{shopName}")]
		public IActionResult CheckExistShopName(string shopName)
		{
			if (string.IsNullOrWhiteSpace(shopName))
			{
				return Ok(new ResponseData(Constants.RESPONSE_CODE_NOT_ACCEPT, "INVALID", false, new()));
			}
			bool result = _shopRepository.IsExistShopName(shopName.Trim());
			return Ok(new ResponseData(!result ? Constants.RESPONSE_CODE_SUCCESS : Constants.RESPONSE_CODE_NOT_ACCEPT, !result ? "SUCCESS" : "INVALID", !result, new()));
		}

		#region register seller
		[Authorize("Customer")]
		[HttpPost("Register")]
		public IActionResult Register(RegisterShopRequestDTO request)
		{
			if (!ModelState.IsValid
				|| string.IsNullOrWhiteSpace(request.ShopName)
				|| string.IsNullOrWhiteSpace(request.ShopDescription))
			{
				return Ok(new ResponseData(Constants.RESPONSE_CODE_NOT_ACCEPT, "INVALID", false, new()));
			}
			try
			{
				long userId = Util.Instance.GetUserId(User);
				_shopRepository.AddShop(request.ShopName.Trim(), userId, request.ShopDescription.Trim());
				return Ok(new ResponseData(Constants.RESPONSE_CODE_SUCCESS, "SUCCESS", true, new()));
			}
			catch (Exception e)
			{
				return Ok(new ResponseData(Constants.RESPONSE_CODE_FAILD, e.Message, false, new()));
			}
		}
		#endregion

	}
}
