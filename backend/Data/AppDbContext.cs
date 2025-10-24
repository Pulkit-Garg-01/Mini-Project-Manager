using Microsoft.EntityFrameworkCore;
using backend.Models; // Your models namespace

namespace backend.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // DbSet properties for your models
        public DbSet<User> Users { get; set; }
        public DbSet<Project> Projects { get; set; }
        public DbSet<ProjectTask> ProjectTasks { get; set; } // Renamed to match your model

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // --- Relationships ---

            // User has many Projects
            modelBuilder.Entity<User>()
                .HasMany(u => u.Projects)
                .WithOne(p => p.User)
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade); // Delete user's projects if user is deleted

            // Project has many Tasks
            modelBuilder.Entity<Project>()
                .HasMany(p => p.Tasks)
                .WithOne(t => t.Project)
                .HasForeignKey(t => t.ProjectId)
                .OnDelete(DeleteBehavior.Cascade); // Delete project's tasks if project is deleted

            // --- Constraints ---

            // Ensure User Email is unique
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            // --- Table Naming (Optional) ---
            // If you want specific table names different from DbSet property names
            // modelBuilder.Entity<User>().ToTable("AppUsers");
            // modelBuilder.Entity<Project>().ToTable("Projects");
            // modelBuilder.Entity<ProjectTask>().ToTable("Tasks");
        }
    }
}