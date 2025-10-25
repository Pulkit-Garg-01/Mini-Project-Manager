using Microsoft.EntityFrameworkCore;
using backend.Data;
using backend.Models.DTOs;

namespace backend.Services;

public class SchedulerService : ISchedulerService
{
    private readonly AppDbContext _context;

    public SchedulerService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<ScheduleResponseDto?> GenerateScheduleAsync(
        int projectId, 
        ScheduleRequestDto request, 
        int userId
    )
    {
        // 1. Verify project exists and belongs to user
        var project = await _context.Projects
            .Include(p => p.Tasks)
            .FirstOrDefaultAsync(p => p.Id == projectId && p.UserId == userId);

        if (project == null)
        {
            return null;
        }

        // 2. Get incomplete tasks
        var incompleteTasks = project.Tasks
            .Where(t => !t.IsCompleted)
            .ToList();

        if (incompleteTasks.Count == 0)
        {
            return new ScheduleResponseDto
            {
                ProjectId = projectId,
                ProjectTitle = project.Title,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                TotalTasks = 0,
                ScheduledTasks = 0,
                Schedule = new(),
                Warnings = new() { "No incomplete tasks to schedule" }
            };
        }

        // 3. Calculate available work days
        var workDays = GetWorkDays(request.StartDate, request.EndDate);
        var totalAvailableHours = workDays.Count * request.DailyWorkHours;

        // 4. Estimate hours per task (simple algorithm)
        var estimatedHoursPerTask = Math.Max(1, totalAvailableHours / incompleteTasks.Count);

        // 5. Sort tasks by priority
        var sortedTasks = SortTasksByPriority(
            incompleteTasks, 
            request.PriorityTaskIds
        );

        // 6. Generate schedule
        var schedule = new List<ScheduledTaskDto>();
        var warnings = new List<string>();
        var currentDay = 0;
        var currentDayHours = 0;

        foreach (var task in sortedTasks)
        {
            if (currentDay >= workDays.Count)
            {
                warnings.Add($"Not enough time to schedule all tasks. {sortedTasks.Count - schedule.Count} tasks remain unscheduled.");
                break;
            }

            var taskHours = Math.Min(estimatedHoursPerTask, request.DailyWorkHours);

            // Check if task fits in current day
            if (currentDayHours + taskHours > request.DailyWorkHours)
            {
                currentDay++;
                currentDayHours = 0;

                if (currentDay >= workDays.Count)
                {
                    warnings.Add($"Not enough time to schedule all tasks.");
                    break;
                }
            }

            var priority = GetTaskPriority(task.Id, request.PriorityTaskIds);
            var reason = GetScheduleReason(task, priority, task.DueDate);

            schedule.Add(new ScheduledTaskDto
            {
                TaskId = task.Id,
                Title = task.Title,
                ScheduledDate = workDays[currentDay],
                EstimatedHours = taskHours,
                Priority = priority,
                Reason = reason
            });

            currentDayHours += taskHours;
        }

        // 7. Check for tasks with due dates
        var overdueTasks = schedule.Where(s => 
        {
            var task = incompleteTasks.FirstOrDefault(t => t.Id == s.TaskId);
            return task?.DueDate != null && s.ScheduledDate > task.DueDate.Value;
        }).ToList();

        if (overdueTasks.Any())
        {
            warnings.Add($"{overdueTasks.Count} task(s) scheduled after their due date.");
        }

        return new ScheduleResponseDto
        {
            ProjectId = projectId,
            ProjectTitle = project.Title,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            TotalTasks = incompleteTasks.Count,
            ScheduledTasks = schedule.Count,
            Schedule = schedule,
            Warnings = warnings
        };
    }

    private List<DateTime> GetWorkDays(DateTime start, DateTime end)
    {
        var workDays = new List<DateTime>();
        var current = start.Date;

        while (current <= end.Date)
        {
            // Exclude weekends (optional - remove this check to include weekends)
            if (current.DayOfWeek != DayOfWeek.Saturday && 
                current.DayOfWeek != DayOfWeek.Sunday)
            {
                workDays.Add(current);
            }
            current = current.AddDays(1);
        }

        return workDays;
    }

    private List<Models.ProjectTask> SortTasksByPriority(
        List<Models.ProjectTask> tasks, 
        List<int>? priorityTaskIds
    )
    {
        return tasks
            .OrderByDescending(t => priorityTaskIds?.Contains(t.Id) ?? false)
            .ThenBy(t => t.DueDate ?? DateTime.MaxValue)
            .ThenBy(t => t.CreatedAt)
            .ToList();
    }

    private string GetTaskPriority(int taskId, List<int>? priorityTaskIds)
    {
        if (priorityTaskIds?.Contains(taskId) ?? false)
            return "High";
        return "Medium";
    }

    private string GetScheduleReason(Models.ProjectTask task, string priority, DateTime? dueDate)
    {
        if (priority == "High")
            return "User marked as priority";
        
        if (dueDate != null)
            return $"Due date: {dueDate.Value:yyyy-MM-dd}";
        
        return "Scheduled based on creation order";
    }
}
