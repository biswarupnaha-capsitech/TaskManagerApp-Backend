using Projects.Dtos.Common;

namespace Projects.Dtos.Task
{
    public class TaskQueryDTO : PaginatedQueryDto
    {
        public string? SearchWord { get; set; }
    }
}
