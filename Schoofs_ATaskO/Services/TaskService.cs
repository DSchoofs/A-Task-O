using Schoofs_ATaskO.Repositories;
using Schoofs_ATaskO.ViewModels;

namespace Schoofs_ATaskO.Services
{
    public class TaskService : ITaskService
    {
        private readonly ITaskRepository _taskRepository;
        private readonly ICurrentUserService _currentUserService;
        private readonly IUserService _userService;

        public string ErrorMessage { get; private set; } = string.Empty;

        public TaskService(ITaskRepository taskRepository, ICurrentUserService currentUserService, IUserService userService)
        {
            _taskRepository = taskRepository;
            _currentUserService = currentUserService;
            _userService = userService;
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

        public List<TaskItem> GetActiveTasks()
        {
            ErrorMessage = string.Empty;

            try
            {
                var tasks = _taskRepository
                    .GetAllTasks()
                    .Where(task => task.IsActive)
                    .ToList();

                if (tasks.Count == 0 &&
                    !string.IsNullOrWhiteSpace(_taskRepository.ErrorMessage))
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
        public List<TaskManagementViewModel> GetAllTaskManagementViewModels()
        {
            ErrorMessage = string.Empty;

            var taskItems = _taskRepository.GetAllTasks();

            var taskViewModels = new List<TaskManagementViewModel>();

            foreach (var taskItem in taskItems)
            {
                taskViewModels.Add(new TaskManagementViewModel
                {
                    Id = taskItem.Id,
                    Name = taskItem.Name,
                    Description = taskItem.Description,
                    Interval = taskItem.Interval,
                    WeekMode = taskItem.WeekMode,
                    IsActive = taskItem.IsActive,
                    CreatedAt = taskItem.CreatedAt,
                    CreatedByUserName = _userService.GetUserDisplayName(taskItem.CreatedByUserId),
                    ModifiedAt = taskItem.ModifiedAt,
                    ModifiedByUserName = taskItem.ModifiedAt.HasValue ? _userService.GetUserDisplayName(taskItem.ModifiedByUserId) : "Noch nicht geändert"
                });
            }

            return taskViewModels;
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

        public bool CreateTask(TaskCreateViewModel viewModel)
        {
            var taskItem = new TaskItem
            {
                Name = viewModel.Name,
                Description = viewModel.Description,
                Interval = viewModel.Interval,
                WeekMode = viewModel.WeekMode!.Value,
                IsActive = viewModel.IsActive,
                CreatedAt = DateTime.UtcNow,
                CreatedByUserId = _currentUserService.UserId
            };

            var result = _taskRepository.AddTask(taskItem);

            ErrorMessage = _taskRepository.ErrorMessage;

            return result;
        }

        public bool UpdateTask(TaskEditViewModel viewModel)
        {
            var taskItem = _taskRepository.GetTaskById(viewModel.Id);

            ErrorMessage = _taskRepository.ErrorMessage;

            if (taskItem is null)
            {
                ErrorMessage = "Die Aufgabe wurde nicht gefunden.";
                return false;
            }

            taskItem.Name = viewModel.Name;
            taskItem.Description = viewModel.Description;
            taskItem.Interval = viewModel.Interval;
            taskItem.WeekMode = viewModel.WeekMode!.Value;
            taskItem.IsActive = viewModel.IsActive;
            taskItem.ModifiedAt = DateTime.UtcNow;
            taskItem.ModifiedByUserId = _currentUserService.UserId;

            var result = _taskRepository.UpdateTask(taskItem);

            ErrorMessage = _taskRepository.ErrorMessage;

            return result;
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