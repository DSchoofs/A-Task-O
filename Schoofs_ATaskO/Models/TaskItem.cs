namespace Schoofs_ATaskO.Models
{
    public class TaskItem
    {
        public int TaskId { get; set; }
        public string TaskName { get; set; } = "";
        public TaskInterval TaskInterval { get; set; }
    }
}