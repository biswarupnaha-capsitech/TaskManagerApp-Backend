

namespace Projects.Dtos.Task
{
    public class TaskDTO
    {
        public string Id { get; set; } = null!;
        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;
        public Projects.Common.TaskStatus Status { get; set; } = Projects.Common.TaskStatus.Pending;
        public bool IsDeleted { get; set; } = false;
    }

    public class CreateTaskDTO
    {
        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;
    }


    public class UpdateTaskDTO
    {
        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;
        public Projects.Common.TaskStatus Status { get; set; } = Projects.Common.TaskStatus.Pending;
    }

    public class TodoFilter
    {
        public bool? IsCompleted { get; set; }

        public string? Title { get; set; }

        public DateTime? CreatedAfter { get; set; }
    }
}
