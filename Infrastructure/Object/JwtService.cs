using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace InfrastructureBase.Object
{
    public class JwtService
    {
        public static SymmetricSecurityKey NewSymmetricSecurityKey() => new SymmetricSecurityKey(Encoding.ASCII.GetBytes(SecretKey));
        private const string SecretKey = "B2PnmAXMuxHE44ujkxxWLYIS4epExsGVwNouVpsmvjttIWx3dYo8MGKXzjM5EOJmD9YPDMvf88Iy45ktWUvhy94r6ykNXCKEUTUhTqizTTCgS3k7hlQggauMDSYmCl0WKCH7WzULLvBar1QVJ0MEH27b6qMfnTKThO7ifksz4MeKwl5fYBllKtVYa6yoduFzAy6H6Ax97ZCHdyceuouLG9iVW0whN8CIQgIc26A0a5V2c3T4tWvwlKHh0HqFDM6F"; // 长度至少为256位
        public static string GenerateToken(int userid, string username, int expireMinutes = 365)
        {
            var _signingKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(SecretKey));
            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, userid.ToString()),
                    new Claim(ClaimTypes.Name, username)
                }),
                Expires = DateTime.UtcNow.AddDays(expireMinutes),
                SigningCredentials = new SigningCredentials(_signingKey, SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
