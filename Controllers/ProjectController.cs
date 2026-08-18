using Capsitech;
using Capsitech.Data.MongoDB;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using TaskManager.Common;
using TaskManager.Dtos.Common;
using TaskManager.Dtos.Project;
using TaskManager.Services.Project;

namespace TaskManager.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = "Bearer")]
    [Authorize(Roles = "ADMIN")]
    [EnableRateLimiting("api")]
    public class ProjectController : ApiControllerBase
    {
        private readonly ILogger<TaskController> _logger;
        private readonly IProjectService _projectService;

        public ProjectController(DBConfiguration dbConfig, ILogger<TaskController> logger, IProjectService projectService) : base(dbConfig)
        {
            _logger = logger;
            _projectService = projectService;
        }


        [HttpGet("GetProjects")]
        public async Task<ApiResponse<PaginatedResultDto<ProjectWithTasksDTO>>> GetProjects([FromQuery] ProjectQueryDTO query)
        {
            ApiResponse<PaginatedResultDto<ProjectWithTasksDTO>> response = new();
            try
            {
                var projects = await _projectService.GetAsync(query);
                response.Message = "Projects retrieved successfully.";
                response.Result = projects;
            }
            catch(Exception ex)
            {
                response.Message = ex.Message;
                response.AddError(ex.Message);
                _logger.LogError(ex, "Error retrieving projects: {Message}", ex.Message);
            }

            return response;
        }

        [HttpPost("CreateProject")]
        public async Task<ApiResponse<ProjectDTO>> CreateProject([FromBody] CreateProjectDTO project)
        {
            ApiResponse<ProjectDTO> response = new();
            try
            {
                var createdProject = await _projectService.CreateAsync(project);
                _logger.LogInformation("Project created with ID: {Id}", createdProject?.Id);
                response.Message = "Project created successfully";
                response.Result = createdProject!;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating project");
                response.AddError(ex.Message);
            }
            return response;
        }

        [HttpPut("UpdateProject/{id}")]
        public async Task<ApiResponse<ProjectDTO>> UpdateProject(string id, [FromBody] UpdateProjectDTO project)
        {
            ApiResponse<ProjectDTO> response = new();
            try
            {
                await _projectService.UpdateAsync(id, project);
                _logger.LogInformation("Project updated with ID: {Id}", id);
                response.Message = "Project updated successfully";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating project");
                response.AddError(ex.Message);
            }
            return response;
        }

        [HttpDelete("DeleteProject/{id}")]
        public async Task<ApiResponse<bool>> DeleteProject(string id)
        {
            ApiResponse<bool> response = new();
            try
            {
                await _projectService.DeleteAsync(id);
                _logger.LogInformation("Project deleted with ID: {Id}", id);
                response.Message = "Project deleted successfully";
                response.Result = true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting project");
                response.AddError(ex.Message);
            }
            return response;
        }
    }
}
