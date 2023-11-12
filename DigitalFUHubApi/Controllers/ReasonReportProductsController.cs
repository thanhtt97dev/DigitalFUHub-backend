using AutoMapper;
using Comons;
using DataAccess.IRepositories;
using DataAccess.Repositories;
using DigitalFUHubApi.Comons;
using DigitalFUHubApi.Services;
using DTOs.ReasonReportProduct;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DigitalFUHubApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReasonReportProductsController : ControllerBase
    {
        private readonly IReasonReportProductRepository reasonReportProductRepository;
        private readonly IMapper mapper;

        public ReasonReportProductsController(IMapper mapper, IReasonReportProductRepository reasonReportProductRepository)
        {
            this.reasonReportProductRepository = reasonReportProductRepository;
            this.mapper = mapper;
        }


        [HttpGet("GetAll")]
        //[Authorize]
        public IActionResult GetAllReasonReportProducts ()
        {
            ResponseData responseData = new ResponseData();
            try
            {
                var reasons = reasonReportProductRepository.GetAll();
                var reasonResponse = mapper.Map<List<ReasonReportProductResponseDTO>>(reasons);
                // OK
                responseData.Status.ResponseCode = Constants.RESPONSE_CODE_SUCCESS;
                responseData.Status.Ok = true;
                responseData.Status.Message = "Success!";
                responseData.Result = reasonResponse;
                return Ok(responseData);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }
    }
}
