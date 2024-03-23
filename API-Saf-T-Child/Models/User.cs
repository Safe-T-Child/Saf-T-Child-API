using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;

namespace API_Saf_T_Child.Models
{
    public class User
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("firstName")]
        [Required]
        public string FirstName { get; set; }

        [BsonElement("lastName")]
        [Required]
        public string LastName { get; set; }

        [BsonElement("userName")]
        public string UserName { get; set; }

        [BsonElement("email")]
        [Required]
        public String Email { get; set; }

        [BsonElement("primaryPhoneNumber")]
        [Required]
        public PhoneNumber PrimaryPhoneNumber { get; set; }

        [BsonElement("secondaryPhoneNumbers")]
        public List<PhoneNumber>? SecondaryPhoneNumbers { get; set; }

        [BsonElement("isEmailVerified")]
        public bool isEmailVerified { get; set; }

        public class PhoneNumber
        {
            [BsonElement("countryCode")]
            [RegularExpression("^[0-9]+$")]
            [Required]
            public int CountryCode { get; set; }

            [BsonElement("phoneNumber")]
            [RegularExpression("^[0-9]+$")]
            [Required]
            public long PhoneNumberValue { get; set; }
        }

        [BsonElement("Name")]
        [Required]
        public string? Name { get; set; }

    }
}
