namespace TaskManager.Dtos.Project
{
    public class CreateProjectDTO
    {
        public string Name { get; set; }
        public string Descritpion { get; set; }
        public List<string> Tags { get; set; }
    }
}
