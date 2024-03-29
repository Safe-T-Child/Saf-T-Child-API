using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using API_Saf_T_Child.Models;



namespace API_Saf_T_Child.Models
{
    public class UserWithRole
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("name")]
        public string Name { get; set; }

        [BsonElement("role")]
        public string Role { get; set; }
    }
    public class Group
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("name")]
        public string Name { get; set; }

        [BsonElement("owner")]
        public NamedDocumentKey Owner { get; set; }

        [BsonElement("users")]
        public List<UserWithRole> Users { get; set; }

        public Group CreateGroup(string name)
        {
            return new Group
            {
                Name = name,
                Users = new List<UserWithRole>()
            };
        }
    }
}
