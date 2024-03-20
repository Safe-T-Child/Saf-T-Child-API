using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Text.Json.Serialization;
using API_Saf_T_Child.Models;

namespace API_Saf_T_Child.Models
{
    public class Vehicle{
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("owner")]
        public NamedDocumentKey Owner { get; set; }

        [BsonElement("name")]
        public string Name { get; set; }

        [BsonElement("make")]
        public string Make { get; set; }

        [BsonElement("model")]
        public string Model { get; set; }
        
        [BsonElement("year")]
        public int Year { get; set; }

        [BsonElement("color")]
        public string Color { get; set; }

        [BsonElement("licensePlate")]
        public string LicensePlate { get; set; }

    }
}