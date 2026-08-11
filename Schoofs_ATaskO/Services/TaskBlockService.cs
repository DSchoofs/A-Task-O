using Schoofs_ATaskO.Helpers;
using Schoofs_ATaskO.Models;
using Schoofs_ATaskO.Repositories;
using Schoofs_ATaskO.ViewModels;

namespace Schoofs_ATaskO.Services
{
    public class TaskBlockService : ITaskBlockService
    {
        private readonly ITaskRepository _taskRepository;
        private readonly ITaskBlockRepository _taskBlockRepository;
        private readonly ICurrentUserService _currentUserService;
        private readonly IUserService _userService;

        public string ErrorMessage { get; private set; } = string.Empty;

        public TaskBlockService(ITaskRepository taskRepository, ITaskBlockRepository taskBlockRepository, ICurrentUserService currentUserService, IUserService userService)
        {
            _taskRepository = taskRepository;
            _taskBlockRepository = taskBlockRepository;
            _currentUserService = currentUserService;
            _userService = userService;
        }

        public TaskViewViewModel? GetTaskView(int taskItemId, DateTime focusDate)
        {
            ErrorMessage = string.Empty;

            var taskItem = _taskRepository.GetTaskById(taskItemId);

            if (taskItem is null)
            {
                ErrorMessage = !string.IsNullOrWhiteSpace(_taskRepository.ErrorMessage) ? _taskRepository.ErrorMessage : "Die Aufgabe wurde nicht gefunden.";

                return null;
            }


            var taskBlocks = _taskBlockRepository.GetBlocksByTaskItemId(taskItemId);

            if (taskBlocks.Count == 0 && !string.IsNullOrWhiteSpace(_taskBlockRepository.ErrorMessage))
            {
                ErrorMessage = _taskBlockRepository.ErrorMessage;
                return null;
            }

            var currentPeriod = GetPeriod(taskItem.Interval, focusDate);

            var previousFocusDate = TaskBlockDateHelper.GetPreviousFocusDate(taskItem.Interval, currentPeriod.Start);

            var nextFocusDate = TaskBlockDateHelper.GetNextFocusDate(taskItem.Interval, currentPeriod.Start);

            var previousPeriod = GetPeriod(taskItem.Interval, previousFocusDate);

            var nextPeriod = GetPeriod(taskItem.Interval, nextFocusDate);

            var currentUserId = _currentUserService.UserId;

            var viewModel = new TaskViewViewModel
            {
                TaskId = taskItem.Id,
                Name = taskItem.Name,
                Description = taskItem.Description,
                Interval = taskItem.Interval,
                WeekMode = taskItem.WeekMode,
                IsActive = taskItem.IsActive,
                FocusDate = focusDate,
                PreviousFocusDate = previousFocusDate,
                NextFocusDate = nextFocusDate,
                CurrentPeriodStart = currentPeriod.Start,
                CurrentPeriodEnd = currentPeriod.End,
                CurrentPeriodLabel = currentPeriod.Label,
                PreviousBlocks = MapBlocks(taskBlocks, previousPeriod.Start, previousPeriod.End, currentUserId),
                CurrentBlocks = MapBlocks(taskBlocks, currentPeriod.Start, currentPeriod.End, currentUserId),
                NextBlocks = MapBlocks(taskBlocks, nextPeriod.Start, nextPeriod.End, currentUserId)
            };

            return viewModel;
        }

        private PeriodInformation GetPeriod(TaskInterval interval, DateTime focusDate)
        {
            var start = TaskBlockDateHelper.GetPeriodStart(interval, focusDate);

            var end = TaskBlockDateHelper.GetNextPeriodStart(interval, start);

            var label = TaskBlockDateHelper.GetPeriodLabel(interval, start, end);

            var period = new PeriodInformation
            {
                Start = start,
                End = end,
                Label = label
            };

            return period;
        }

        private List<TaskBlockViewModel> MapBlocks(List<TaskBlock> blocks, DateTime periodStart, DateTime periodEnd, int? currentUserId)
        {
            // Block wird angezeigt, wenn er den Zeitraum überschneidet ([Start,End) Logik)
            var matchingBlocks = blocks.Where(block => block.Start < periodEnd && block.End > periodStart).OrderBy(block => block.Start).ToList();

            var viewModels = new List<TaskBlockViewModel>();

            foreach (var block in matchingBlocks)
            {
                var isCompleted = block.Status == TaskBlockStatus.Completed;

                var isOverdue = !isCompleted && block.End < DateTime.Now;

                var cssClass = GetCssClass(block, isOverdue, currentUserId);

                var statusText = GetStatusText(block.Status, isOverdue);

                var vm = new TaskBlockViewModel
                {
                    Id = block.Id,
                    TaskItemId = block.TaskItemId,
                    Start = block.Start,
                    End = block.End,
                    Status = block.Status,
                    StatusText = statusText,
                    CssClass = cssClass,
                    AssignedByUserId = block.AssignedByUserId,
                    AssignedByUserName = _userService.GetUserDisplayName(block.AssignedByUserId),
                    AssignedAt = block.AssignedAt,
                    CompletedByUserId = block.CompletedByUserId,
                    CompletedByUserName = _userService.GetUserDisplayName(block.CompletedByUserId),
                    CompletedAt = block.CompletedAt,
                    Note = block.Note,
                    IsCompleted = isCompleted,
                    IsOverdue = isOverdue,
                    CanBeTakenOver = block.Status == TaskBlockStatus.Free,
                    IsSelected = false
                };

                viewModels.Add(vm);
            }

            return viewModels;
        }

        private string GetCssClass(TaskBlock block, bool isOverdue, int? currentUserId)
        {
            if (block.Status == TaskBlockStatus.Completed)
            {
                return "block-completed";
            }

            if (isOverdue || block.Status == TaskBlockStatus.Overdue)
            {
                return "block-overdue";
            }

            if (block.Status == TaskBlockStatus.Assigned)
            {
                if (block.AssignedByUserId == currentUserId)
                {
                    return "block-mine";
                }

                return "block-other";
            }

            return "block-free";
        }

        private string GetStatusText(TaskBlockStatus status, bool isOverdue)
        {
            if (isOverdue && status != TaskBlockStatus.Completed)
            {
                return "Überfällig";
            }

            string text;

            switch (status)
            {
                case TaskBlockStatus.Free:
                    text = "Frei";
                    break;

                case TaskBlockStatus.Assigned:
                    text = "Übernommen";
                    break;

                case TaskBlockStatus.Completed:
                    text = "Erledigt";
                    break;

                case TaskBlockStatus.Overdue:
                    text = "Überfällig";
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(status), status, "Unbekannter Blockstatus.");
            }

            return text;
        }

        private class PeriodInformation
        {
            public DateTime Start { get; set; }

            public DateTime End { get; set; }

            public string Label { get; set; } = string.Empty;
        }
    }
}