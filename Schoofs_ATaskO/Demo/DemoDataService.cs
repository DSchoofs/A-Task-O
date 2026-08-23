using Dapper;
using Microsoft.Data.SqlClient;
using Schoofs_ATaskO.Helpers;
using Schoofs_ATaskO.Models;
using Schoofs_ATaskO.Services;

namespace Schoofs_ATaskO.Demo
{
    public class DemoDataService : IDemoDataService
    {
        private readonly IConfiguration _configuration;
        private readonly IPasswordService _passwordService;

        public string ErrorMessage { get; private set; } = string.Empty;

        public DemoDataService(
            IConfiguration configuration,
            IPasswordService passwordService)
        {
            _configuration = configuration;
            _passwordService = passwordService;
        }

        public bool CanCreateDemoData()
        {
            ErrorMessage = string.Empty;

            var connectionString = _configuration.GetConnectionString("ATaskO");

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                ErrorMessage = "Es ist keine Datenbankverbindung konfiguriert.";
                return false;
            }

            try
            {
                const string sql = """
                    SELECT COUNT(1)
                    FROM Users
                    WHERE Email IN
                    (
                        'user1@web.de',
                        'user2@web.de',
                        'user3@web.de',
                        'user4@web.de',
                        'user5@web.de'
                    );
                    """;

                using var connection = new SqlConnection(connectionString);

                connection.Open();

                var existingDemoUsers = connection.ExecuteScalar<int>(sql);

                if (existingDemoUsers > 0)
                {
                    ErrorMessage = "Die Demo-Daten wurden bereits erstellt.";
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Die Demo-Daten konnten nicht geprüft werden: {ex.Message}";
                return false;
            }
        }

        public bool CreateDemoData()
        {
            ErrorMessage = string.Empty;

            if (!CanCreateDemoData())
            {
                return false;
            }

            var connectionString = _configuration.GetConnectionString("ATaskO");

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                ErrorMessage = "Es ist keine Datenbankverbindung konfiguriert.";
                return false;
            }

            try
            {
                using var connection = new SqlConnection(connectionString);

                connection.Open();

                using var transaction = connection.BeginTransaction();

                var userIds = CreateDemoUsers(connection, transaction);

                var taskIds = CreateDemoTasks(connection, transaction, userIds);

                CreateDemoBlocks(connection, transaction, userIds, taskIds);

                UpdateCompletedTasks(connection, transaction, userIds);

                transaction.Commit();

                return true;
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Die Demo-Daten konnten nicht erstellt werden: {ex.Message}";
                return false;
            }
        }

        private Dictionary<int, int> CreateDemoUsers(
            SqlConnection connection,
            SqlTransaction transaction)
        {
            const string sql = """
                INSERT INTO Users
                (
                    FirstName,
                    LastName,
                    Email,
                    PasswordHash,
                    PasswordSalt,
                    IsAdmin,
                    CompletedTasks
                )
                OUTPUT INSERTED.UserId
                VALUES
                (
                    @FirstName,
                    @LastName,
                    @Email,
                    @PasswordHash,
                    @PasswordSalt,
                    @IsAdmin,
                    @CompletedTasks
                );
                """;

            var userIds = new Dictionary<int, int>();

            for (var number = 1; number <= 5; number++)
            {
                var salt = _passwordService.GenerateSalt();
                var passwordHash = _passwordService.HashPassword("user", salt);

                var userId = connection.QuerySingle<int>(
                    sql,
                    new
                    {
                        FirstName = $"User{number}",
                        LastName = $"User{number}",
                        Email = $"user{number}@web.de",
                        PasswordHash = passwordHash,
                        PasswordSalt = salt,
                        IsAdmin = false,
                        CompletedTasks = 0
                    },
                    transaction);

                userIds.Add(number, userId);
            }

            return userIds;
        }

