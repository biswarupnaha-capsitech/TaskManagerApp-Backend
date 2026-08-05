namespace TaskManagerApp.Dtos.Task
{
    public class CreateTaskDTO
    {
        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;
        public Projects.Common.TaskStatus Status { get; set; } = Projects.Common.TaskStatus.Pending;
        public bool IsDeleted { get; set; } = false;
    }
}
