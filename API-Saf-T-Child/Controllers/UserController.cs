using System;
using Microsoft.AspNetCore.Mvc;
using API_Saf_T_Child.Models;
using API_Saf_T_Child.Services;
using Microsoft.AspNetCore.Authorization;

namespace API_Saf_T_Child.Controllers
{
    [Route("api/user")]
    public class UserController: Controller
    {
        private readonly MongoDBService _mongoDBService;
        private readonly MessageService _messageService;

        public UserController(MongoDBService mongoDBService, MessageService messageService)
        {
            _mongoDBService = mongoDBService;
            _messageService = messageService;
        }

        // TODO: DELETE THIS API ENDPOINT
        [HttpGet("getAllUsers")]
        public async Task<ActionResult<IEnumerable<User>>> Get()
        {
            var users = await _mongoDBService.GetUsersAsync();
            return Ok(users);
        }

        [HttpGet("getUserById")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<User>>> Get(string id)
        {
            var user = await _mongoDBService.GetUserByIdAsync(id);
            return Ok(user);
        }

        [HttpGet("getUserByManyIds")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<User>>> Get(List<string> ids)
        {
            var users = await _mongoDBService.GetUsersByIdsAsync(ids);
            return Ok(users);
        }

        [HttpPost("insertUser")]
        public async Task<IActionResult> InsertNewUser([FromBody] User user, int deviceActivationNumber)
        {
            await _mongoDBService.InsertUserAsync(user, deviceActivationNumber);
            return Ok(user);
        }

        [HttpPost("insertTempUser")]
        public async Task<IActionResult> InsertNewTempUser([FromBody] User user, string groupId)
        {
            await _mongoDBService.InsertTempUserAsync(user, groupId);
            return Ok(user);
        }

        [HttpPut("updateUser")]
        [Authorize]
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
        [Authorize]
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
        

        [HttpGet("getRoles")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<string>>> GetRoles()
        {
            var roles = await _mongoDBService.GetRolesAsync();
            return Ok(roles);
        }

        [HttpPost("insertRole")]
        [Authorize]
        public async Task<IActionResult> InsertNewRole([FromBody] Role role)
        {
            await _mongoDBService.InsertRoleAsync(role);
            return Ok(role);
        }
    }
}
