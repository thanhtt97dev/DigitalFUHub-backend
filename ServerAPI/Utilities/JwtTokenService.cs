using BusinessObject;
using DataAccess.IRepositories;
using DataAccess.Repositories;
using DTOs;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using ServerAPI.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace ServerAPI.Services
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
				new Claim(ClaimTypes.Name, user.Email ?? string.Empty),
				new Claim(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
				new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
				new Claim(JwtRegisteredClaimNames.Sub, user.UserId.ToString()),
				new Claim(ClaimTypes.Role, (user.Role == null ? string.Empty : user.Role.RoleName) ?? string.Empty),
			};

			int tokenExpiredDate;
			int.TryParse(_configuration["JWT:TokenAge"], out tokenExpiredDate);
			var accessTokenExpiredDate = DateTime.UtcNow.AddDays(tokenExpiredDate);

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

			//Create Refresh token
			var refreshToken = GenerateRefreshToken();
			var refreshTokenModel = new RefreshToken
			{
				UserId = user.UserId,
				TokenRefresh = refreshToken,
				JwtId = token.Id,
				ExpiredDate = DateTime.UtcNow.AddDays(tokenExpiredDate + 1),
			};

			//Add refresh token to DB
			await _refreshTokenRepository.AddRefreshTokenAsync(refreshTokenModel);

			var response = new UserSignInResponseDTO
			{
				UserId = user.UserId,
				Email = user.Email,
				RoleName = user.Role == null ? string.Empty : user.Role.RoleName,
				JwtId = token.Id,
				AccessToken = accessToken,
				RefreshToken = refreshToken,
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

			var tokenValidationParameters = new TokenValidationParameters
			{
				ValidateIssuer = false,
				ValidateAudience = false,

				ValidateIssuerSigningKey = true,
				IssuerSigningKey = new SymmetricSecurityKey(secretKey),

				/*
				ValidateIssuer = true,
				ValidIssuer = configuration["JWT:Issuer"],
				ValidateAudience = true,
				ValidAudience = configuration["JWT:Audience"],
				*/

				ClockSkew = TimeSpan.FromMinutes(3),
				ValidateLifetime = false
			};

			ClaimsPrincipal tokenVerification = jwtSecurityTokenHandler
				.ValidateToken(accessToken, tokenValidationParameters, out var validatedToken);

			var jti = tokenVerification.Claims.First(x => x.Type == JwtRegisteredClaimNames.Jti).Value;

			return jti;
		}
		#endregion

	}
}
