using Projects.Models.Base;

namespace Projects.Models
{
    public class Task : BaseModel
    {
        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;
        public Projects.Common.TaskStatus Status { get; set; } = Projects.Common.TaskStatus.Pending;
        public bool IsDeleted { get; set; } = false;
    }
}
