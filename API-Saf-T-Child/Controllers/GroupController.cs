using System;
using Microsoft.AspNetCore.Mvc;
using API_Saf_T_Child.Models;
using API_Saf_T_Child.Services;
using Microsoft.AspNetCore.Authorization;

namespace API_Saf_T_Child.Controllers
{
    [Route("api/group")]
    public class GroupController : Controller
    {
        // Private field representing an instance of the MongoDBService class, which will be used to interact with the MongoDB database.
        private readonly MongoDBService _mongoDBService;

        // This constructor injects an instance of MongoDBService into the controller.
        public GroupController(MongoDBService mongoDBService)
        {
            _mongoDBService = mongoDBService;
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
    }
}
