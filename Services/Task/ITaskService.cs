using Projects.Dtos.Common;
using Projects.Dtos.Task;

namespace Projects.Services.Task
{
    public interface ITaskService
    {
        Task<PaginatedResultDto<TaskDTO>> GetAsync(PaginatedQueryDto query);
        Task<TaskDTO> CreateAsync(CreateTaskDTO task);
        System.Threading.Tasks.Task UpdateAsync(string id, UpdateTaskDTO task);
        System.Threading.Tasks.Task DeleteAsync(string id, bool hardDelete=true);
    }
}
