using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace API_Saf_T_Child.Models
{

    //make a string type enum for the role type
    public static class RoleType
    {
        public const string Admin = "Admin";
        public const string Member = "Member";
    }

    public class Role
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("name")]
        public string Name { get; set; }

        [BsonElement("permissions")]
        public string[] Permissions { get; set; }

        [BsonElement("description")]
        public string Description { get; set; }


    }
}