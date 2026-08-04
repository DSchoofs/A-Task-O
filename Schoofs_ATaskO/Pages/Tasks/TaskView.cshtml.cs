using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Schoofs_ATaskO.Models;
using Schoofs_ATaskO.Services;

namespace Schoofs_ATaskO.Pages.Tasks
{
    [Authorize]
    public class TaskViewModel : PageModel
    {
        private readonly ITaskService _taskService;

        public TaskItem? TaskItem { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;

        public TaskViewModel(ITaskService taskService)
        {
            _taskService = taskService;
        }

        public IActionResult OnGet(int id)
        {
            TaskItem = _taskService.GetTaskById(id);

            if (TaskItem == null)
            {
                ErrorMessage = "Die angeforderte Aufgabe wurde nicht gefunden.";
                return Page();
            }

            return Page();
        }
    }
}