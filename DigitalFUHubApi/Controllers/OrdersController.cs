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
        public IActionResult AddOrder([FromBody] AddOrderRequestDTO addOrderRequest)
        {
            try
            {
                Order order = _mapper.Map<Order>(addOrderRequest);
                _repository.AddOrder(order);
                return Ok(new Status
                {
                    Message = "Add Order Successfully"
                    ,
                    ResponseCode = Constants.RESPONSE_CODE_SUCCESS
                    ,
                    Ok = true
                });

            }
            catch (ArgumentNullException ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
