using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ApiGateway.Models;
namespace ApiGateway.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        public AuthController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpPost("login")]
        public ActionResult<LoginResponse> Login(LoginRequest request)
        {
            if (ValidarUsuario(request.Username, request.Password))
            {
                var token = GerarToken(request.Username);
                var expiresAt = DateTime.UtcNow.AddHours(2);
                return Ok(new LoginResponse
                {
                    Token = token,
                    Username = request.Username,
                    ExpiresAt = expiresAt
                });
            }
            return Unauthorized(new { message = "Credenciais inválidas" });
        }

        private bool ValidarUsuario(string username, string password)
        {
            return username == "admin" && password == "admin123";
        }

        private string GerarToken(string username)
        {
            var jwtKey = _configuration["Jwt:Key"] ?? "ChaveSecretaSuperSegura123456789012";
            var key = Encoding.ASCII.GetBytes(jwtKey);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, username),
                new Claim(ClaimTypes.Role, "User")
            }),
                Expires = DateTime.UtcNow.AddHours(2),
                SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature)
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }
    }
}