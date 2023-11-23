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
	using global::Comons;
	using static QRCoder.PayloadGenerator;
	using Google.Apis.Auth;
    using DTOs.Seller;
	using DataAccess.Repositories;
	using DTOs.Admin;
    using Azure.Core;

	[Route("api/[controller]")]
	[ApiController]
	public class UsersController : ControllerBase
	{
		private readonly IUserRepository _userRepository;
		private readonly IRefreshTokenRepository _refreshTokenRepository;
		private readonly IAccessTokenRepository _accessTokenRepository;
		private readonly IMapper _mapper;
		private readonly ITwoFactorAuthenticationRepository _twoFactorAuthenticationRepository;

		private readonly JwtTokenService _jwtTokenService;
		private readonly TwoFactorAuthenticationService _twoFactorAuthenticationService;
		private readonly MailService _mailService;
        private readonly StorageService _storageService;

        public UsersController(IUserRepository userRepository, IMapper mapper,
			IRefreshTokenRepository refreshTokenRepository,
			IAccessTokenRepository accessTokenRepository,
			ITwoFactorAuthenticationRepository twoFactorAuthenticationRepository,
			JwtTokenService jwtTokenService,
			TwoFactorAuthenticationService twoFactorAuthenticationService,
			MailService mailService,
            StorageService storageService
            )
		{
			_userRepository = userRepository;
			_mapper = mapper;
			_refreshTokenRepository = refreshTokenRepository;
			_accessTokenRepository = accessTokenRepository;
			_jwtTokenService = jwtTokenService;
			_twoFactorAuthenticationService = twoFactorAuthenticationService;
			_twoFactorAuthenticationRepository = twoFactorAuthenticationRepository;
			_mailService = mailService;
			_storageService = storageService;

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

		#region SignIn Google
		[HttpPost("SignInGoogle")]
		public async Task<IActionResult> SignInGoogle([FromBody] UserSignInGoogleRequestDTO request)
		{
			if (string.IsNullOrWhiteSpace(request.GToken))
			{
				return UnprocessableEntity();
			}
			try
			{
				GoogleJsonWebSignature.Payload payload = await GoogleJsonWebSignature.ValidateAsync(request.GToken);
				if (payload == null)
				{
					return Conflict();
				}

				User? user = _userRepository.GetUserByEmail(payload.Email);

				if (user == null)
				{
					User newUser = new User
					{
						Email = payload.Email,
						TwoFactorAuthentication = false,
						RoleId = Constants.CUSTOMER_ROLE,
						Username = _userRepository.GenerateRandomUsername(payload.Email),
						SignInGoogle = true,
						Status = true,
						IsConfirm = true,
						Fullname = payload.Name,
						IsChangeUsername = false
					};
					_userRepository.AddUser(newUser);
					user = _userRepository.GetUserByEmail(payload.Email);
				}
				else
				{
					if (!user.Status) return Conflict("Your account was baned!");
					if (user.TwoFactorAuthentication)
						return StatusCode(StatusCodes.Status416RangeNotSatisfiable, user.UserId);
				}

				var token = await _jwtTokenService.GenerateTokenAsync(user);
				return Ok(token);
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
				bool isExistUsernameOrEmail = _userRepository.IsExistUsernameOrEmail(request.Username.ToLower(), request.Email.ToLower());
				if (isExistUsernameOrEmail)
				{
					return Conflict();
				}
				User userSignUp = new User
				{
					Email = request.Email,
					Password = request.Password,
					Fullname = request.Fullname,
					RoleId = Constants.CUSTOMER_ROLE,
					SignInGoogle = false,
					Status = true,
					Username = request.Username,
					Avatar = "",
					AccountBalance = 0,
					TwoFactorAuthentication = false,
					IsConfirm = false,
					IsChangeUsername = false
				};
				_userRepository.AddUser(userSignUp);

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
		public IActionResult ConfirmEmail(string token)
		{
			try
			{
				bool result = _jwtTokenService.ValidateTokenConfirmEmail(token);
				return Ok(new ResponseData(result ? Constants.RESPONSE_CODE_SUCCESS : Constants.RESPONSE_CODE_FAILD,
					result ? "Success" : "Invalid", result, new()));
			}
			catch (Exception e)
			{
				return Ok(new ResponseData(Constants.RESPONSE_CODE_FAILD, e.Message, false, new()));
			}

		}
		#endregion

		#region reset password
		[HttpGet("ResetPassword/{email}")]
		public async Task<IActionResult> ResetPassword(string email)
		{
			if (string.IsNullOrWhiteSpace(email))
			{
				return Ok(new ResponseData(Constants.RESPONSE_CODE_NOT_ACCEPT, "Invalid", false, new()));
			}
			else
			{
				try
				{
					User? user = _userRepository.GetUserByEmail(email.Trim());
					if (user == null)
					{
						return Ok(new ResponseData(Constants.RESPONSE_CODE_DATA_NOT_FOUND, "Not found", false, new()));
					}
					if (!user.IsConfirm)
					{
						return Ok(new ResponseData(Constants.RESPONSE_CODE_RESET_PASSWORD_NOT_CONFIRM, "Not confirm email", false, new()));
					}
					if (string.IsNullOrEmpty(user.Username))
					{
						return Ok(new ResponseData(Constants.RESPONSE_CODE_RESET_PASSWORD_SIGNIN_GOOGLE, "Username not register", false, new()));
					}
					string newPassword = Util.Instance.RandomPassword8Chars();
					string passwordHash = Util.Instance.Sha256Hash(newPassword);
					user.Password = passwordHash;
					_userRepository.UpdateUser(user);
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
			User? user = _userRepository.GetUserByEmail(email);
			if (user == null)
			{
				return Ok(new ResponseData(Constants.RESPONSE_CODE_DATA_NOT_FOUND, "Not found", false, new()));
			}
			else
			{
				if (user.IsConfirm)
				{
					return Ok(new ResponseData(Constants.RESPONSE_CODE_CONFIRM_PASSWORD_IS_CONFIRMED, "Not confirm email", false, new()));
				}
				else
				{
					string token = _jwtTokenService.GenerateTokenConfirmEmail(user);
					await _mailService.SendEmailAsync(user.Email, "DigitalFUHub: Xác nhận đăng ký tài khoản.", $"<a href='http://localhost:3000/confirmEmail?token={token}'>Nhấn vào đây để xác nhận.</a>");
				}
			}
			return Ok(new ResponseData(Constants.RESPONSE_CODE_SUCCESS, "Success", true, new()));
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
				string body = $"<html><body><p>Here's an image:</p> <img src='{qrCode}'/></body></html>";


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

				string accessToken = Util.GetAccessToken(HttpContext);

				var userIdInAccessToken = _jwtTokenService.GetUserIdByAccessToken(User);
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

				string username = user.Email.Split('@')[0];
				user.Email = Util.HideCharacters(user.Email, 5);

				return Ok(_mapper.Map<UserResponeDTO>(user));
			}
			catch (Exception ex)
			{
				return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
			}
		}
		#endregion

		#region Get online status user
		[Authorize]
		[HttpGet("GetOnlineStatusUser/{id}")]
		public IActionResult GetOnlineStatusUser(int id)
		{
			if (id == 0) return BadRequest();
			try
			{
				var user = _userRepository.GetUserById(id);
				if (user == null) return NotFound();


				return Ok(_mapper.Map<UserOnlineStatusResponseDTO>(user));
			}
			catch (Exception ex)
			{
				return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
			}
		}
		#endregion

		#region Get Customer Balance
		[Authorize]
		[HttpGet("GetCustomerBalance/{userId}")]
		public IActionResult GetCustomerBalance(int userId)

		{
			ResponseData response = new ResponseData();
			try
			{
				if (userId == 0)
				{
					response.Status.Ok = false;
					response.Status.Message = "Invalid";
					response.Status.ResponseCode = Constants.RESPONSE_CODE_NOT_ACCEPT;
					return Ok(response);
				}

				var user = _userRepository.GetUserById(userId);

				if (user == null)
				{
					response.Status.ResponseCode = Constants.RESPONSE_CODE_DATA_NOT_FOUND;
					response.Status.Ok = false;
					response.Status.Message = "Data not found!";
					return Ok(response);
				}

				response.Status.ResponseCode = Constants.RESPONSE_CODE_SUCCESS;
				response.Status.Message = "Success";
				response.Status.Ok = true;
				response.Result = user.AccountBalance;
			}
			catch (Exception)
			{
				response.Status.ResponseCode = Constants.RESPONSE_CODE_NOT_ACCEPT;
				response.Status.Ok = false;
				response.Status.Message = "Invalid";
				return Ok(response);
			}
			return Ok(response);
		}
		#endregion

		#region Edit user info
		[Authorize]
		[HttpPut("EditUserInfo")]
		public async Task<IActionResult> EditUserInfo([FromForm] UserUpdateRequestDTO request)
		{
            ResponseData responseData = new ResponseData();
            Status status = new Status();

            try
			{
                if (request.UserId != _jwtTokenService.GetUserIdByAccessToken(User))
                {
                    return Unauthorized();
                }

                if (request == null)
				{
                    status.ResponseCode = Constants.RESPONSE_CODE_NOT_ACCEPT;
                    status.Message = "request invalid!";
                    status.Ok = false;
                    responseData.Status = status;
                    return Ok(responseData);
                }

                User? user = _userRepository.GetUserById(request.UserId);

				if (user == null)
				{
                    status.ResponseCode = Constants.RESPONSE_CODE_DATA_NOT_FOUND;
                    status.Message = "user not found";
                    status.Ok = false;
                    responseData.Status = status;
                    return Ok(responseData);
                }

                var userUpdate = _mapper.Map<User>(request);

                // Declares
                string urlNewAvatar = "";
                string filename = "";
                DateTime now;
				//

                // Check update avatar user or not
                if (request.Avatar != null)
                {
                    now = DateTime.Now;
                    filename = string.Format("{0}_{1}{2}{3}{4}{5}{6}{7}{8}", request.UserId, now.Year, now.Month,
                        now.Day, now.Millisecond, now.Second, now.Minute, now.Hour,
                        request.Avatar.FileName.Substring(request.Avatar.FileName.LastIndexOf(".")));

                    urlNewAvatar = await _storageService.UploadFileToAzureAsync(request.Avatar, filename);
					userUpdate.Avatar = urlNewAvatar;

                    // delete avatar old
                    await _storageService.RemoveFileFromAzureAsync(user.Avatar.Substring(user.Avatar.LastIndexOf("/") + 1));
                }

				// Ok
				_userRepository.EditUserInfo(userUpdate);
                status.ResponseCode = Constants.RESPONSE_CODE_SUCCESS;
                status.Message = "Success";
                status.Ok = true;
                responseData.Status = status;
                return Ok(responseData);
            }
			catch (Exception ex)
			{
				return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
			}
		}
		#endregion

		#region Get Coin
		[Authorize]
		[HttpGet("GetCoin/{userId}")]
		public IActionResult UpdateBalance(long userId)
		{
			ResponseData response = new ResponseData();
			try
			{
				if (userId == 0)
				{
					response.Status.Ok = false;
					response.Status.Message = "Invalid";
					response.Status.ResponseCode = Constants.RESPONSE_CODE_NOT_ACCEPT;
					return Ok(response);
				}

				var user = _userRepository.GetUserById(userId);

				if (user == null)
				{
					response.Status.ResponseCode = Constants.RESPONSE_CODE_DATA_NOT_FOUND;
					response.Status.Ok = false;
					response.Status.Message = "Data not found!";
					return Ok(response);
				}

				response.Status.ResponseCode = Constants.RESPONSE_CODE_SUCCESS;
				response.Status.Message = "Success";
				response.Status.Ok = true;
				response.Result = new UserCoinResponseDTO { Coin = user.Coin };
			}
			catch (Exception)
			{
				response.Status.ResponseCode = Constants.RESPONSE_CODE_NOT_ACCEPT;
				response.Status.Ok = false;
				response.Status.Message = "Invalid";
				return Ok(response);
			}
			return Ok(response);
		}
		#endregion

		#region Check Exist Email
		[HttpGet("IsExistEmail/{email}")]
		public IActionResult CheckExistEmail(string email)
		{
			if (string.IsNullOrWhiteSpace(email))
			{
				return Ok(new ResponseData(Constants.RESPONSE_CODE_NOT_ACCEPT, "Invalid", false, new()));
			}
			User? user = _userRepository.GetUserByEmail(email);
			if (user == null)
			{
				return Ok(new ResponseData(Constants.RESPONSE_CODE_SUCCESS, "Success", true, new()));
			}
			return Ok(new ResponseData(Constants.RESPONSE_CODE_NOT_ACCEPT, "Invalid", false, new()));
		}
		#endregion

		#region Check Exist Username
		[HttpGet("IsExistUsername/{username}")]
		public IActionResult CheckExistUsername(string username)
		{
			if (string.IsNullOrWhiteSpace(username))
			{
				return Ok(new ResponseData(Constants.RESPONSE_CODE_NOT_ACCEPT, "Invalid", false, new()));
			}
			User? user = _userRepository.GetUserByUsername(username);
			if (user == null)
			{
				return Ok(new ResponseData(Constants.RESPONSE_CODE_SUCCESS, "Success", true, new()));
			}
			return Ok(new ResponseData(Constants.RESPONSE_CODE_NOT_ACCEPT, "Invalid", false, new()));
		}
		#endregion

		#region Get all users for admin
		[Authorize(Roles = "Admin")]
		[HttpPost("GetUsers")]
		public IActionResult GetUsers(UsersRequestDTO requestDTO)
		{

			if (requestDTO == null || requestDTO.Email == null ||
				requestDTO.FullName == null || requestDTO.UserId == null)
			{
				return BadRequest();
			}

			ResponseData responseData = new ResponseData();
			Status status = new Status();

			try
			{
				long userId = 0;
				long.TryParse(requestDTO.UserId, out userId);

				var users = _userRepository.GetUsers(userId, requestDTO.Email, requestDTO.FullName, requestDTO.RoleId, requestDTO.Status);
				var result = _mapper.Map<List<UsersResponseDTO>>(users);

				status.Message = "Success!";
				status.Ok = true;
				status.ResponseCode = Constants.RESPONSE_CODE_SUCCESS;
				responseData.Status = status;
				responseData.Result = result;
				return Ok(responseData);
			}
			catch (Exception ex)
			{
				return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
			}
		}
		#endregion

		#region Get user info by Id for admin
		[Authorize(Roles = "Admin")]
		[HttpPost("GetUser/{id}")]
		public IActionResult GetUser(int id)
		{
			if (id == 0) return BadRequest();
			ResponseData responseData = new ResponseData();

			try
			{


				responseData.Status.ResponseCode = Constants.RESPONSE_CODE_SUCCESS;
				responseData.Status.Ok = true;
				responseData.Status.Message = "Success!";

				return Ok(responseData);
			}
			catch (Exception ex)
			{
				return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
			}
		}
		#endregion

	}
}
