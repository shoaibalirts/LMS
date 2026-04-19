using LMS_API.Models;
using LMS_API.Services;
using Microsoft.EntityFrameworkCore;

namespace LMS_API.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(ApplicationDbContext context)
    {
        await context.Database.EnsureCreatedAsync();

        if (await context.Teacher.AnyAsync()) return;

        var hasher = new BCryptPasswordHasher();

        // --- Teacher ---
        var teacher = new Teacher
        {
            FirstName = "Morten",
            LastName = "Domsgard",
            Email = "morten.domsgard@ucl.dk",
            Password = hasher.Hash("1234567890"),
            CreatedDate = new DateTime(2026, 3, 13),
            UpdatedDate = new DateTime(2026, 3, 13)
        };
        context.Teacher.Add(teacher);
        await context.SaveChangesAsync();

        // --- Assignment + AssignmentSet ---
        var assignment = new Assignment
        {
            Points = 10,
            Type = "Delprøve 1",
            ClassLevel = "A",
            Subject = "Mathematics",
            PictureUrl = "https://example.com/assignment1.png",
            VideoUrl = "https://www.youtube.com/watch?v=dQw4w9WgXcQ",
            TeacherId = teacher.Id,
            CreatedDate = new DateTime(2026, 3, 25)
        };
        var assignmentSet = new AssignmentSet
        {
            Name = "Math Set 1",
            TeacherId = teacher.Id,
            CreatedDate = new DateTime(2026, 3, 25)
        };
        context.Assignments.Add(assignment);
        context.AssignmentSets.Add(assignmentSet);
        await context.SaveChangesAsync();

        // --- Assignment ↔ AssignmentSet (M:M) ---
        context.AssignmentAssignmentSets.Add(new AssignmentAssignmentSet
        {
            AssignmentId = assignment.Id,
            AssignmentSetId = assignmentSet.Id
        });

        // --- Students ---
        var shoaib = new Student
        {
            FirstName = "Shoaib",
            LastName = "Ali",
            Email = "shoaib.ali@student.ucl.dk",
            Password = hasher.Hash("password123"),
            CreatedByTeacherId = teacher.Id,
            CreatedDate = new DateTime(2026, 4, 7),
            UpdatedDate = new DateTime(2026, 4, 7)
        };
        var imran = new Student
        {
            FirstName = "Imran",
            LastName = "Khan",
            Email = "imran.khan@student.ucl.dk",
            Password = hasher.Hash("password123"),
            CreatedByTeacherId = teacher.Id,
            CreatedDate = new DateTime(2026, 4, 7),
            UpdatedDate = new DateTime(2026, 4, 7)
        };
        context.Students.AddRange(shoaib, imran);

        // --- StudyClasses ---
        var classA = new StudyClass
        {
            Name = "Class A",
            TeacherId = teacher.Id,
            CreatedDate = new DateTime(2026, 4, 10)
        };
        var classB = new StudyClass
        {
            Name = "Class B",
            TeacherId = teacher.Id,
            CreatedDate = new DateTime(2026, 4, 10)
        };
        context.StudyClasses.AddRange(classA, classB);
        await context.SaveChangesAsync();

        // --- Student ↔ StudyClass (M:M) ---
        context.StudentStudyClasses.AddRange(
            new StudentStudyClass { StudentId = shoaib.Id, StudyClassId = classA.Id },
            new StudentStudyClass { StudentId = shoaib.Id, StudyClassId = classB.Id },
            new StudentStudyClass { StudentId = imran.Id,  StudyClassId = classA.Id }
        );
        await context.SaveChangesAsync();
    }
}
