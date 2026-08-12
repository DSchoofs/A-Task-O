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
        private readonly ITaskService _taskService;

        public TaskViewViewModel? TaskView { get; set; }

        public string ErrorMessage { get; set; } = string.Empty;

        public TaskViewModel(ITaskBlockService taskBlockService, ITaskService taskService)
        {
            _taskBlockService = taskBlockService;
            _taskService = taskService;
        }

        public IActionResult OnGet(int id, DateTime? focusDate, int? selectedBlockId)
        {
            var taskItem = _taskService.GetTaskById(id);

            if (taskItem == null)
            {
                return RedirectToPage("/Account/AccessDenied");
            }

            if (!taskItem.IsActive && !User.IsInRole("Admin"))
            {
                return RedirectToPage("/Account/AccessDenied");
            }

            var selectedDate = focusDate ?? DateTime.Today;

            TaskView = _taskBlockService.GetTaskView(id, selectedDate, selectedBlockId);

            if (TaskView == null)
            {
                ErrorMessage = !string.IsNullOrWhiteSpace(_taskBlockService.ErrorMessage) ? _taskBlockService.ErrorMessage : "Die angeforderte Aufgabe wurde nicht gefunden.";

                return Page();
            }

            return Page();
        }

        public IActionResult OnPostTakeOver(int id, int blockId, DateTime? focusDate)   
        {
            var selectedDate = focusDate ?? DateTime.Today;

            var success = _taskBlockService.TakeOverBlock(blockId);

            if (!success)
            {
                ErrorMessage = !string.IsNullOrWhiteSpace(_taskBlockService.ErrorMessage) ? _taskBlockService.ErrorMessage : "Der Aufgabenblock konnte nicht übernommen werden.";

                TaskView = _taskBlockService.GetTaskView(id, selectedDate, blockId);

                return Page();
            }

            return RedirectToPage( new { id, focusDate = selectedDate, selectedBlockId = blockId });
        }

        public IActionResult OnPostComplete(int id, int blockId, DateTime? focusDate)
        {
            var selectedDate = focusDate ?? DateTime.Today;

            var success = _taskBlockService.CompleteBlock(blockId);

            if (!success)
            {
                ErrorMessage = !string.IsNullOrWhiteSpace(_taskBlockService.ErrorMessage) ? _taskBlockService.ErrorMessage : "Der Aufgabenblock konnte nicht als erledigt gespeichert werden.";

                TaskView = _taskBlockService.GetTaskView(id, selectedDate, blockId);

                return Page();
            }

            return RedirectToPage( new { id, focusDate = selectedDate, selectedBlockId = blockId });
        }

        public IActionResult OnPostRelease(int id, int blockId, DateTime? focusDate)
        {
            var selectedDate = focusDate ?? DateTime.Today;

            var success = _taskBlockService.ReleaseBlock(blockId);

            if (!success)
            {
                ErrorMessage = !string.IsNullOrWhiteSpace(_taskBlockService.ErrorMessage) ? _taskBlockService.ErrorMessage : "Der Aufgabenblock konnte nicht freigegeben werden.";

                TaskView = _taskBlockService.GetTaskView(id, selectedDate, blockId);

                return Page();
            }

            return RedirectToPage(new { id, focusDate = selectedDate });
        }
    }
}