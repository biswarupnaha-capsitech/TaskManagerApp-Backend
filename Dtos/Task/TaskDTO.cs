

namespace TaskManager.Dtos.Task
{
    public class TaskDTO
    {
        public string Id { get; set; } = null!;
        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;
        public TaskManager.Common.TaskStatus Status { get; set; } = TaskManager.Common.TaskStatus.Pending;
        public bool IsDeleted { get; set; } = false;
        public DateTime DueDate { get; set; }
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
        public TaskManager.Common.TaskStatus Status { get; set; } = TaskManager.Common.TaskStatus.Pending;
    }

    public class TodoFilter
    {
        public bool? IsCompleted { get; set; }

        public string? Title { get; set; }

        public DateTime? CreatedAfter { get; set; }
    }
}
