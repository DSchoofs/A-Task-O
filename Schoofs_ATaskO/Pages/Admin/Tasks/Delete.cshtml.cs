using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Schoofs_ATaskO.Models;
using Schoofs_ATaskO.Services;

namespace Schoofs_ATaskO.Pages.Admin.Tasks
{
    [Authorize(Roles = "Admin")]
    public class DeleteModel : PageModel
    {
        private readonly ITaskService _taskService;

        [BindProperty]
        public TaskItem? TaskItem { get; set; }

        public string ErrorMessage { get; set; } = string.Empty;

        public DeleteModel(ITaskService taskService)
        {
            _taskService = taskService;
        }

        public IActionResult OnGet(int id)
        {
            TaskItem = _taskService.GetTaskById(id);

            if (TaskItem == null)
            {
                return RedirectToPage("/Admin/Tasks/TaskManagement");
            }

            return Page();
        }

        public IActionResult OnPost(int id)
        {
            var result = _taskService.DeleteTaskById(id);

            if (!result)
            {
                ErrorMessage = _taskService.ErrorMessage;
                TaskItem = _taskService.GetTaskById(id);
                return Page();
            }

            return RedirectToPage("/Admin/Tasks/TaskManagement");
        }
    }
}