using System.Security.Claims;
using Projects.Config.Db;
using Projects.Models.Auth;
using Projects.Models.Base;
using Projects.Models.Common;
using Projects.Services.Auth;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace Projects.Services.Base
{
    public class BaseService<Model>
        where Model : BaseModel
    {
        private readonly IMongoCollection<Model> _collection;
        protected readonly IServiceProvider _serviceProvider;

        protected readonly IHttpContextAccessor _httpContextAccessor;

        public BaseService(
            string collectionName,
            IOptions<DbSettings> dbSettings,
            IServiceProvider serviceProvider,
            IHttpContextAccessor httpContextAccessor
        )
        {
            _serviceProvider = serviceProvider;
            _httpContextAccessor = httpContextAccessor;

            if (string.IsNullOrWhiteSpace(collectionName))
            {
                throw new ArgumentException("Collection name cannot be null or empty.");
            }

            if (dbSettings == null)
            {
                throw new ArgumentNullException(
                    nameof(dbSettings),
                    "Database settings cannot be null."
                );
            }

            var mongoClient = new MongoClient(dbSettings.Value.ConnectionString);
            var mongoDatabase = mongoClient.GetDatabase(dbSettings.Value.DatabaseName);
            _collection = mongoDatabase.GetCollection<Model>(collectionName);
        }

        protected ClaimsPrincipal User => _httpContextAccessor?.HttpContext?.User!;
        protected AccountService _accountService =>
            _serviceProvider.GetRequiredService<AccountService>();

        public async Task<List<Model>> GetByIdAsync(string id)
        {
            return await _collection.Find(x => x.Id == id).ToListAsync();
        }

        public async Task<List<Model>> GetListAsync()
        {
            return await _collection.Find(_ => true).ToListAsync();
        }

    }
}
