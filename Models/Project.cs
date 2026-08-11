using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using TaskManager.Models.Base;

namespace TaskManager.Models
{
    public class Project : BaseModel
    {
        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;
        public bool IsDeleted { get; set; }
        public bool IsCompleted { get; set; } = false;

        [BsonRepresentation(BsonType.ObjectId)]
        public string UserId { get; set; } = null!;
    }
}
