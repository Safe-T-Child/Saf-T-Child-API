using System;
using Microsoft.AspNetCore.Mvc;
using API_Saf_T_Child.Models;
using API_Saf_T_Child.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace API_Saf_T_Child.Controllers
{
    [Route("api/group")]
    public class GroupController : Controller
    {
        // Private field representing an instance of the MongoDBService class, which will be used to interact with the MongoDB database.
        private readonly MongoDBService _mongoDBService;
        private readonly MessageService _messageService;
        private readonly IConfiguration _configuration;

        // This constructor injects an instance of MongoDBService into the controller.
        public GroupController(MongoDBService mongoDBService, MessageService messageService, IConfiguration configuration)
        {
            _mongoDBService = mongoDBService;
            _messageService = messageService;
            _configuration = configuration;
        }

        // TODO: DELETE THIS API ENDPOINT
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Group>>> Get()
        {
            var group = await _mongoDBService.GetAllGroupsAsync();
            return Ok(group);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Post([FromBody] Group group)
        {
            await _mongoDBService.InsertGroupAsync(group);
            return Ok(group);
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<Group>> Get(string id)
        {
            var group = await _mongoDBService.GetGroupByIdAsync(id);

            if (group == null)
            {
                return NotFound(); // Return 404 Not Found if the group with the specified ID is not found
            }

            return Ok(group);
        }

        

        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> UpdateGroup(string id, [FromBody] Group group)
        {
            var result = await _mongoDBService.UpdateGroupAsync(id, group);

            if (result)
            {
                return Ok(group);
            }
            else
            {
                return NotFound();
            }
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> Delete(string id)
        {
            var result = await _mongoDBService.DeleteGroupAsync(id);

            if (result)
            {
                return Ok();
            }
            else
            {
                return NotFound();
            }
        }

        [HttpGet("by-owner/{id}")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<Device>>> GetGroupsByOwner(string id)
        {
            var devices = await _mongoDBService.GetGroupsByOwnerAsync(id);
            
            return Ok(devices);
        }

        [HttpPost("sendGroupInviteEmail")]
        public async Task<IActionResult> SendGroupInviteEmail(string newUserId, string groupId)
        {
            var group = await _mongoDBService.GetGroupByIdAsync(groupId);
            var newUser = await _mongoDBService.GetUserByIdAsync(newUserId);

            UserWithRole groupUser = new UserWithRole();
            groupUser.Name = newUser.FirstName + " " + newUser.LastName;
            groupUser.Role = RoleType.Member;
            groupUser.AcceptedInvite = false;


            if (group != null && groupUser != null)
            {
                group.Users.Add(groupUser);
                var result = await _mongoDBService.UpdateGroupAsync(group.Id, group);

                if (result)
                {
                    var secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
                    var signinCredentials = new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256);

                    var tokeOptions = new JwtSecurityToken(
                        issuer: _configuration["Jwt:Issuer"],
                        audience: _configuration["Jwt:Issuer"],
                        expires: DateTime.Now.AddHours(24),
                        signingCredentials: signinCredentials,
                        claims: new List<Claim>{
                    new Claim("groupId", group.Id),
                    new Claim("newUserID", groupUser.Id)
                        }
                    );

                    var tokenString = new JwtSecurityTokenHandler().WriteToken(tokeOptions);

                    if (group != null && newUser.Email != null)
                    {
                        string linkUrl = "http://localhost:4200/accept-group-invite/?token=" + tokenString;
                        string body = "<p align= 'center'>You've been invited to join" + group.Owner.Name + "'s Family Group!</br> " +
                                        "Click the link below to accept: </br> " +
                                        " <a href='" + linkUrl + "'>Join Group</a></p>";
                        bool success = await _messageService.SendEmail(newUser.Email, "Invitation to join Saf-T-Child Family Group", body);
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
                    return BadRequest("group update failed");
                }
            }
            else
            {
                return BadRequest("Invalid User");
            }

        }

        [HttpGet("verifyGroupLink")]
        public async Task<ActionResult<bool>> verifyGroupLink(string tokenstring)
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

            var newUserIdClaim = token.Claims.FirstOrDefault(claim => claim.Type == "newUserID");
            var groupClaim = token.Claims.FirstOrDefault(claim => claim.Type == "groupId");

            if (newUserIdClaim != null && groupClaim != null)
            {
                string newUserIdValue = newUserIdClaim.Value;
                string groupIdvalue = groupClaim.Value;
                var group = await _mongoDBService.GetGroupByIdAsync(groupIdvalue);
                if(group != null)
                {
                    var user = group.Users.Find(u => u.Id == newUserIdValue);

                    if (user != null)
                    {
                        user.AcceptedInvite = true;

                        var result = await _mongoDBService.UpdateGroupAsync(group.Id, group);

                        if (result)
                        {
                            return Ok(user);
                        }
                        else
                        {
                            return BadRequest("Update Failed");
                        }
                    }
                    else
                    {
                        return BadRequest("User with ID not found.");
                    }
                }
                else
                {
                    return BadRequest("Group with ID not found.");
                }
                

            }
            else
            {
                return BadRequest("User ID claim not found.");
            }
        }
    }
}
