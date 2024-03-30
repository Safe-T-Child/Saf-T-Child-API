using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using API_Saf_T_Child.Models;
using System.Security.Claims;
using API_Saf_T_Child.Services;
using Microsoft.OpenApi.Any;
namespace API_Saf_T_Child.Controllers
{
    [Route("api/authentication")]
    public class AuthenticationController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly MongoDBService _mongoDBService;
        public AuthenticationController(IConfiguration configuration, MongoDBService mongoDBService)
        {
            _mongoDBService = mongoDBService;
            _configuration = configuration;
        }

        [HttpPost]
        public IActionResult Post([FromBody] Login login)
        {
            if (login == null || login.Email == null || login.Password == null)
            {
                return BadRequest("Invalid client request");
            }

            var user = _mongoDBService.LoginUserAsync(login.Email, login.Password).Result;

            if(user == null)
            {
                return NotFound();
            }


            var secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var signinCredentials = new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256);

            var tokeOptions = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Issuer"],
                expires: DateTime.Now.AddHours(2),
                signingCredentials: signinCredentials,
                claims : new List<Claim>{
                    //new Claim("username", user.UserName),
                    new Claim("userId", user.Id),
                    new Claim("email", user.Email),
                    new Claim("firstName", user.FirstName),
                    new Claim("lastName", user.LastName),
                }
            );

            var tokenString = new JwtSecurityTokenHandler().WriteToken(tokeOptions);
            return Ok(new { Token = tokenString });
            
        }
    }
}