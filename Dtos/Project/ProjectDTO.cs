namespace TaskManager.Dtos.Project
{
    public class ProjectDTO
    {
        public string Id { get; set; } = null!;
        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;
        public bool IsDeleted { get; set; } = false;
    }

    public class MutateProjectDTO
    {
        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;
    }
}