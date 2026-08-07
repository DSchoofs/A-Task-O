using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Schoofs_ATaskO.Services;
using Schoofs_ATaskO.ViewModels;

namespace Schoofs_ATaskO.Pages.Admin.Tasks;

[Authorize(Roles = "Admin")]
public class DeleteModel : PageModel
{
    private readonly ITaskService _taskService;

    [BindProperty]
    public TaskDeleteViewModel Input { get; set; } = new();

    public string ErrorMessage { get; private set; } = string.Empty;

    public DeleteModel(ITaskService taskService)
    {
        _taskService = taskService;
    }

    public IActionResult OnGet(int id)
    {
        var taskItem = _taskService.GetTaskById(id);

        if (taskItem is null)
        {
            return NotFound();
        }

        Input = new TaskDeleteViewModel
        {
            Id = taskItem.Id,
            Name = taskItem.Name,
            Description = taskItem.Description,
            Interval = taskItem.Interval,
            IsActive = taskItem.IsActive,
            CreatedAt = taskItem.CreatedAt,
            CreatedByUserId = taskItem.CreatedByUserId,
            ModifiedAt = taskItem.ModifiedAt,
            ModifiedByUserId = taskItem.ModifiedByUserId
        };

        return Page();
    }

    public IActionResult OnPost()
    {
        var success = _taskService.DeleteTaskById(Input.Id);

        if (!success)
        {
            ErrorMessage = _taskService.ErrorMessage;

            var taskItem = _taskService.GetTaskById(Input.Id);

            if (taskItem is not null)
            {
                Input = new TaskDeleteViewModel
                {
                    Id = taskItem.Id,
                    Name = taskItem.Name,
                    Description = taskItem.Description,
                    Interval = taskItem.Interval,
                    IsActive = taskItem.IsActive,
                    CreatedAt = taskItem.CreatedAt,
                    CreatedByUserId = taskItem.CreatedByUserId,
                    ModifiedAt = taskItem.ModifiedAt,
                    ModifiedByUserId = taskItem.ModifiedByUserId
                };
            }

            return Page();
        }

        return RedirectToPage("TaskManagement");
    }
}