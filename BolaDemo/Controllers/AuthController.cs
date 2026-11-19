using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using Microsoft.IdentityModel.JsonWebTokens;

namespace BolaDemo.Controllers
{
    [ApiController]
    [Route("api/auth")]

    public class AuthController(IConfiguration config) : ControllerBase
    {
        private static readonly Dictionary<string, (string Password, int UserId)> _users = new()
        {
            { "user1", ("password1", 1) },
            { "user2", ("password2", 2) }
         };
        public record LoginRequest(string Username, string Password);

        [Microsoft.AspNetCore.Authorization.AllowAnonymous] // login must stay open when FallbackPolicy is on
        [HttpPost("login")]
        public IActionResult Login(LoginRequest request)
        {
            if(!_users.TryGetValue(request.Username, out var user) || user.Password != request.Password)
            {
                return Unauthorized();
            }
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"]));
            var descriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim (ClaimTypes.NameIdentifier, user.UserId.ToString()),
                    new Claim(ClaimTypes.Name, request.Username)
                }),
                Expires = DateTime.UtcNow.AddHours(1),
                SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature),
                Issuer = config["Jwt:Issuer"],
                Audience = config["Jwt:Audience"]
            };
            var token = new JsonWebTokenHandler().CreateToken(descriptor);
            return Ok(new { Token = token });
        }
    }
}
