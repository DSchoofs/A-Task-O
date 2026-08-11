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

        public TaskViewViewModel? GetTaskView(int taskItemId, DateTime focusDate, int? selectedBlockId = null)
        {
            ErrorMessage = string.Empty;

            var taskItem = _taskRepository.GetTaskById(taskItemId);

            if (taskItem is null)
            {
                ErrorMessage = !string.IsNullOrWhiteSpace(_taskRepository.ErrorMessage)
                    ? _taskRepository.ErrorMessage
                    : "Die Aufgabe wurde nicht gefunden.";

                return null;
            }

            var blocksGenerated = GenerateBlocks(taskItemId, focusDate);

            if (!blocksGenerated)
            {
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
                PreviousBlocks = MapBlocks(taskBlocks,taskItem.Interval,taskItem.WeekMode, previousPeriod.Start,previousPeriod.End,currentUserId),
                CurrentBlocks = MapBlocks(taskBlocks, taskItem.Interval,taskItem.WeekMode,currentPeriod.Start,currentPeriod.End, currentUserId),
                NextBlocks = MapBlocks(taskBlocks,taskItem.Interval,taskItem.WeekMode,nextPeriod.Start,nextPeriod.End,currentUserId),
                PreviousPeriodTitle = TaskBlockDateHelper.GetPeriodTitle( taskItem.Interval,PeriodPosition.Previous),
                CurrentPeriodTitle = TaskBlockDateHelper.GetPeriodTitle(taskItem.Interval,PeriodPosition.Current),
                NextPeriodTitle = TaskBlockDateHelper.GetPeriodTitle( taskItem.Interval, PeriodPosition.Next),
                PreviousPeriodDateRange = previousPeriod.DateRange,
                CurrentPeriodDateRange = currentPeriod.DateRange,
                NextPeriodDateRange = nextPeriod.DateRange,
                PreviousCalendarWeek = previousPeriod.CalendarWeek,
                CurrentCalendarWeek = currentPeriod.CalendarWeek,
                NextCalendarWeek = nextPeriod.CalendarWeek
            };

            if (selectedBlockId.HasValue)
            {
                var selectedBlock = viewModel.PreviousBlocks.Concat(viewModel.CurrentBlocks).Concat(viewModel.NextBlocks).FirstOrDefault(block => block.Id == selectedBlockId.Value);

                if (selectedBlock is not null)
                {
                    selectedBlock.IsSelected = true;
                    viewModel.SelectedBlock = selectedBlock;
                }
            }

            return viewModel;
        }
        public bool TakeOverBlock(int blockId)
        {
            ErrorMessage = string.Empty;

            var currentUserId = _currentUserService.UserId;

            if (!currentUserId.HasValue)
            {
                ErrorMessage = "Es konnte kein angemeldeter Benutzer ermittelt werden.";
                return false;
            }

            var block = _taskBlockRepository.GetBlockById(blockId);

            if (block is null)
            {
                ErrorMessage = !string.IsNullOrWhiteSpace(_taskBlockRepository.ErrorMessage) ? _taskBlockRepository.ErrorMessage : "Der Aufgabenblock wurde nicht gefunden.";

                return false;
            }

            if (block.Status != TaskBlockStatus.Free)
            {
                ErrorMessage = "Dieser Aufgabenblock ist nicht mehr frei.";
                return false;
            }

            block.Status = TaskBlockStatus.Assigned;
            block.AssignedByUserId = currentUserId.Value;
            block.AssignedAt = DateTime.UtcNow;

            var updated = _taskBlockRepository.UpdateBlock(block);

            if (!updated)
            {
                ErrorMessage = !string.IsNullOrWhiteSpace(_taskBlockRepository.ErrorMessage) ? _taskBlockRepository.ErrorMessage : "Der Aufgabenblock konnte nicht übernommen werden.";

                return false;
            }

            return true;
        }

        public bool CompleteBlock(int blockId)
        {
            ErrorMessage = string.Empty;

            var currentUserId = _currentUserService.UserId;

            if (!currentUserId.HasValue)
            {
                ErrorMessage = "Es konnte kein angemeldeter Benutzer ermittelt werden.";
                return false;
            }

            var block = _taskBlockRepository.GetBlockById(blockId);

            if (block is null)
            {
                ErrorMessage = !string.IsNullOrWhiteSpace(_taskBlockRepository.ErrorMessage) ? _taskBlockRepository.ErrorMessage : "Der Aufgabenblock wurde nicht gefunden.";

                return false;
            }

            if (block.Status != TaskBlockStatus.Assigned)
            {
                ErrorMessage = "Nur übernommene Aufgabenblöcke können erledigt werden.";
                return false;
            }

            if (block.AssignedByUserId != currentUserId.Value)
            {
                ErrorMessage = "Nur der Benutzer, der den Block übernommen hat, darf ihn erledigen.";
                return false;
            }

            block.Status = TaskBlockStatus.Completed;
            block.CompletedByUserId = currentUserId.Value;
            block.CompletedAt = DateTime.UtcNow;

            var updated = _taskBlockRepository.UpdateBlock(block);

            if (!updated)
            {
                ErrorMessage = !string.IsNullOrWhiteSpace(_taskBlockRepository.ErrorMessage) ? _taskBlockRepository.ErrorMessage : "Der Aufgabenblock konnte nicht als erledigt gespeichert werden.";

                return false;
            }

            return true;
        }

        private PeriodInformation GetPeriod(TaskInterval interval, DateTime focusDate)
        {
            var start = TaskBlockDateHelper.GetPeriodStart(interval, focusDate);

            var end = TaskBlockDateHelper.GetNextPeriodStart(interval, start);

            var label = TaskBlockDateHelper.GetPeriodLabel(interval, start, end);

            var dateRange = TaskBlockDateHelper.GetPeriodDateRange(start, end);

            var calendarWeek = TaskBlockDateHelper.GetCalendarWeekLabel(interval, start);

            var period = new PeriodInformation
            {
                Start = start,
                End = end,
                Label = label,
                DateRange = dateRange,
                CalendarWeek = calendarWeek
            };

            return period;
        }

        private List<TaskBlockViewModel> MapBlocks(List<TaskBlock> blocks, TaskInterval interval, TaskWeekMode weekMode, DateTime periodStart, DateTime periodEnd, int? currentUserId)
        {
            var matchingBlocks = blocks.Where(block => block.Interval == interval && block.Start < periodEnd && block.End > periodStart).OrderBy(block => block.Start).ToList();
            var viewModels = new List<TaskBlockViewModel>();

            foreach (var block in matchingBlocks)
            {
                var isCompleted = block.Status == TaskBlockStatus.Completed;

                var isOverdue = !isCompleted && block.End < DateTime.Now;

                var cssClass = GetCssClass(block, isOverdue, currentUserId);

                var statusText = GetStatusText(block.Status, isOverdue);

                var continuesFromPreviousPeriod = block.Start < periodStart;

                var continuesIntoNextPeriod = block.End > periodEnd;
                var displayName = TaskBlockDateHelper.GetBlockDisplayName(interval, weekMode, block.Start, block.End);
                var vm = new TaskBlockViewModel
                {
                    Id = block.Id,
                    TaskItemId = block.TaskItemId,
                    Start = block.Start,
                    End = block.End,
                    DisplayTime = displayName,
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
                    CanBeCompleted = block.Status == TaskBlockStatus.Assigned && block.AssignedByUserId == currentUserId,
                    IsSelected = false,
                    ContinuesFromPreviousPeriod = continuesFromPreviousPeriod,
                    ContinuesIntoNextPeriod = continuesIntoNextPeriod
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

        public bool GenerateBlocks(int taskItemId, DateTime focusDate)
        {
            ErrorMessage = string.Empty;

            var taskItem = _taskRepository.GetTaskById(taskItemId);

            if (taskItem is null)
            {
                ErrorMessage = !string.IsNullOrWhiteSpace(_taskRepository.ErrorMessage)
                    ? _taskRepository.ErrorMessage
                    : "Die Aufgabe wurde nicht gefunden.";

                return false;
            }

            if (!taskItem.IsActive)
            {
                return true;
            }

            var currentPeriod = GetPeriod(taskItem.Interval, focusDate);

            var previousFocusDate = TaskBlockDateHelper.GetPreviousFocusDate(taskItem.Interval, currentPeriod.Start);

            var nextFocusDate = TaskBlockDateHelper.GetNextFocusDate(taskItem.Interval, currentPeriod.Start);

            var previousPeriod = GetPeriod(taskItem.Interval, previousFocusDate);

            var nextPeriod = GetPeriod(taskItem.Interval, nextFocusDate);

            var existingBlocks = _taskBlockRepository.GetBlocksByTaskItemId(taskItemId);

            if (existingBlocks.Count == 0 && !string.IsNullOrWhiteSpace(_taskBlockRepository.ErrorMessage))
            {
                ErrorMessage = _taskBlockRepository.ErrorMessage;
                return false;
            }

            var rangeStart = previousPeriod.Start;
            var rangeEnd = nextPeriod.End;

            var blocksToCreate = CreateExpectedBlocks(taskItem, rangeStart, rangeEnd);

            foreach (var block in blocksToCreate)
            {
                var alreadyExists = existingBlocks.Any(existingBlock => existingBlock.Start == block.Start && existingBlock.End == block.End && existingBlock.Interval == block.Interval);

                if (alreadyExists)
                {
                    continue;
                }

                var blockId = _taskBlockRepository.AddBlock(block);

                if (blockId <= 0)
                {
                    ErrorMessage = !string.IsNullOrWhiteSpace(_taskBlockRepository.ErrorMessage)
                        ? _taskBlockRepository.ErrorMessage
                        : "Ein Aufgabenblock konnte nicht angelegt werden.";

                    return false;
                }
            }

            return true;
        }

        private List<TaskBlock> CreateExpectedBlocks(TaskItem taskItem, DateTime rangeStart, DateTime rangeEnd)
        {
            var blocks = new List<TaskBlock>();

            switch (taskItem.Interval)
            {
                case TaskInterval.Daily:
                    blocks = CreateDailyBlocks(taskItem, rangeStart, rangeEnd);
                    break;

                case TaskInterval.Weekly:
                    blocks = CreateWeeklyBlocks(taskItem, rangeStart, rangeEnd);
                    break;

                case TaskInterval.Monthly:
                    blocks = CreateMonthlyBlocks(taskItem, rangeStart, rangeEnd);
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(taskItem.Interval), taskItem.Interval, "Unbekanntes Aufgabenintervall.");
            }

            return blocks;
        }
        private List<TaskBlock> CreateDailyBlocks(TaskItem taskItem, DateTime rangeStart, DateTime rangeEnd)
        {
            var blocks = new List<TaskBlock>();

            var currentStart = rangeStart.Date;

            while (currentStart < rangeEnd)
            {
                var includeDay = true;

                switch (taskItem.WeekMode)
                {
                    case TaskWeekMode.Weekdays:
                        includeDay = currentStart.DayOfWeek != DayOfWeek.Saturday && currentStart.DayOfWeek != DayOfWeek.Sunday;
                        break;

                    case TaskWeekMode.MondayToSaturday:
                        includeDay = currentStart.DayOfWeek != DayOfWeek.Sunday;
                        break;

                    case TaskWeekMode.FullWeek:
                        includeDay = true;
                        break;

                    default:
                        throw new ArgumentOutOfRangeException(nameof(taskItem.WeekMode), taskItem.WeekMode, "Unbekannte Wochenendregel.");
                }

                if (includeDay)
                {
                    var currentEnd = currentStart.AddDays(1);

                    var block = CreateFreeBlock(taskItem.Id, taskItem.Interval, currentStart, currentEnd);

                    blocks.Add(block);
                }

                currentStart = currentStart.AddDays(1);
            }

            return blocks;
        }
        private List<TaskBlock> CreateWeeklyBlocks(TaskItem taskItem, DateTime rangeStart, DateTime rangeEnd)
        {
            var blocks = new List<TaskBlock>();

            var currentStart = TaskBlockDateHelper.StartOfWeek(rangeStart);

            while (currentStart < rangeEnd)
            {
                var currentEnd = TaskBlockDateHelper.GetWeeklyBlockEnd(currentStart, taskItem.WeekMode);

                var overlapsRange = currentStart < rangeEnd && currentEnd > rangeStart;

                if (overlapsRange)
                {
                    var block = CreateFreeBlock(taskItem.Id, taskItem.Interval, currentStart, currentEnd);

                    blocks.Add(block);
                }

                currentStart = currentStart.AddDays(7);
            }

            return blocks;
        }
        private List<TaskBlock> CreateMonthlyBlocks(TaskItem taskItem, DateTime rangeStart, DateTime rangeEnd)
        {
            var blocks = new List<TaskBlock>();

            var currentStart = new DateTime(rangeStart.Year, rangeStart.Month, 1);

            while (currentStart < rangeEnd)
            {
                var currentEnd = currentStart.AddMonths(1);

                var block = CreateFreeBlock(taskItem.Id, taskItem.Interval, currentStart, currentEnd);

                blocks.Add(block);

                currentStart = currentEnd;
            }

            return blocks;
        }
        private TaskBlock CreateFreeBlock(int taskItemId, TaskInterval interval, DateTime start, DateTime end)
        {
            var block = new TaskBlock
            {
                TaskItemId = taskItemId,
                Start = start,
                End = end,
                Interval = interval,
                Status = TaskBlockStatus.Free,
                AssignedByUserId = null,
                AssignedAt = null,
                CompletedByUserId = null,
                CompletedAt = null,
                Note = null
            };

            return block;
        }
        private class PeriodInformation
        {
            public DateTime Start { get; set; }

            public DateTime End { get; set; }

            public string Label { get; set; } = string.Empty;

            public string DateRange { get; set; } = string.Empty;

            public string CalendarWeek { get; set; } = string.Empty;
        }
    }
}