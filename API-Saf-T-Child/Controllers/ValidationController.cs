using System;
using Microsoft.AspNetCore.Mvc;
using API_Saf_T_Child.Models;
using API_Saf_T_Child.Services;
using System.Text.RegularExpressions;
using System.Numerics;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.DataProtection;
using Twilio;
using Twilio.Types;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Rest.Verify.V2.Service;
using API_Saf_T_Child.Templates;

namespace API_Saf_T_Child.Controllers
{
    [Route("api/validation")]
    public class ValidationController : Controller
    {
        // Private field representing an instance of the MongoDBService class, which will be used to interact with the MongoDB database.
        private readonly MongoDBService _mongoDBService;
        private readonly MessageService _messageService;
        private readonly IConfiguration _configuration;

        private readonly string _accountSid;
        private readonly string _authToken;
        private readonly string _verifyServiceSid;

        

        // This constructor injects an instance of MongoDBService into the controller.
        public ValidationController(IConfiguration configuration, MongoDBService mongoDBService, MessageService messageService)
        {
            _configuration = configuration;
            _mongoDBService = mongoDBService;
            _messageService = messageService;

            _authToken = configuration["TwilioSettings:AuthToken"];
            _accountSid = configuration["TwilioSettings:AccountSid"];
            _verifyServiceSid = configuration["TwilioSettings:VerifyServiceSid"];

            // Initialize Twilio client with your account SID and auth token
            Twilio.TwilioClient.Init(_accountSid, _authToken);
        }

        [HttpPost("SendVerificationCode")]
        public async Task<IActionResult> SendVerificationCode(string phoneNumber)
        {
            try
            {
                var verification = await VerificationResource.CreateAsync(
                    to: phoneNumber,
                    channel: "sms", // or "call" for voice verification
                    pathServiceSid: _verifyServiceSid);

                return Ok(new { verification.Status });
            }
            catch (System.Exception ex)
            {
                return BadRequest($"Error sending verification code: {ex.Message}");
            }
        }

        [HttpPost("VerifyCode")]
        public async Task<IActionResult> VerifyCode([FromBody] VerificationRequestModel model)
        {
            try
            {
                var verificationCheck = await VerificationCheckResource.CreateAsync(
                    to: model.PhoneNumber,
                    code: model.Code,
                    pathServiceSid: _verifyServiceSid);

                return Ok(new { verificationCheck.Status });
            }
            catch (System.Exception ex)
            {
                return BadRequest($"Error verifying code: {ex.Message}");
            }
        }


        [HttpGet("checkEmail")]
        public async Task<ActionResult<UserStatus>> CheckEmailAvailability(string email)
        {
            UserStatus returnVal = new UserStatus
            {
                isEmailTaken = false,
                isTempUser = null
            };

            var user = await _mongoDBService.GetUserByEmailAsync(email);

            if(user != null)
            {
                returnVal.isEmailTaken = true;
                returnVal.isTempUser = user.isTempUser;
            }

            return Ok(returnVal);
        }

        [HttpGet("checkPhoneNumber")]
        public async Task<ActionResult<bool>> CheckPhoneNumberAvailability(PhoneNumberDetails phoneNumber)
        {
            
            var user= await _mongoDBService.GetUserByPhoneNumberAsync(phoneNumber);
            
            if(user != null)
            {
                return Ok(true);
            }
            else
            {
                return Ok(false);
            }
        }

        [HttpGet("verifyEmailAddress")]
        public async Task<ActionResult<bool>> VerifyEmailAddress(string tokenstring)
        {
            try
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
                    return BadRequest("User ID claim not found in token.");
                }
            }
            catch (SecurityTokenValidationException)
            {
                return BadRequest("Token validation failed. Invalid signature or other validation error.");
            }

        }

        [HttpPost("sendVerificationEmail")]
        public async Task<IActionResult> SendVerificationEmail(string email)
        {
            var user = await _mongoDBService.GetUserByEmailAsync(email);

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

                if (user != null && user.Email != null)
                {
                    string linkUrl = "https://saf-t-child-app.azurewebsites.net/verify-email/?token=" + tokenString;
                    string body = emailTemplates.verifyEmailMessage(linkUrl);
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
        public async Task<IActionResult> SendPaswordResetEmail(string email)
        {
            var user = await _mongoDBService.GetUserByEmailAsync(email);

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

                if (user != null && user.Email != null)
                {
                    string linkUrl = "https://saf-t-child-app.azurewebsites.net/reset-password/?token=" + tokenString;
                    string body = emailTemplates.forgotPasswordMessage(linkUrl);
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

    public class UserStatus
    {
        public bool isEmailTaken { get; set; }
        public bool? isTempUser { get; set; }
    }

    public class VerificationRequestModel
    {
        [RegularExpression("^[0-9]+$")]
        [Required]
        public string PhoneNumber { get; set; }

        [RegularExpression("^[0-9]+$")]
        [Required]
        public string Code { get; set; }
    }
}
