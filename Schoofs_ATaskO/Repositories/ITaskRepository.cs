using Schoofs_ATaskO.Models;

namespace Schoofs_ATaskO.Repositories
{
    public interface ITaskRepository
    {
        List<TaskItem> GetAllTasks();
        TaskItem? GetTaskById(int id);
        int AddTask(TaskItem task);
        bool UpdateTask(TaskItem task);
        bool DeleteTaskById(int id);
    }
}