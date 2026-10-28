using System.IdentityModel.Tokens.Jwt;
using Letter_Request_Management_System.Models.Domain;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;

namespace Letter_Request_Management_System.Services
{
    public class JwtService
    {
        private readonly IConfiguration _configuration;

        public JwtService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public DateTime GetExpiration()
        {
            int expireMinutes = int.Parse(
                _configuration["Jwt:ExpireMinutes"] ?? "60"
            );

            return DateTime.UtcNow.AddMinutes(expireMinutes);
        }

        public string GenerateToken(UserMl user, string selectedRole)
        {
            string issuer = _configuration["Jwt:Issuer"]
                ?? throw new InvalidOperationException(
                    "JWT issuer is not configured.");

            string audience = _configuration["Jwt:Audience"]
                ?? throw new InvalidOperationException(
                    "JWT audience is not configured.");

            string secret = _configuration["Jwt:Key"]
                ?? throw new InvalidOperationException(
                    "JWT signing key is not configured.");

            // Ensure the selected role belongs to this user.
            bool hasRole = user.Roles.Exists(role =>
                role.Equals(
                    selectedRole,
                    StringComparison.OrdinalIgnoreCase));

            if (!hasRole)
            {
                throw new InvalidOperationException("The selected role is not assigned to this user.");
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier,user.UserId.ToString()),
                new Claim(ClaimTypes.Email,user.Email),
                new Claim(ClaimTypes.Name,user.FullName),
                new Claim(ClaimTypes.Role,selectedRole)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
            var credentials = new SigningCredentials(key,SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: GetExpiration(),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
