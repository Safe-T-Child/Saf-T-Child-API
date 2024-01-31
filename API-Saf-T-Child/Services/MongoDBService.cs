using Saf_T_Child_API_1.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using MongoDB.Bson;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Saf_T_Child_API_1.Services
{
    public class MongoDBService
    {
        private readonly IMongoCollection<User> _userCollection;

        public MongoDBService(IOptions<MongoDBSettings> mongoDBSettings)
        {
            MongoClient client = new MongoClient(mongoDBSettings.Value.ConnectionURI);
            IMongoDatabase database = client.GetDatabase(mongoDBSettings.Value.DatabaseName);
            _userCollection = database.GetCollection<User>(mongoDBSettings.Value.CollectionName);
        }

        public async Task<List<User>> GetUsersAsync()
        {
            var users = await _userCollection.Find(_ => true).ToListAsync();
            return users;
        }

        public async Task InsertUserAsync(User user)
        {
            await _userCollection.InsertOneAsync(user);
        }

        public async Task<bool> UpdateUserAsync(string id, User user)
        {
            var filter = Builders<User>.Filter.Eq("_id", ObjectId.Parse(id));
            var update = Builders<User>.Update
                .Set("Name", user.FirstName) // Update other properties as needed
                                        // Add more update operations as needed
                .CurrentDate("LastModified");

            var result = await _userCollection.UpdateOneAsync(filter, update);

            return result.ModifiedCount > 0;
        }

        public async Task<bool> DeleteUserAsync(string id)
        {
            var filter = Builders<User>.Filter.Eq("_id", ObjectId.Parse(id));
            var result = await _userCollection.DeleteOneAsync(filter);

            return result.DeletedCount > 0;
        }
    }
}
