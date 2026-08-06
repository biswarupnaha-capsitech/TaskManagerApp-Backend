using TaskManager.Models.Base;

namespace TaskManager.Models
{
    public class Task : BaseModel
    {
        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;
        public TaskManager.Common.TaskStatus Status { get; set; } = TaskManager.Common.TaskStatus.Pending;
        public bool IsDeleted { get; set; } = false;
        public string UserId { get; set; } = null!;
    }
}
