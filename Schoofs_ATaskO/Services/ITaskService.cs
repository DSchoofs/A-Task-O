using Schoofs_ATaskO.Models;
using Schoofs_ATaskO.ViewModels;

namespace Schoofs_ATaskO.Services;

public interface ITaskService
{
    List<TaskItem> GetAllTasks();

    TaskItem? GetTaskById(int id);

    bool CreateTask(TaskCreateViewModel viewModel);

    bool UpdateTask(TaskEditViewModel viewModel);

    bool DeleteTaskById(int id);

    string ErrorMessage { get; }
}