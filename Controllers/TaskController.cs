using Capsitech;
using Capsitech.Data.MongoDB;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using TaskManager.Common;
using TaskManager.Dtos.Common;
using TaskManager.Dtos.Task;
using TaskManager.Services.Task;

namespace TaskManager.Controllers
{
    [Route("api/[controller]")]
    [Authorize(AuthenticationSchemes = "Bearer")]
    [Authorize(Roles = "ADMIN")]
    [EnableRateLimiting("api")]
    [ApiController]
    public class TaskController : ApiControllerBase
    {
        private readonly ILogger<TaskController> _logger;
        private readonly ITaskService _taskService;


        public TaskController(ILogger<TaskController> logger, ITaskService taskService, DBConfiguration dbConfig) : base(dbConfig)
        {
            _logger = logger;
            _taskService = taskService;
        }


        [HttpGet("GetTasks")]
        public async Task<ApiResponse<PaginatedResultDto<TaskDTO>>> GetTasks([FromQuery] PaginatedQueryDto query)
        {
            ApiResponse<PaginatedResultDto<TaskDTO>> response = new();
            try
            {
                var tasks = await _taskService.GetAsync(query);
                _logger.LogInformation("Retrieved {Count} tasks", tasks.Total);
                response.Message = "Tasks retrieved successfully";
                response.Result = tasks;
            }
            catch (Exception ex)
            {

                _logger.LogError(ex, "Error fetching tasks");
                response.AddError(ex.Message);
            }
            return response;
        }

        [HttpPost("CreateTask")]
        public async Task<ApiResponse<TaskDTO>> CreateTask([FromBody] CreateTaskDTO task)
        {
            ApiResponse<TaskDTO> response = new();
            try
            {
                var createdTask = await _taskService.CreateAsync(task);
                _logger.LogInformation("Task created with ID: {Id}", createdTask?.Id);
                response.Message = "Task created successfully";
                response.Result = createdTask!;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating task");
                response.AddError(ex.Message);
            }
            return response;
        }

        [HttpPut("UpdateTask/{id}")]
        public async Task<ApiResponse<TaskDTO>> UpdateTask(string id, [FromBody] UpdateTaskDTO task)
        {
            ApiResponse<TaskDTO> response = new();
            try
            {
                await _taskService.UpdateAsync(id, task);
                _logger.LogInformation("Task updated with ID: {Id}", id);
                response.Message = "Task updated successfully";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating task");
                response.AddError(ex.Message);
            }
            return response;
        }

        [HttpDelete("DeleteTask/{id}")]
        public async Task<ApiResponse<bool>> DeleteTask(string id)
        {
            ApiResponse<bool> response = new();
            try
            {
                await _taskService.DeleteAsync(id);
                _logger.LogInformation("Task deleted with ID: {Id}", id);
                response.Message = "Task deleted successfully";
                response.Result = true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting task");
                response.AddError(ex.Message);
            }
            return response;
        }
    }
}
