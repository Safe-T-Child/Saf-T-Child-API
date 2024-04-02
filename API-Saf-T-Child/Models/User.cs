using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;

namespace API_Saf_T_Child.Models
{
    public class User
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("firstName")]
        [Required]
        public string FirstName { get; set; }

        [BsonElement("lastName")]
        [Required]
        public string LastName { get; set; }

        [BsonElement("email")]
        [Required]
        public String Email { get; set; }

        [BsonElement("primaryPhoneNumber")]
        [Required]
        public PhoneNumber PrimaryPhoneNumber { get; set; }

        [BsonElement("password")]
        public string? Password { get; set; }

        [BsonElement("secondaryPhoneNumbers")]
        public List<PhoneNumber>? SecondaryPhoneNumbers { get; set; }

        [BsonElement("isEmailVerified")]
        public bool isEmailVerified { get; set; }

        [BsonElement("isTempUser")]
        public bool isTempUser { get; set; }

        

    }
}
