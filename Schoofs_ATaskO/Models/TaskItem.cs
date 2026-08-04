namespace Schoofs_ATaskO.Models
{
    public class TaskItem
    {
        public int TaskId { get; set; }
        public string TaskName { get; set; } = string.Empty;
        public TaskInterval TaskInterval { get; set; }
    }
}