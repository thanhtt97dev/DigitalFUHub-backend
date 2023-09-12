namespace ServerAPI.Controllers
{
    using AutoMapper;
    using BusinessObject;
    using DataAccess.IRepositories;
    using DTOs;
	using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.SignalR;
    using Newtonsoft.Json;
	using ServerAPI.Comons;
    using ServerAPI.Hubs;
    using ServerAPI.Managers;
    using ServerAPI.Services;


    [Route("api/[controller]")]
	[ApiController]
	public class UsersController : ControllerBase
	{
		private readonly IUserRepository _userRepository;
		private readonly IRefreshTokenRepository _refreshTokenRepository;
		private readonly IAccessTokenRepository _accessTokenRepository;
		private readonly IMapper _mapper;
		private readonly IHubContext<NotificationHub> _notificationHubContext;
		private readonly IConnectionManager _connectionManager;
		private readonly INotificationRepositiory _notificationRepositiory;
		private readonly ITwoFactorAuthenticationRepository _twoFactorAuthenticationRepository;

		private readonly JwtTokenService _jwtTokenService;
		private readonly TwoFactorAuthenticationService _twoFactorAuthenticationService;

		public UsersController(IUserRepository userRepository, IMapper mapper,
			IRefreshTokenRepository refreshTokenRepository, 
			IAccessTokenRepository accessTokenRepository,
			IHubContext<NotificationHub> notificationHubContext, IConnectionManager connectionManager,
			INotificationRepositiory notificationRepositiory,
			ITwoFactorAuthenticationRepository twoFactorAuthenticationRepository,
			JwtTokenService jwtTokenService,
			TwoFactorAuthenticationService twoFactorAuthenticationService
			)
		{
			_userRepository = userRepository;
			_mapper = mapper;
			_refreshTokenRepository = refreshTokenRepository;
			_accessTokenRepository = accessTokenRepository;
			_jwtTokenService = jwtTokenService;
			_notificationHubContext = notificationHubContext;
			_connectionManager = connectionManager;
			_notificationRepositiory = notificationRepositiory;
			_twoFactorAuthenticationService = twoFactorAuthenticationService;
			_twoFactorAuthenticationRepository = twoFactorAuthenticationRepository;
		}

		#region SignIn
		[HttpPost("SignIn")]
		public async Task<IActionResult> SignInAsync(UserSignInRequestDTO userSignIn)
		{
			try
			{
				User? user = _userRepository.GetUserByEmailAndPassword(userSignIn.Email, userSignIn.Password);

				if (user == null) return NotFound("Email or Password not correct!");
				if (!user.Status) return Conflict("Your account was baned!");
				if (user.TwoFactorAuthentication)
				{
					return StatusCode(StatusCodes.Status416RangeNotSatisfiable, user.UserId);
				}
			
				var token = _jwtTokenService.GenerateTokenAsync(user);

				return Ok(await token);
			}
			catch (Exception ex) 
			{
				return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
			}
		}
		#endregion

		#region Generate access token by Two Factor Authentication Code
		[HttpPost("GenerateAccessToken/{id}")]
		public async Task<IActionResult> GenerateAccessTokenBy2FA(int id, User2FARequestValidateDTO user2FARequestValidateDTO)
		{
			try
			{
				if (string.IsNullOrEmpty(user2FARequestValidateDTO.Code)) return BadRequest();
				var user = _userRepository.GetUserById(id);
				if (user == null) return BadRequest();
				if (!user.TwoFactorAuthentication)
					return Conflict("This account is not using Two Factor Authentication!");

				var secretKey = _twoFactorAuthenticationRepository.Get2FAKey(id);
				if (secretKey == null) return BadRequest();

				bool isPinvalid = _twoFactorAuthenticationService
					.ValidateTwoFactorPin(secretKey, user2FARequestValidateDTO.Code);
				if (!isPinvalid) return Conflict("Code is invalid!");

				var token = _jwtTokenService.GenerateTokenAsync(user);

				return Ok(await token);
			}
			catch (Exception ex)
			{
				return StatusCode(500, ex.Message);
			}
		}
		#endregion

		#region Refresh token
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

		#endregion

		#region Revoke token
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
		#endregion

		#region Generate Two Factor Authentication Key
		[HttpPost("Generate2FaKey/{id}")]
		public IActionResult Generate2FaKey(int id)
		{
			try
			{
				var user = _userRepository.GetUserById(id);
				if (user == null) return BadRequest();
				if (user.TwoFactorAuthentication) 
					return Conflict("This account has enabled Two Factor Authentication!");

				string secretKey = _twoFactorAuthenticationService.GenerateSecretKey();

				string qrCode = _twoFactorAuthenticationService.GenerateQrCode(secretKey, user.Email);

				User2FAResponeDTO user2FAResponeDTO = new User2FAResponeDTO()
				{
					SecretKey = secretKey,
					QRCode = qrCode
				};

				return Ok(user2FAResponeDTO);
			}
			catch (Exception ex)
			{
				return StatusCode(500, ex.Message);
			}
		}
		#endregion

		#region Activate Two Factor Authentication
		//[Authorize]
		[HttpPut("Activate2Fa/{id}")]
		public IActionResult ActivateTwoFactorAuthentication(int id, User2FARequestActivateDTO user2FARequestDTO)
		{
			try
			{
				if (string.IsNullOrEmpty(user2FARequestDTO.SecretKey) || 
					string.IsNullOrEmpty(user2FARequestDTO.Code)) return BadRequest();
				var user = _userRepository.GetUserById(id);
				if (user == null) return BadRequest();
				if (user.TwoFactorAuthentication) 
					return Conflict("This account has enabled Two Factor Authentication!");

				bool isPinvalid = _twoFactorAuthenticationService
					.ValidateTwoFactorPin(user2FARequestDTO.SecretKey, user2FARequestDTO.Code);
				if (!isPinvalid) return Conflict("Code is invalid!");
				_twoFactorAuthenticationRepository.Add2FAKey(id, user2FARequestDTO.SecretKey);
				_userRepository.Update2FA(id);
				return Ok();
			}
			catch (Exception ex)
			{
				return StatusCode(500, ex.Message);
			}
		}
		#endregion

		#region Disable Two Factor Authentication
		//[Authorize]
		[HttpPut("Deactivate2Fa/{id}")]
		public IActionResult DisableTwoFactorAuthentication(int id, User2FARequestDisableDTO user2FARequestDisableDTO)
		{
			try
			{
				if(string.IsNullOrEmpty(user2FARequestDisableDTO.Code)) return BadRequest();
				var user = _userRepository.GetUserById(id);
				if (user == null) return BadRequest();
				if (!user.TwoFactorAuthentication) 
					return Conflict("This account is not using Two Factor Authentication!");

				var secretKey = _twoFactorAuthenticationRepository.Get2FAKey(id);
				if (secretKey == null) return BadRequest();

				bool isPinvalid = _twoFactorAuthenticationService
					.ValidateTwoFactorPin(secretKey, user2FARequestDisableDTO.Code);
				if (!isPinvalid) return Conflict("Code is invalid!");

				_userRepository.Update2FA(id);
				_twoFactorAuthenticationRepository.Delete2FAKey(id);

				return Ok();
			}
			catch (Exception ex)
			{
				return StatusCode(500, ex.Message);
			}
		}
		#endregion

		#region Get users by conditions
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
		#endregion

		#region Get user by Id, Access token (for authentication)
		[Authorize]
		[HttpGet("GetUser/{id}")]
		public IActionResult GetUserForAuth(int id)
		{
			if (id == 0) return BadRequest();
			try
			{
				var user = _userRepository.GetUserById(id);
				if (user == null) return NotFound();

				string accessToken = Util.Instance.GetAccessToken(HttpContext);

				var userIdInAccessToken = _jwtTokenService.GetUserIdByAccessToken(accessToken);
				if (user.UserId != userIdInAccessToken) return NotFound();

				return Ok(_mapper.Map<UserResponeDTO>(user));
			}
			catch (Exception)
			{
				return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred on the server");
			}
		}
		#endregion

		#region Get user by Id
		[Authorize]
		[HttpGet("GetUserById/{id}")]
		public IActionResult GetUserById(int id)
		{
			if (id == 0) return BadRequest();
			try
			{
				var user = _userRepository.GetUserById(id);
				if (user == null) return NotFound();
				return Ok(_mapper.Map<UserResponeDTO>(user));
			}
			catch (Exception)
			{
				return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred on the server");
			}
		}
		#endregion

		#region Edit user info
		[Authorize]
		[HttpPut("EditUserInfo/{id}")]
		public async Task<IActionResult> EditUserInfo(int id, UserRequestDTO userRequestDTO)
		{
			if(userRequestDTO == null)	return BadRequest();
			try
			{
				User? user = _userRepository.GetUserById(id);
				if (user == null) return NotFound();

				//Just for testing notification
				if (userRequestDTO.Status != 1)
				{
					HashSet<string>? connections = _connectionManager.GetConnections(id);
					Notification notification = new Notification()
					{
						UserId = id,
						Title = "Change status account",
						Content = $"You account has been Ban",
						Link = "",
						DateCreated = DateTime.Now,
						IsReaded = false,
					};

					if (connections != null)
					{
						foreach (var connection in connections)
						{
							await _notificationHubContext.Clients.Clients(connection).SendAsync("ReceiveNotification",
								JsonConvert.SerializeObject(_mapper.Map<NotificationRespone>(notification)));
						}
					}
					_notificationRepositiory.AddNotification(notification);

				}
				var userUpdate = _mapper.Map<User>(userRequestDTO);	
				await _userRepository.EditUserInfo(id, userUpdate);
				return NoContent();
			}
			catch(Exception ex) 
			{
				var x = ex;
				return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred on the server");
			}
		}
		#endregion

	}
}
