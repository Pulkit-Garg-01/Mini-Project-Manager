using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using backend.Models.DTOs;
using backend.Services;

namespace backend.Controllers;

[ApiController]
[Route("api/projects/{projectId}/[controller]")]
[Authorize]
public class SchedulerController : ControllerBase
{
    private readonly ISchedulerService _schedulerService;
    
    public SchedulerController(ISchedulerService schedulerService)
    {
        _schedulerService = schedulerService;
    }
    
    private int GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.Parse(userIdClaim ?? "0");
    }
    
    [HttpPost("schedule")]
    public async Task<IActionResult> GenerateSchedule(
        int projectId,
        [FromBody] ScheduleRequestDto request
    )
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        if (request.StartDate >= request.EndDate)
        {
            return BadRequest(new { message = "End date must be after start date" });
        }

        var userId = GetUserId();
        var schedule = await _schedulerService.GenerateScheduleAsync(projectId, request, userId);
        
        if (schedule == null)
        {
            return NotFound(new { message = "Project not found" });
        }
        
        return Ok(schedule);
    }
}
