﻿using System;
using Microsoft.AspNetCore.Mvc;
using API_Saf_T_Child.Models;
using API_Saf_T_Child.Services;

namespace API_Saf_T_Child.Controllers
{
    [Route("api/device")]
    public class DeviceController : Controller
    {
        // Private field representing an instance of the MongoDBService class, which will be used to interact with the MongoDB database.
        private readonly MongoDBService _mongoDBService;

        // This constructor injects an instance of MongoDBService into the controller.
        public DeviceController(MongoDBService mongoDBService)
        {
            _mongoDBService = mongoDBService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Device>>> Get()
        {
            var devices = await _mongoDBService.GetAllDevicesAsync();
            return Ok(devices);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<IEnumerable<Device>>> Get(string id)
        {
            var devices = await _mongoDBService.GetDeviceByIdAsync(id);
            return Ok(devices);
        }

        [HttpGet("by-activation-code/{activationCode}")]
        public async Task<ActionResult<Device>> GetByActivationCode(long activationCode)
        {
            // Convert activationCode to string to check its length
            string activationCodeStr = activationCode.ToString();
            
            // Check if the length of the activationCode is exactly 9 digits
            if (activationCodeStr.Length != 9)
            {
                // Return BadRequest if not exactly 9 digits
                return BadRequest("The activation code must be exactly 9 digits long.");
            }

            var device = await _mongoDBService.GetDeviceByActivationCodeAsync(activationCode);
            
            if (device == null)
            {
                // Return NotFound if the device is not found
                return NotFound("Device not found.");
            }

            return Ok(device);
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Device device)
        {
            await _mongoDBService.InsertDeviceAsync(device);
            return Ok(device);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateDevice(string id, [FromBody] Device device)
        {
            var result = await _mongoDBService.UpdateDeviceAsync(id, device);

            if (result)
            {
                return Ok(device);
            }
            else
            {
                return NotFound();
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var result = await _mongoDBService.DeleteDeviceAsync(id);

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
        public async Task<ActionResult<IEnumerable<Device>>> GetDevicesByOwner(string id)
        {
            var devices = await _mongoDBService.GetDevicesByOwnerAsync(id);
            
            return Ok(devices);
        }
    }
}
