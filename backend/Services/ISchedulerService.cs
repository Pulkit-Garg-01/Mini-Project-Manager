using backend.Models.DTOs;

namespace backend.Services;

public interface ISchedulerService
{
    Task<ScheduleResponseDto?> GenerateScheduleAsync(
        int projectId, 
        ScheduleRequestDto request, 
        int userId
    );
}