        private static Dictionary<int, int> CreateDemoTasks(
            SqlConnection connection,
            SqlTransaction transaction,
            Dictionary<int, int> userIds)
        {
            const string sql = """
                INSERT INTO TaskItems
                (
                    Name,
                    Description,
                    Interval,
                    WeekMode,
                    IsActive,
                    CreatedAt,
                    CreatedByUserId,
                    ModifiedAt,
                    ModifiedByUserId
                )
                OUTPUT INSERTED.Id
                VALUES
                (
                    @Name,
                    @Description,
                    @Interval,
                    @WeekMode,
                    @IsActive,
                    @CreatedAt,
                    @CreatedByUserId,
                    @ModifiedAt,
                    @ModifiedByUserId
                );
                """;

            var createdAt = DateTime.Today.AddDays(-14);

            var tasks = new[]
            {
                new
                {
                    Number = 1,
                    Name = "Arbeitsplatz aufräumen",
                    Description = "Arbeitsplatz am Ende des Tages aufräumen und Materialien sortieren.",
                    Interval = TaskInterval.Daily,
                    WeekMode = TaskWeekMode.Weekdays,
                    CreatedByUserNumber = 1
                },
                new
                {
                    Number = 2,
                    Name = "E-Mails bearbeiten",
                    Description = "Neue E-Mails prüfen, bearbeiten und bei Bedarf weiterleiten.",
                    Interval = TaskInterval.Daily,
                    WeekMode = TaskWeekMode.Weekdays,
                    CreatedByUserNumber = 2
                },
                new
                {
                    Number = 3,
                    Name = "Wochenbericht erstellen",
                    Description = "Wöchentliche Zusammenfassung der erledigten Aufgaben erstellen.",
                    Interval = TaskInterval.Weekly,
                    WeekMode = TaskWeekMode.FullWeek,
                    CreatedByUserNumber = 3
                },
                new
                {
                    Number = 4,
                    Name = "Teammeeting vorbereiten",
                    Description = "Agenda erstellen und benötigte Unterlagen vorbereiten.",
                    Interval = TaskInterval.Weekly,
                    WeekMode = TaskWeekMode.Weekdays,
                    CreatedByUserNumber = 4
                },
                new
                {
                    Number = 5,
                    Name = "Monatsstatistik erstellen",
                    Description = "Monatliche Kennzahlen zusammenstellen und prüfen.",
                    Interval = TaskInterval.Monthly,
                    WeekMode = TaskWeekMode.FullWeek,
                    CreatedByUserNumber = 5
                },
                new
                {
                    Number = 6,
                    Name = "Materialbestand prüfen",
                    Description = "Verbrauchsmaterialien prüfen und fehlende Bestände notieren.",
                    Interval = TaskInterval.Monthly,
                    WeekMode = TaskWeekMode.Weekdays,
                    CreatedByUserNumber = 1
                },
                new
                {
                    Number = 7,
                    Name = "Sicherheitsunterweisung",
                    Description = "Regelmäßige Sicherheitsunterweisung dokumentieren.",
                    Interval = TaskInterval.Monthly,
                    WeekMode = TaskWeekMode.FullWeek,
                    CreatedByUserNumber = 2
                }
            };

            var taskIds = new Dictionary<int, int>();

            foreach (var task in tasks)
            {
                var taskId = connection.QuerySingle<int>(
                    sql,
                    new
                    {
                        task.Name,
                        task.Description,
                        Interval = (int)task.Interval,
                        WeekMode = (int)task.WeekMode,
                        IsActive = true,
                        CreatedAt = createdAt,
                        CreatedByUserId = userIds[task.CreatedByUserNumber],
                        ModifiedAt = (DateTime?)null,
                        ModifiedByUserId = (int?)null
                    },
                    transaction);

                taskIds.Add(task.Number, taskId);
            }

            return taskIds;
        }

