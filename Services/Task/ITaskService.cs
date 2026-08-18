using TaskManager.Dtos.Common;
using TaskManager.Dtos.Task;

namespace TaskManager.Services.Task
{
    public interface ITaskService
    {
        Task<PaginatedResultDto<TaskDTO>> GetAsync(PaginatedQueryDto query);
        Task<TaskDTO> CreateAsync(CreateTaskDTO task);
        System.Threading.Tasks.Task UpdateAsync(string id, UpdateTaskDTO task);
        System.Threading.Tasks.Task DeleteAsync(string id, bool hardDelete=true);
        System.Threading.Tasks.Task CompleteTasksByProjectAsync(string projectId);
    }
}
