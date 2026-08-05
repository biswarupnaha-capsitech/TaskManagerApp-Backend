using TaskManagerApp.Dtos.Task;

namespace Projects.Services.Task
{
    public interface ITaskService
    {
        Task<List<Models.Task>> GetAsync();
        Task<Models.Task?> GetAsync(string id);
        Task<Models.Task?> CreateAsync(CreateTaskDTO task);
        System.Threading.Tasks.Task UpdateAsync(string id, UpdateTaskDTO task);
        System.Threading.Tasks.Task DeleteAsync(string id);
    }
}
