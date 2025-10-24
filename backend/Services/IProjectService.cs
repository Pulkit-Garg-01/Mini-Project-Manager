using backend.Models.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace backend.Services
{
    public interface IProjectService
    {
        Task<IEnumerable<ProjectDto>> GetUserProjectsAsync(int userId);
        Task<ProjectDetailDto?> GetProjectByIdAsync(int projectId, int userId);
        Task<ProjectDto?> CreateProjectAsync(CreateProjectDto createDto, int userId);
        Task<ProjectDto?> UpdateProjectAsync(int projectId, UpdateProjectDto updateDto, int userId);
        Task<bool> DeleteProjectAsync(int projectId, int userId);
        Task<bool> UserOwnsProjectAsync(int projectId, int userId); // Helper for checking ownership
    }
}
