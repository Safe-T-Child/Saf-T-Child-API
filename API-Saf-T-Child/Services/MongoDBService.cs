using API_Saf_T_Child.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using MongoDB.Bson;
using BCrypt.Net;
using Microsoft.IdentityModel.Tokens;

namespace API_Saf_T_Child.Services
{
    public class MongoDBService
    {
        private readonly IMongoCollection<User> _usersCollection;
        private readonly IMongoCollection<Group> _groupsCollection;
        private readonly IMongoCollection<Device> _devicesCollection;
        private readonly IMongoCollection<Vehicle> _vehiclesCollection;

        private readonly IMongoCollection<Role> _rolesCollection;
        private readonly MongoClient client;

        public MongoDBService(IOptions<MongoDBSettings> mongoDBSettings)
        {
            client = new MongoClient(mongoDBSettings.Value.ConnectionURI);
            IMongoDatabase database = client.GetDatabase(mongoDBSettings.Value.DatabaseName);
            _usersCollection = database.GetCollection<User>(mongoDBSettings.Value.UsersCollection);
            _groupsCollection = database.GetCollection<Group>(mongoDBSettings.Value.GroupsCollection);
            _devicesCollection = database.GetCollection<Device>(mongoDBSettings.Value.DevicesCollection);
            _vehiclesCollection = database.GetCollection<Vehicle>(mongoDBSettings.Value.VehiclesCollection);
            _rolesCollection = database.GetCollection<Role>(mongoDBSettings.Value.RolesCollection);
        }

        # region Users

        public async Task<User> GetUserByPhoneNumberAsync(PhoneNumberDetails phoneNumber)
        {
            var filter = Builders<User>.Filter.Eq(p=> p.PrimaryPhoneNumber, phoneNumber );
            var user = await _usersCollection.Find(filter).FirstOrDefaultAsync();

            if(user == null)
            {
                return null;
            }

            return user;
        }

      

        public async Task<List<User>> GetUsersAsync()
        { 
            
            var users = await _usersCollection.Find(_ => true).ToListAsync();
            return users;
        }
        public async Task<User> GetUserByIdAsync(string id)
        {
            var objectId = new ObjectId(id); // Convert string ID to ObjectId

            var filter = Builders<User>.Filter.Eq("_id", objectId); // Filter by group ID
            var user = await _usersCollection.Find(filter).FirstOrDefaultAsync();

            return user;
        }

        public async Task<List<User>> GetUsersByIdsAsync(List<string> ids)
        {
            var objectIds = ids.Select(id => new ObjectId(id)).ToList(); 

            var filter = Builders<User>.Filter.In("_id", objectIds); 
            var users = await _usersCollection.Find(filter).ToListAsync();

            return users;
        }

        public async Task<List<User>> GetUsersByGroupIdAsync(string groupId)
        {
            var filter = Builders<Group>.Filter.Eq( g=> g.Id, groupId);
            var group = await _groupsCollection.Find(filter).FirstOrDefaultAsync();

            if(group == null)
            {
                return null;
            }

            var userIds = group.Users.Select(u => u.Id).ToList();

            var users = await GetUsersByIdsAsync(userIds);


            return users;
        }

        public async Task<User> LoginUserAsync(string email)
        {
            var user = await GetUserByEmailAsync(email);

            if (user == null)
            {
                return null;
            }
            return user;
        }

        public async Task<User?> GetUserByEmailAsync(string email)
        {
            var filter = Builders<User>.Filter.Regex("email", new BsonRegularExpression(email, "i"));
            var user = await _usersCollection.Find(filter).FirstOrDefaultAsync();

            //now check that they are exact matches not just partial
            if (user != null && user.Email.ToLower() == email.ToLower())
            {
                return user;
            }
            return null;
        }

        # endregion

        # region Groups

        public async Task<List<Group>> GetAllGroupsAsync()
        {
            var groups = await _groupsCollection.Find(_ => true).ToListAsync();
            return groups;
        }

        public async Task<Group> GetGroupByIdAsync(string id)
        {
            var objectId = new ObjectId(id); // Convert string ID to ObjectId

            var filter = Builders<Group>.Filter.Eq("_id", objectId); // Filter by group ID
            var group = await _groupsCollection.Find(filter).FirstOrDefaultAsync();

            return group;
        }

        public async Task<List<Device>> GetAllDevicesAsync()
        {
            var devices = await _devicesCollection.Find(_ => true).ToListAsync();
            return devices;
        }

