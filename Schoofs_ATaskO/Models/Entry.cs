namespace Schoofs_ATaskO.Models
{
    public class Entry
    {
        public int EntryId { get; set; }
        public int TaskId { get; set; }
        public int UserId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public bool IsCompleted { get; set; }
    }
}