using AutoMapper;
using BusinessObject;
using DataAccess.IRepositories;
using DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ServerAPI.Services;

namespace ServerAPI.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class UsersController : ControllerBase
	{
		private readonly ApiResponseHelper _apiResponseHelper;
		private readonly IConfiguration _configuration;
		private readonly IUserRepository _userRepository;
		private readonly IRefreshTokenRepository _refreshTokenRepository;
		private readonly IMapper _mapper;

		public UsersController(ApiResponseHelper apiResponseHelper, IConfiguration configuration, IUserRepository userRepository, IRefreshTokenRepository refreshTokenRepository, IMapper mapper)
		{
			_apiResponseHelper = apiResponseHelper;
			_configuration = configuration;
			_userRepository = userRepository;
			_mapper = mapper;
			_refreshTokenRepository  = refreshTokenRepository;
		}

		[HttpPost("SignIn")]
		public async Task<IActionResult> SignInAsync(UserSignInRequestDTO userSignIn)
		{
			try
			{
				User? user = await _userRepository.GetUserByEmailAndPasswordAsync(userSignIn.Email, userSignIn.Password);

				if (user == null)
				{
					return NotFound("Email or Password not correct!");
				}

				var token = await JwtTokenService.Instance.GenerateTokenAsync(user, _refreshTokenRepository, _configuration);

				return Ok(token);
			}
			catch (Exception) 
			{
				return _apiResponseHelper.StatusCode(500);
			}
		}

		[HttpPost("RefreshToken")]
		public async Task<IActionResult> RefreshTokenAsync(RefreshTokenRequestDTO refreshTokenRequestDTO)
		{
			try
			{
				var isValidRefreshToken = await JwtTokenService.Instance
					.IsValidRefreshTokenAsync(refreshTokenRequestDTO.AccessToken, refreshTokenRequestDTO.RefreshToken, _refreshTokenRepository, _configuration);

				if (!isValidRefreshToken)
				{
					return _apiResponseHelper.StatusCode(409,"Refresh token is invalid!");
				}

				var user = await _userRepository.GetUserFromRefreshTokenAsync(refreshTokenRequestDTO.RefreshToken);

				if(user == null)
				{
					return _apiResponseHelper.StatusCode(409);
				}

				var token = await JwtTokenService.Instance.GenerateTokenAsync(user, _refreshTokenRepository, _configuration);

				await _refreshTokenRepository.RemoveRefreshTokenAysnc(refreshTokenRequestDTO.RefreshToken);

				return Ok(token);
			}
			catch (Exception) 
			{
				return _apiResponseHelper.StatusCode(500);
			}
		}
	}
}
