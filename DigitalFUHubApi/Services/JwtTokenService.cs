using DataAccess.IRepositories;
using DataAccess.Repositories;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using DigitalFUHubApi.Comons;
using DigitalFUHubApi.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using BusinessObject.Entities;
using DTOs.User;
using Quartz.Util;

namespace DigitalFUHubApi.Services
{
	public class JwtTokenService
	{
		private readonly IConfiguration _configuration;
		private readonly IUserRepository _userRepository;
		private readonly IRefreshTokenRepository _refreshTokenRepository;
		private readonly IAccessTokenRepository _accessTokenRepository;

		public JwtTokenService(IConfiguration configuration, IUserRepository userRepository, IRefreshTokenRepository refreshTokenRepository, IAccessTokenRepository accessTokenRepository)
		{
			_configuration = configuration;
			_userRepository = userRepository;
			_refreshTokenRepository = refreshTokenRepository;
			_accessTokenRepository = accessTokenRepository;
		}

		#region Generate access token
		public async Task<UserSignInResponseDTO> GenerateTokenAsync(User user)
		{
			//Create access token
			var tokenHandler = new JwtSecurityTokenHandler();
			var secretKeyBytes = Encoding.UTF8.GetBytes(_configuration["JWT:Secret"] ?? string.Empty);
			var signingCredentials =
				new SigningCredentials(new SymmetricSecurityKey(secretKeyBytes), SecurityAlgorithms.HmacSha512Signature);

			var claims = new[]
			{
				new Claim(ClaimTypes.Name, user.Username ?? string.Empty),
				new Claim(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
				new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
				new Claim(JwtRegisteredClaimNames.Sub, user.UserId.ToString()),
				new Claim(ClaimTypes.Role, (user.Role == null ? string.Empty : user.Role.RoleName) ?? string.Empty),
			};

			//rule: Admin use  token-base authentication(session), other roles use cookie-base authentication
			// settings token expired time for specific user's role
			int tokenExpiredDate;
			int.TryParse(_configuration["JWT:TokenAge"], out tokenExpiredDate);
			DateTime accessTokenExpiredDate = DateTime.UtcNow;
			if (user.RoleId == Constants.ADMIN_ROLE)
			{
				accessTokenExpiredDate = DateTime.UtcNow.AddMinutes(tokenExpiredDate);
			}
			else
			{
				accessTokenExpiredDate = DateTime.UtcNow.AddDays(tokenExpiredDate);
			}

			var tokenDescription = new SecurityTokenDescriptor
			{
				Subject = new ClaimsIdentity(claims),
				Expires = accessTokenExpiredDate,
				SigningCredentials = signingCredentials
			};

			var token = tokenHandler.CreateToken(tokenDescription);
			var accessToken = tokenHandler.WriteToken(token);

			//Add access token to DB
			var accessTokenModel = new AccessToken
			{
				UserId = user.UserId,
				JwtId = token.Id,
				Token = accessToken,
				ExpiredDate = accessTokenExpiredDate,
				isRevoked = false,
			};

			await _accessTokenRepository.AddAccessTokenAsync(accessTokenModel);

			//Create Refresh token just for token-base authentication
			string? refreshToken = null;
			if (user.RoleId == Constants.ADMIN_ROLE)
			{
				refreshToken = GenerateRefreshToken();
				int refreshTokenExpiredDate;
				int.TryParse(_configuration["JWT:RefreshTokenAge"], out refreshTokenExpiredDate);
				var refreshTokenModel = new RefreshToken
				{
					UserId = user.UserId,
					TokenRefresh = refreshToken,
					JwtId = token.Id,
					ExpiredDate = DateTime.UtcNow.AddMinutes(refreshTokenExpiredDate),
				};

				//Add refresh token to DB
				await _refreshTokenRepository.AddRefreshTokenAsync(refreshTokenModel);
			}

			var response = new UserSignInResponseDTO
			{
				UserId = user.UserId,
				Email = user.Email,
				RoleName = user.Role == null ? string.Empty : user.Role.RoleName,
				JwtId = token.Id,
				AccessToken = accessToken,
				RefreshToken = refreshToken,
				TwoFactorAuthentication = user.TwoFactorAuthentication,
				Avatar = user.Avatar,
				Fullname = user.Fullname,
				SignInGoogle = user.SignInGoogle,
				Username = user.Username
			};
			return response;
		}
		#endregion

		#region Generate token confirm email
		public async Task<string> GenerateTokenConfirmEmail(User user)
		{
			JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
			byte[] secretKeyBytes = Encoding.UTF8.GetBytes(_configuration["JWT:Secret"] ?? string.Empty);
			SigningCredentials signingCredentials =
				new SigningCredentials(new SymmetricSecurityKey(secretKeyBytes), SecurityAlgorithms.HmacSha512Signature);

			Claim[] claims = new[]
			{
				new Claim(ClaimTypes.Name, user.Username ?? string.Empty),
				new Claim(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
				new Claim(JwtRegisteredClaimNames.GivenName, user.Fullname),
			};

			SecurityTokenDescriptor tokenDescription = new SecurityTokenDescriptor
			{
				Subject = new ClaimsIdentity(claims),
				Expires = DateTime.UtcNow.AddMinutes(15),
				SigningCredentials = signingCredentials
			};

			SecurityToken securityToken = tokenHandler.CreateToken(tokenDescription);
			string token = tokenHandler.WriteToken(securityToken);

			using (ApiContext context = new ApiContext())
			{
				User userUp = context.User.First(x => x.Email == user.Email);
				userUp.IsConfirm = true;
				context.User.Update(userUp);
				await context.SaveChangesAsync();
			}
			return token;
		}
		#endregion

		#region Generate token key
		private string GenerateRefreshToken()
		{
			var random = new byte[32];
			using (var rng = RandomNumberGenerator.Create())
			{
				rng.GetBytes(random);

				return Convert.ToBase64String(random);
			}
		}
		#endregion

		#region Check token valid
		internal bool CheckRefreshTokenIsValid(string? accessToken, string? refreshTokenKey)
		{
			try
			{
				//check access token is revoked was doing in program.cs

				// check refreshToken exist in DB
				var refreshToken = _refreshTokenRepository.GetRefreshToken(refreshTokenKey);
				if (refreshToken == null) return false;

				// check refreshToken expired
				if (refreshToken.ExpiredDate < DateTime.UtcNow)
				{
					return false;
				}

				// check accessToken id equal jwtId of refreshToken
				var jti = GetJwtIdByAccessToken(accessToken);
				if (refreshToken.JwtId != jti) return false;
			}
			catch (Exception)
			{
				return false;
			}

			return true;
		}
		#endregion

		#region Check token confirm email
		internal async Task<bool> CheckTokenConfirmEmailAsync(string token)
		{

			JwtSecurityTokenHandler jwtSecurityTokenHandler = new JwtSecurityTokenHandler();
			byte[] secretKey = Encoding.UTF8.GetBytes(_configuration["JWT:Secret"] ?? "");

			var tokenValidationParams = new JwtValidationParameters()
			{
				ValidateIssuer = false,
				ValidateAudience = false,

				ClockSkew = TimeSpan.Zero,
				ValidateLifetime = false
			};

			ClaimsPrincipal tokenVerification = jwtSecurityTokenHandler
				.ValidateToken(token, tokenValidationParams, out SecurityToken validatedToken);

			if (validatedToken is JwtSecurityToken jwtSecurityToken)
			{
				bool result = jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha512,
					StringComparison.InvariantCultureIgnoreCase);
				if (!result) throw new NullReferenceException("invalid");
			}

			string? username = tokenVerification.Claims.First(x => x.Type == ClaimTypes.Name).Value;
			string? email = tokenVerification.Claims.First(x => x.Type == ClaimTypes.Email).Value;
			string? fullname = tokenVerification.Claims.First(x => x.Type == ClaimTypes.GivenName).Value;
			if (fullname == null || username == null || email == null) throw new NullReferenceException("invalid");
			User? user = await _userRepository.GetUser(email, username, fullname);
			if (user == null) throw new NullReferenceException("invalid");
			if (user.IsConfirm) return false;
			long utcExpireDate = long.Parse(tokenVerification.Claims.First(x => x.Type == JwtRegisteredClaimNames.Exp).Value);
			DateTime expireDate = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
			expireDate = expireDate.AddSeconds(utcExpireDate).ToUniversalTime();

			if (expireDate < DateTime.UtcNow) throw new ArgumentOutOfRangeException("expired");


			return true;
		}
		#endregion

		#region Get JwtId by access token
		internal string GetJwtIdByAccessToken(string? accessToken)
		{
			if (string.IsNullOrEmpty(accessToken)) throw new NullReferenceException(nameof(accessToken));

			JwtSecurityTokenHandler jwtSecurityTokenHandler = new JwtSecurityTokenHandler();
			var secretKey = Encoding.UTF8.GetBytes(_configuration["JWT:Secret"] ?? string.Empty);

			var tokenValidationParameters = new JwtValidationParameters();

			ClaimsPrincipal tokenVerification = jwtSecurityTokenHandler
				.ValidateToken(accessToken, tokenValidationParameters, out var validatedToken);

			var jti = tokenVerification.Claims.First(x => x.Type == JwtRegisteredClaimNames.Jti).Value;

			return jti;
		}
		#endregion

		#region Get UserId by access token
		internal int GetUserIdByAccessToken(string? accessToken)
		{
			if (string.IsNullOrEmpty(accessToken)) throw new NullReferenceException(nameof(accessToken));

			JwtSecurityTokenHandler jwtSecurityTokenHandler = new JwtSecurityTokenHandler();
			var secretKey = Encoding.UTF8.GetBytes(_configuration["JWT:Secret"] ?? string.Empty);

			var tokenValidationParameters = new JwtValidationParameters();

			ClaimsPrincipal tokenVerification = jwtSecurityTokenHandler
				.ValidateToken(accessToken, tokenValidationParameters, out var validatedToken);

			var userIdRaw = tokenVerification.Claims.First(x => x.Type == ClaimTypes.NameIdentifier).Value; //Sub
			int userId;
			int.TryParse(userIdRaw, out userId);
			return userId;
		}
		#endregion

	}
}
