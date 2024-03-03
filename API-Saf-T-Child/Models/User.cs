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
        public string? FirstName { get; set; }

        [BsonElement("lastName")]
        [Required]
        public string? LastName { get; set; }

        [BsonElement("userName")]
        public string UserName { get; set; }

        [BsonElement("email")]
        [Required]
        public List<String>? Email { get; set; }

        [BsonElement("primaryPhoneNumber")]
        [Required]
        public PhoneNumber PrimaryPhoneNumber { get; set; }

        [BsonElement("secondaryPhoneNumbers")]
        public List<PhoneNumber> SecondaryPhoneNumbers { get; set; }

        public class PhoneNumber
        {
            [BsonElement("areaCode")]
            [RegularExpression("^[0-9]+$")]
            [Required]
            public string AreaCode { get; set; }

            [BsonElement("phoneNumber")]
            [RegularExpression("^[0-9]+$")]
            [Required]
            public string PhoneNumberValue { get; set; }
        }

    }
}
