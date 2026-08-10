using TaskManager.Dtos.Common;
using TaskManager.Dtos.Task;

namespace TaskManager.Dtos.Project
{
    public class ProjectQueryDTO : PaginatedQueryDto
    {
            public int? Year { get; set; }
            public int? Month { get; set; }
            public int? Day { get; set; }
    }

    public sealed class ProjectWithTasksDTO : ProjectDTO
    {
        public List<TaskDateGroupDTO> ProjectTasks { get; set; } = [];
    }

    public sealed class TaskDateGroupDTO
    {
        public DateTime Date { get; set; }

        public List<TaskDTO> Tasks { get; set; } = [];
    }
}
