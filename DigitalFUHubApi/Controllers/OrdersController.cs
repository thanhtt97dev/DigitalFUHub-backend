using AutoMapper;
using BusinessObject.Entities;
using Comons;
using DataAccess.IRepositories;
using DataAccess.Repositories;
using DigitalFUHubApi.Comons;
using DTOs.Cart;
using DTOs.Order;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DigitalFUHubApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly IOrderRepository _repository;

        public OrdersController(IMapper mapper, IOrderRepository repository)
        {
            _mapper = mapper;
            _repository = repository;
        }

        [HttpPost("AddOrder")]
        //[Authorize]
        public IActionResult AddOrder([FromBody] List<AddOrderRequestDTO> addOrderRequest)
        {
            try
            {
                if (addOrderRequest == null) return BadRequest();

				(string responseCode, string message) = _repository.AddOrder(addOrderRequest);

				ResponseData responseData = new ResponseData();
				Status status = new Status()
                {
					Message = message,
					Ok = responseCode == Constants.RESPONSE_CODE_SUCCESS,
					ResponseCode = responseCode
			    };
                responseData.Status = status;
				return Ok(responseData);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
