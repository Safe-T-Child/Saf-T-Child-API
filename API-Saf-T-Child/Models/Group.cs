using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using API_Saf_T_Child.Models;

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
        public User Owner { get; set; }

        [BsonElement("users")]
        public List<User> Users { get; set; }
    }
}
