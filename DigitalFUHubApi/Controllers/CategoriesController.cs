using BusinessObject.Entities;
using DataAccess.IRepositories;
using DigitalFUHubApi.Comons;
using Microsoft.AspNetCore.Mvc;
using Comons;
using Microsoft.AspNetCore.Authorization;

namespace DigitalFUHubApi.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class CategoriesController : ControllerBase
	{
		private readonly ICategoryRepository _categoryRepository;

		public CategoriesController(ICategoryRepository categoryRepository)
		{
			_categoryRepository = categoryRepository;
		}
		[HttpGet("GetAll")]
		public ActionResult<ResponseData> GetAll()
		{
			List<Category> categories = _categoryRepository.GetAll();
			return Ok(new ResponseData(Constants.RESPONSE_CODE_SUCCESS, "Success!", false, categories));
		}

		[Authorize("Seller")]
		[HttpGet("Seller/All")]
		public ActionResult<ResponseData> GetAllForSeller()
		{
			List<Category> categories = _categoryRepository.GetAll();
			return Ok(new ResponseData(Constants.RESPONSE_CODE_SUCCESS, "Success!", false, categories));
		}

	}
}
