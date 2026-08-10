using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Reflection;
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

            var previousFocusDate = GetPreviousFocusDate(taskItem.Interval, currentPeriod.Start);

            var nextFocusDate = GetNextFocusDate(taskItem.Interval, currentPeriod.Start);

            var previousPeriod = GetPeriod(taskItem.Interval, previousFocusDate);

            var nextPeriod = GetPeriod(taskItem.Interval, nextFocusDate);

            var currentUserId = _currentUserService.UserId;

            return new TaskViewViewModel
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
                NextBlocks = MapBlocks(taskBlocks, nextPeriod.Start,nextPeriod.End, currentUserId)
            };
        }

        private List<TaskBlockViewModel> MapBlocks(List<TaskBlock> blocks, DateTime periodStart, DateTime periodEnd, int? currentUserId)
        {
            var matchingBlocks = blocks.Where(block => block.Start >= periodStart && block.Start < periodEnd).OrderBy(block => block.Start).ToList();

            var viewModels = new List<TaskBlockViewModel>();

            foreach (var block in matchingBlocks)
            {
                var isCompleted = block.Status == TaskBlockStatus.Completed;

                var isOverdue = !isCompleted && block.End < DateTime.Now;

                var cssClass = GetCssClass(block, isOverdue, currentUserId);

                var statusText = GetStatusText(block.Status, isOverdue);

                viewModels.Add(new TaskBlockViewModel
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
                });
            }

            return viewModels;
        }

        private string GetCssClass(TaskBlock block, bool isOverdue, int? currentUserId)
        {
            if (block.Status == TaskBlockStatus.Completed)
            {
                return "block-completed";
            }

            if (isOverdue ||
                block.Status == TaskBlockStatus.Overdue)
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

            var displayAttribute = typeof(TaskBlockStatus).GetMember(status.ToString()).FirstOrDefault()?.GetCustomAttribute<DisplayAttribute>();

            return displayAttribute?.Name ?? status.ToString();
        }

        private PeriodInformation GetPeriod(TaskInterval interval, DateTime focusDate)
        {
            var start = GetPeriodStart(interval, focusDate);
            var end = GetNextPeriodStart(interval, start);
            var label = GetPeriodLabel(interval, start, end);

            return new PeriodInformation
            {
                Start = start,
                End = end,
                Label = label
            };
        }

        private DateTime GetPeriodStart(TaskInterval interval, DateTime focusDate)
        {
            var date = focusDate.Date;

            return interval switch
            {
                TaskInterval.Daily => StartOfWeek(date),

                TaskInterval.Weekly => new DateTime(date.Year, date.Month, 1),

                TaskInterval.Monthly => new DateTime(date.Year, 1, 1), _ => date
            };
        }

        private DateTime GetNextPeriodStart(TaskInterval interval, DateTime periodStart)
        {
            return interval switch
            {
                TaskInterval.Daily => periodStart.AddDays(7),

                TaskInterval.Weekly => periodStart.AddMonths(1),

                TaskInterval.Monthly => periodStart.AddYears(1), _ => periodStart.AddDays(1)
            };
        }

        private DateTime GetPreviousFocusDate(TaskInterval interval, DateTime currentPeriodStart)
        {
            return interval switch
            {
                TaskInterval.Daily => currentPeriodStart.AddDays(-7),

                TaskInterval.Weekly => currentPeriodStart.AddMonths(-1),

                TaskInterval.Monthly => currentPeriodStart.AddYears(-1), _ => currentPeriodStart.AddDays(-1)
            };
        }

        private DateTime GetNextFocusDate(TaskInterval interval, DateTime currentPeriodStart)
        {
            return interval switch
            {
                TaskInterval.Daily => currentPeriodStart.AddDays(7),

                TaskInterval.Weekly => currentPeriodStart.AddMonths(1),

                TaskInterval.Monthly => currentPeriodStart.AddYears(1), _ => currentPeriodStart.AddDays(1)
            };
        }

        private DateTime StartOfWeek(DateTime date)
        {
            var dayOfWeek = date.DayOfWeek;

            var daysSinceMonday = dayOfWeek == DayOfWeek.Sunday ? 6 : (int)dayOfWeek - 1;

            return date.AddDays(-daysSinceMonday).Date;
        }

        private string GetPeriodLabel(TaskInterval interval,DateTime start, DateTime end)
        {
            var culture = CultureInfo.GetCultureInfo("de-DE");

            return interval switch
            {
                TaskInterval.Daily => $"{start:dd.MM.yyyy} - {end.AddDays(-1):dd.MM.yyyy}",

                TaskInterval.Weekly => start.ToString("MMMM yyyy", culture),

                TaskInterval.Monthly => start.ToString("yyyy", culture), _ => start.ToString("dd.MM.yyyy")
            };
        }

        private class PeriodInformation
        {
            public DateTime Start { get; set; }

            public DateTime End { get; set; }

            public string Label { get; set; } = string.Empty;
        }
    }
}