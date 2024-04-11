using System;
using Microsoft.AspNetCore.Mvc;
using API_Saf_T_Child.Models;
using API_Saf_T_Child.Services;
using Microsoft.AspNetCore.Authorization;

namespace API_Saf_T_Child.Controllers
{
    [Route("api/vehicle")]
    public class VehicleController : Controller
    {
        // Private field representing an instance of the MongoDBService class, which will be used to interact with the MongoDB database.
        private readonly MongoDBService _mongoDBService;

        // This constructor injects an instance of MongoDBService into the controller.
        public VehicleController(MongoDBService mongoDBService)
        {
            _mongoDBService = mongoDBService;
        }

        [HttpGet("by-owner/{id}")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<Vehicle>>> GetGroupsByOwner(string id)
        {
            var devices = await _mongoDBService.GetVehiclesByOwnerAsync(id);
            
            return Ok(devices);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> InsertVehicle([FromBody] Vehicle vehicle)
        {
            await _mongoDBService.InsertVehicleAsync(vehicle);
            return Ok(vehicle);
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<Vehicle>> GetVehicle(string id)
        {
            var vehicle = await _mongoDBService.GetVehicleByIdAsync(id);

            if (vehicle == null)
            {
                return NotFound(); // Return 404 Not Found if the vehicle with the specified ID is not found
            }

            return Ok(vehicle);
        }

        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> UpdateVehicle(string id, [FromBody] Vehicle vehicle)
        {
            var result = await _mongoDBService.UpdateVehicleAsync(id, vehicle);

            if (result)
            {
                return Ok(vehicle);
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
            var result = await _mongoDBService.DeleteVehicleAsync(id);

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
