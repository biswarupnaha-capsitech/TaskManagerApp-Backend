

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
        public string ProjectId { get; set; } = null!; 
        public DateTime DueDate { get; set; }
    }


    public class UpdateTaskDTO
    {
        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;
        public TaskManager.Common.TaskStatus Status { get; set; } = TaskManager.Common.TaskStatus.Pending;
        public DateTime DueDate { get; set; }
        public bool IsDeleted { get; set; }
    }
}
