using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using TaskManager.Models.Base;

namespace TaskManager.Models
{
    public class Task : BaseModel
    {
        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;
        public TaskManager.Common.TaskStatus Status { get; set; } = TaskManager.Common.TaskStatus.Pending;
        public bool IsDeleted { get; set; } = false;
        public DateTime DueDate { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public string UserId { get; set; } = null!;

        [BsonRepresentation(BsonType.ObjectId)]
        public string ProjectId { get; set; } = null!;
    }
}