        private static void CreateDemoBlocks(
            SqlConnection connection,
            SqlTransaction transaction,
            Dictionary<int, int> userIds,
            Dictionary<int, int> taskIds)
        {
            var today = DateTime.Today;

            var currentWeekday = IsIncludedDailyDay(today, TaskWeekMode.Weekdays)
                ? today
                : GetPreviousIncludedDailyDate(today, TaskWeekMode.Weekdays);

            var lastWeekday = GetPreviousIncludedDailyDate(
                currentWeekday.AddDays(-1),
                TaskWeekMode.Weekdays);

            var secondLastWeekday = GetPreviousIncludedDailyDate(
                lastWeekday.AddDays(-1),
                TaskWeekMode.Weekdays);

            var nextWeekday = GetNextIncludedDailyDate(
                currentWeekday.AddDays(1),
                TaskWeekMode.Weekdays);

            var secondNextWeekday = GetNextIncludedDailyDate(
                nextWeekday.AddDays(1),
                TaskWeekMode.Weekdays);

            var blocks = new List<DemoBlock>
            {
                CreateDailyBlock(
                    taskIds[1],
                    secondLastWeekday,
                    TaskWeekMode.Weekdays,
                    TaskBlockStatus.Completed,
                    userIds[1],
                    userIds[1],
                    "Arbeitsplatz vollständig aufgeräumt."),

                CreateDailyBlock(
                    taskIds[1],
                    lastWeekday,
                    TaskWeekMode.Weekdays,
                    TaskBlockStatus.Completed,
                    userIds[3],
                    userIds[3],
                    "Materialien sortiert."),

                CreateDailyBlock(
                    taskIds[1],
                    currentWeekday,
                    TaskWeekMode.Weekdays,
                    TaskBlockStatus.Assigned,
                    userIds[2],
                    null,
                    null),

                CreateDailyBlock(
                    taskIds[1],
                    nextWeekday,
                    TaskWeekMode.Weekdays,
                    TaskBlockStatus.Assigned,
                    userIds[3],
                    null,
                    null),

                CreateDailyBlock(
                    taskIds[1],
                    secondNextWeekday,
                    TaskWeekMode.Weekdays,
                    TaskBlockStatus.Free,
                    null,
                    null,
                    null),

                CreateDailyBlock(
                    taskIds[2],
                    secondLastWeekday,
                    TaskWeekMode.Weekdays,
                    TaskBlockStatus.Completed,
                    userIds[2],
                    userIds[2],
                    "Postfach vollständig bearbeitet."),

                CreateDailyBlock(
                    taskIds[2],
                    lastWeekday,
                    TaskWeekMode.Weekdays,
                    TaskBlockStatus.Completed,
                    userIds[1],
                    userIds[1],
                    "Rückfragen beantwortet."),

                CreateDailyBlock(
                    taskIds[2],
                    currentWeekday,
                    TaskWeekMode.Weekdays,
                    TaskBlockStatus.Assigned,
                    userIds[4],
                    null,
                    null),

                CreateDailyBlock(
                    taskIds[2],
                    nextWeekday,
                    TaskWeekMode.Weekdays,
                    TaskBlockStatus.Assigned,
                    userIds[5],
                    null,
                    null),

                CreateDailyBlock(
                    taskIds[2],
                    secondNextWeekday,
                    TaskWeekMode.Weekdays,
                    TaskBlockStatus.Assigned,
                    userIds[2],
                    null,
                    null),

                CreateWeeklyBlock(
                    taskIds[3],
                    today.AddDays(-14),
                    TaskWeekMode.FullWeek,
                    TaskBlockStatus.Completed,
                    userIds[3],
                    userIds[3],
                    "Wochenbericht vollständig abgegeben."),

                CreateWeeklyBlock(
                    taskIds[3],
                    today.AddDays(-7),
                    TaskWeekMode.FullWeek,
                    TaskBlockStatus.Completed,
                    userIds[5],
                    userIds[5],
                    "Bericht mit allen Kennzahlen abgeschlossen."),

                CreateWeeklyBlock(
                    taskIds[3],
                    today,
                    TaskWeekMode.FullWeek,
                    TaskBlockStatus.Assigned,
                    userIds[5],
                    null,
                    null),

                CreateWeeklyBlock(
                    taskIds[3],
                    today.AddDays(7),
                    TaskWeekMode.FullWeek,
                    TaskBlockStatus.Assigned,
                    userIds[2],
                    null,
                    null),

                CreateWeeklyBlock(
                    taskIds[4],
                    today.AddDays(-7),
                    TaskWeekMode.Weekdays,
                    TaskBlockStatus.Free,
                    null,
                    null,
                    null),

                CreateWeeklyBlock(
                    taskIds[4],
                    today,
                    TaskWeekMode.Weekdays,
                    TaskBlockStatus.Assigned,
                    userIds[1],
                    null,
                    null),

                CreateWeeklyBlock(
                    taskIds[4],
                    today.AddDays(7),
                    TaskWeekMode.Weekdays,
                    TaskBlockStatus.Assigned,
                    userIds[4],
                    null,
                    null),

                CreateMonthlyBlock(
                    taskIds[5],
                    today.AddMonths(-2),
                    TaskBlockStatus.Completed,
                    userIds[4],
                    userIds[4],
                    "Monatskennzahlen geprüft und dokumentiert."),

                CreateMonthlyBlock(
                    taskIds[5],
                    today.AddMonths(-1),
                    TaskBlockStatus.Completed,
                    userIds[4],
                    userIds[4],
                    "Kennzahlen geprüft."),

                CreateMonthlyBlock(
                    taskIds[5],
                    today.AddMonths(1),
                    TaskBlockStatus.Assigned,
                    userIds[3],
                    null,
                    null),

                CreateMonthlyBlock(
                    taskIds[6],
                    today.AddMonths(-1),
                    TaskBlockStatus.Completed,
                    userIds[1],
                    userIds[1],
                    "Bestand geprüft und Nachbestellung notiert."),

                CreateMonthlyBlock(
                    taskIds[6],
                    today,
                    TaskBlockStatus.Free,
                    null,
                    null,
                    null),

                CreateMonthlyBlock(
                    taskIds[6],
                    today.AddMonths(1),
                    TaskBlockStatus.Assigned,
                    userIds[2],
                    null,
                    null),

                CreateMonthlyBlock(
                    taskIds[7],
                    today.AddMonths(-2),
                    TaskBlockStatus.Completed,
                    userIds[5],
                    userIds[5],
                    "Unterweisung dokumentiert."),

                CreateMonthlyBlock(
                    taskIds[7],
                    today.AddMonths(1),
                    TaskBlockStatus.Assigned,
                    userIds[5],
                    null,
                    null)
            };

            const string sql = """
                INSERT INTO TaskBlocks
                (
                    TaskItemId,
                    Start,
                    [End],
                    Interval,
                    Status,
                    AssignedByUserId,
                    AssignedAt,
                    CompletedByUserId,
                    CompletedAt,
                    Note
                )
                VALUES
                (
                    @TaskItemId,
                    @Start,
                    @End,
                    @Interval,
                    @Status,
                    @AssignedByUserId,
                    @AssignedAt,
                    @CompletedByUserId,
                    @CompletedAt,
                    @Note
                );
                """;

            foreach (var block in blocks)
            {
                connection.Execute(
                    sql,
                    new
                    {
                        block.TaskItemId,
                        block.Start,
                        block.End,
                        Interval = (int)block.Interval,
                        Status = (int)block.Status,
                        block.AssignedByUserId,
                        block.AssignedAt,
                        block.CompletedByUserId,
                        block.CompletedAt,
                        block.Note
                    },
                    transaction);
            }
        }

