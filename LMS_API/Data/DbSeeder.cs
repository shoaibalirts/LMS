using LMS_API.Models;
using Microsoft.EntityFrameworkCore;

namespace LMS_API.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(ApplicationDbContext context)
    {
        // Ensure database exists and apply migrations
        await context.Database.EnsureCreatedAsync();

        // Check if users already exist
        try
        {
            if (await context.Users.AnyAsync())
            {
                return;
            }
        }
        catch
        {
            // Table doesn't exist yet - will be created below
            return;
        }

        // Seed Teacher user
        var teacher = new User
        {
            Email = "teacher@school.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Teacher123!"),
            Role = "Teacher",
            CreatedAt = DateTime.UtcNow
        };

        // Seed Student user
        var student = new User
        {
            Email = "student@school.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Student123!"),
            Role = "Student",
            CreatedAt = DateTime.UtcNow
        };

        context.Users.AddRange(teacher, student);
        await context.SaveChangesAsync();
    }
}
