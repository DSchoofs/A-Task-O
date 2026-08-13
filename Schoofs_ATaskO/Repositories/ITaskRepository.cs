using Schoofs_ATaskO.Models;

namespace Schoofs_ATaskO.Repositories
{
    public interface ITaskRepository
    {
        List<TaskItem> GetAllTasks();
        TaskItem? GetTaskById(int id);
        bool AddTask(TaskItem task);
        bool UpdateTask(TaskItem task);
        bool DeleteTaskWithBlocksById(int id);
        string ErrorMessage { get; set; }
    }
}