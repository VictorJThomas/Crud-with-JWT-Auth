using Crud_with_JWT_Auth.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Crud_with_JWT_Auth.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        public static Users users = new Users();
        private readonly IConfiguration _configuration;

        public AuthController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpPost("register")]
        public ActionResult<Users> Register(Users request)
        {
            string passwordHash
                = BCrypt.Net.BCrypt.HashPassword(request.Password);

            string userHash
                = BCrypt.Net.BCrypt.HashString(request.UserName, 8);

            users.UserName = userHash;
            users.Email = request.Email;
            users.Password = passwordHash;

            return Ok(users);
        }

        [HttpPost("login")]
        public ActionResult<Users> Login(Users request)
        {
            if (users.Email != request.Email)
            {
                return BadRequest("User not found.");
            }

            if (!BCrypt.Net.BCrypt.Verify(request.Password, users.Password))
            {
                return BadRequest("User not found.");
            }

            string token = CreateToken(users);

            return Ok(token);
        }

        private string CreateToken(Users users)
        {
            List<Claim> claims = new List<Claim> {
                new Claim(ClaimTypes.Name, users.UserName),
                new Claim(ClaimTypes.Role, "Admin"),
                new Claim(ClaimTypes.Role, "Users"),
            };
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
                _configuration.GetSection("AppSettings:Token").Value!));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            var token = new JwtSecurityToken(
                    claims: claims,
                    expires: DateTime.Now.AddDays(1),
                    signingCredentials: creds
                );

            var jwt = new JwtSecurityTokenHandler().WriteToken(token);

            return jwt;
        }
    }
}
