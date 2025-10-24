using Microsoft.EntityFrameworkCore;
using backend.Data;
using backend.Models;
using backend.Models.DTOs;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace backend.Services
{
    public class TaskService : ITaskService
    {
        private readonly AppDbContext _context;
        private readonly IProjectService _projectService; // Inject ProjectService for ownership check

        public TaskService(AppDbContext context, IProjectService projectService)
        {
            _context = context;
            _projectService = projectService;
        }

        // Helper to map ProjectTask entity to TaskDto
        private static TaskDto MapTaskToDto(ProjectTask task)
        {
            return new TaskDto
            {
                Id = task.Id,
                Title = task.Title,
                DueDate = task.DueDate,
                IsCompleted = task.IsCompleted,
                CreatedAt = task.CreatedAt,
                ProjectId = task.ProjectId
            };
        }

        public async Task<IEnumerable<TaskDto>> GetProjectTasksAsync(int projectId, int userId)
        {
            // First, verify the user owns the project
            if (!await _projectService.UserOwnsProjectAsync(projectId, userId))
            {
                 // Return empty list or throw an exception - returning empty is safer
                return Enumerable.Empty<TaskDto>();
            }

            return await _context.ProjectTasks
                .Where(t => t.ProjectId == projectId)
                .OrderBy(t => t.CreatedAt) // Order by creation time
                .Select(t => MapTaskToDto(t)) // Use mapping function
                .ToListAsync();
        }

        public async Task<TaskDto?> GetTaskByIdAsync(int taskId, int userId)
        {
            var task = await _context.ProjectTasks
                .Include(t => t.Project) // Include project to check ownership
                .FirstOrDefaultAsync(t => t.Id == taskId);

            // Check if task exists AND if the user owns the parent project
            if (task == null || task.Project == null || task.Project.UserId != userId)
            {
                return null;
            }

            return MapTaskToDto(task);
        }

        public async Task<TaskDto?> CreateTaskAsync(CreateTaskDto createDto, int userId)
        {
            // Verify the user owns the target project
            if (!await _projectService.UserOwnsProjectAsync(createDto.ProjectId, userId))
            {
                return null; // User doesn't own the project
            }

            var task = new ProjectTask
            {
                Title = createDto.Title,
                DueDate = createDto.DueDate,
                IsCompleted = false, // Default to not completed
                ProjectId = createDto.ProjectId,
                CreatedAt = DateTime.UtcNow
            };

            _context.ProjectTasks.Add(task);
            await _context.SaveChangesAsync();

            return MapTaskToDto(task);
        }

        public async Task<TaskDto?> UpdateTaskAsync(int taskId, UpdateTaskDto updateDto, int userId)
        {
            var task = await _context.ProjectTasks
                .Include(t => t.Project) // Include project for ownership check
                .FirstOrDefaultAsync(t => t.Id == taskId);

            // Check ownership
            if (task == null || task.Project == null || task.Project.UserId != userId)
            {
                return null; // Task not found or user doesn't own it
            }

            // Update properties
            task.Title = updateDto.Title;
            task.DueDate = updateDto.DueDate;
            task.IsCompleted = updateDto.IsCompleted;

            _context.ProjectTasks.Update(task);
            await _context.SaveChangesAsync();

            return MapTaskToDto(task);
        }

         public async Task<TaskDto?> ToggleTaskCompletionAsync(int taskId, int userId)
        {
             var task = await _context.ProjectTasks
                .Include(t => t.Project) // Include project for ownership check
                .FirstOrDefaultAsync(t => t.Id == taskId);

            // Check ownership
            if (task == null || task.Project == null || task.Project.UserId != userId)
            {
                return null; // Task not found or user doesn't own it
            }

            // Toggle completion status
            task.IsCompleted = !task.IsCompleted;

            _context.ProjectTasks.Update(task);
            await _context.SaveChangesAsync();

            return MapTaskToDto(task);
        }


        public async Task<bool> DeleteTaskAsync(int taskId, int userId)
        {
            var task = await _context.ProjectTasks
                .Include(t => t.Project) // Include project for ownership check
                .FirstOrDefaultAsync(t => t.Id == taskId);

            // Check ownership
            if (task == null || task.Project == null || task.Project.UserId != userId)
            {
                return false; // Task not found or user doesn't own it
            }

            _context.ProjectTasks.Remove(task);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
