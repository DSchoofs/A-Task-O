using Schoofs_ATaskO.Helpers;
using Schoofs_ATaskO.Models;
using Schoofs_ATaskO.Repositories;
using Schoofs_ATaskO.ViewModels;

namespace Schoofs_ATaskO.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly ITaskRepository _taskRepository;
        private readonly ITaskBlockRepository _taskBlockRepository;
        private readonly ICurrentUserService _currentUserService;
        private readonly IUserService _userService;

        public DashboardService(
            ITaskRepository taskRepository,
            ITaskBlockRepository taskBlockRepository,
            ICurrentUserService currentUserService,
            IUserService userService)
        {
            _taskRepository = taskRepository;
            _taskBlockRepository = taskBlockRepository;
            _currentUserService = currentUserService;
            _userService = userService;
        }

        public DashboardViewModel GetDashboard()
        {
            var today = DateTime.Today;
            var tomorrow = today.AddDays(1);
            var upcomingEnd = today.AddDays(8);

            var currentUserId = _currentUserService.UserId;
            var isAdmin = _currentUserService.IsAdmin;

            var tasksById = _taskRepository.GetAllTasks()
                .Where(task => task.IsActive)
                .ToDictionary(task => task.Id);

            var visibleBlocks = _taskBlockRepository.GetAllBlocks()
                .Where(block => tasksById.ContainsKey(block.TaskItemId))
                .Where(block => block.Status != TaskBlockStatus.Completed)
                .Where(block => CanBeShownToUser(block, currentUserId, isAdmin))
                .ToList();

            var overdueBlocks = visibleBlocks
                .Where(block => block.End < DateTime.Now)
                .OrderBy(block => block.End)
                .ToList();

            var todayBlocks = visibleBlocks
                .Where(block => block.Start < tomorrow && block.End > today)
                .Where(block => !overdueBlocks.Any(overdueBlock => overdueBlock.Id == block.Id))
                .OrderBy(block => GetPriority(block, currentUserId))
                .ThenBy(block => block.Start)
                .ToList();

            var upcomingBlocks = visibleBlocks
                .Where(block => block.Start >= tomorrow && block.Start < upcomingEnd)
                .Where(block => !overdueBlocks.Any(overdueBlock => overdueBlock.Id == block.Id))
                .Where(block => !todayBlocks.Any(todayBlock => todayBlock.Id == block.Id))
                .OrderBy(block => block.Start)
                .ToList();

            var userName = currentUserId.HasValue
                ? _userService.GetUserDisplayName(currentUserId)
                : string.Empty;

            return new DashboardViewModel
            {
                UserName = userName,
                Today = today,
                OverdueBlocks = MapBlocks(overdueBlocks, tasksById, currentUserId),
                TodayBlocks = MapBlocks(todayBlocks, tasksById, currentUserId),
                UpcomingBlocks = MapBlocks(upcomingBlocks, tasksById, currentUserId)
            };
        }

        private static bool CanBeShownToUser(
            TaskBlock block,
            int? currentUserId,
            bool isAdmin)
        {
            if (isAdmin)
            {
                return true;
            }

            return block.Status == TaskBlockStatus.Free ||
                   block.AssignedByUserId == currentUserId;
        }

        private static int GetPriority(
            TaskBlock block,
            int? currentUserId)
        {
            if (block.AssignedByUserId == currentUserId)
            {
                return 1;
            }

            if (block.Status == TaskBlockStatus.Free)
            {
                return 2;
            }

            return 3;
        }

        private List<DashboardBlockViewModel> MapBlocks(
            List<TaskBlock> blocks,
            Dictionary<int, TaskItem> tasksById,
            int? currentUserId)
        {
            var viewModels = new List<DashboardBlockViewModel>();

            foreach (var block in blocks)
            {
                var taskItem = tasksById[block.TaskItemId];

                var isOverdue = block.End < DateTime.Now;

                var viewModel = new DashboardBlockViewModel
                {
                    TaskId = block.TaskItemId,
                    BlockId = block.Id,
                    TaskName = taskItem.Name,
                    Start = block.Start,
                    End = block.End,
                    DisplayTime = TaskBlockDateHelper.GetBlockDisplayName(
                        block.Interval,
                        taskItem.WeekMode,
                        block.Start,
                        block.End),
                    StatusText = GetStatusText(block, isOverdue),
                    CssClass = GetCssClass(block, isOverdue, currentUserId),
                    AssignedByUserId = block.AssignedByUserId,
                    AssignedByUserName = _userService.GetUserDisplayName(block.AssignedByUserId)
                };

                viewModels.Add(viewModel);
            }

            return viewModels;
        }

        private static string GetStatusText(
            TaskBlock block,
            bool isOverdue)
        {
            if (isOverdue)
            {
                return "Überfällig";
            }

            if (block.Status == TaskBlockStatus.Free)
            {
                return "Frei";
            }

            if (block.Status == TaskBlockStatus.Assigned)
            {
                return "Übernommen";
            }

            return string.Empty;
        }

        private static string GetCssClass(
            TaskBlock block,
            bool isOverdue,
            int? currentUserId)
        {
            if (isOverdue)
            {
                return "block-overdue";
            }

            if (block.Status == TaskBlockStatus.Assigned)
            {
                return block.AssignedByUserId == currentUserId
                    ? "block-mine"
                    : "block-other";
            }

            return "block-free";
        }
    }
}