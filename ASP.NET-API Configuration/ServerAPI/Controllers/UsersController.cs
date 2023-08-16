using AutoMapper;
using BusinessObject;
using DataAccess.IRepositories;
using DTOs;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ServerAPI.Services;

namespace ServerAPI.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class UsersController : ControllerBase
	{
		private readonly IConfiguration _configuration;
		private readonly IUserRepository _userRepository;
		private readonly IRefreshTokenRepository _refreshTokenRepository;
		private readonly IAccessTokenRepository _accessTokenRepository;
		private readonly IMapper _mapper;

		private readonly JwtTokenService _jwtTokenService;	

		public UsersController(IConfiguration configuration, IUserRepository userRepository, IMapper mapper,
			IRefreshTokenRepository refreshTokenRepository, IAccessTokenRepository accessTokenRepository, JwtTokenService jwtTokenService)
		{
			_configuration = configuration;
			_userRepository = userRepository;
			_mapper = mapper;
			_refreshTokenRepository = refreshTokenRepository;
			_accessTokenRepository = accessTokenRepository;
			_jwtTokenService = jwtTokenService;
		}

		[HttpPost("SignIn")]
		public async Task<IActionResult> SignInAsync(UserSignInRequestDTO userSignIn)
		{
			try
			{
				User? user = _userRepository.GetUserByEmailAndPassword(userSignIn.Email, userSignIn.Password);

				if (user == null)
				{
					return NotFound("Email or Password not correct!");
				}

				var token = await _jwtTokenService.GenerateTokenAsync(user);

				return Ok(token);
			}
			catch (Exception) 
			{
				return StatusCode(500);
			}
		}

		/*
		[Authorize]
		[HttpPost("RefreshToken")]
		public async Task<IActionResult> RefreshTokenAsync(RefreshTokenRequestDTO refreshTokenRequestDTO)
		{
			try
			{
				var isValidRefreshToken =  _jwtTokenService
					.CheckRefreshTokenIsValidAsync(refreshTokenRequestDTO.AccessToken, refreshTokenRequestDTO.RefreshToken);

				if (!isValidRefreshToken) return Unauthorized();

				var user = await _userRepository.GetUserByRefreshTokenAsync(refreshTokenRequestDTO.RefreshToken);

				if (user == null) return Unauthorized();

				var token = await _jwtTokenService.GenerateTokenAsync(user);

				await _refreshTokenRepository.RemoveRefreshTokenAysnc(refreshTokenRequestDTO.RefreshToken);

				return Ok(token);
			}
			catch (Exception) 
			{
				return StatusCode(500);
			}
		}
		*/

		[Authorize]
		[HttpPost("RefreshToken")]
		public async Task<IActionResult> RefreshTokenAsync(RefreshTokenRequestDTO refreshTokenRequestDTO)
		{
			try
			{
				var isValidRefreshToken = _jwtTokenService
					.CheckRefreshTokenIsValid(refreshTokenRequestDTO.AccessToken, refreshTokenRequestDTO.RefreshToken);

				if (!isValidRefreshToken) return Unauthorized();

				var user = _userRepository.GetUserByRefreshToken(refreshTokenRequestDTO.RefreshToken);

				if (user == null) return Unauthorized();

				var token = _jwtTokenService.GenerateTokenAsync(user);

				await _refreshTokenRepository.RemoveRefreshTokenAysnc(refreshTokenRequestDTO.RefreshToken);

				return Ok(await token);
			}
			catch (Exception)
			{
				return StatusCode(500);
			}
		}

		[Authorize]
		[HttpPost("RevokeToken")]
		public async Task<IActionResult> RevokeTokenAsync(RefreshTokenRequestDTO refreshTokenRequestDTO)
		{
			try
			{
				return Ok();

			}
			catch (Exception)
			{
				return StatusCode(500);
			}
		}


		[Authorize(Roles = "Admin")]
		[HttpPost("test")]
		public IActionResult Test()
		{
			string? accessToken = HttpContext.GetTokenAsync("access_token").Result;
			return Ok(accessToken);
		}

		[HttpPost("testConflict")]
		public IActionResult testConflict()
		{
			string? accessToken = HttpContext.GetTokenAsync("access_token").Result;
			return Conflict("Remove token");
		}

		[Authorize(Roles = "Admin")]
		[HttpGet("hehe")]
		public IActionResult daw()
		{
			return Ok("hieuld6");
		}

	}
}
