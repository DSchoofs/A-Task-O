namespace Schoofs_ATaskO.Models
{
    public class TaskItem
    {
        public int TaskId { get; set; }
        public string Name { get; set; } = "";
        public TaskInterval Interval { get; set; }
    }
}