using System;
using Microsoft.AspNetCore.Mvc;
using API_Saf_T_Child.Models;
using API_Saf_T_Child.Services;
using System.Text.RegularExpressions;
using System.Numerics;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.DataProtection;

namespace API_Saf_T_Child.Controllers
{
    [Route("api/validation")]
    public class ValidationController : Controller
    {
        // Private field representing an instance of the MongoDBService class, which will be used to interact with the MongoDB database.
        private readonly MongoDBService _mongoDBService;
        private readonly MessageService _messageService;
        private readonly IConfiguration _configuration;

        // This constructor injects an instance of MongoDBService into the controller.
        public ValidationController(IConfiguration configuration, MongoDBService mongoDBService, MessageService messageService)
        {
            _configuration = configuration;
            _mongoDBService = mongoDBService;
            _messageService = messageService;
        }

        //[HttpGet("checkUsername")]
        //public async Task<ActionResult<bool>> CheckUsernameAvailability(string userName)
        //{
        //    var users = await _mongoDBService.GetUsersAsync();
        //    bool isUsernameTaken = users.Any(u => u.UserName == userName);

        //    // Return a response based on the availability
        //    return Ok(isUsernameTaken);
        //}

        [HttpGet("checkEmail")]
        public async Task<ActionResult<(bool,bool)>> CheckEmailAvailability(string email)
        {
            (bool, bool) returnVal = (false, false);
            var users = await _mongoDBService.GetUsersAsync();
            bool isEmailTaken = users.Any(u => u.Email != null && u.Email == email);

            if(isEmailTaken)
            {
                returnVal.Item1 = isEmailTaken;
                returnVal.Item2 = users.First(u => u.Email != null && u.Email == email).isTempUser;
            }

            // Return a response based on the availability
            return Ok(returnVal);
        }

        [HttpGet("checkPhoneNumber")]
        public async Task<ActionResult<bool>> CheckPhoneNumberAvailability(PhoneNumber phoneNumber)
        {
            var users = await _mongoDBService.GetUsersAsync();
            bool isPhoneNumberTaken = users.Any(u => u.PrimaryPhoneNumber.CountryCode == phoneNumber.CountryCode 
                                                    && u.PrimaryPhoneNumber.PhoneNumberValue == phoneNumber.PhoneNumberValue);

            // Return a response based on the availability
            return Ok(isPhoneNumberTaken);
        }

        [HttpGet("verifyEmailAddress")]
        public async Task<ActionResult<bool>> VerifyEmailAddress(string tokenstring)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.ReadToken(tokenstring) as JwtSecurityToken;

            // Step 2: Validate token signature
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"])),
                ValidateIssuer = false,
                ValidateAudience = false
            };

            SecurityToken validatedToken;
            try
            {
                tokenHandler.ValidateToken(tokenstring, validationParameters, out validatedToken);
            }
            catch (SecurityTokenValidationException)
            {
                return BadRequest("Token validation failed. Invalid signature or other validation error.");
            }

            // Step 3: Check if the token is not expired
            if (token.ValidTo < DateTime.UtcNow)
            {
                return BadRequest("Token is Expired");
            }

            var userIdClaim = token.Claims.FirstOrDefault(claim => claim.Type == "userId");

            if (userIdClaim != null)
            {
                string userIdValue = userIdClaim.Value;
                var user = await _mongoDBService.GetUserByIdAsync(userIdValue);
                if (user != null)
                {
                    user.isEmailVerified = true;
                    var result = await _mongoDBService.UpdateUserAsync(userIdValue, user);

                    if (result)
                    {
                        return Ok(user);
                    }
                    else
                    {
                        return NotFound();
                    }
                }
                else
                {
                    return BadRequest("User with ID not found.");
                }
                
            }
            else
            {
                return BadRequest("User ID claim not found.");
            }
        }

        [HttpPost("sendVerificationEmail")]
        public async Task<IActionResult> SendVerificationEmail(string id)
        {
            var user = await _mongoDBService.GetUserByIdAsync(id);

            if (user != null)
            {
                var secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
                var signinCredentials = new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256);

                var tokeOptions = new JwtSecurityToken(
                    issuer: _configuration["Jwt:Issuer"],
                    audience: _configuration["Jwt:Issuer"],
                    expires: DateTime.Now.AddHours(24),
                    signingCredentials: signinCredentials,
                    claims: new List<Claim>{
                    new Claim("userId", user.Id)
                    }
                );

                var tokenString = new JwtSecurityTokenHandler().WriteToken(tokeOptions);

                if (user != null && user.Email != null && id != null)
                {
                    string linkUrl = "http://localhost:4200/api/validation/verifyEmailAddress/" + tokenString;
                    string body = "<p align= 'center'>Thank You for singing up for Saf-T-Child! </br> " +
                                    "Click the link below to verify your email: </br> " +
                                    " <a href='" + linkUrl + "'>Verify your Email</a></p>";
                    bool success = await _messageService.SendEmail(user.Email, "Verify your email with Saf-T-Child", body);
                    if (success)
                    {
                        return Ok(success);
                    }
                    else
                    {
                        return BadRequest("SMTP Failed");
                    }
                }
                else
                {
                    return BadRequest("Invalid User");
                }
            }
            else
            {
                return BadRequest("Invalid User");
            }
        }

        [HttpPost("sendPasswordReset")]
        public async Task<IActionResult> SendPaswordResetEmail(string id)
        {
            var user = await _mongoDBService.GetUserByIdAsync(id);

            if (user != null)
            {
                var secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
                var signinCredentials = new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256);

                var tokeOptions = new JwtSecurityToken(
                    issuer: _configuration["Jwt:Issuer"],
                    audience: _configuration["Jwt:Issuer"],
                    expires: DateTime.Now.AddHours(24),
                    signingCredentials: signinCredentials,
                    claims: new List<Claim>{
                    new Claim("userId", user.Id),
                    new Claim("resetPassword", "true"),
                    }
                );

                var tokenString = new JwtSecurityTokenHandler().WriteToken(tokeOptions);

                if (user != null && user.Email != null && id != null)
                {
                    string linkUrl = "http://localhost:4200/api/validation/verifyEmailAddress/" + tokenString;
                    string body = "<p align= 'center'>Saf-T-Child Password Reset Request </br> " +
                                    "Click the link below to change your password: </br> " +
                                    " <a href='" + linkUrl + "'>Change Your Password</a></p>";
                    bool success = await _messageService.SendEmail(user.Email, "Saf-T-Child Password Reset Request", body);
                    if (success)
                    {
                        return Ok(success);
                    }
                    else
                    {
                        return BadRequest("SMTP Failed");
                    }
                }
                else
                {
                    return BadRequest("Invalid User");
                }
            }
            else
            {
                return BadRequest("Invalid User");
            }
        }

        [HttpGet("checkGroupName")]
        public async Task<ActionResult<bool>> CheckGroupNameAvailability(string userId, string groupName)
        {
            var groups = await _mongoDBService.GetAllGroupsAsync();

            if(groups != null)
            {
                bool isGroupNameTaken = groups.Any(g => g.Owner.Id == userId && g.Name == groupName);
                return Ok(isGroupNameTaken);
            }
            else
            {
                return BadRequest("Invalid User ID");
            }
        }
    }
}
