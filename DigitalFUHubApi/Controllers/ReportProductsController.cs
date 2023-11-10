using AutoMapper;
using Comons;
using DataAccess.IRepositories;
using DataAccess.Repositories;
using DigitalFUHubApi.Comons;
using DigitalFUHubApi.Services;
using DTOs.Product;
using DTOs.ReportProduct;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DigitalFUHubApi.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class ReportProductsController : ControllerBase
	{
		private readonly IReportProductRepository reportProductRepository;
		private readonly IMapper mapper;

		public ReportProductsController(IReportProductRepository reportProductRepository, IMapper mapper)
		{
			this.reportProductRepository = reportProductRepository;
			this.mapper = mapper;
		}

		[Authorize("Admin")]
		[HttpPost("admin/update")]
		public IActionResult GetProductDetailAdmin(UpdateReportProductRequestDTO request)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest();
			}
			try
			{
				reportProductRepository.UpdateReportProduct(request.ReportProductId, request.Status, request.Note);
				return Ok(new ResponseData(Constants.RESPONSE_CODE_SUCCESS, "SUCCESS", true, new()));
			}
			catch (Exception ex)
			{
				return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
			}
		}
	}
}
