using System.ComponentModel.DataAnnotations;

namespace backend.Models.DTOs;

public class ScheduleRequestDto
{
    [Required]
    public DateTime StartDate { get; set; }
    
    [Required]
    public DateTime EndDate { get; set; }
    
    [Range(1, 24)]
    public int DailyWorkHours { get; set; } = 8;
    
    public List<int>? PriorityTaskIds { get; set; }
}

public class ScheduledTaskDto
{
    public int TaskId { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateTime ScheduledDate { get; set; }
    public int EstimatedHours { get; set; }
    public string Priority { get; set; } = "Medium";
    public string Reason { get; set; } = string.Empty;
}

public class ScheduleResponseDto
{
    public int ProjectId { get; set; }
    public string ProjectTitle { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int TotalTasks { get; set; }
    public int ScheduledTasks { get; set; }
    public List<ScheduledTaskDto> Schedule { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
}
