using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Schoofs_ATaskO.Services;
using Schoofs_ATaskO.ViewModels;

namespace Schoofs_ATaskO.Pages.Tasks
{
    [Authorize]
    public class TaskViewModel : PageModel
    {
        private readonly ITaskBlockService _taskBlockService;

        public TaskViewViewModel? TaskView { get; set; }

        public string ErrorMessage { get; set; } = string.Empty;

        public TaskViewModel(ITaskBlockService taskBlockService)
        {
            _taskBlockService = taskBlockService;
        }

        public IActionResult OnGet(int id, DateTime? focusDate)
        {
            var selectedDate = focusDate ?? DateTime.Today;

            TaskView = _taskBlockService.GetTaskView(id, selectedDate);

            if (TaskView == null)
            {
                ErrorMessage = !string.IsNullOrWhiteSpace(
                    _taskBlockService.ErrorMessage)
                    ? _taskBlockService.ErrorMessage
                    : "Die angeforderte Aufgabe wurde nicht gefunden.";

                return Page();
            }

            return Page();
        }
    }
}