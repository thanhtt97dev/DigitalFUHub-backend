using BusinessObject;
using DataAccess.IRepositories;
using DTOs;
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
		private static JwtTokenService? instance;
		private static readonly object instanceLock = new object();

		public static JwtTokenService Instance
		{
			get
			{
				lock (instanceLock)
				{
					if (instance == null)
					{
						instance = new JwtTokenService();
					}
				}
				return instance;
			}
		}

		public async Task<UserSignInResponseDTO> GenerateTokenAsync(User user, IRefreshTokenRepository refreshTokenRepository, IConfiguration configuration)
		{
			var tokenHandler = new JwtSecurityTokenHandler();
			var secretKeyBytes = Encoding.UTF8.GetBytes(configuration["JWT:Secret"] ?? string.Empty);
			var signingCredentials =
				new SigningCredentials(new SymmetricSecurityKey(secretKeyBytes), SecurityAlgorithms.HmacSha512Signature);

			var claims = new[]
			{
				new Claim(ClaimTypes.Name, user.Email ?? string.Empty),
				new Claim(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
				new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
				new Claim("Id", user.UserId.ToString()),
				new Claim(ClaimTypes.Role, (user.Role == null ? string.Empty : user.Role.RoleName) ?? string.Empty),
			};

			var tokenDescription = new SecurityTokenDescriptor
			{
				Subject = new ClaimsIdentity(claims),
				Expires = DateTime.UtcNow.AddMinutes(int.Parse(configuration["JWT:TokenAge"] ?? string.Empty)),
				SigningCredentials = signingCredentials
			};

			var token = tokenHandler.CreateToken(tokenDescription);
			var accessToken = tokenHandler.WriteToken(token);


			var refreshToken = GenerateRefreshToken();
			var refreshTokenModel = new RefreshToken
			{
				UserId = user.UserId,
				TokenRefresh = refreshToken,
				JwtId = token.Id,
				ExpiredDate = DateTime.UtcNow.AddMinutes(int.Parse(configuration["JWT:RefreshTokenAge"] ?? string.Empty)),
			};

			await refreshTokenRepository.AddRefreshTokenAsync(refreshTokenModel);
			var response = new UserSignInResponseDTO
			{
				UserId = user.UserId,
				Email = user.Email,
				RoleName = user.Role == null ? string.Empty : user.Role.RoleName,
				AccessToken = accessToken,
				RefreshToken = refreshToken
			};
			return response;
		}

		private string GenerateRefreshToken()
		{
			var random = new byte[32];
			using (var rng = RandomNumberGenerator.Create())
			{
				rng.GetBytes(random);

				return Convert.ToBase64String(random);
			}
		}

		internal async Task<bool> IsValidRefreshTokenAsync(string? accessToken, string? refreshTokenKey, IRefreshTokenRepository refreshTokenRepository, IConfiguration configuration)
		{
			try
			{
				JwtSecurityTokenHandler jwtSecurityTokenHandler = new JwtSecurityTokenHandler();
				var secretKey = Encoding.UTF8.GetBytes(configuration["JWT:Secret"] ?? string.Empty);

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

				if (validatedToken is JwtSecurityToken jwtSecurityToken)
				{
					bool result = jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha512,
						StringComparison.InvariantCultureIgnoreCase);
					if (!result) return false;
				}

				// check accessToken expired 
				long utcexpireDate = long.Parse(tokenVerification.Claims.First(x => x.Type == JwtRegisteredClaimNames.Exp).Value);
				DateTime expireDate = Util.Instance.ConvertUnitTimeToDateTime(utcexpireDate);
				if (expireDate < DateTime.UtcNow)
				{
					return false;
				}

				// check refreshToken exist in DB
				var refreshToken = await refreshTokenRepository.GetRefreshToken(refreshTokenKey);
				if (refreshToken == null) return false;

				// check refreshToken expired
				if (refreshToken.ExpiredDate < DateTime.UtcNow)
				{
					return false;
				}

				// check accessToken id equal jwtId of refreshToken
				var jti = tokenVerification.Claims.First(x => x.Type == JwtRegisteredClaimNames.Jti).Value;
				if (refreshToken.JwtId != jti) return false;
			}
			catch (ArgumentException)
			{
				return false;
			}

			return true;
		}


	}
}
