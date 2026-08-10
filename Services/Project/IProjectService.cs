using TaskManager.Dtos.Common;
using TaskManager.Dtos.Project;
using TaskManager.Dtos.Task;

namespace TaskManager.Services.Project
{
    public interface IProjectService
    {
        Task<PaginatedResultDto<ProjectWithTasksDTO>> GetAsync(ProjectQueryDTO query);
        Task<ProjectDTO> CreateAsync(MutateProjectDTO project);
        System.Threading.Tasks.Task UpdateAsync(string id, MutateProjectDTO project);
        System.Threading.Tasks.Task DeleteAsync(string id, bool hardDelete = true);
    }
}
