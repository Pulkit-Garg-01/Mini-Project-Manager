using Microsoft.EntityFrameworkCore;
using backend.Data;
using backend.Models;
using backend.Models.DTOs;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace backend.Services
{
    public class ProjectService : IProjectService
    {
        private readonly AppDbContext _context;

        public ProjectService(AppDbContext context)
        {
            _context = context;
        }

        // Helper to check ownership cleanly
        public async Task<bool> UserOwnsProjectAsync(int projectId, int userId)
        {
            var project = await _context.Projects.FindAsync(projectId);
            return project != null && project.UserId == userId;
        }

        public async Task<IEnumerable<ProjectDto>> GetUserProjectsAsync(int userId)
        {
            return await _context.Projects
                .Where(p => p.UserId == userId)
                .OrderByDescending(p => p.CreatedAt) // Show newest first
                .Select(p => new ProjectDto // Map to the summary DTO
                {
                    Id = p.Id,
                    Title = p.Title,
                    Description = p.Description,
                    CreatedAt = p.CreatedAt,
                    // Efficiently count tasks using EF Core features
                    TaskCount = p.Tasks.Count(),
                    CompletedTaskCount = p.Tasks.Count(t => t.IsCompleted)
                })
                .ToListAsync();
        }

        public async Task<ProjectDetailDto?> GetProjectByIdAsync(int projectId, int userId)
        {
            var project = await _context.Projects
                .Include(p => p.Tasks) // Eager load tasks for the detail view
                .Where(p => p.Id == projectId && p.UserId == userId)
                .Select(p => new ProjectDetailDto // Map to the detailed DTO
                {
                    Id = p.Id,
                    Title = p.Title,
                    Description = p.Description,
                    CreatedAt = p.CreatedAt,
                    Tasks = p.Tasks.OrderBy(t => t.CreatedAt).Select(t => new TaskDto // Map tasks as well
                    {
                        Id = t.Id,
                        Title = t.Title,
                        DueDate = t.DueDate,
                        IsCompleted = t.IsCompleted,
                        CreatedAt = t.CreatedAt,
                        ProjectId = t.ProjectId
                    }).ToList()
                })
                .FirstOrDefaultAsync();

            return project;
        }

        public async Task<ProjectDto?> CreateProjectAsync(CreateProjectDto createDto, int userId)
        {
            var project = new Project
            {
                Title = createDto.Title,
                Description = createDto.Description,
                UserId = userId,
                CreatedAt = DateTime.UtcNow
            };

            _context.Projects.Add(project);
            await _context.SaveChangesAsync();

            // Return the summary DTO for the newly created project
            return new ProjectDto
            {
                Id = project.Id,
                Title = project.Title,
                Description = project.Description,
                CreatedAt = project.CreatedAt,
                TaskCount = 0, // Starts with 0 tasks
                CompletedTaskCount = 0
            };
        }

        public async Task<ProjectDto?> UpdateProjectAsync(int projectId, UpdateProjectDto updateDto, int userId)
        {
            var project = await _context.Projects
                                .Include(p => p.Tasks) // Include tasks to calculate counts
                                .FirstOrDefaultAsync(p => p.Id == projectId && p.UserId == userId);

            if (project == null)
            {
                return null; // Not found or not owned by user
            }

            // Update properties
            project.Title = updateDto.Title;
            project.Description = updateDto.Description;
            // Note: CreatedAt and UserId should generally not be updated

            _context.Projects.Update(project);
            await _context.SaveChangesAsync();

            // Return the updated summary DTO
            return new ProjectDto
            {
                Id = project.Id,
                Title = project.Title,
                Description = project.Description,
                CreatedAt = project.CreatedAt,
                TaskCount = project.Tasks.Count(),
                CompletedTaskCount = project.Tasks.Count(t => t.IsCompleted)
            };
        }

        public async Task<bool> DeleteProjectAsync(int projectId, int userId)
        {
            var project = await _context.Projects
                                .FirstOrDefaultAsync(p => p.Id == projectId && p.UserId == userId);

            if (project == null)
            {
                return false; // Not found or not owned by user
            }

            _context.Projects.Remove(project); // Cascade delete will handle tasks
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
