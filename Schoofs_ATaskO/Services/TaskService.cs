using Schoofs_ATaskO.Models;
using Schoofs_ATaskO.Repositories;

namespace Schoofs_ATaskO.Services
{
    public class TaskService : ITaskService
    {
        private readonly ITaskRepository _taskRepository;

        public string ErrorMessage { get; private set; } = string.Empty;

        public TaskService(ITaskRepository taskRepository)
        {
            _taskRepository = taskRepository;
        }

        public List<TaskItem> GetAllTasks()
        {
            ErrorMessage = string.Empty;

            try
            {
                var tasks = _taskRepository.GetAllTasks();

                if (tasks.Count == 0 && !string.IsNullOrWhiteSpace(_taskRepository.ErrorMessage))
                {
                    ErrorMessage = _taskRepository.ErrorMessage;
                }

                return tasks;
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Unexpected Error: {ex.Message}";
                return new List<TaskItem>();
            }
        }

        public TaskItem? GetTaskById(int id)
        {
            ErrorMessage = string.Empty;

            try
            {
                var task = _taskRepository.GetTaskById(id);

                if (task == null && !string.IsNullOrWhiteSpace(_taskRepository.ErrorMessage))
                {
                    ErrorMessage = _taskRepository.ErrorMessage;
                }

                return task;
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Unexpected Error: {ex.Message}";
                return null;
            }
        }

        public bool CreateTask(TaskItem task)
        {
            ErrorMessage = string.Empty;

            try
            {
                var result = _taskRepository.AddTask(task);

                if (!result)
                {
                    ErrorMessage = _taskRepository.ErrorMessage;
                }

                return result;
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Unexpected Error: {ex.Message}";
                return false;
            }
        }

        public bool UpdateTask(TaskItem task)
        {
            ErrorMessage = string.Empty;

            try
            {
                var result = _taskRepository.UpdateTask(task);

                if (!result)
                {
                    ErrorMessage = _taskRepository.ErrorMessage;
                }

                return result;
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Unexpected Error: {ex.Message}";
                return false;
            }
        }

        public bool DeleteTaskById(int id)
        {
            ErrorMessage = string.Empty;

            try
            {
                var result = _taskRepository.DeleteTaskById(id);

                if (!result)
                {
                    ErrorMessage = _taskRepository.ErrorMessage;
                }

                return result;
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Unexpected Error: {ex.Message}";
                return false;
            }
        }
    }
}