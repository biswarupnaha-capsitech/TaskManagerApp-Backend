namespace Projects.Services.Task
{
    public interface ITaskService
    {
        Task<List<Models.Task>> GetAsync();
        Task<Models.Task?> GetAsync(string id);
        Task<Models.Task?> CreateAsync(Models.Task task);
        System.Threading.Tasks.Task UpdateAsync(string id, Models.Task task);
        System.Threading.Tasks.Task DeleteAsync(string id);
    }
}
