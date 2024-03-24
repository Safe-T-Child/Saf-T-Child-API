using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using API_Saf_T_Child.Models;
using System.Security.Claims;

namespace API_Saf_T_Child.Controllers
{
    [Route("api/authentication")]
    public class AuthenticationController : Controller
    {
        private readonly IConfiguration _configuration;
        public AuthenticationController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpPost]
        public IActionResult Post([FromBody] Login login)
        {
            if (login.Username == "admin" && login.Password == "admin")
            {
                var secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
                var signinCredentials = new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256);
                var firstName = "TestingName";
                var lastName = "TestingLastName";
                var userId = "65f5ed8aeb4ca31830d949f5" ;

                var tokeOptions = new JwtSecurityToken(
                    issuer: _configuration["Jwt:Issuer"],
                    audience: _configuration["Jwt:Issuer"],
                    expires: DateTime.Now.AddMinutes(5),
                    signingCredentials: signinCredentials,
                    claims : new List<Claim>{
                        new Claim("userId", userId),
                        new Claim("firstName", firstName),
                        new Claim(ClaimTypes.Surname, lastName)
                    }
                );

                var tokenString = new JwtSecurityTokenHandler().WriteToken(tokeOptions);
                return Ok(new { Token = tokenString });
            }
            else
            {
                return NotFound();
            }
        }
    }
}