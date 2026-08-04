using Schoofs_ATaskO.Models;

namespace Schoofs_ATaskO.Services
{
    public interface ITaskService
    {
        List<TaskItem> GetAllTasks();
        TaskItem? GetTaskById(int id);
        bool CreateTask(TaskItem task);
        bool UpdateTask(TaskItem task);
        bool DeleteTaskById(int id);
        string ErrorMessage { get; }
    }
}