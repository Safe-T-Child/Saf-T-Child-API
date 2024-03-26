﻿using API_Saf_T_Child.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using MongoDB.Bson;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace API_Saf_T_Child.Services
{
    public class MongoDBService
    {
        private readonly IMongoCollection<User> _userCollection;
        private readonly IMongoCollection<Group> _groupCollection;
        private readonly IMongoCollection<Device> _deviceCollection;

        private readonly IMongoCollection<Vehicle> _vehicleCollection;

        public MongoDBService(IOptions<MongoDBSettings> mongoDBSettings)
        {
            MongoClient client = new MongoClient(mongoDBSettings.Value.ConnectionURI);
            IMongoDatabase database = client.GetDatabase(mongoDBSettings.Value.DatabaseName);
            _userCollection = database.GetCollection<User>(mongoDBSettings.Value.Collection1);
            _groupCollection = database.GetCollection<Group>(mongoDBSettings.Value.Collection3);
            _deviceCollection = database.GetCollection<Device>(mongoDBSettings.Value.Collection4);
            _vehicleCollection = database.GetCollection<Vehicle>(mongoDBSettings.Value.VehicleCollection);
        }

        // Get
        public async Task<List<User>> GetUsersAsync()
        {
            var users = await _userCollection.Find(_ => true).ToListAsync();
            return users;
        }

        public async Task<User> GetUserByIdAsync(string id)
        {
            var objectId = new ObjectId(id); // Convert string ID to ObjectId

            var filter = Builders<User>.Filter.Eq("_id", objectId); // Filter by group ID
            var user = await _userCollection.Find(filter).FirstOrDefaultAsync();

            return user;
        }

        public async Task<List<Group>> GetAllGroupsAsync()
        {
            var groups = await _groupCollection.Find(_ => true).ToListAsync();
            return groups;
        }

        public async Task<Group> GetGroupByIdAsync(string id)
        {
            var objectId = new ObjectId(id); // Convert string ID to ObjectId

            var filter = Builders<Group>.Filter.Eq("_id", objectId); // Filter by group ID
            var group = await _groupCollection.Find(filter).FirstOrDefaultAsync();

            return group;
        }

        public async Task<List<Device>> GetAllDevicesAsync()
        {
            var devices = await _deviceCollection.Find(_ => true).ToListAsync();
            return devices;
        }

        public async Task<Device> GetDeviceByIdAsync(string id)
        {
            var objectId = new ObjectId(id); // Convert string ID to ObjectId

            var filter = Builders<Device>.Filter.Eq("_id", objectId); // Filter by group ID
            var device = await _deviceCollection.Find(filter).FirstOrDefaultAsync();

            return device;
        }

        public async Task<Device> GetDeviceByActivationCodeAsync(long activationCode)
        {
            var filter = Builders<Device>.Filter.Eq("deviceActivationCode", activationCode);
            var device = await _deviceCollection.Find(filter).FirstOrDefaultAsync();

            return device;
        }

        public async Task<List<Device>> GetDevicesByOwnerAsync(string ownerId)
        {
            var filter = Builders<Device>.Filter.Eq("deviceOwner._id", ObjectId.Parse(ownerId));
            var devices = await _deviceCollection.Find(filter).ToListAsync();

            return devices;
        }

        public async Task<List<Group>> GetGroupsByOwnerAsync(string userId)
        {
            var filter = Builders<Group>.Filter.Eq("owner._id", ObjectId.Parse(userId));
            var groups = await _groupCollection.Find(filter).ToListAsync();

            return groups;
        }

        public async Task<List<Vehicle>> GetVehiclesByOwnerAsync(string ownerId)
        {
            var filter = Builders<Vehicle>.Filter.Eq("owner._id", ObjectId.Parse(ownerId));
            var vehicles = await _vehicleCollection.Find(filter).ToListAsync();
            return vehicles;
        }

        //Insert
        public async Task InsertUserAsync(User user)
        {
            var users = await GetUsersAsync();
            bool isUsernameTaken = users.Any(u => u.UserName == user.UserName);

            if (isUsernameTaken)
            {
                throw new ArgumentNullException(nameof(user.UserName), "Username is already taken.");
            }

            if (user == null)
            {
                throw new ArgumentNullException(nameof(user), "User object cannot be null.");
            }

            if (user.UserName == null)
            {
                throw new ArgumentNullException(nameof(user.UserName), "UserName cannot be null.");
            }

            if (user.Id == null)
            {
                throw new ArgumentNullException(nameof(user.Id), "User Id cannot be null.");
            }

            if (user.Email == null)
            {
                throw new ArgumentNullException(nameof(user.Email), "Email cannot be null.");
            }

            if (user.PrimaryPhoneNumber == null)
            {
                throw new ArgumentNullException(nameof(user.PrimaryPhoneNumber), "Primary phone number cannot be null.");
            }

            await _userCollection.InsertOneAsync(user);
        }

        public async Task InsertGroupAsync(Group group)
        {
            if (group == null)
            {
                throw new ArgumentNullException(nameof(group), "Group object cannot be null.");
            }

            if (group.Owner == null)
            {
                throw new ArgumentNullException(nameof(group.Owner), "Missing group owner. This field cannot be null.");
            }

            if (group.Id == null)
            {
                throw new ArgumentNullException(nameof(group.Id), "Missing group Id. This field cannot be null.");
            }

            await _groupCollection.InsertOneAsync(group);
        }

        public async Task InsertDeviceAsync(Device device)
        {
            if (device == null)
            {
                throw new ArgumentNullException(nameof(device), "Device object cannot be null.");
            }

            if (device.Id == null)
            {
                throw new ArgumentNullException(nameof(device), "Device Id cannot be null.");
            }

            if (device.DeviceId == null)
            {
                throw new ArgumentNullException(nameof(device.DeviceId), "Device Id (Serial) cannot be null.");
            }

            if (device.Name == null)
            {
                throw new ArgumentNullException(nameof(device.Name), "Device name cannot be null.");
            }

            if (device.Model == null)
            {
                throw new ArgumentNullException(nameof(device.Model), "Device model cannot be null.");
            }

            if (device.Car == null)
            {
                throw new ArgumentNullException(nameof(device.Car), "Car cannot be null.");
            }

            // TODO: Additional validation logic for the Device object, if needed

            // Assign a default group to the device
            // a family
            Group group = new Group(
                ObjectId.GenerateNewId().ToString(),
                "Default Family",
                device.Owner
            );

            await InsertGroupAsync(group);

            try{
                await _deviceCollection.InsertOneAsync(device);
            } catch (Exception e){
                await DeleteGroupAsync(group.Id);
                throw e;
            }
        }

        // Update
        public async Task<bool> UpdateUserAsync(string id, User user)
        {
            var users = await GetUsersAsync();
            bool isUsernameTaken = users.Any(u => u.UserName == user.UserName);

            if (isUsernameTaken)
            {
                throw new ArgumentNullException(nameof(user.UserName), "Username is already taken.");
            }

            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentException("User ID cannot be null or empty.", nameof(id));
            }


            if (user == null)
            {
                throw new ArgumentNullException(nameof(user), "User object cannot be null.");
            }

            var filter = Builders<User>.Filter.Eq("_id", ObjectId.Parse(id));

            var update = Builders<User>.Update
                .Set("userName", user.UserName)
                .Set("firstName", user.FirstName)
                .Set("lastName", user.LastName)
                .Set("email", user.Email)
                .Set("primaryPhoneNumer", user.PrimaryPhoneNumber)
                .Set("secondaryPhoneNumbers", user.SecondaryPhoneNumbers);

            var result = await _userCollection.UpdateOneAsync(filter, update);

            return result.ModifiedCount > 0;
        }

        public async Task<bool> UpdateGroupAsync(string id, Group group)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentException("User ID cannot be null or empty.", nameof(id));
            }


            if (group == null)
            {
                throw new ArgumentNullException(nameof(group), "Group object cannot be null.");
            }

            var filter = Builders<Group>.Filter.Eq(g => g.Id, id);
            var update = Builders<Group>.Update
                .Set(g => g.Name, group.Name)
                .Set(g => g.Owner, group.Owner)
                .Set(g => g.Users, group.Users);

            var result = await _groupCollection.UpdateOneAsync(filter, update);
            return result.ModifiedCount > 0;
        }

        public async Task<bool> UpdateDeviceAsync(string id, Device updatedDevice)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentException("Device ID cannot be null or empty.", nameof(id));
            }


            if (updatedDevice == null)
            {
                throw new ArgumentNullException(nameof(updatedDevice), "Device object cannot be null.");
            }

            var filter = Builders<Device>.Filter.Eq(d => d.Id, id);
            var update = Builders<Device>.Update
                .Set(d => d.Type, updatedDevice.Type)
                .Set(d => d.Name, updatedDevice.Name)
                .Set(d => d.Model, updatedDevice.Model)
                .Set(d => d.DeviceId, updatedDevice.DeviceId)
                .Set(d => d.DeviceActivationCode, updatedDevice.DeviceActivationCode)
                .Set(d => d.Car, updatedDevice.Car)
                .Set(d => d.Status, updatedDevice.Status)
                .Set(d => d.Owner, updatedDevice.Owner)
                .Set(d => d.Group, updatedDevice.Group);

            var result = await _deviceCollection.UpdateOneAsync(filter, update);
            return result.ModifiedCount > 0;
        }

        // Delete
        public async Task<bool> DeleteUserAsync(string id)
        {
            var filter = Builders<User>.Filter.Eq("_id", ObjectId.Parse(id));
            var result = await _userCollection.DeleteOneAsync(filter);

            return result.DeletedCount > 0;
        }

        public async Task<bool> DeleteGroupAsync(string id)
        {
            var filter = Builders<Group>.Filter.Eq("_id", ObjectId.Parse(id));
            var result = await _groupCollection.DeleteOneAsync(filter);

            return result.DeletedCount > 0;
        }

        public async Task<bool> DeleteDeviceAsync(string id)
        {
            var filter = Builders<Device>.Filter.Eq("_id", ObjectId.Parse(id));
            var result = await _deviceCollection.DeleteOneAsync(filter);

            return result.DeletedCount > 0;
        }
    }
}
