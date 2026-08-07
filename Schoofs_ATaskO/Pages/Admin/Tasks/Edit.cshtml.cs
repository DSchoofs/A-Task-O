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
        public TaskEditViewModel Input { get; set; } = new();

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

            Input = new TaskEditViewModel
            {
                Id = task.Id,
                Name = task.Name,
                Interval = task.Interval,
                Description = task.Description,
                IsActive = task.IsActive
            };

            return Page();
        }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var result = _taskService.UpdateTask(Input);

            if (!result)
            {
                ErrorMessage = _taskService.ErrorMessage;
                return Page();
            }

            return RedirectToPage("/Admin/Tasks/TaskManagement");
        }
    }
}