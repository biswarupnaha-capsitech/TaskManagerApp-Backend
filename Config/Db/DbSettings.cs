namespace TaskManager.Config.Db
{
    public class DbSettings
    {
        public string ConnectionString { get; set; } = null!;
        public string IdentityCollectionName { get; set; } = null!;
        public string DatabaseName { get; set; } = null!;
        public string TasksCollectionName { get; set; } = null!;
    }
}
