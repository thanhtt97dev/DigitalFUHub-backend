namespace ServerAPI.Controllers
{
	using AutoMapper;
	using BusinessObject;
	using DataAccess.IRepositories;
	using DTOs;
	using Microsoft.AspNetCore.Authorization;
	using Microsoft.AspNetCore.Mvc;
	using ServerAPI.Services;


	[Route("api/[controller]")]
	[ApiController]
	public class UsersController : ControllerBase
	{
		private readonly IUserRepository _userRepository;
		private readonly IRefreshTokenRepository _refreshTokenRepository;
		private readonly IAccessTokenRepository _accessTokenRepository;
		private readonly IMapper _mapper;

		private readonly JwtTokenService _jwtTokenService;	

		public UsersController(IUserRepository userRepository, IMapper mapper,
			IRefreshTokenRepository refreshTokenRepository, IAccessTokenRepository accessTokenRepository, JwtTokenService jwtTokenService)
		{
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

				if (user == null) return NotFound("Email or Password not correct!");
			
				var token = _jwtTokenService.GenerateTokenAsync(user);

				return Ok(await token);
			}
			catch (Exception ex) 
			{
				return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
			}
		}


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
				return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred on the server");
			}
		}

		[Authorize]
		[HttpPost("RevokeToken")]
		public IActionResult RevokeToken([FromBody]string jwtId)
		{
			try
			{
				if (string.IsNullOrEmpty(jwtId)) return BadRequest("Cannot revoke access token!");
				_accessTokenRepository.RevokeToken(jwtId);
				return Ok();
			}
			catch (Exception)
			{
				return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred on the server");
			}
		}

		[Authorize]
		[HttpGet("GetUsers")]
		public IActionResult GetUsersByCondition(int? role, int? status, string email = "")
		{
			if(role == null || status == null) return BadRequest();
			
			try
			{
				var userRequestDTO = new UserRequestDTO()
				{
					Email = email == null ? string.Empty : email,
					RoleId = role,
					Status = status
				};
				var users = _userRepository.GetUsers(userRequestDTO);

				return Ok(_mapper.Map<List<UserResponeDTO>>(users));
			}
			catch (Exception)
			{
				return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred on the server");
			}
		}

		[Authorize]
		[HttpGet("GetUserById/{id}")]
		public IActionResult GetUserById(int id)
		{
			if (id == 0) return BadRequest();
			try
			{
				var user = _userRepository.GetUserById(id);
				if (user == null) return Conflict();
				return Ok(_mapper.Map<UserResponeDTO>(user));
			}
			catch (Exception)
			{
				return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred on the server");
			}
		}

		[Authorize]
		[HttpPut("EditUserInfo/{id}")]
		public async Task<IActionResult> EditUserInfo(int id, UserRequestDTO userRequestDTO)
		{
			if(userRequestDTO == null)	return BadRequest();
			try
			{
				User? user = _userRepository.GetUserById(id);
				if (user == null) return NotFound();	
				var userUpdate = _mapper.Map<User>(userRequestDTO);	
				await _userRepository.EditUserInfo(id, userUpdate);
				return NoContent();
			}catch(Exception ex) 
			{
				var x = ex;
				return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred on the server");
			}
		}

		[HttpGet("test")]
		public IActionResult Get()
		{
			var user = _userRepository.GetUserById(1);
			return Ok(user);
		}
	}
}
