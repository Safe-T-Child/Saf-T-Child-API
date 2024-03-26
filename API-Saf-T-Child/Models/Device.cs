using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Text.Json.Serialization;
using API_Saf_T_Child.Models;

namespace API_Saf_T_Child.Models
{
    public class Device
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("deviceType")]
        public string Type { get; set; }

        [BsonElement("deviceName")]
        public string Name { get; set; }

        [BsonElement("deviceModel")]
        public string Model { get; set; }
        
        [BsonElement("deviceSerial")]
        public string DeviceId { get; set; }

        [BsonElement("deviceStatus")]
        public string Status { get; set; }

        [BsonElement("deviceActivationCode")]
        public int DeviceActivationCode { get; set; }

        [BsonElement("car")]
        public NamedDocumentKey Car { get; set; }

        [BsonElement("deviceOwner")]
        public NamedDocumentKey? Owner { get; set; }

        [BsonElement("deviceGroup")]
        public NamedDocumentKey? Group { get; set; }
    }
}
