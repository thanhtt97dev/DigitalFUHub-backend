﻿using AutoMapper;
using DataAccess.IRepositories;
using DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ServerAPI.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class RolesController : ControllerBase
	{
		private readonly IRoleRepository _roleRepository;
		private readonly IMapper _mapper;

		public RolesController(IRoleRepository roleRepository, IMapper mapper)
		{
			_roleRepository = roleRepository;
			_mapper = mapper;
		}

		[HttpGet("GetAllRoles")]
		public IActionResult GetAllRoles() 
		{
			try
			{
				var roles = _roleRepository.GetAllRole();
				return Ok(_mapper.Map<List<RoleDTO>>(roles));
			}catch (Exception) 
			{
				return StatusCode(500);
			}
		}
	}
}