        private static DemoBlock CreateDailyBlock(
            int taskItemId,
            DateTime date,
            TaskWeekMode weekMode,
            TaskBlockStatus status,
            int? assignedByUserId,
            int? completedByUserId,
            string? note)
        {
            var start = date.Date;
            var end = start.AddDays(1);

            if (!IsIncludedDailyDay(start, weekMode))
            {
                throw new InvalidOperationException(
                    $"Für {start:dddd, dd.MM.yyyy} darf wegen der Wochenendregel kein Tagesblock angelegt werden.");
            }

            return CreateBlock(
                taskItemId,
                start,
                end,
                TaskInterval.Daily,
                status,
                assignedByUserId,
                completedByUserId,
                note);
        }

        private static DemoBlock CreateWeeklyBlock(
            int taskItemId,
            DateTime dateInWeek,
            TaskWeekMode weekMode,
            TaskBlockStatus status,
            int? assignedByUserId,
            int? completedByUserId,
            string? note)
        {
            var start = TaskBlockDateHelper.StartOfWeek(dateInWeek);
            var end = TaskBlockDateHelper.GetWeeklyBlockEnd(start, weekMode);

            return CreateBlock(
                taskItemId,
                start,
                end,
                TaskInterval.Weekly,
                status,
                assignedByUserId,
                completedByUserId,
                note);
        }

