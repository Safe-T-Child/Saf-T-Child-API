using System.ComponentModel.DataAnnotations;
using MongoDB.Bson.Serialization.Attributes;

namespace API_Saf_T_Child.Models
{
    public class PhoneNumberDetails
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
}