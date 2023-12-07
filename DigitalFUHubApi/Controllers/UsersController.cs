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
	using Quartz.Util;
	using System.Text.RegularExpressions;

	[Route("api/[controller]")]
	[ApiController]
	public class UsersController : ControllerBase
	{
		private readonly IUserRepository _userRepository;
		private readonly IRefreshTokenRepository _refreshTokenRepository;
		private readonly IAccessTokenRepository _accessTokenRepository;
		private readonly IConfiguration _configuration;
		private readonly IMapper _mapper;
		private readonly ITwoFactorAuthenticationRepository _twoFactorAuthenticationRepository;

		private readonly JwtTokenService _jwtTokenService;
		private readonly TwoFactorAuthenticationService _twoFactorAuthenticationService;
		private readonly MailService _mailService;
		private readonly AzureFilesService _azureFilesService;

		public UsersController(IUserRepository userRepository, IMapper mapper,
			IConfiguration configuration,
			IRefreshTokenRepository refreshTokenRepository,
			IAccessTokenRepository accessTokenRepository,
			ITwoFactorAuthenticationRepository twoFactorAuthenticationRepository,
			JwtTokenService jwtTokenService,
			TwoFactorAuthenticationService twoFactorAuthenticationService,
			MailService mailService,
			AzureFilesService azureFilesService
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
			_azureFilesService = azureFilesService;
			_configuration = configuration;

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
						IsChangeUsername = false,
						LastTimeOnline = DateTime.Now,
						IsOnline = false,
						CreateDate = DateTime.Now,
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
					IsChangeUsername = true,
					LastTimeOnline = DateTime.Now,
					IsOnline = false,
					CreateDate = DateTime.Now,
				};
				_userRepository.AddUser(userSignUp);

				string token = _jwtTokenService.GenerateTokenConfirmEmail(userSignUp);
				string html = $"<div>" +
					$"<h3>Xin chào {userSignUp.Fullname},</h3>" +
					$"<div>Chào mừng bạn đến với DigitalFUHub.</div>" +
					$"<div><span>Để có thể truy cập vào ứng dụng vui lòng nhấn vào:</span> <a href='{string.Concat(_configuration["EndpointFE:BaseUrl"], "/confirmEmail?token=", token)}'>tại đây</a></div>" +
					$"<b>Mọi thông tin thắc mắc xin vui lòng liên hệ: digitalfuhub@gmail.com</b>\r\n    " +
					$"</div>";
				await _mailService.SendEmailAsync(userSignUp.Email, "DigitalFUHub: Xác thực đăng ký tài khoản.", html);
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

		#region Reset password
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
					string html = $"<div>" +
						$"<h3>Xin chào ${user.Fullname},</h3>" +
						$"<div>Yêu cầu đặt lại mật khẩu.</div>" +
						$"<div>Mật khẩu mới của bạn là: <b>{newPassword}</b></div>" +
						$"<div>Sau khi đăng nhập vào ứng dụng vui lòng đổi lại mật khẩu mới để đảm bảo tài khoản của bạn được an toàn.</div>" +
						$"<b>Mọi thông tin thắc mắc xin vui lòng liên hệ: digitalfuhub@gmail.com</b>" +
						$"</div>";
					await _mailService.SendEmailAsync(user.Email, "DigitalFUHub: Đặt lại mật khẩu.", html);
				}
				catch (Exception)
				{
					return Conflict();
				}
			}
			return Ok();


		}
		#endregion

		#region Send email confirm
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
					string html = $"<div>" +
					$"<h3>Xin chào {user.Fullname},</h3>" +
					$"<div>Chào mừng bạn đến với DigitalFUHub.</div>" +
					$"<div><span>Để có thể truy cập vào ứng dụng vui lòng nhấn vào:</span> <a href='{string.Concat(_configuration["EndpointFE:BaseUrl"], "/confirmEmail?token=", token)}'>tại đây</a></div>" +
					$"<b>Mọi thông tin thắc mắc xin vui lòng liên hệ: digitalfuhub@gmail.com</b>\r\n    " +
					$"</div>";
					await _mailService.SendEmailAsync(user.Email, "DigitalFUHub: Xác thực đăng ký tài khoản.", html);
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

				(string account, string manualEntryKey) = _twoFactorAuthenticationService.GetAccountManualEntryKey(secretKey, user.Email);

				string title = "DigitalFUHub: Mã QR cho xác thực hai yếu tố.";
				string body = $"<html>" +
					$"<body>" +
					$"<div>Xin chào {user.Fullname},</div>" +
					$"<b>Hướng dẫn:<b/>" +
					$"<p>Bước 1: Mở ứng dụng Google Authenticator<p/>" +
					$"<p>Bước 2: Chọn mục nhập khóa thiết lập<p/>" +
					$"<p>Bước 3: Tài khoản: <b>{_configuration["JWT:Issuer"]}:{user.Email}<b/><p/>" +
					$"<p>Bước 4: Khóa: <b>{manualEntryKey}<b/><p/>" +
					$"<p>Bước 5: Bấm Thêm<p/>" +
					$"<p>Hoàn tất<p/>" +
					$"<i>Vui lòng không gửi thông tin này cho bất cứ ai!</>" +
					$"<div><b>Mọi thông tin thắc mắc xin vui lòng liên hệ: digitalfuhub@gmail.com</b></div>" +
					$"</body>" +
					$"</html>";
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
			try
			{
				if (userId == 0)
				{
					return Ok(new ResponseData(Constants.RESPONSE_CODE_NOT_ACCEPT, "Invalid", false, new()));
				}

				var user = _userRepository.GetUserById(userId);

				if (user == null)
				{
					return Ok(new ResponseData(Constants.RESPONSE_CODE_DATA_NOT_FOUND, "Data not found!", false, new()));
				}
				return Ok(new ResponseData(Constants.RESPONSE_CODE_SUCCESS, "Success", true, user.AccountBalance));
			}
			catch (Exception ex)
			{
				return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
			}
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

					urlNewAvatar = await _azureFilesService.UploadFileToAzureAsync(request.Avatar, filename);
					userUpdate.Avatar = urlNewAvatar;

					// delete avatar old
					await _azureFilesService.RemoveFileFromAzureAsync(user.Avatar.Substring(user.Avatar.LastIndexOf("/") + 1));
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

        #region Edit fullName user
        [Authorize]
        [HttpPut("EditFullNameUser")]
        public IActionResult EditFullNameUser(SettingPersonalUpdateFullNameUserRequestDTO request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.Fullname))
                {
                    return Ok(new ResponseData(Constants.RESPONSE_CODE_NOT_ACCEPT, "request invalid!", false, new()));
                }

                User? user = _userRepository.GetUserById(request.UserId);

                if (user == null)
                {
                    return Ok(new ResponseData(Constants.RESPONSE_CODE_DATA_NOT_FOUND, "user not found", false, new()));
                }

				// update fullname
				user.Fullname = request.Fullname;

                // Ok
                _userRepository.UpdateSettingPersonalInfo(user);
                return Ok(new ResponseData(Constants.RESPONSE_CODE_SUCCESS, "Success", true, new()));
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }
        #endregion

        #region Edit avatar user
        [Authorize]
        [HttpPut("EditAvatarUser")]
        public async Task<IActionResult> EditAvatarUser([FromForm] SettingPersonalUpdateAvatarUserRequestDTO request)
        {
            try
            {
                if (request.Avatar == null)
                {
                    return Ok(new ResponseData(Constants.RESPONSE_CODE_NOT_ACCEPT, "request invalid!", false, new()));
                }

                User? user = _userRepository.GetUserById(request.UserId);

                if (user == null)
                {
                    return Ok(new ResponseData(Constants.RESPONSE_CODE_DATA_NOT_FOUND, "user not found", false, new()));
                }

                // Declares
                string urlNewAvatar = "";
                string filename = "";
                DateTime now;
                //

                // update avatar user
                now = DateTime.Now;
                filename = string.Format("{0}_{1}{2}{3}{4}{5}{6}{7}{8}", request.UserId, now.Year, now.Month,
                    now.Day, now.Millisecond, now.Second, now.Minute, now.Hour,
                    request.Avatar.FileName.Substring(request.Avatar.FileName.LastIndexOf(".")));

                urlNewAvatar = await _azureFilesService.UploadFileToAzureAsync(request.Avatar, filename);
				string urlOld = user.Avatar;
                user.Avatar = urlNewAvatar;

                // delete avatar old
                await _azureFilesService.RemoveFileFromAzureAsync(urlOld.Substring(urlOld.LastIndexOf("/") + 1));

                // Ok
                _userRepository.UpdateSettingPersonalInfo(user);
                return Ok(new ResponseData(Constants.RESPONSE_CODE_SUCCESS, "Success", true, new()));
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
		public IActionResult GetCoin(long userId)
		{
			try
			{
				if (userId == 0)
				{
					return Ok(new ResponseData(Constants.RESPONSE_CODE_NOT_ACCEPT, "Invalid", false, new()));
				}

				var user = _userRepository.GetUserById(userId);

				if (user == null)
				{
					return Ok(new ResponseData(Constants.RESPONSE_CODE_DATA_NOT_FOUND, "Data not found!", false, new()));
				}

				return Ok(new ResponseData(Constants.RESPONSE_CODE_SUCCESS, "Success!", true, new UserCoinResponseDTO { Coin = user.Coin }));
			}
			catch (Exception ex)
			{
				return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
			}
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

		#region Active UserName and Password
		[HttpPost("ActiveUserNameAndPassword")]
		[Authorize]
		public IActionResult ActiveUserNameAndPassword(ActiveUserNameAndPasswordRequestDTO request)
		{
			try
			{
				if (request.UserId != _jwtTokenService.GetUserIdByAccessToken(User))
				{
					return Unauthorized();
				}

				if (!Regex.IsMatch(request.Username, Constants.REGEX_USERNAME_SIGN_UP))
				{
					return Ok(new ResponseData(Constants.RESPONSE_CODE_NOT_ACCEPT, "UserName Invalid", false, new()));
				}

				//if (!Regex.IsMatch(request.Password, Constants.REGEX_PASSWORD_SIGN_UP))
				//{
				//	return Ok(new ResponseData(Constants.RESPONSE_CODE_NOT_ACCEPT, "Password Invalid", false, new()));
				//}

				// check user exists
				var user = _userRepository.GetUserById(request.UserId);

				if (user == null)
				{
					return Ok(new ResponseData(Constants.RESPONSE_CODE_NOT_ACCEPT, "User not found", false, new()));
				}

				if (user.IsChangeUsername)
				{
					return Ok(new ResponseData(Constants.RESPONSE_CODE_NOT_ACCEPT, "This user has an activated username and password", false, new()));
				}

				var userFind = _userRepository.GetUserByUserNameOtherUserId(request.UserId, request.Username);

				// check username exist
				if (userFind != null)
				{
					return Ok(new ResponseData(Constants.RESPONSE_CODE_USER_USERNAME_ALREADY_EXISTS, "UserName already exists", false, new()));
				}

				_userRepository.ActiveUserNameAndPassword(request.UserId, request.Username, request.Password);
				return Ok(new ResponseData(Constants.RESPONSE_CODE_SUCCESS, "Success", true, new()));
			}
			catch (Exception ex)
			{
				return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
			}
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

			try
			{
				long userId = 0;
				long.TryParse(requestDTO.UserId, out userId);

				var users = _userRepository.GetUsers(userId, requestDTO.Email, requestDTO.FullName, requestDTO.RoleId, requestDTO.Status);
				var result = _mapper.Map<List<UsersResponseDTO>>(users);

				return Ok(new ResponseData(Constants.RESPONSE_CODE_SUCCESS, "Success", false, result));
			}
			catch (Exception ex)
			{
				return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
			}
		}
		#endregion

		#region Get user info by Id for admin
		[Authorize("Admin")]
		[HttpGet("Admin/UserInfo/{id}")]
		public IActionResult GetUser(int id)
		{
			try
			{
				if (id == Constants.ADMIN_USER_ID) throw new Exception("Not allowed");
				User? user = _userRepository.GetUserInfo(id);
				if(user == null)
				{
					return Ok(new ResponseData(Constants.RESPONSE_CODE_DATA_NOT_FOUND, "Not found", false, new()));
				}
				return Ok(new ResponseData(Constants.RESPONSE_CODE_SUCCESS, "Success", true, _mapper.Map<UserInfoResponseDTO>(user)));
			}
			catch (Exception ex)
			{
				return BadRequest(ex.Message);
			}
		}
        #endregion

        #region Change Password
        [HttpPost("changePassword")]
        [Authorize]
        public IActionResult ChangePassword(ChangePasswordRequestDTO request)
        {
            try
            {
                if (request.UserId != _jwtTokenService.GetUserIdByAccessToken(User))
                {
                    return Unauthorized();
                }

                //if (!Regex.IsMatch(request.Password, Constants.REGEX_PASSWORD_SIGN_UP))
                //{
                //	return Ok(new ResponseData(Constants.RESPONSE_CODE_NOT_ACCEPT, "Password Invalid", false, new()));
                //}

                // check user exists
                var user = _userRepository.GetUserById(request.UserId);

                if (user == null)
                {
                    return Ok(new ResponseData(Constants.RESPONSE_CODE_DATA_NOT_FOUND, "User not found", false, new()));
                }

                if (!user.IsChangeUsername)
                {
                    return Ok(new ResponseData(Constants.RESPONSE_CODE_USER_USERNAME_PASSWORD_NOT_ACTIVE, "This user has not yet activated their account and password", false, new()));
                }

                if (!user.Password.Equals(request.OldPassword))
                {
                    return Ok(new ResponseData(Constants.RESPONSE_CODE_USER_PASSWORD_OLD_INCORRECT, "The old password is incorrect", false, new()));
                }

				// set new password
				user.Password = request.NewPassword;

                _userRepository.UpdateUser(user);
                return Ok(new ResponseData(Constants.RESPONSE_CODE_SUCCESS, "Success", true, new()));
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }
        #endregion

    }
}
