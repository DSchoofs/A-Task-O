using Schoofs_ATaskO.ViewModels;

namespace Schoofs_ATaskO.Services
{
    public interface ITaskBlockService
    {
        TaskViewViewModel? GetTaskView(int taskItemId, DateTime focusDate);

        string ErrorMessage { get; }
    }
}