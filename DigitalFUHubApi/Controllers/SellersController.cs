using AutoMapper;
using BusinessObject.Entities;
using DataAccess.IRepositories;
using DataAccess.Repositories;
using DigitalFUHubApi.Services;
using DTOs.Seller;
using DTOs.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DigitalFUHubApi.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class SellersController : ControllerBase
	{
		private readonly IConfiguration configuration;
		private readonly IProductRepository productRepository;
		private readonly IMapper mapper;

		public SellersController(IConfiguration configuration, IProductRepository productRepository, IMapper mapper)
		{
			this.configuration = configuration;
			this.productRepository = productRepository;
			this.mapper = mapper;
		}

		#region Get All Product with shopId (userId)
		//[Authorize("Seller")]
		[HttpGet("GetAllProduct/{id}")]
		public IActionResult GetAllProduct(int id)
		{
			try
			{
				if(id == 0) return BadRequest();
				var products = productRepository.GetAllProduct(id);
				return Ok(products);
			}
			catch (Exception ex)
			{
				return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
			}
		}
		#endregion

		#region Get Product Variants
		[HttpGet("GetProductVariants/{id}")]
		public IActionResult GetProductVariants(int id)
		{
			try
			{
				if (id == 0) return BadRequest();
				var products = productRepository.GetProductVariants(id);
				return Ok(products);
			}
			catch (Exception ex)
			{
				return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
			}
		}
		#endregion

		#region Add new product
		[HttpPost("Product/New")]
		public IActionResult AddProduct([FromForm] AddProductRequestDTO request)
		{
			return Ok();
		}
		#endregion
	}
}
