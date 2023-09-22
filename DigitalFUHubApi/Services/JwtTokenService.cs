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

		#region Generate token
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
