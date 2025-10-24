using backend.Models.DTOs; // Your DTOs namespace

namespace backend.Services
{
    public interface IAuthService
    {
        Task<AuthResponseDto?> RegisterAsync(RegisterDto registerDto);
        Task<AuthResponseDto?> LoginAsync(LoginDto loginDto);
        // Removed GenerateJwtToken from interface as it's an internal detail
    }
}
