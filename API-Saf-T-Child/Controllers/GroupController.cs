using System;
using Microsoft.AspNetCore.Mvc;
using API_Saf_T_Child.Models;
using API_Saf_T_Child.Services;

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

        [HttpGet("Get")]
        public async Task<ActionResult<IEnumerable<Group>>> Get()
        {
            var group = await _mongoDBService.GetGroupAsync();
            return Ok(group);
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Group group)
        {
            await _mongoDBService.InsertGroupAsync(group);
            return Ok(group);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(string id, [FromBody] Group group)
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
    }
}
