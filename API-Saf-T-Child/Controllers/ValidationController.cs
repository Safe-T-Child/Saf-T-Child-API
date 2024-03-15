using System;
using Microsoft.AspNetCore.Mvc;
using API_Saf_T_Child.Models;
using API_Saf_T_Child.Services;
using System.Text.RegularExpressions;

namespace API_Saf_T_Child.Controllers
{
    [Route("api/device")]
    public class ValidationController : Controller
    {
        // Private field representing an instance of the MongoDBService class, which will be used to interact with the MongoDB database.
        private readonly MongoDBService _mongoDBService;

        // This constructor injects an instance of MongoDBService into the controller.
        public ValidationController(MongoDBService mongoDBService)
        {
            _mongoDBService = mongoDBService;
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
            bool isEmailTaken = users.Any(u => u.Email != null && u.Email == email);

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

        [HttpGet("verifyEmail")]
        public async Task<ActionResult<bool>> VerifyEmailAddress(string id)
        {
            var user = await _mongoDBService.GetUserByIdAsync(id);

            if(user != null)
            {
                user.isEmailVerified = true;
                await _mongoDBService.UpdateUserAsync(id, user);
                return Ok("Email Verified Succesfully");
            }
            else
            {
                return BadRequest("Invalid User ID");
            }
        }
    }
}
