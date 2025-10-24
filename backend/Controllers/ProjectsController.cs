using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using backend.Models.DTOs;
using backend.Services;

namespace backend.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProjectsController : ControllerBase
{
    private readonly IProjectService _projectService;
    
    public ProjectsController(IProjectService projectService)
    {
        _projectService = projectService;
    }
    
    private int GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.Parse(userIdClaim ?? "0");
    }
    
    [HttpGet]
    public async Task<IActionResult> GetProjects()
    {
        var userId = GetUserId();
        var projects = await _projectService.GetUserProjectsAsync(userId);
        return Ok(projects);
    }
    
    [HttpGet("{id}")]
    public async Task<IActionResult> GetProject(int id)
    {
        var userId = GetUserId();
        var project = await _projectService.GetProjectByIdAsync(id, userId);
        
        if (project == null)
        {
            return NotFound(new { message = "Project not found" });
        }
        
        return Ok(project);
    }
    
    [HttpPost]
    public async Task<IActionResult> CreateProject([FromBody] CreateProjectDto createDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        
        var userId = GetUserId();
        var project = await _projectService.CreateProjectAsync(createDto, userId);
        
        if (project == null)
        {
            return BadRequest(new { message = "Failed to create project" });
        }
        
        return CreatedAtAction(nameof(GetProject), new { id = project.Id }, project);
    }
    
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateProject(int id, [FromBody] UpdateProjectDto updateDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        
        var userId = GetUserId();
        var project = await _projectService.UpdateProjectAsync(id, updateDto, userId);
        
        if (project == null)
        {
            return NotFound(new { message = "Project not found" });
        }
        
        return Ok(project);
    }
    
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteProject(int id)
    {
        var userId = GetUserId();
        var result = await _projectService.DeleteProjectAsync(id, userId);
        
        if (!result)
        {
            return NotFound(new { message = "Project not found" });
        }
        
        return NoContent();
    }
}
