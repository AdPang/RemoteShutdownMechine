using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace WebHost.Services
{
    /// <summary>
    /// 签发与校验 JWT。密钥、颁发者、受众、有效期均来自 appsettings.json 的 "Jwt" 节。
    /// </summary>
    public class JwtService
    {
        private readonly IConfiguration _config;

        public JwtService(IConfiguration config)
        {
            _config = config;
        }

        private string Secret => _config["Jwt:Secret"] ?? "LanShutdownDefaultSecretChangeMe";
        private string Issuer => _config["Jwt:Issuer"] ?? "LanShutdownWebHost";
        private string Audience => _config["Jwt:Audience"] ?? "LanShutdownApp";

        public int ExpiryMinutes
        {
            get
            {
                if (int.TryParse(_config["Jwt:ExpiryMinutes"], out var m) && m > 0)
                {
                    return m;
                }

                return 1440;
            }
        }

        /// <summary>为指定用户名签发一份访问令牌。</summary>
        public string GenerateToken(string username)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Secret));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var claims = new[] { new Claim(ClaimTypes.Name, username) };

            var token = new JwtSecurityToken(
                issuer: Issuer,
                audience: Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(ExpiryMinutes),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
