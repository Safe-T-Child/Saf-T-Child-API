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
    }
}
