using TaskManager.Dtos.Common;
using TaskManager.Dtos.Task;

namespace TaskManager.Dtos.Project
{
    public class ProjectQueryDTO : PaginatedQueryDto
    {
        public int? Year { get; set; }
        public int? Month { get; set; }
        public int? Day { get; set; }
        public DateTime? EndDate { get; set; }
    }

    public sealed class ProjectWithTasksDTO : ProjectDTO
    {
        public List<TaskForProjectDTO> Tasks { get; set; } = [];
    }

    public class ProjectWithRawTasks : Models.Project
    {
        public List<Models.Task> Tasks { get; set; } = new();
    }
}
