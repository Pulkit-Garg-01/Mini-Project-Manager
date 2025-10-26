using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models; // For Swagger
using System.Text;
using backend.Data;      // Your Data namespace
using backend.Services; // Your Services namespace

var builder = WebApplication.CreateBuilder(args);

// --- 1. Configure Services ---

// Add DbContext
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(connectionString)); // Use SQLite based on appsettings

// Add custom application services (Dependency Injection)
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IProjectService, ProjectService>();
builder.Services.AddScoped<ITaskService, TaskService>();

// Add services for controllers and API endpoints
builder.Services.AddControllers();

// Configure JWT Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"];

if (string.IsNullOrEmpty(secretKey))
{
    // Throw an exception during startup if the key is missing
    throw new InvalidOperationException("JWT SecretKey configuration (JwtSettings:SecretKey) is missing or empty.");
}
if (secretKey.Length < 32)
{
     Console.Error.WriteLine("Warning: JWT SecretKey is shorter than 32 characters, which might be insecure.");
}


builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true, // Validate the server that generates the token
        ValidateAudience = true, // Validate the recipient of the token is authorized to receive
        ValidateLifetime = true, // Check if the token is not expired and the signing key is valid
        ValidateIssuerSigningKey = true, // Validate the signature of the token

        ValidIssuer = jwtSettings["Issuer"],       // Read from appsettings
        ValidAudience = jwtSettings["Audience"],   // Read from appsettings
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
    };
});

// Configure Authorization (optional, if you have specific policies)
builder.Services.AddAuthorization();

// Configure CORS (Cross-Origin Resource Sharing) to allow frontend access
// IMPORTANT: For production, be more restrictive than AllowAnyOrigin()
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", // Give the policy a name
        policy =>
        {
            policy.AllowAnyOrigin() // Replace with your React app's actual origin
                  .AllowAnyHeader()
                  .AllowAnyMethod();
            // In production, consider .WithOrigins("YOUR_FRONTEND_URL")
            // For development using wildcard origins like AllowAnyOrigin() might be easier initially
            // policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
        });
});


// Add Swagger/OpenAPI for API documentation and testing (optional but recommended)
// Add this with other service registrations
builder.Services.AddScoped<ISchedulerService, SchedulerService>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Mini Project Manager API", Version = "v1" });

    // Configure Swagger to use JWT Authentication
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = @"JWT Authorization header using the Bearer scheme. <br/>
                      Enter 'Bearer' [space] and then your token in the text input below.<br/>
                      Example: 'Bearer 12345abcdef'",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement()
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                },
                Scheme = "oauth2",
                Name = "Bearer",
                In = ParameterLocation.Header,
            },
            new List<string>()
        }
    });
});


// --- 2. Build the Application ---
var app = builder.Build();

// --- 3. Configure the HTTP Request Pipeline (Middleware) ---

// Configure Swagger in Development environment
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Mini Project Manager API v1"));
    // Automatically create/update the database on startup in development
    // Be cautious using this in production - prefer explicit migrations
    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        // dbContext.Database.EnsureCreated(); // Creates DB if not exists, doesn't handle migrations
         dbContext.Database.Migrate(); // Applies pending migrations
    }
}

// Redirect HTTP requests to HTTPS
app.UseHttpsRedirection();

// Enable CORS using the policy defined above
app.UseCors("AllowAll"); // Use the specific policy name
// app.UseCors("AllowFrontend");
// Enable Authentication middleware (must come before Authorization)
app.UseAuthentication();

// Enable Authorization middleware
app.UseAuthorization();

// Map controller endpoints
app.MapControllers();
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

// --- 4. Run the Application ---
app.Run();
