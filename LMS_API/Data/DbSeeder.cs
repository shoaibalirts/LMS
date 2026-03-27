using LMS_API.Models;
using Microsoft.EntityFrameworkCore;

namespace LMS_API.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(ApplicationDbContext context)
    {
        // Ensure database exists and apply migrations
        await context.Database.EnsureCreatedAsync();

    
        // Seed Teacher user
        var teacher = new Teacher
        {
            Email = "teacher@school.com",
            Password = "password123",
            FirstName = "John",
            LastName = "Doe",
            CreatedDate = DateTime.UtcNow,
        };

        context.Teacher.AddRange(teacher);
        await context.SaveChangesAsync();
    }
}
