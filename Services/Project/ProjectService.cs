using System.Text.RegularExpressions;
using Capsitech.Extensions;
using MongoDB.Bson;
using MongoDB.Driver;
using TaskManager.Dtos.Common;
using TaskManager.Dtos.Project;
using TaskManager.Dtos.Task;

namespace TaskManager.Services.Project
{
    public class ProjectService : IProjectService
    {
        private readonly IMongoCollection<Models.Project>? _projectCollection;
        private readonly IMongoCollection<Models.Task>? _taskCollection;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ProjectService(IMongoDatabase database, IHttpContextAccessor httpContextAccessor)
        {
            _projectCollection = database.GetCollection<Models.Project>("Projects");
            _taskCollection = database.GetCollection<Models.Task>("Tasks");
            _httpContextAccessor = httpContextAccessor;
        }

        public Task<ProjectDTO> CreateAsync(MutateProjectDTO project)
        {
            throw new NotImplementedException();
        }

        public System.Threading.Tasks.Task DeleteAsync(string id, bool hardDelete = true)
        {
            throw new NotImplementedException();
        }

        public System.Threading.Tasks.Task UpdateAsync(string id, MutateProjectDTO project)
        {
            throw new NotImplementedException();
        }

        public async Task<PaginatedResultDto<ProjectWithTasksDTO>> GetAsync(ProjectQueryDTO query)
        {
            var projectBuilder =
                Builders<Models.Project>.Filter;

            var taskBuilder =
                Builders<Models.Task>.Filter;


            // =========================================================
            // USER
            // =========================================================

            var userId = _httpContextAccessor.HttpContext?.User.GetUserId();

            if (string.IsNullOrEmpty(userId))
                throw new UnauthorizedAccessException("User ID not found in the token.");


            // =========================================================
            // MONTH
            // =========================================================

            var now = DateTime.UtcNow;

            var year = query.Year ?? now.Year;
            var month = query.Month ?? now.Month;

            if (month < 1 || month > 12)
            {
                throw new ArgumentException(
                    "Month must be between 1 and 12."
                );
            }

            var startDate = new DateTime(year, month, 1, 0, 0, 0, DateTimeKind.Utc);

            var endDate = startDate.AddMonths(1);


            // =========================================================
            // TASK FILTER
            // =========================================================

            var taskFilter = taskBuilder.Empty;

            taskFilter &= taskBuilder.Eq(
                x => x.UserId,
                userId
            );

            taskFilter &= taskBuilder.Eq(
                x => x.IsDeleted,
                false
            );

            taskFilter &= taskBuilder.Gte(
                x => x.DueDate,
                startDate
            );

            taskFilter &= taskBuilder.Lt(
                x => x.DueDate,
                endDate
            );


            // =========================================================
            // OPTIONAL DAY FILTER
            // =========================================================

            if (query.Day.HasValue)
            {
                var daysInMonth =
                    DateTime.DaysInMonth(year, month);

                if (query.Day.Value < 1 || query.Day.Value > daysInMonth)
                    throw new ArgumentException("Invalid day for selected month.");

                var dayStart = startDate.AddDays(query.Day.Value - 1);

                var dayEnd = dayStart.AddDays(1);

                taskFilter &= taskBuilder.Gte(
                    x => x.DueDate,
                    dayStart
                );

                taskFilter &= taskBuilder.Lt(
                    x => x.DueDate,
                    dayEnd
                );
            }


            // =========================================================
            // PROJECT FILTER
            // =========================================================

            var projectFilter = projectBuilder.Empty;
            projectFilter &= projectBuilder.Eq(x => x.UserId, userId);
            projectFilter &= projectBuilder.Eq(x => x.IsDeleted, false);


            // =========================================================
            // PROJECT SEARCH
            // =========================================================

            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                var regex = new BsonRegularExpression(Regex.Escape(query.Search), "i");

                projectFilter &= projectBuilder.Or(
                    projectBuilder.Regex(x => x.Id, regex),
                    projectBuilder.Regex(x => x.Title, regex),
                    projectBuilder.Regex(x => x.Description, regex)
                );
            }


            // =========================================================
            // 1. FIND TASKS FOR SELECTED MONTH/DAY
            // =========================================================

            var matchingTaskProjectIds = await _taskCollection
                    .Find(taskFilter)
                    .Project(x => x.ProjectId)
                    .ToListAsync();


            // =========================================================
            // 2. PROJECTS MUST HAVE A MATCHING TASK
            // =========================================================

            projectFilter &= projectBuilder.In(x => x.Id, matchingTaskProjectIds);


            // =========================================================
            // 3. PAGINATE PROJECTS
            // =========================================================

            var page = Math.Max(query.Page, 1);
            var pageSize = Math.Clamp(query.PageSize, 1, 30);
            var projectQuery = _projectCollection.Find(projectFilter);
            var total = await projectQuery.CountDocumentsAsync();


            var projects = query.FetchAll
                ? await projectQuery
                    .SortBy(x => x.Title)
                    .ToListAsync()
                : await projectQuery
                    .SortBy(x => x.Title)
                    .Skip((page - 1) * pageSize)
                    .Limit(pageSize)
                    .ToListAsync();


            // =========================================================
            // 4. NO PROJECTS
            // =========================================================

            if (projects.Count == 0)
            {
                return new PaginatedResultDto<ProjectWithTasksDTO>
                {
                    Results = [],
                    Total = total,
                    Page = page,
                    PageSize = pageSize
                };
            }


            // =========================================================
            // 5. ONLY LOAD TASKS FOR PAGINATED PROJECTS
            // =========================================================

            var projectIds = projects
                    .Select(x => x.Id!)
                    .ToList();

            taskFilter &= taskBuilder.In(x => x.ProjectId, projectIds);


            // =========================================================
            // 6. AGGREGATION
            // =========================================================

            var taskGroups = await _taskCollection
                    .Aggregate()
                    .Match(taskFilter)
                    .Group(
                        x => new
                        {
                            x.ProjectId,
                            x.DueDate
                        },
                        g => new
                        {
                            ProjectId = g.Key.ProjectId!,
                            Date = g.Key.DueDate,
                            Tasks = g.ToList()
                        }
                    )
                    .SortBy(x => x.Date)
                    .ToListAsync();


            // =========================================================
            // 7. MAP AGGREGATED TASKS TO PROJECTS
            // =========================================================

            var tasksByProject = taskGroups
                    .GroupBy(x => x.ProjectId)
                    .ToDictionary(
                        x => x.Key,
                        x => x
                            .OrderBy(g => g.Date)
                            .Select(g => new TaskDateGroupDTO
                            {
                                Date = g.Date,

                                Tasks = g.Tasks
                                    .Select(task => new TaskDTO
                                    {
                                        Id = task.Id!,
                                        Title = task.Title,
                                        Description = task.Description,
                                        Status = task.Status,
                                        DueDate = task.DueDate,
                                    })
                                    .ToList()
                            })
                            .ToList()
                    );


            // =========================================================
            // 8. FINAL RESPONSE
            // =========================================================

            var results = projects.Select(project => new ProjectWithTasksDTO
            {
                Id = project.Id!,
                Title = project.Title,
                Description = project.Description,
                IsDeleted = project.IsDeleted,
                ProjectTasks = tasksByProject.TryGetValue(project.Id!, out var groups) ? groups : []
            }).ToList();


            return new PaginatedResultDto<ProjectWithTasksDTO>
            {
                Results = results,
                Total = total,
                Page = page,
                PageSize = pageSize
            };
        }
    }
}
