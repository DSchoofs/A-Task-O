using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Schoofs_ATaskO.Models;
using Schoofs_ATaskO.Services;
using Schoofs_ATaskO.ViewModels;

namespace Schoofs_ATaskO.Pages.Admin.Tasks
{
    [Authorize(Roles = "Admin")]
    public class CreateModel : PageModel
    {
        private readonly ITaskService _taskService;

        [BindProperty]
        public TaskCreateViewModel Input { get; set; } = new();

        public string ErrorMessage { get; set; } = string.Empty;

        public CreateModel(ITaskService taskService)
        {
            _taskService = taskService;
        }

        public void OnGet()
        {
            Input.IsActive = true;
            Input.WeekMode = TaskWeekMode.Weekdays;
        
        }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            

            var result = _taskService.CreateTask(Input);

            if (!result)
            {
                ErrorMessage = _taskService.ErrorMessage;
                return Page();
            }

            return RedirectToPage("/Admin/Tasks/TaskManagement");
        }
    }
}