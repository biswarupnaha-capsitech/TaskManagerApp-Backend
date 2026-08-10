using TaskManager.Models.Base;

namespace TaskManager.Models
{
    public class Project : BaseModel
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public bool isDeleted { get; set; }
        public List<string> Tags { get; set; }
        public string UserId { get; set; }
    }
}
