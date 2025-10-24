using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using backend.Data;          // Your Data namespace
using backend.Models;       // Your Models namespace
using backend.Models.DTOs;  // Your DTOs namespace
// Ensure you have the using directive for the library's namespace
using BCryptNet = BCrypt.Net; // Use an alias to avoid potential conflicts if needed, or just use BCrypt.Net directly

namespace backend.Services
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration; // To read JWT settings from appsettings

        public AuthService(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public async Task<AuthResponseDto?> RegisterAsync(RegisterDto registerDto)
        {
            // 1. Check if email already exists
            if (await _context.Users.AnyAsync(u => u.Email == registerDto.Email))
            {
                return null; // Email already taken
            }

            // 2. Hash the password using the full class path
            string passwordHash = BCryptNet.BCrypt.HashPassword(registerDto.Password); // Corrected Call

            // 3. Create the new user
            var user = new User
            {
                Email = registerDto.Email,
                PasswordHash = passwordHash,
                CreatedAt = DateTime.UtcNow // Set creation time
            };

            // 4. Add to database and save
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // 5. Generate JWT token for immediate login (optional but common)
            var token = GenerateJwtToken(user);

            // 6. Return response DTO
            return new AuthResponseDto
            {
                Token = token,
                Email = user.Email,
                UserId = user.Id
            };
        }

        public async Task<AuthResponseDto?> LoginAsync(LoginDto loginDto)
        {
            // 1. Find user by email
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == loginDto.Email);

            // 2. Check if user exists and password is correct using the full class path
            if (user == null || !BCryptNet.BCrypt.Verify(loginDto.Password, user.PasswordHash)) // Corrected Call
            {
                return null; // Invalid credentials
            }

            // 3. Generate JWT token
            var token = GenerateJwtToken(user);

            // 4. Return response DTO
            return new AuthResponseDto
            {
                Token = token,
                Email = user.Email,
                UserId = user.Id
            };
        }

        // --- Helper method to generate JWT ---
        private string GenerateJwtToken(User user)
        {
            // --- UPDATED CONFIGURATION KEYS ---
            var jwtKey = _configuration["JwtSettings:SecretKey"];
            var jwtIssuer = _configuration["JwtSettings:Issuer"];
            var jwtAudience = _configuration["JwtSettings:Audience"];
            var expiryMinutesStr = _configuration["JwtSettings:ExpiryMinutes"];
            // --- END UPDATED CONFIGURATION KEYS ---


            if (string.IsNullOrEmpty(jwtKey))
            {
                // Log this error properly in a real application
                Console.Error.WriteLine("JWT SecretKey is missing or empty in configuration (JwtSettings:SecretKey).");
                throw new InvalidOperationException("JWT SecretKey configuration is missing.");
            }
             if (jwtKey.Length < 32) // Basic check for minimum length often recommended for HS256
            {
                 Console.Error.WriteLine("Warning: JWT SecretKey is shorter than 32 characters, which might be insecure.");
            }
             if (!int.TryParse(expiryMinutesStr, out int expiryMinutes))
             {
                 expiryMinutes = 60; // Default to 60 minutes if config is missing or invalid
                 Console.Error.WriteLine($"Warning: JwtSettings:ExpiryMinutes not found or invalid. Defaulting to {expiryMinutes} minutes.");
             }


            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            // Claims identify the user
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()), // Standard claim for user ID (Subject)
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()), // Explicit NameIdentifier often used by authorization
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(ClaimTypes.Email, user.Email), // Explicit Email claim
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()) // Unique token identifier
                // Add other claims like roles if needed: new Claim(ClaimTypes.Role, "Admin")
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(expiryMinutes), // Use expiry from config
                Issuer = jwtIssuer,       // Read from config
                Audience = jwtAudience,   // Read from config
                SigningCredentials = credentials
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }
    }
}

