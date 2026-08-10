using Capsitech.Extensions;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using TaskManager.Config.Db;
using TaskManager.Dtos.Common;
using TaskManager.Dtos.Task;

namespace TaskManager.Services.Task
{
    public class TaskService : ITaskService
    {
        private readonly IMongoCollection<Models.Task> _taskCollection;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public TaskService(IOptions<DbSettings> dbSettings, IHttpContextAccessor httpContextAccessor)
        {
            var mongoClient = new MongoClient(dbSettings.Value.ConnectionString);
            var database = mongoClient.GetDatabase(dbSettings.Value.DatabaseName);
            _taskCollection = database.GetCollection<Models.Task>(dbSettings.Value.TasksCollectionName);
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<PaginatedResultDto<TaskDTO>> GetAsync(PaginatedQueryDto query)
        {
            var builder = Builders<Models.Task>.Filter;
            var filter = builder.Empty;
            var userId = _httpContextAccessor.HttpContext?.User.GetUserId();

            //if (!string.IsNullOrEmpty(userId))
            //{
            //    filter &= builder.Eq(x => x.UserId, userId);
            //}
            if (string.IsNullOrEmpty(userId))
                throw new UnauthorizedAccessException("User ID not found in the token.");


            filter &= builder.Eq(x => x.UserId, userId);
            filter &= builder.Eq(x => x.IsDeleted, false);
            filter &= builder.Eq(x => x.UserId, userId);

            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                var regex = new BsonRegularExpression(query.Search, "i");

                filter &= builder.Or(
                    builder.Regex(x => x.Id, regex),
                    builder.Regex(x => x.Title, regex),
                    builder.Regex(x => x.Description, regex),
                    builder.Regex(x => x.Status, regex)
                );
            }

            var find = _taskCollection.Find(filter);
            var total = await find.CountDocumentsAsync();

            var tasks = query.FetchAll
                ? await find.ToListAsync()
                : await find.Skip((query.Page - 1) * query.PageSize)
                    .Limit(query.PageSize)
                    .ToListAsync();

            var result = new PaginatedResultDto<TaskDTO>
            {
                Results = tasks.Select(t => new TaskDTO
                {
                    Id = t.Id!,
                    Title = t.Title,
                    Description = t.Description,
                    Status = t.Status,
                    IsDeleted = t.IsDeleted
                }).ToList(),
                Total = total,
                Page = query.Page,
                PageSize = query.PageSize,
            };

            return result;
        }

        public async Task<TaskDTO> CreateAsync(CreateTaskDTO task)
        {
            var userId = _httpContextAccessor.HttpContext?.User.GetUserId();
            var newTask = new Models.Task
            {
                Title = task.Title,
                Description = task.Description,
                Status = Common.TaskStatus.Pending,
                IsDeleted = false,
                UserId = userId!
            };
            await _taskCollection.InsertOneAsync(newTask);
            return new TaskDTO
            {
                Id = newTask.Id!,
                Title = newTask.Title,
                Description = newTask.Description,
                Status = newTask.Status,
                IsDeleted = newTask.IsDeleted
            };
        }

        public async System.Threading.Tasks.Task UpdateAsync(string id, UpdateTaskDTO updatedTask)
        {
            var userId = _httpContextAccessor.HttpContext?.User.GetUserId();
            var update = Builders<Models.Task>.Update
                            .Set(x => x.Title, updatedTask.Title)
                            .Set(x => x.Description, updatedTask.Description)
                            .Set(x => x.Status, updatedTask.Status)
                            .Set(x => x.UpdatedAt, DateTime.UtcNow);

            await _taskCollection.UpdateOneAsync(x => x.Id == id, update);
        }

        public async System.Threading.Tasks.Task DeleteAsync(string id, bool hardDelete = true)
        {
            if (hardDelete)
            {
                await _taskCollection.DeleteOneAsync(x => x.Id == id);
            }
            else
            {
                var userId = _httpContextAccessor.HttpContext?.User.GetUserId();
                await _taskCollection.UpdateOneAsync(x => x.Id == id && x.UserId == userId,
                    Builders<Models.Task>.Update
                    .Set(x => x.IsDeleted, true)
                    .Set(x => x.UpdatedAt, DateTime.UtcNow));
            }
        }
    }
}
