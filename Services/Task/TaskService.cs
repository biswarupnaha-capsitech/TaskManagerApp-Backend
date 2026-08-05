using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using Projects.Config.Db;
using Projects.Dtos.Common;
using Projects.Dtos.Task;

namespace Projects.Services.Task
{
    public class TaskService : ITaskService
    {
        private readonly IMongoCollection<Models.Task> _taskCollection;

        private readonly IMongoCollection<BsonDocument> _taskCollectionBson;

        public TaskService(IOptions<DbSettings> dbSettings)
        {
            var mongoClient = new MongoClient(dbSettings.Value.ConnectionString);
            var database = mongoClient.GetDatabase(dbSettings.Value.DatabaseName);
            _taskCollection = database.GetCollection<Models.Task>(dbSettings.Value.TasksCollectionName);
            _taskCollectionBson = database.GetCollection<BsonDocument>(dbSettings.Value.TasksCollectionName);
        }

        public async Task<List<Models.Task>> GetAsync() =>
            await _taskCollection.Find(_ => true).ToListAsync();

        public async Task<Models.Task?> GetAsync(string id) =>
            await _taskCollection.Find(x => x.Id == id).FirstOrDefaultAsync();

        public async Task<Models.Task> CreateAsync(Models.Task task)
        {
            await _taskCollection.InsertOneAsync(task);
            return task;
        }

        public async System.Threading.Tasks.Task UpdateAsync(string id, Models.Task updatedTask) =>
            await _taskCollection.ReplaceOneAsync(x => x.Id == id, updatedTask);


        public async System.Threading.Tasks.Task RemoveAsync(string id) =>
            await _taskCollection.DeleteOneAsync(x => x.Id == id);

        public async System.Threading.Tasks.Task DeleteAsync(String id)=>
            await _taskCollection.DeleteOneAsync(x => x.Id == id);

        public async Task<PaginatedResultDto<Models.Task>> GetPaginatedAsync(TaskQueryDTO query)
        {
            var filter = Builders<Models.Task>.Filter.Empty;

            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                var regex = new BsonRegularExpression(query.Search, "i");
                filter = Builders<Models.Task>.Filter.Or(
                    Builders<Models.Task>.Filter.Regex("title", regex),
                    Builders<Models.Task>.Filter.Regex("description", regex)
                );
            }

            var find = _taskCollection.Find(filter);
            var total = await find.CountDocumentsAsync();

            var tasks = query.FetchAll
                ? await find.ToListAsync()
                : await find.Skip((query.Page - 1) * query.PageSize)
                    .Limit(query.PageSize)
                    .ToListAsync();

            var result = new PaginatedResultDto<Models.Task>
            {
                Results = tasks,
                Total = total,
                Page = query.Page,
                PageSize = query.PageSize,
            };

            return result;
        }
    }
}
