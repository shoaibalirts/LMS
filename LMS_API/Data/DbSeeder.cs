using LMS_API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;

namespace LMS_API.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(ApplicationDbContext context)
    {
        // If an existing database was created outside EF migrations, applying
        // InitialCreate can fail because tables already exist.
        var migrationEntries = await context.Database
            .SqlQueryRaw<int>("SELECT CASE WHEN OBJECT_ID(N'__EFMigrationsHistory') IS NULL THEN 0 ELSE (SELECT COUNT(1) FROM [__EFMigrationsHistory]) END AS [Value]")
            .SingleAsync();

        var knownTables = await context.Database
            .SqlQueryRaw<int>("SELECT COUNT(1) AS [Value] FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME IN ('Assignments', 'Teacher', 'Students')")
            .SingleAsync();

        var shouldSkipMigrate = migrationEntries == 0 && knownTables > 0;

        try
        {
            if (!shouldSkipMigrate)
            {
                await context.Database.MigrateAsync();
            }
            else
            {
                await EnsureAssignedAssignmentTablesAsync(context);
            }
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("PendingModelChangesWarning"))
        {
            // Do not drop data on startup. If DB does not exist yet, create it.
            var canConnect = await context.Database.CanConnectAsync();
            if (!canConnect)
            {
                await context.Database.EnsureCreatedAsync();
            }
        }
        catch (SqlException ex) when (ex.Number == 2714)
        {
            // Existing table/object detected during migration. Keep startup alive
            // for already-provisioned development databases.
        }

        // Seed one default teacher only if it does not already exist.
        var teacherExists = await context.Teacher
            .AsNoTracking()
            .AnyAsync(t => t.Email.ToLower() == "teacher@school.com");

        if (!teacherExists)
        {
            var teacher = new Teacher
            {
                Email = "teacher@school.com",
                Password = "password123",
                FirstName = "John",
                LastName = "Doe",
                CreatedDate = DateTime.UtcNow,
            };

            context.Teacher.Add(teacher);
            await context.SaveChangesAsync();
        }
    }

    private static async Task EnsureAssignedAssignmentTablesAsync(ApplicationDbContext context)
    {
        const string sql = @"
IF OBJECT_ID(N'[AssignedAssignmentSets]', N'U') IS NULL
BEGIN
    CREATE TABLE [AssignedAssignmentSets] (
        [Id] INT NOT NULL IDENTITY,
        [DateOfAssigned] DATE NOT NULL,
        [Deadline] DATE NOT NULL,
        [TeacherId] INT NOT NULL,
        [StudentId] INT NOT NULL,
        CONSTRAINT [PK_AssignedAssignmentSets] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AssignedAssignmentSets_Teacher_TeacherId] FOREIGN KEY ([TeacherId]) REFERENCES [Teacher]([Id]),
        CONSTRAINT [FK_AssignedAssignmentSets_Students_StudentId] FOREIGN KEY ([StudentId]) REFERENCES [Students]([Id])
    );

    CREATE INDEX [IX_AssignedAssignmentSets_TeacherId] ON [AssignedAssignmentSets]([TeacherId]);
    CREATE INDEX [IX_AssignedAssignmentSets_StudentId] ON [AssignedAssignmentSets]([StudentId]);
END;

IF OBJECT_ID(N'[AssignedAssignments]', N'U') IS NULL
BEGIN
    CREATE TABLE [AssignedAssignments] (
        [Id] INT NOT NULL IDENTITY,
        [AssignedAssignmentSetId] INT NOT NULL,
        [AssignmentId] INT NOT NULL,
        [StudentResultFileName] NVARCHAR(MAX) NULL,
        [StudentResultContentType] NVARCHAR(MAX) NULL,
        [SubmittedAtUtc] DATETIME2 NULL,
        [Feedback] NVARCHAR(MAX) NULL,
        [StudentResultPath] NVARCHAR(MAX) NULL,
        CONSTRAINT [PK_AssignedAssignments] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AssignedAssignments_AssignedAssignmentSets_AssignedAssignmentSetId] FOREIGN KEY ([AssignedAssignmentSetId]) REFERENCES [AssignedAssignmentSets]([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_AssignedAssignments_Assignments_AssignmentId] FOREIGN KEY ([AssignmentId]) REFERENCES [Assignments]([Id])
    );

    CREATE INDEX [IX_AssignedAssignments_AssignedAssignmentSetId] ON [AssignedAssignments]([AssignedAssignmentSetId]);
    CREATE INDEX [IX_AssignedAssignments_AssignmentId] ON [AssignedAssignments]([AssignmentId]);
END;";

        await context.Database.ExecuteSqlRawAsync(sql);
    }
}
