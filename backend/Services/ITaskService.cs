using backend.Models.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace backend.Services
{
    public interface ITaskService
    {
        Task<IEnumerable<TaskDto>> GetProjectTasksAsync(int projectId, int userId);
        Task<TaskDto?> GetTaskByIdAsync(int taskId, int userId);
        Task<TaskDto?> CreateTaskAsync(CreateTaskDto createDto, int userId);
        Task<TaskDto?> UpdateTaskAsync(int taskId, UpdateTaskDto updateDto, int userId);
        Task<TaskDto?> ToggleTaskCompletionAsync(int taskId, int userId);
        Task<bool> DeleteTaskAsync(int taskId, int userId);
    }
}
