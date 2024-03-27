
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;

namespace API_Saf_T_Child.Models
{
    public class PhoneNumber
    {
        [BsonElement("countryCode")]
        [RegularExpression("^[0-9]+$")]
        [Required]
        public int CountryCode { get; set; }

        [BsonElement("phoneNumber")]
        [RegularExpression("^[0-9]+$")]
        [Required]
        public int PhoneNumberValue { get; set; }
    }
}