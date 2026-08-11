using System.Globalization;
using Schoofs_ATaskO.Models;

namespace Schoofs_ATaskO.Helpers
{
    public static class TaskBlockDateHelper
    {
        public static DateTime StartOfWeek(DateTime date)
        {
            date = date.Date;

            var dayOfWeek = date.DayOfWeek;

            var daysSinceMonday = dayOfWeek == DayOfWeek.Sunday ? 6 : (int)dayOfWeek - 1;

            var monday = date.AddDays(-daysSinceMonday);

            return monday;
        }

        public static DateTime GetPeriodStart(TaskInterval interval, DateTime focusDate)
        {
            var date = focusDate.Date;

            DateTime start;

            switch (interval)
            {
                case TaskInterval.Daily:
                    start = StartOfWeek(date);
                    break;

                case TaskInterval.Weekly:
                    start = new DateTime(date.Year, date.Month, 1);
                    break;

                case TaskInterval.Monthly:
                    start = new DateTime(date.Year, 1, 1);
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(interval), interval, "Unbekanntes Aufgabenintervall.");
            }

            return start;
        }

        public static DateTime GetNextPeriodStart(TaskInterval interval, DateTime periodStart)
        {
            DateTime next;

            switch (interval)
            {
                case TaskInterval.Daily:
                    next = periodStart.AddDays(7);
                    break;

                case TaskInterval.Weekly:
                    next = periodStart.AddMonths(1);
                    break;

                case TaskInterval.Monthly:
                    next = periodStart.AddYears(1);
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(interval), interval, "Unbekanntes Aufgabenintervall.");
            }

            return next;
        }

        public static DateTime GetPreviousFocusDate(TaskInterval interval, DateTime currentPeriodStart)
        {
            DateTime previous;

            switch (interval)
            {
                case TaskInterval.Daily:
                    previous = currentPeriodStart.AddDays(-7);
                    break;

                case TaskInterval.Weekly:
                    previous = currentPeriodStart.AddMonths(-1);
                    break;

                case TaskInterval.Monthly:
                    previous = currentPeriodStart.AddYears(-1);
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(interval), interval, "Unbekanntes Aufgabenintervall.");
            }

            return previous;
        }

        public static DateTime GetNextFocusDate(TaskInterval interval, DateTime currentPeriodStart)
        {
            DateTime next;

            switch (interval)
            {
                case TaskInterval.Daily:
                    next = currentPeriodStart.AddDays(7);
                    break;

                case TaskInterval.Weekly:
                    next = currentPeriodStart.AddMonths(1);
                    break;

                case TaskInterval.Monthly:
                    next = currentPeriodStart.AddYears(1);
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(interval), interval, "Unbekanntes Aufgabenintervall.");
            }

            return next;
        }

        public static DateTime GetWeeklyBlockEnd(DateTime monday, TaskWeekMode weekMode)
        {
            DateTime end;
            
            switch (weekMode)
            {
                case TaskWeekMode.Weekdays:
                    end = monday.Date.AddDays(5);
                    break;

                case TaskWeekMode.MondayToSaturday:
                    end = monday.Date.AddDays(6);
                    break;

                case TaskWeekMode.FullWeek:
                    end = monday.Date.AddDays(7);
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(weekMode), weekMode, "Unbekannte Wochenendregel.");
            }

            return end;
        }

        public static string GetPeriodLabel(TaskInterval interval, DateTime start, DateTime end)
        {
            var culture = CultureInfo.GetCultureInfo("de-DE");

            string label;

            switch (interval)
            {
                case TaskInterval.Daily:
                    label = $"{start:dd.MM.yyyy} - {end.AddDays(-1):dd.MM.yyyy}";
                    break;

                case TaskInterval.Weekly:
                    label = start.ToString("MMMM yyyy", culture);
                    break;

                case TaskInterval.Monthly:
                    label = start.ToString("yyyy", culture);
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(interval), interval, "Unbekanntes Aufgabenintervall.");
            }

            return label;
        }
        public static string GetBlockDisplayName(TaskInterval interval, TaskWeekMode weekMode, DateTime start, DateTime end)
        {
            var culture = CultureInfo.GetCultureInfo("de-DE");

            string displayName;

            switch (interval)
            {
                case TaskInterval.Daily:
                    displayName = start.ToString("dddd dd.MM.", culture);
                    break;

                case TaskInterval.Weekly:
                    var lastDay = end.AddDays(-1);

                    displayName = $"{start.ToString("dddd dd.MM.", culture)} - {lastDay.ToString("dddd dd.MM.", culture)}";
                    break;

                case TaskInterval.Monthly:
                    displayName = start.ToString("MMMM", culture);
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(interval), interval, "Unbekanntes Aufgabenintervall.");
            }

            return displayName;
        }
    }
}