        public async Task<List<Group>> GetGroupsByOwnerAsync(string userId)
        {
            var filter = Builders<Group>.Filter.Eq("owner._id", ObjectId.Parse(userId));
            var groups = await _groupsCollection.Find(filter).ToListAsync();

            return groups;
        }
        # endregion

        #region Devices
        public async Task<Device> GetDeviceByIdAsync(string id)
        {
            var objectId = new ObjectId(id); // Convert string ID to ObjectId

            var filter = Builders<Device>.Filter.Eq("_id", objectId); // Filter by group ID
            var device = await _devicesCollection.Find(filter).FirstOrDefaultAsync();

            return device;
        }

        public async Task<Device> GetDeviceByActivationCodeAsync(long activationCode)
        {
            var filter = Builders<Device>.Filter.Eq("deviceActivationCode", activationCode);
            var device = await _devicesCollection.Find(filter).FirstOrDefaultAsync();

            return device;
        }

        public async Task<Boolean> UpdateUserPasswordAsync(string id, string password)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentException("User ID cannot be null or empty.", nameof(id));
            }

            if (string.IsNullOrEmpty(password))
            {
                throw new ArgumentException("Password cannot be null or empty.", nameof(password));
            }

            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);

            var filter = Builders<User>.Filter.Eq("_id", ObjectId.Parse(id));
            var update = Builders<User>.Update.Set("password", hashedPassword);

            var result = await _usersCollection.UpdateOneAsync(filter, update);

            return result.MatchedCount > 0;
        }

        public async Task<List<Device>> GetDevicesByOwnerAsync(string ownerId)
        {
            var filter = Builders<Device>.Filter.Eq("deviceOwner._id", ObjectId.Parse(ownerId));

            var devices = await _devicesCollection.Find(filter).ToListAsync();

            return devices;
        }

        public async Task<List<Device>> GetDevicesByUserAsync(string userId)
        {
            // Parse the user's ObjectId
            ObjectId userObjectId = ObjectId.Parse(userId);

            // Define a filter to find groups containing the user
            var filterGroups = Builders<Group>.Filter.AnyEq("Users._id", userObjectId);

            // Get the list of groups that the user belongs to
            var groups = await _groupsCollection.Find(filterGroups).ToListAsync();

            // Extract group IDs from the retrieved groups and  
            // convert to a list of ObjectId
            List<ObjectId> groupIds = groups.Select(g => ObjectId.Parse(g.Id)).ToList();

            // Define a filter to find devices based on group IDs
            var filterDevices = Builders<Device>.Filter.In("deviceGroup._id", groupIds);

            // Get the list of devices that belong to these groups
            var devices = await _devicesCollection.Find(filterDevices).ToListAsync();

            return devices;
        }

        public async Task<string> GetMonarchCoreInformationByActivationCodeAsync(int activationCode)
        {
            Device device;
            Group group;
            User owner;
            List<User> users;
            Vehicle vehicle;

            try
            {
                device = await GetDeviceByActivationCodeAsync(activationCode);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to get device by activation code {activationCode}.", ex);
            }

            try
            {
                group = await GetGroupByIdAsync(device.Group.Id);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to get group by ID: {device.Group.Id}.", ex);
            }

            try
            {
                owner = await GetUserByIdAsync(group.Owner.Id);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to get owner by ID: {group.Owner.Id}.", ex);
            }

            try
            {
                users = await GetUsersByIdsAsync(group.Users.Select(u => u.Id).ToList());
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to get users by IDs.", ex);
            }

            try
            {
                vehicle = await GetVehicleByIdAsync(device.Car.Id);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to get vehicle by ID: {device.Car.Id}.", ex);
            }

        
            var vehicleInfo = $"{vehicle.Make},{vehicle.Model},{vehicle.Color},{vehicle.Year},{vehicle.LicensePlate}";
            var ownerPhoneNumber = owner.PrimaryPhoneNumber.PhoneNumberValue.ToString();
            var ownerName = owner.FirstName + " " + owner.LastName;

            // Get all users' phone numbers and finish with a comma
            var usersInfo = string.Join(",", users.Select(u => u.PrimaryPhoneNumber.PhoneNumberValue.ToString())) + ",";
            var countOfUsers = users.Count + 1; // Owner is included in the count

            //TODO - Get local emergency number dynamically if necessary
            var emergencyNumber = "6063363510"; // Placeholder emergency number

            // Structure to follow for Monarch Core Information
            // Vehicle Info: Make, Model, Color, Year, License Plate, emergencyNumber, Owner's Name, Number of Users (owner included), Owner's Phone Number, Users' Phone Numbers , 
            var monarchCoreInfo = $"{vehicleInfo},{emergencyNumber},{ownerName}, {countOfUsers},{ownerPhoneNumber},{usersInfo}";

            return monarchCoreInfo ?? throw new InvalidOperationException("Failed to generate Monarch Core Information.");
        }



       

        #endregion

        #region Vehicles

        public async Task<Vehicle> GetVehicleByIdAsync(string id)
        {
            var objectId = new ObjectId(id); // Convert string ID to ObjectId

            var filter = Builders<Vehicle>.Filter.Eq("_id", objectId); // Filter by group ID
            var vehicle = await _vehiclesCollection.Find(filter).FirstOrDefaultAsync();

            return vehicle;
        }
        public async Task<List<Vehicle>> GetVehiclesByOwnerAsync(string ownerId)
        {
            var filter = Builders<Vehicle>.Filter.Eq("owner._id", ObjectId.Parse(ownerId));
            var vehicles = await _vehiclesCollection.Find(filter).ToListAsync();
            return vehicles;
        }

        #endregion

        #region Insert
        public async Task InsertUserAsync(User user, int deviceActivationNumber)
        {
            if(user == null)
            {
                throw new ArgumentNullException(nameof(user), "User object cannot be null.");
            }

            bool isEmailTaken = await IsEmailTakenAsync(user.Email);

            if(deviceActivationNumber == 0)
            {
                throw new ArgumentException(nameof(deviceActivationNumber), "Device Activation Number is required to create a new account.");
            }

            var device = await GetDeviceByActivationCodeAsync(deviceActivationNumber);

            if(device == null)
            {
                throw new ArgumentException(nameof(device), "Cannot find device. Double check the device activation number and try again.");
            }

            if (isEmailTaken)
            {
                throw new ArgumentNullException(nameof(user.Email), "Email is already being used.");
            }

            if (user.FirstName == null)
            {
                throw new ArgumentNullException(nameof(user.FirstName), "First name cannot be null.");
            }

            if (user.LastName == null)
            {
                throw new ArgumentNullException(nameof(user.LastName), "Last name cannot be null.");
            }

            if (user.Email == null)
            {
                throw new ArgumentNullException(nameof(user.Email), "Email cannot be null.");
            }

            if (user.PrimaryPhoneNumber == null)
            {
                throw new ArgumentNullException(nameof(user.PrimaryPhoneNumber), "Primary phone number cannot be null.");
            }

            var name = user.FirstName + " " + user.LastName;
            user.isTempUser = false;

            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(user.Password);
            user.Password = hashedPassword;
            user.isEmailVerified = false;

            using (var session = await client.StartSessionAsync())
            {
                // Start the transaction
                session.StartTransaction();

                try
                {
                    await _usersCollection.InsertOneAsync(session, user);

                    Group group = new Group();
                    group = group.CreateGroup("Family Name");

                    group.Owner = new NamedDocumentKey { Id = user.Id, Name = name };

                    device.Owner = new NamedDocumentKey { Id = user.Id, Name = name };

                    await _groupsCollection.InsertOneAsync(session, group);

                    // Update Owner and Status of the device
                    await _devicesCollection.UpdateOneAsync(session, Builders<Device>.Filter.Eq(d => d.Id, device.Id), Builders<Device>.Update
                        .Set(d => d.Owner, device.Owner)
                        .Set(d => d.Status, true)
                        .Set(d => d.Group, new NamedDocumentKey { Id = group.Id, Name = group.Name })
                    );

                    // Commit the transaction if all operations succeed
                    await session.CommitTransactionAsync();
                }
                catch (Exception ex)
                {
                    // Abort the transaction on error
                    await session.AbortTransactionAsync();
                    throw; // Rethrow the exception or handle it as needed
                }
            }
        }

        public async Task InsertTempUserAsync(User user, string groupId)
        {
            var emailExists = await GetUserByEmailAsync(user.Email) != null;

            if (emailExists == true)
            {
                throw new ArgumentNullException(nameof(user.Email), "Email already exists.");
            }

            var phoneNumberExists = await GetUserByPhoneNumberAsync(user.PrimaryPhoneNumber) != null;

            if (phoneNumberExists == true)
            {
                throw new ArgumentNullException(nameof(user), "User alraedy exists.");
            }

            if (user == null)
            {
                throw new ArgumentNullException(nameof(user), "User object cannot be null.");
            }

            if (user.Email == null)
            {
                throw new ArgumentNullException(nameof(user.Email), "Email cannot be null.");
            }

            if (user.FirstName == null)
            {
                throw new ArgumentNullException(nameof(user.FirstName), "First name cannot be null.");
            }

            if (user.LastName == null)
            {
                throw new ArgumentNullException(nameof(user.LastName), "Last name cannot be null.");
            }

            if (user.PrimaryPhoneNumber == null)
            {
                throw new ArgumentNullException(nameof(user.PrimaryPhoneNumber), "Primary phone number cannot be null.");
            }

            user.isEmailVerified = false;
            user.isTempUser = true;

            using (var session = await client.StartSessionAsync())
            {
                // Start the transaction
                session.StartTransaction();

                try
                {
                    await _usersCollection.InsertOneAsync(session, user);
                    
                    UserWithRole userWithRole = new UserWithRole(user.Id, user.FirstName + " " + user.LastName, RoleType.Member);
                    await _groupsCollection.UpdateOneAsync(session, Builders<Group>.Filter.Eq(g => g.Id, groupId), 
                    Builders<Group>.Update.Push(u => u.Users, userWithRole));

                    // Commit the transaction if all operations succeed
                    await session.CommitTransactionAsync();
                }
                catch (Exception ex)
                {
                    // Abort the transaction on error
                    await session.AbortTransactionAsync();
                    throw; // Rethrow the exception or handle it as needed
                }
            }
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

            await _groupsCollection.InsertOneAsync(group);
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

            await _devicesCollection.InsertOneAsync(device);
        }

        public async Task InsertVehicleAsync(Vehicle vehicle)   
        {
            if (vehicle == null)
            {
                throw new ArgumentNullException(nameof(vehicle), "Vehicle object cannot be null.");
            }

            if (vehicle.Owner == null)
            {
                throw new ArgumentNullException(nameof(vehicle.Owner), "Missing vehicle owner. This field cannot be null.");
            }

            if (vehicle.Name == null)
            {
                throw new ArgumentNullException(nameof(vehicle.Name), "Missing vehicle name. This field cannot be null.");
            }

            if (vehicle.Make == null)
            {
                throw new ArgumentNullException(nameof(vehicle.Make), "Missing vehicle make. This field cannot be null.");
            }

            if (vehicle.Model == null)
            {
                throw new ArgumentNullException(nameof(vehicle.Model), "Missing vehicle model. This field cannot be null.");
            }

            if (vehicle.Color == null)
            {
                throw new ArgumentNullException(nameof(vehicle.Color), "Missing vehicle color. This field cannot be null.");
            }

            if (vehicle.LicensePlate == null)
            {
                throw new ArgumentNullException(nameof(vehicle.LicensePlate), "Missing vehicle license plate. This field cannot be null.");
            }

            await _vehiclesCollection.InsertOneAsync(vehicle);
        }

        #endregion

        #region Update
        public async Task<bool> UpdateUserAsync(string id, User user)
        {

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
                //.Set("userName", user.UserName)
                .Set("firstName", user.FirstName)
                .Set("lastName", user.LastName)
                .Set("email", user.Email)
                .Set("primaryPhoneNumber", user.PrimaryPhoneNumber)
                .Set("secondaryPhoneNumbers", user.SecondaryPhoneNumbers)
                .Set("isEmailVerified", user.isEmailVerified)
                .Set("isTempUser", user.isTempUser);

            var result = await _usersCollection.UpdateOneAsync(filter, update);

            return result.MatchedCount > 0;
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

            if(!group.Users.IsNullOrEmpty())
            {
                if(group.Users.All(u => u.Role != RoleType.Admin && u.Role != RoleType.Member))
                {
                    throw new ArgumentException(nameof(group.Users), "Invalid role type. Role type must be either 'Admin' or 'Member'.");
                }
            }

            

            var filter = Builders<Group>.Filter.Eq(g => g.Id, id);
            var update = Builders<Group>.Update
                .Set(g => g.Name, group.Name)
                .Set(g => g.Owner, group.Owner)
                .Set(g => g.Users, group.Users);

            var result = await _groupsCollection.UpdateOneAsync(filter, update);
            return result.MatchedCount > 0;
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

            var result = await _devicesCollection.UpdateOneAsync(filter, update);
            return result.ModifiedCount > 0;
        }

        public async Task<bool> UpdateVehicleAsync(string id, Vehicle updatedVehicle)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentException("Vehicle ID cannot be null or empty.", nameof(id));
            }


            if (updatedVehicle == null)
            {
                throw new ArgumentNullException(nameof(updatedVehicle), "Vehicle object cannot be null.");
            }

            var filter = Builders<Vehicle>.Filter.Eq(d => d.Id, id);
            var update = Builders<Vehicle>.Update
                .Set(d => d.Owner, updatedVehicle.Owner)
                .Set(d => d.Name, updatedVehicle.Name)
                .Set(d => d.Make, updatedVehicle.Make)
                .Set(d => d.Model, updatedVehicle.Model)
                .Set(d => d.Year, updatedVehicle.Year)
                .Set(d => d.Color, updatedVehicle.Color)
                .Set(d => d.LicensePlate, updatedVehicle.LicensePlate);

            var result = await _vehiclesCollection.UpdateOneAsync(filter, update);
            return result.ModifiedCount > 0;
        }
        #endregion

        #region Delete
        public async Task<bool> DeleteUserAsync(string id)
        {
            var filter = Builders<User>.Filter.Eq("_id", ObjectId.Parse(id));
            var result = await _usersCollection.DeleteOneAsync(filter);

            return result.DeletedCount > 0;
        }

        public async Task<bool> DeleteGroupAsync(string id)
        {
            var filter = Builders<Group>.Filter.Eq("_id", ObjectId.Parse(id));
            var result = await _groupsCollection.DeleteOneAsync(filter);

            return result.DeletedCount > 0;
        }

        public async Task<bool> DeleteDeviceAsync(string id)
        {
            var filter = Builders<Device>.Filter.Eq("_id", ObjectId.Parse(id));
            var result = await _devicesCollection.DeleteOneAsync(filter);

            return result.DeletedCount > 0;
        }

        public async Task<bool> DeleteVehicleAsync(string id)
        {
            var filter = Builders<Vehicle>.Filter.Eq("_id", ObjectId.Parse(id));
            var result = await _vehiclesCollection.DeleteOneAsync(filter);

            return result.DeletedCount > 0;
        }
        #endregion 
    
        #region Validation
        public async Task<bool> IsEmailTakenAsync(string email)
        {
            // Look for a user with the same email
            var filter = Builders<User>.Filter.Eq("email", email);
            var user = await _usersCollection.Find(filter).FirstOrDefaultAsync();
            return user != null;
        }
        #endregion

        #region Roles
        public async Task<List<Role>> GetRolesAsync()
        {
            var roles = await _rolesCollection.Find(_ => true).ToListAsync();
            return roles;
        }  

        public async Task InsertRoleAsync(Role role)
        {
            if (role == null)
            {
                throw new ArgumentNullException(nameof(role), "Role object cannot be null.");
            }

            if (RoleType.Admin != role.Name && RoleType.Member != role.Name )
            {
                throw new ArgumentException(nameof(role.Name), "Role name is invalid.");
            }

            if (role.Permissions == null)
            {
                throw new ArgumentNullException(nameof(role.Permissions), "Role permissions cannot be null.");
            }

            if (role.Description == null)
            {
                throw new ArgumentNullException(nameof(role.Description), "Role description cannot be null.");
            }

            await _rolesCollection.InsertOneAsync(role);
        }

        #endregion

        public async Task AddToGroupByGroupId(User user, string groupId)
        {
            var filter = Builders<Group>.Filter.Eq(g => g.Id, groupId);
            var fullName = user.FirstName + " " + user.LastName;

            if(user == null)
            {
                throw new ArgumentNullException(nameof(user), "User object cannot be null.");
            }
            if(user.Id == null)
            {
                throw new ArgumentNullException(nameof(user.Id), "User ID cannot be null.");
            }

            var userWithRole = new UserWithRole (user.Id, fullName, RoleType.Member) ;

            var update = Builders<Group>.Update.Push(g => g.Users, userWithRole);

            await _groupsCollection.UpdateOneAsync(filter, update);
        }

    }
}
