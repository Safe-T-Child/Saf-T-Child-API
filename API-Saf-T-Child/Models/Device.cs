using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Text.Json.Serialization;

namespace API_Saf_T_Child.Models
{
    public class Device
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("name")]
        public string? Name { get; set; }
        
        [BsonElement("type")]
        public string? Type { get; set; }

        [BsonElement("model")]
        public string? Model { get; set; }
        
        [BsonElement("serialNumber")]
        public string? DeviceId { get; set; }
        
        [BsonElement("manufacturer")]
        public string? Manufacturer { get; set; }

        [BsonElement("resetCode")]
        public int? ResetCode { get; set; }

        [BsonElement("status")]
        public string? Status { get; set; }

        [BsonElement("owner")]
        public string? Owner { get; set; }

        [BsonElement("secondaryUsers")]
        public List<string>? SecondaryUsers { get; set; }
    }
}
