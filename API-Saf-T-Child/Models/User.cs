using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Text.Json.Serialization;

namespace Saf_T_Child_API_1.Models
{
    public class User
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("firstName")]
        public string? FirstName { get; set; }

        [BsonElement("lastName")]
        public string? LastName { get; set; }

        [BsonElement("email")]
        public List<String>? Email { get; set; }

        [BsonElement("primaryPhoneNumber")]
        public int? PrimaryPhone { get; set; }

        [BsonElement("secondaryPhoneNumbers")]
        public List<int>? SecondaryNumbers { get; set; }

    }
}
