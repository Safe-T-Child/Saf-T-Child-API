using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using API_Saf_T_Child.Models;

namespace API_Saf_T_Child.Models
{
    public class NamedDocumentKey
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("name")]
        public string Name { get; set; }

    }
}
