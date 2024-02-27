using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Text.Json.Serialization;
using Saf_T_Child_API_1.Models;

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

        [BsonElement("car")]
        public string Car { get; set; }

        [BsonElement("deviceStatus")]
        public string Status { get; set; }

        [BsonElement("deviceOwner")]
        public User Owner { get; set; }

        [BsonElement("deviceGroup")]
        public int GroupID { get; set; }
    }
}
