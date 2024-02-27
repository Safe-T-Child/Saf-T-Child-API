using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Saf_T_Child_API_1.Models;

namespace API_Saf_T_Child.Models
{
    public class Group
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("name")]
        public string Name { get; set; }

        [BsonElement("owner")]
        public User owner { get; set; }

        [BsonElement("users")]
        public List<User> Users { get; set; }
    }
}
