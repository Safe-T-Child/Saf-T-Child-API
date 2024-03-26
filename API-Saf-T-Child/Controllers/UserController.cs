using System;
using Microsoft.AspNetCore.Mvc;
using API_Saf_T_Child.Models;
using API_Saf_T_Child.Services;

namespace API_Saf_T_Child.Controllers
{
    [Route("api/user")]
    public class UserController: Controller
    {
        // Private field representing an instance of the MongoDBService class, which will be used to interact with the MongoDB database.
        private readonly MongoDBService _mongoDBService;
        private readonly MessageService _messageService;

        // This constructor injects an instance of MongoDBService into the controller.
        public UserController(MongoDBService mongoDBService, MessageService messageService)
        {
            _mongoDBService = mongoDBService;
            _messageService = messageService;
        }

        [HttpGet("getAllUsers")]
        public async Task<ActionResult<IEnumerable<User>>> Get()
        {
            var users = await _mongoDBService.GetUsersAsync();
            return Ok(users);
        }

        [HttpGet("getUserById")]
        public async Task<ActionResult<IEnumerable<User>>> Get(string id)
        {
            var users = await _mongoDBService.GetUserByIdAsync(id);
            return Ok(users);
        }

        //[HttpPost("insertUser")]
        //public async Task<IActionResult> Post([FromBody] User user)
        //{
        //    await _mongoDBService.InsertUserAsync(user);
        //    return Ok(user);
        //}

        [HttpPost("insertUser")]
        public async Task<IActionResult> InsertUser([FromBody] User user)
        {
            try
            {
                if (user == null)
                {
                    return BadRequest("Invalid user object");
                }

                if (!string.IsNullOrEmpty(user.GroupReferralCode))
                {
                    var groupExists = await _mongoDBService.GroupExistsAsync(user.GroupReferralCode);
                    if (!groupExists)
                    {
                        return BadRequest("Group referral code does not exist");
                    }
                }

                if (!string.IsNullOrEmpty(user.DeviceActivationNumber))
                {
                    var deviceExists = await _mongoDBService.DeviceExistsAsync(device.DeviceActivationNumber);
                    if (!deviceExists)
                    {
                        return BadRequest("Device activation number does not exist");
                    }

                    await _mongoDBService.InsertUserAsync(user);
                return Ok(new { message = "User inserted successfully", user });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        [HttpPut("updateUser")]
        public async Task<IActionResult> UpdateUser(string id, [FromBody] User user)
        {
            var result = await _mongoDBService.UpdateUserAsync(id, user);

            if (result)
            {
                return Ok(user);
            }
            else
            {
                return NotFound();
            }
        }

        [HttpDelete("deleteUser")]
        public async Task<IActionResult> Delete(string id)
        {
            var result = await _mongoDBService.DeleteUserAsync(id);

            if (result)
            {
                return Ok();
            }
            else
            {
                return NotFound();
            }
        }

        [HttpPost("forgotUserPassword")]
        public async Task<IActionResult> SendForgotPasswordLink(string email)
        {
            var users = await _mongoDBService.GetUsersAsync();
            var user = users.Where(u => u.Email != null && u.Email.Contains(email)).First();
            if (user != null)
            {
                //TODO - Link Pasword Update Page stuff here
                string linkUrl = "http://localhost:4200/updatePasswordPage/" + user.Id;
                string body = "<p align= 'center'> Click the link below to update your password: </br> " +
                                " <a href='" + linkUrl + "'>Update your Password</a></p>";
                bool success = await _messageService.SendEmail(email, "Pasword Reset Request", body); ;
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
    }
}
