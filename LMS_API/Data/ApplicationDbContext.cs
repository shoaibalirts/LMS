using LMS_API.Models;
using Microsoft.EntityFrameworkCore;

namespace LMS_API.Data
{
    public class ApplicationDbContext(DbContextOptions options) : DbContext(options)
    {
        public DbSet<Teacher> Teacher { get; set; }
        public DbSet<Assignment> Assignments { get; set; }
        public DbSet<AssignmentSet> AssignmentSets { get; set; }
        public DbSet<Student> Students { get; set; }

        // MANY-TO-MANY relation between Assignment and AssignmentSet
        public DbSet<AssignmentAssignmentSet> AssignmentAssignmentSets { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // -------------------------
            // Teacher → AssignmentSet (1:M)
            // -------------------------
            modelBuilder.Entity<AssignmentSet>()
                .HasOne(a => a.Teacher)
                .WithMany(t => t.AssignmentSets)
                .HasForeignKey(a => a.TeacherId);

            // -------------------------
            // Assignment ↔ AssignmentSet (M:M)
            // -------------------------
            modelBuilder.Entity<AssignmentAssignmentSet>()
                .HasKey(x => new { x.AssignmentId, x.AssignmentSetId });

            modelBuilder.Entity<AssignmentAssignmentSet>()
                .HasOne(x => x.Assignment)
                .WithMany(a => a.AssignmentAssignmentSets)
                .HasForeignKey(x => x.AssignmentId);

            modelBuilder.Entity<AssignmentAssignmentSet>()
                .HasOne(x => x.AssignmentSet)
                .WithMany(s => s.AssignmentAssignmentSets)
                .HasForeignKey(x => x.AssignmentSetId);

            // -------------------------
            // Seed Teacher
            // -------------------------
            modelBuilder.Entity<Teacher>().HasData(
                new Teacher
                {
                    Id = 1,
                    FirstName = "Morten",
                    LastName = "Domsgard",
                    Email = "morten.domsgard@ucl.dk",
                    Password = "1234567890",
                    CreatedDate = new DateTime(2026, 3, 13),
                    UpdatedDate = new DateTime(2026, 3, 13)
                }
            );

            // -------------------------
            // Seed Assignment
            // -------------------------
            modelBuilder.Entity<Assignment>().HasData(
                new Assignment
                {
                    Id = 1,
                    Points = 10,
                    Type = "Delprøve 1",
                    ClassLevel = "A",
                    Subject = "Mathematics",
                    PictureUrl = "https://example.com/assignment1.png",
                    VideoUrl = "https://www.youtube.com/watch?v=dQw4w9WgXcQ",
                    CreatedDate = new DateTime(2026, 3, 25)
                }
            );

            // -------------------------
            // Seed AssignmentSet
            // -------------------------
            modelBuilder.Entity<AssignmentSet>().HasData(
                new AssignmentSet
                {
                    Id = 1,
                    Name = "Math Set 1",
                    TeacherId = 1,
                    CreatedDate = new DateTime(2026, 3, 25)
                }
            );

            // -------------------------
            // Seed MANY-TO-MANY relation
            // -------------------------
            modelBuilder.Entity<AssignmentAssignmentSet>().HasData(
                new AssignmentAssignmentSet
                {
                    AssignmentId = 1,
                    AssignmentSetId = 1
                }
            );

            modelBuilder.Entity<Student>().HasData(
                new Student
                {
                    Id = 1,
                    FirstName = "Shoaib",
                    LastName = "Ali",
                    Email = "shoaib.ali@student.ucl.dk",
                    Password = "hashed_password",
                    CreatedDate = new DateTime(2026, 4, 7),
                    UpdatedDate = new DateTime(2026, 4, 7)
                },
                new Student
                {
                    Id = 2,
                    FirstName = "Imran",
                    LastName = "Khan",
                    Email = "imran.khan@student.ucl.dk",
                    Password = "hashed_password",
                    CreatedDate = new DateTime(2026, 4, 7),
                    UpdatedDate = new DateTime(2026, 4, 7)
                }
            );
        }
    }
}