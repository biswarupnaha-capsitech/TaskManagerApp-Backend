using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Projects.Models.Common
{
    public class IdName
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = null!;
        public string Name { get; set; } = null!;
    }
}
