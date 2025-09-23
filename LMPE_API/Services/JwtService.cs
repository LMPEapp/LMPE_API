using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace LMPE_API.Services
{
    public class JwtService
    {
        private readonly string _secret;
        private readonly int _expireHours;

        public JwtService(IConfiguration config)
        {
            _secret = config["Jwt:Secret"]!;
            _expireHours = int.Parse(config["Jwt:ExpireHours"]!);
        }

        public string GenerateToken(long userId, bool isAdmin, int? expireMinutes = null)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secret));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var claims = new[]
            {
                new Claim("id", userId.ToString()),
                new Claim("isAdmin", isAdmin.ToString())
            };
            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expireMinutes ?? (_expireHours * 60)),
                signingCredentials: creds
            );
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
