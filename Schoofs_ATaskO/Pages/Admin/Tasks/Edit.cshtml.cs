using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Schoofs_ATaskO.Models;
using Schoofs_ATaskO.Services;
using Schoofs_ATaskO.ViewModels;

namespace Schoofs_ATaskO.Pages.Admin.Tasks
{
    [Authorize(Roles = "Admin")]
    public class EditModel : PageModel
    {
        private readonly ITaskService _taskService;

        [BindProperty]
        public EditTaskViewModel ViewModel { get; set; } = new();

        public string ErrorMessage { get; set; } = string.Empty;

        public EditModel(ITaskService taskService)
        {
            _taskService = taskService;
        }

        public IActionResult OnGet(int id)
        {
            var task = _taskService.GetTaskById(id);

            if (task == null)
            {
                return RedirectToPage("/Admin/Tasks/TaskManagement");
            }

            ViewModel = new EditTaskViewModel
            {
                TaskId = task.Id,
                TaskName = task.Name,
                TaskInterval = task.Interval
            };

            return Page();
        }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var task = new TaskItem
            {
                Id = ViewModel.TaskId,
                Name = ViewModel.TaskName,
                Interval = ViewModel.TaskInterval!.Value
            };

            var result = _taskService.UpdateTask(task);

            if (!result)
            {
                ErrorMessage = _taskService.ErrorMessage;
                return Page();
            }

            return RedirectToPage("/Admin/Tasks/TaskManagement");
        }
    }
}