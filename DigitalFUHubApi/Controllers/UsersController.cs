namespace DigitalFUHubApi.Controllers
{
	using AutoMapper;
	using DataAccess.IRepositories;
	using Microsoft.AspNetCore.Authorization;
	using Microsoft.AspNetCore.Mvc;
	using Microsoft.AspNetCore.SignalR;
	using Newtonsoft.Json;
	using DigitalFUHubApi.Comons;
	using DigitalFUHubApi.Hubs;
	using DigitalFUHubApi.Managers;
	using DigitalFUHubApi.Services;
	using Microsoft.Extensions.Azure;
	using BusinessObject.Entities;
	using DTOs.User;
	using System.Net;

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
		private readonly MailService _mailService;

		public UsersController(IUserRepository userRepository, IMapper mapper,
			IRefreshTokenRepository refreshTokenRepository,
			IAccessTokenRepository accessTokenRepository,
			IHubContext<NotificationHub> notificationHubContext, IConnectionManager connectionManager,
			INotificationRepositiory notificationRepositiory,
			ITwoFactorAuthenticationRepository twoFactorAuthenticationRepository,
			JwtTokenService jwtTokenService,
			TwoFactorAuthenticationService twoFactorAuthenticationService,
			MailService mailService
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
			_mailService = mailService;
		}

		#region SignIn
		[HttpPost("SignIn")]
		public async Task<IActionResult> SignInAsync(UserSignInRequestDTO userSignIn)
		{
			try
			{
				User? user = _userRepository.GetUserByUsernameAndPassword(userSignIn.Username, userSignIn.Password);

				if (user == null) return NotFound("Username or Password not correct!");
				if (!user.Status) return Conflict("Your account was baned!");
				if (!user.IsConfirm) return Conflict("Your account not authenticate email!");
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

		#region SignInGoogle
		[HttpPost("SignInhGoogle")]
		public async Task<IActionResult> SignInGoogleAsync(UserSignInGoogleRequestDTO userSignIn)
		{
			try
			{
				User? user = await _userRepository.GetUserByEmail(userSignIn.Email);

				if (user == null)
				{
					
					User newUser = new User
					{
						Email = userSignIn.Email,
						TwoFactorAuthentication = false,
						RoleId = 2,
						SignInGoogle = true,
						Status = true
					};
					await _userRepository.AddUser(newUser);
					user = await _userRepository.GetUserByEmail(userSignIn.Email);
					var tokenForNewUser = _jwtTokenService.GenerateTokenAsync(newUser);
					return Ok(await tokenForNewUser);
				}

				if (!user.Status) return Conflict("Your account was baned!");
				if (user.TwoFactorAuthentication)
					return StatusCode(StatusCodes.Status416RangeNotSatisfiable, user.UserId);

				var token = _jwtTokenService.GenerateTokenAsync(user);
				return Ok(await token);
			}
			catch (Exception ex)
			{
				return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
			}
		}
		#endregion

		#region SignUp
		[HttpPost]
		[Route("SignUp")]
		public async Task<IActionResult> SignUp([FromBody] UserSignUpRequestDTO request)
		{
			if (!ModelState.IsValid)
			{
				return UnprocessableEntity(ModelState);
			}
			try
			{
				bool isExistUsernameOrEmail = await _userRepository.IsExistUsernameOrEmail(request.Username.ToLower(), request.Email.ToLower());
				if (isExistUsernameOrEmail)
				{
					return Conflict();
				}
				User userSignUp = new User
				{
					Email = request.Email,
					Password = request.Password,
					Fullname = request.Fullname,
					RoleId = 1,
					SignInGoogle = false,
					Status = true,
					Username = request.Username,
					Avatar = "",
					CustomerBalance = 0,
					SellerBalance = 0,
					TwoFactorAuthentication = false,
					IsConfirm = false,
				};
				await _userRepository.AddUser(userSignUp);

				string token = _jwtTokenService.GenerateTokenConfirmEmail(userSignUp);
				await _mailService.SendEmailAsync(userSignUp.Email, "DigitalFUHub: Xác nhận đăng ký tài khoản.", $"<a href='http://localhost:3000/confirmEmail?token={token}'>Nhấn vào đây để xác nhận.</a>");
			}
			catch (Exception)
			{

				return Conflict();
			}
			return Ok();
		}
		#endregion

		#region Confirm Email
		[HttpGet("ConfirmEmail/{token}")]
		public async Task<IActionResult> ConfirmEmail(string token)
		{
			try
			{
				bool result = await _jwtTokenService.CheckTokenConfirmEmailAsync(token);
				return result ? Ok("Y") : Ok("N");
			}
			catch (NullReferenceException)
			{
				return NotFound();
			}
			catch (ArgumentOutOfRangeException)
			{
				return Conflict();
			}
			catch (Exception)
			{
				return Conflict();
			}

		}
		#endregion

		#region reset password
		[HttpGet("ResetPassword/{email}")]
		public async Task<IActionResult> ResetPassword(string email)
		{
			if (string.IsNullOrEmpty(email.Trim()))
			{
				return UnprocessableEntity();
			}
			else
			{
				try
				{
					User? user = await _userRepository.GetUserByEmail(email.Trim());
					if (user == null)
					{
						return NotFound();
					}
					if (!user.IsConfirm)
					{
						return Conflict();
					}
					if (user.SignInGoogle)
					{
						return Conflict();
					}
					string newPassword = Util.Instance.RandomPassword8Chars();
					string passwordHash = Util.Instance.Sha256Hash(newPassword);
					user.Password = passwordHash;
					await _userRepository.UpdateUser(user);
					await _mailService.SendEmailAsync(user.Email, "DigitalFUHub: Đặt lại mật khẩu.", $"<div>Mật khẩu mới: {newPassword}</div>");
				}
				catch (Exception)
				{
					return Conflict();
				}
			}
			return Ok();


		}
		#endregion

		#region Generate token confirm email
		[HttpGet("GenerateTokenConfirmEmail/{email}")]
		public async Task<IActionResult> GenerateTokenConfirmEmail(string email)
		{
			User? user = await _userRepository.GetUserByEmail(email);
			if (user == null)
			{
				return NotFound();
			}
			else
			{
				if (user.IsConfirm)
				{
					return Conflict();
				}
				else
				{
					string token = _jwtTokenService.GenerateTokenConfirmEmail(user);
					await _mailService.SendEmailAsync(user.Email, "DigitalFUHub: Xác nhận đăng ký tài khoản.", $"<a href='http://localhost:3000/confirmEmail?token={token}'>Nhấn vào đây để xác nhận.</a>");
				}
			}
			return Ok();
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
				return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
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
			catch (Exception ex)
			{
				return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
			}
		}

		#endregion

		#region Revoke token
		[Authorize]
		[HttpPost("RevokeToken")]
		public IActionResult RevokeToken([FromBody] string jwtId)
		{
			try
			{
				if (string.IsNullOrEmpty(jwtId)) return BadRequest("Cannot revoke access token!");
				_accessTokenRepository.RevokeToken(jwtId);
				return Ok();
			}
			catch (Exception ex)
			{
				return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
			}
		}
		#endregion

		#region Generate Two Factor Authentication Key
		[Authorize]
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
				return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
			}
		}
		#endregion

		#region Activate Two Factor Authentication
		[Authorize]
		[HttpPost("Activate2Fa/{id}")]
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
				return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
			}
		}
		#endregion

		#region Disable Two Factor Authentication
		[Authorize]
		[HttpPost("Deactivate2Fa/{id}")]
		public IActionResult DisableTwoFactorAuthentication(int id, User2FARequestDisableDTO user2FARequestDisableDTO)
		{
			try
			{
				if (string.IsNullOrEmpty(user2FARequestDisableDTO.Code)) return BadRequest();
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
				return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
			}
		}
		#endregion

		#region Send QRCode Factor Authentication to user's mail
		[HttpPost("Send2FaQrCode/{id}")]
		public async Task<IActionResult> SendTwoFactorAuthenticationQrCode(int id)
		{
			try
			{

				var user = _userRepository.GetUserById(id);
				if (user == null) return BadRequest();
				if (!user.TwoFactorAuthentication)
					return Conflict("This account is not using Two Factor Authentication!");

				var secretKey = _twoFactorAuthenticationRepository.Get2FAKey(id);
				if (secretKey == null) return BadRequest();

				var qrCode = _twoFactorAuthenticationService.GenerateQrCode(secretKey, user.Email);
				string title = "FU-Market: QR Code for Two Factor Authentication";
				string body = $"<html><body><p>Here's an image:</p> <a href='data:image/png;base64,{qrCode}'>click here</a></body></html>";


				await _mailService.SendEmailAsync(user.Email, title, body);

				return Ok();
			}
			catch (Exception ex)
			{
				return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
			}
		}
		#endregion

		#region Get users by conditions
		[Authorize]
		[HttpGet("GetUsers")]
		public IActionResult GetUsersByCondition(int? role, int? status, string email = "")
		{
			if (role == null || status == null) return BadRequest();

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
			catch (Exception ex)
			{
				return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
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

				return Ok(_mapper.Map<UserSignInResponseDTO>(user));
			}
			catch (Exception ex)
			{
				return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
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
			catch (Exception ex)
			{
				return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
			}
		}
		#endregion

		#region Get Customer Balance
		[Authorize]
		[HttpGet("GetCustomerBalance/{id}")]
		public IActionResult GetCustomerBalanceById(int id)
		{
			if (id == 0) return BadRequest();
			try
			{
				var user = _userRepository.GetUserById(id);
				if (user == null) return NotFound();
				return Ok(user.CustomerBalance);
			}
			catch (Exception ex)
			{
				return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
			}
		}
		#endregion

		#region Get Seller Balance
		[Authorize]
		[HttpGet("GetSellerBalance/{id}")]
		public IActionResult GetSellerBalanceById(int id)
		{
			if (id == 0) return BadRequest();
			try
			{
				var user = _userRepository.GetUserById(id);
				if (user == null) return NotFound();
				return Ok(user.SellerBalance);
			}
			catch (Exception ex)
			{
				return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
			}
		}
		#endregion

		#region Edit user info
		[Authorize]
		[HttpPut("EditUserInfo/{id}")]
		public async Task<IActionResult> EditUserInfo(int id, UserUpdateRequestDTO userUpdateRequestDTO)
		{
			if (userUpdateRequestDTO == null) return BadRequest();
			try
			{
				User? user = _userRepository.GetUserById(id);
				if (user == null) return Conflict();
				var userUpdate = _mapper.Map<User>(userUpdateRequestDTO);
				await _userRepository.EditUserInfo(id, userUpdate);
				return NoContent();
			}
			catch (Exception ex)
			{
				return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
			}
		}
		#endregion

		#region Get Check Exist Email
		[HttpGet("CheckExistEmail/{email}")]
		public async Task<IActionResult> CheckExistEmail(string email)
		{
			User? user = await _userRepository.GetUserByEmail(email);
			if (user == null)
			{
				return Ok("N");
			}
			return Ok("Y");
		}
		#endregion

		#region Get Check Exist Username
		[HttpGet("CheckExistUsername/{username}")]
		public async Task<IActionResult> CheckExistUsername(string username)
		{
			User? user = await _userRepository.GetUserByUsername(username);
			if (user == null)
			{
				return Ok("N");
			}
			return Ok("Y");
		}
		#endregion

	}
}
