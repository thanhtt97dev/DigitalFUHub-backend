﻿using AutoMapper;
using DataAccess.IRepositories;
using DigitalFUHubApi.Comons;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DigitalFUHubApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly IProductRepository _productRepository;
        private readonly IMapper _mapper;

        public ProductsController(IConfiguration configuration, IProductRepository productRepository, IMapper mapper)
        {
            _configuration = configuration;
            _productRepository = productRepository;
            _mapper = mapper;
        }

        [HttpGet("GetById/{productId}")]
        public IActionResult GetById(long productId)
        {
            try
            {
                if (productId == 0)
                {
                    return BadRequest(new Status());
                }

                var product = _productRepository.GetProductById(productId);
                if (product == null)
                {
                    return NotFound(new Status());
                }
                return Ok(product);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return BadRequest(new Status());
            }
        }

        #region Get All Product 
        [HttpGet("GetAllProduct")]
        public IActionResult GetAllProduct()
        {
            try
            {
                var products = _productRepository.GetAllProduct();
                return Ok(products);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }
        #endregion
    }
}
