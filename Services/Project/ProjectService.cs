using System.Text.Json;
using System.Text.RegularExpressions;
using Capsitech.Extensions;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using TaskManager.Config.Db;
using TaskManager.Dtos.Common;
using TaskManager.Dtos.Project;
using TaskManager.Dtos.Task;
using TaskManager.Models;

namespace TaskManager.Services.Project
{
    public class ProjectService : IProjectService
    {
        private readonly IMongoCollection<Models.Project>? _projectCollection;
        private readonly IMongoCollection<Models.Task>? _taskCollection;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ProjectService(IOptions<DbSettings> dbSettings, IHttpContextAccessor httpContextAccessor)
        {
            var mongoClient = new MongoClient(dbSettings.Value.ConnectionString);
            var database = mongoClient.GetDatabase(dbSettings.Value.DatabaseName);
            _taskCollection = database.GetCollection<Models.Task>(dbSettings.Value.TasksCollectionName);
            _projectCollection = database.GetCollection<Models.Project>(dbSettings.Value.ProjectsCollectionName);
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<ProjectDTO> CreateAsync(CreateProjectDTO project)
        {
            var userId = _httpContextAccessor.HttpContext?.User.GetUserId();
            var newProject = new Models.Project
            {
                Title = project.Title,
                Description = project.Description!,
                IsDeleted = false,
                IsCompleted = false,
                UserId = userId!
            };
            await _projectCollection!.InsertOneAsync(newProject);
            return new ProjectDTO
            {
                Id = newProject.Id!,
                Title = newProject.Title,
                Description = newProject.Description,
                IsDeleted = newProject.IsDeleted,
                IsCompleted = newProject.IsCompleted
            };
        }

        public async System.Threading.Tasks.Task DeleteAsync(string id, bool hardDelete = true)
        {
            if (hardDelete)
            {
                await _projectCollection.DeleteOneAsync(x => x.Id == id);
            }
            else
            {
                var userId = _httpContextAccessor.HttpContext?.User.GetUserId();
                await _projectCollection.UpdateOneAsync(x => x.Id == id && x.UserId == userId,
                    Builders<Models.Project>.Update
                    .Set(x => x.IsDeleted, true)
                    .Set(x => x.UpdatedAt, DateTime.UtcNow));
            }
        }

        public async System.Threading.Tasks.Task UpdateAsync(string id, UpdateProjectDTO project)
        {
            var userId = _httpContextAccessor.HttpContext?.User.GetUserId();
            var update = Builders<Models.Project>.Update
                            .Set(x => x.Title, project.Title)
                            .Set(x => x.Description, project.Description)
                            .Set(x => x.IsCompleted, project.IsCompleted)
                            .Set(x => x.UpdatedAt, DateTime.UtcNow);

            await _projectCollection.UpdateOneAsync(x => x.Id == id, update);
        }

        public async Task<PaginatedResultDto<ProjectWithTasksDTO>> GetAsync(ProjectQueryDTO query)
        {
            var projectBuilder = Builders<Models.Project>.Filter;
            var taskBuilder = Builders<Models.Task>.Filter;
            var userId = _httpContextAccessor.HttpContext?.User.GetUserId();

            if (string.IsNullOrEmpty(userId))
                throw new UnauthorizedAccessException("User ID not found in the token.");

            var year = query.Year ?? DateTime.UtcNow.Year;
            var month = query.Month ?? DateTime.UtcNow.Month;
            var day = query.Day ?? DateTime.UtcNow.Day;

            if (month < 1 || month > 12)
                throw new ArgumentException("Month must be between 1 and 12.");


            var startDate = query.Day.HasValue ?
                new DateTime(year, month, day, 0, 0, 0, DateTimeKind.Utc) :
                new DateTime(year, month, 1, 0, 0, 0, DateTimeKind.Utc);
            var endDate = query.EndDate?.ToUniversalTime() ?? startDate.AddMonths(1);

            var projectFilter = projectBuilder.Empty;
            projectFilter &= projectBuilder.Eq(x => x.UserId, userId);
            projectFilter &= projectBuilder.Eq(x => x.IsDeleted, false);



            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                var regex = new BsonRegularExpression(Regex.Escape(query.Search), "i");

                projectFilter &= projectBuilder.Or(
                    projectBuilder.Regex(x => x.Id, regex),
                            projectBuilder.Regex(x => x.Title, regex),
                            projectBuilder.Regex(x => x.Description, regex)
                        );
            }

            var aggregatePipeline =
                _projectCollection.Aggregate()
                .Match(projectFilter)
                .Lookup<Models.Project, Models.Task, ProjectWithRawTasks>(
                    foreignCollection: _taskCollection,
                    localField: p => p.Id,
                    foreignField: t => t.ProjectId,
                    @as: p => p.Tasks
                )
            .Match(p => p.Tasks
                .Any(t =>
                !t.IsDeleted && t.UserId == userId && t.DueDate >= startDate && t.DueDate < endDate)
            )
            .Project(x => new ProjectWithTasksDTO
            {
                Id = x.Id!,
                Title = x.Title,
                Description = x.Description,
                IsCompleted = x.IsCompleted,
                IsDeleted = x.IsDeleted,
                Tasks = x.Tasks
                    .Select(t => new TaskDTO
                    {
                        Id = t.Id!,
                        Title = t.Title,
                        Description = t.Description,
                        Status = t.Status,
                        DueDate = t.DueDate
                    })
                    .ToList()
            });

            var result = new List<ProjectWithTasksDTO>();
            if (!query.FetchAll)
            {
                result = await aggregatePipeline
                    .Skip((query.Page - 1) * query.PageSize)
                    .Limit(query.PageSize)
                    .ToListAsync();
            }
            else
            {
                result = await aggregatePipeline
                    .ToListAsync();
            }

            var total = result.Count();


            return new PaginatedResultDto<ProjectWithTasksDTO>
            {
                Results = result,
                Total = total,
                Page = query.Page,
                PageSize = query.PageSize
            };
        }
    }
}
