using System;
using Microsoft.AspNetCore.Mvc;
using API_Saf_T_Child.Models;
using API_Saf_T_Child.Services;
using System.Text.RegularExpressions;
using System.Numerics;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace API_Saf_T_Child.Controllers
{
    [Route("api/validation")]
    public class ValidationController : Controller
    {
        // Private field representing an instance of the MongoDBService class, which will be used to interact with the MongoDB database.
        private readonly MongoDBService _mongoDBService;
        private readonly MessageService _messageService;

        // This constructor injects an instance of MongoDBService into the controller.
        public ValidationController(MongoDBService mongoDBService, MessageService messageService)
        {
            _mongoDBService = mongoDBService;
            _messageService = messageService;
        }

        [HttpGet("checkUsername")]
        public async Task<ActionResult<bool>> CheckUsernameAvailability(string userName)
        {
            var users = await _mongoDBService.GetUsersAsync();
            bool isUsernameTaken = users.Any(u => u.UserName == userName);

            // Return a response based on the availability
            return Ok(isUsernameTaken);
        }

        [HttpGet("checkEmail")]
        public async Task<ActionResult<bool>> CheckEmailAvailability(string email)
        {
            var users = await _mongoDBService.GetUsersAsync();
            bool isEmailTaken = users.Any(u => u.Email != null && u.Email.Contains(email));

            // Return a response based on the availability
            return Ok(isEmailTaken);
        }

        [HttpGet("checkPhoneNumber")]
        public async Task<ActionResult<bool>> CheckPhoneNumberAvailability(User.PhoneNumber phoneNumber)
        {
            var users = await _mongoDBService.GetUsersAsync();
            bool isPhoneNumberTaken = users.Any(u => u.PrimaryPhoneNumber.CountryCode == phoneNumber.CountryCode 
                                                    && u.PrimaryPhoneNumber.PhoneNumberValue == phoneNumber.PhoneNumberValue);

            // Return a response based on the availability
            return Ok(isPhoneNumberTaken);
        }

        [HttpGet("verifyEmailAddress")]
        public async Task<ActionResult<bool>> VerifyEmailAddress(string id)
        {
            var user = await _mongoDBService.GetUserByIdAsync(id);
            if (user != null)
            {
                user.isEmailVerified = true;
            }
            return Ok();
        }

        [HttpPost("sendVerificationEmail")]
        public async Task<IActionResult> SendVerificationEmail(string id)
        {
            var user = await _mongoDBService.GetUserByIdAsync(id);
            if(user != null && user.Email != null && id != null)
            {
                string linkUrl = "http://localhost:4200/api/validation/verifyEmailAddress/" + id;
                string body = "<p align=\"center\"> Thank you for signing up for Saf - T - Child! </ br > Please click the link below to verify your email! </ br > < a href = \"" + linkUrl + "\" > Verify your Email </ a > </p>";
                bool success = await _messageService.SendEmail(user.Email, "Verify your Saf-T-Child Email", body);
                if(success)
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
    }
}
