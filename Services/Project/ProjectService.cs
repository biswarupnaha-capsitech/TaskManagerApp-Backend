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
using TaskManager.Services.Base;
using TaskManager.Services.Task;

namespace TaskManager.Services.Project
{
    public class ProjectService(IOptions<DbSettings> dbSettings, IMongoClient mongoClient, IServiceProvider serviceProvider, IHttpContextAccessor httpContextAccessor, ITaskService taskService) :
        BaseService<Models.Project>(DbCollections.Projects, dbSettings, serviceProvider, mongoClient, httpContextAccessor), 
        IProjectService
    {
        private readonly ITaskService _taskService = taskService;
        public async Task<ProjectDTO> CreateAsync(CreateProjectDTO project)
        {
            var userId = User.GetUserId();
            var newProject = new Models.Project
            {
                Title = project.Title,
                Description = project.Description!,
                IsDeleted = false,
                IsCompleted = false,
                UserId = userId!
            };
            await _collection!.InsertOneAsync(newProject);
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
                await _collection.DeleteOneAsync(x => x.Id == id);
            }
            else
            {
                var userId = User.GetUserId();
                await _collection.UpdateOneAsync(x => x.Id == id && x.UserId == userId,
                    Builders<Models.Project>.Update
                    .Set(x => x.IsDeleted, true)
                    .Set(x => x.UpdatedAt, DateTime.UtcNow));
            }
        }

        public async System.Threading.Tasks.Task UpdateAsync(string id, UpdateProjectDTO project)
        {
            var userId = User.GetUserId();
            var update = Builders<Models.Project>.Update
                            .Set(x => x.Title, project.Title)
                            .Set(x => x.Description, project.Description)
                            .Set(x => x.IsCompleted, project.IsCompleted)
                            .Set(x => x.UpdatedAt, DateTime.UtcNow);
            await _collection.UpdateOneAsync(x => x.Id == id, update);
            if (project.IsCompleted)
            {
                await _taskService.CompleteTasksByProjectAsync(id);
            }
        }

        public async Task<PaginatedResultDto<ProjectWithTasksDTO>> GetAsync(ProjectQueryDTO query)
        {
            var indexKeys = Builders<Models.Project>.IndexKeys.Ascending(p => p.UserId);
            var indexModel = new CreateIndexModel<Models.Project>(indexKeys);
            await _collection!.Indexes.CreateOneAsync(indexModel);

            var projectBuilder = Builders<Models.Project>.Filter;
            var taskBuilder = Builders<Models.Task>.Filter;
            var userId = User.GetUserId();

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
                _collection.Aggregate()
                .Match(projectFilter)
                .Lookup<Models.Task, ProjectWithRawTasks>(
                    foreignCollectionName: DbCollections.Tasks,
                    localField: "_id",
                    foreignField: "projectId",
                    @as: "Tasks"
                    )
                .Project(x => new ProjectWithTasksDTO
                {
                    Id = x.Id!,
                    Title = x.Title,
                    Description = x.Description,
                    IsCompleted = x.IsCompleted,
                    IsDeleted = x.IsDeleted,
                    Tasks = x.Tasks
                        .Select(t => new TaskForProjectDTO
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

            var total = result.Count;


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
