using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using backend.Models.DTOs;
using backend.Services;

namespace backend.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TasksController : ControllerBase
{
    private readonly ITaskService _taskService;
    
    public TasksController(ITaskService taskService)
    {
        _taskService = taskService;
    }
    
    private int GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.Parse(userIdClaim ?? "0");
    }
    
    [HttpGet("project/{projectId}")]
    public async Task<IActionResult> GetProjectTasks(int projectId)
    {
        var userId = GetUserId();
        var tasks = await _taskService.GetProjectTasksAsync(projectId, userId);
        return Ok(tasks);
    }
    
    [HttpGet("{id}")]
    public async Task<IActionResult> GetTask(int id)
    {
        var userId = GetUserId();
        var task = await _taskService.GetTaskByIdAsync(id, userId);
        
        if (task == null)
        {
            return NotFound(new { message = "Task not found" });
        }
        
        return Ok(task);
    }
    
    [HttpPost]
    public async Task<IActionResult> CreateTask([FromBody] CreateTaskDto createDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        
        var userId = GetUserId();
        var task = await _taskService.CreateTaskAsync(createDto, userId);
        
        if (task == null)
        {
            return BadRequest(new { message = "Failed to create task. Project not found." });
        }
        
        return CreatedAtAction(nameof(GetTask), new { id = task.Id }, task);
    }
    
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateTask(int id, [FromBody] UpdateTaskDto updateDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        
        var userId = GetUserId();
        var task = await _taskService.UpdateTaskAsync(id, updateDto, userId);
        
        if (task == null)
        {
            return NotFound(new { message = "Task not found" });
        }
        
        return Ok(task);
    }
    
    [HttpPatch("{id}/toggle")]
    public async Task<IActionResult> ToggleTaskCompletion(int id)
    {
        var userId = GetUserId();
        var task = await _taskService.ToggleTaskCompletionAsync(id, userId);
        
        if (task == null)
        {
            return NotFound(new { message = "Task not found" });
        }
        
        return Ok(task);
    }
    
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTask(int id)
    {
        var userId = GetUserId();
        var result = await _taskService.DeleteTaskAsync(id, userId);
        
        if (!result)
        {
            return NotFound(new { message = "Task not found" });
        }
        
        return NoContent();
    }
}