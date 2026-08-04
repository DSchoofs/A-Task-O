using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Schoofs_ATaskO.Models;
using Schoofs_ATaskO.Services;

namespace Schoofs_ATaskO.Pages.Admin.Tasks
{
    [Authorize(Roles = "Admin")]
    public class TaskManagementModel : PageModel
    {
        private readonly ITaskService _taskService;

        public List<TaskItem> Tasks { get; set; } = new();
        public string ErrorMessage { get; set; } = string.Empty;

        public TaskManagementModel(ITaskService taskService)
        {
            _taskService = taskService;
        }

        public void OnGet()
        {
            Tasks = _taskService.GetAllTasks();
            ErrorMessage = _taskService.ErrorMessage;
        }
    }
}