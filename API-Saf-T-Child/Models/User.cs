using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace Saf_T_Child_API_1.Models
{
    public class User
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        //These Bsonelements are the fields that will be stored in the database
        //Since in mongoDB the fields are case sensitive, we need to make sure that the fields are the same as the ones in the database
        //Camel case is used for the fields in the database
        [BsonElement("userType")]
        [Required]
        public string UserType { get; set; }

        [BsonElement("username")]
        public string Username { get; set; }

        [BsonElement("firstName")]
        [Required]
        public string FirstName { get; set; }

        [BsonElement("lastName")]
        [Required]
        public string LastName { get; set; }

        [BsonElement("email")]
        [Required]
        public List<string> Email { get; set; }

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
