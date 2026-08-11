namespace TaskManager.Dtos.Project
{
    public class ProjectDTO
    {
        public string Id { get; set; } = null!;
        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;
        public bool IsDeleted { get; set; } = false;
        public bool IsCompleted { get; set; } = false;
    }

    public class CreateProjectDTO
    {
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
    }
    public class UpdateProjectDTO
    {
        public string? Title { get; set; } 
        public string? Description { get; set; } 
        public bool IsCompleted { get; set; } 
    }
}