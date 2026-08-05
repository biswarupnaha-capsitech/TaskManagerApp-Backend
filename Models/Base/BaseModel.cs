using Projects.Models.Common;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Projects.Models.Base
{
    public class BaseModel
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; } = DateTime.UtcNow;

        public IdName? CreatedBy { get; set; } = null;
        public IdName? UpdatedBy { get; set; } = null;
    }
}
