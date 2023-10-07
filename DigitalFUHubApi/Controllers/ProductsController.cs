﻿using AutoMapper;
using DataAccess.IRepositories;
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
                var product = _productRepository.GetProductById(productId);
                return Ok(product);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
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