        private static DemoBlock CreateMonthlyBlock(
            int taskItemId,
            DateTime dateInMonth,
            TaskBlockStatus status,
            int? assignedByUserId,
            int? completedByUserId,
            string? note)
        {
            var start = new DateTime(dateInMonth.Year, dateInMonth.Month, 1);
            var end = start.AddMonths(1);

            return CreateBlock(
                taskItemId,
                start,
                end,
                TaskInterval.Monthly,
                status,
                assignedByUserId,
                completedByUserId,
                note);
        }

        private static DemoBlock CreateBlock(
            int taskItemId,
            DateTime start,
            DateTime end,
            TaskInterval interval,
            TaskBlockStatus status,
            int? assignedByUserId,
            int? completedByUserId,
            string? note)
        {
            DateTime? assignedAt = assignedByUserId.HasValue
                ? start.AddHours(8)
                : null;

            DateTime? completedAt = completedByUserId.HasValue
                ? end.AddHours(-1)
                : null;

            return new DemoBlock
            {
                TaskItemId = taskItemId,
                Start = start,
                End = end,
                Interval = interval,
                Status = status,
                AssignedByUserId = assignedByUserId,
                AssignedAt = assignedAt,
                CompletedByUserId = completedByUserId,
                CompletedAt = completedAt,
                Note = note
            };
        }

        private static bool IsIncludedDailyDay(
            DateTime date,
            TaskWeekMode weekMode)
        {
            return weekMode switch
            {
                TaskWeekMode.Weekdays =>
                    date.DayOfWeek != DayOfWeek.Saturday &&
                    date.DayOfWeek != DayOfWeek.Sunday,

                TaskWeekMode.MondayToSaturday =>
                    date.DayOfWeek != DayOfWeek.Sunday,

                TaskWeekMode.FullWeek => true,

                _ => throw new ArgumentOutOfRangeException(
                    nameof(weekMode),
                    weekMode,
                    "Unbekannte Wochenendregel.")
            };
        }

        private static DateTime GetPreviousIncludedDailyDate(
            DateTime date,
            TaskWeekMode weekMode)
        {
            var candidate = date.Date;

            while (!IsIncludedDailyDay(candidate, weekMode))
            {
                candidate = candidate.AddDays(-1);
            }

            return candidate;
        }

        private static DateTime GetNextIncludedDailyDate(
            DateTime date,
            TaskWeekMode weekMode)
        {
            var candidate = date.Date;

            while (!IsIncludedDailyDay(candidate, weekMode))
            {
                candidate = candidate.AddDays(1);
            }

            return candidate;
        }

        private static void UpdateCompletedTasks(
            SqlConnection connection,
            SqlTransaction transaction,
            Dictionary<int, int> userIds)
        {
            const string sql = """
                UPDATE Users
                SET CompletedTasks =
                (
                    SELECT COUNT(1)
                    FROM TaskBlocks
                    WHERE CompletedByUserId = @UserId
                      AND Status = @CompletedStatus
                )
                WHERE UserId = @UserId;
                """;

            foreach (var userId in userIds.Values)
            {
                connection.Execute(
                    sql,
                    new
                    {
                        UserId = userId,
                        CompletedStatus = (int)TaskBlockStatus.Completed
                    },
                    transaction);
            }
        }

        private class DemoBlock
        {
            public int TaskItemId { get; set; }

            public DateTime Start { get; set; }

            public DateTime End { get; set; }

            public TaskInterval Interval { get; set; }

            public TaskBlockStatus Status { get; set; }

            public int? AssignedByUserId { get; set; }

            public DateTime? AssignedAt { get; set; }

            public int? CompletedByUserId { get; set; }

            public DateTime? CompletedAt { get; set; }

            public string? Note { get; set; }
        }
    }
}