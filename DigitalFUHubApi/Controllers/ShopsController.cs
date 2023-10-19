using BusinessObject.Entities;
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

		#region register seller
		[Authorize("Customer")]
		[HttpPost("Register")]
		public ActionResult<ResponseData> Register(RegisterShopRequestDTO request)
		{
			ResponseData response = new ResponseData();
			if (!ModelState.IsValid || string.IsNullOrWhiteSpace(request.ShopName) || string.IsNullOrWhiteSpace(request.ShopDescription))
			{
				response.Status.ResponseCode = Constants.RESPONSE_CODE_NOT_ACCEPT;
				response.Status.Message = "Vui lòng kiểm tra lại dữ liệu.";
				response.Status.Ok = false;
				return response;
			}
			User? user;
			try
			{
				user = _userRepository.GetUserById(request.UserId);
				if (user == null) throw new Exception("Người dùng không khả dụng.");

				bool userShopExist = _shopRepository.UserHasShop(request.UserId);
				if (userShopExist) throw new Exception("Đã tồn tại cửa hàng không thể tạo thêm.");

				bool shopNameExist = _shopRepository.CheckShopNameExisted(request.ShopName.Trim());
				if (shopNameExist) throw new Exception("Tên cửa hàng đã tồn tại.");

				_shopRepository.CreateShop(request.ShopName.Trim(), request.UserId, request.ShopDescription.Trim());
			}
			catch (Exception e)
			{
				response.Status.ResponseCode = Constants.RESPONSE_CODE_FAILD;
				response.Status.Message = e.Message;
				response.Status.Ok = false;
				return response;
			}


			response.Status.ResponseCode = Constants.RESPONSE_CODE_SUCCESS;
			response.Status.Message = "Đăng ký cửa hàng thành công.";
			response.Status.Ok = true;
			return response;
		}
		#endregion

	}
}
