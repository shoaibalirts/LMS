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
        public DbSet<AssignedAssignmentSet> AssignedAssignmentSets { get; set; }
        public DbSet<AssignedAssignment> AssignedAssignments { get; set; }

        public DbSet<StudyClass> StudyClasses { get; set; }
        public DbSet<StudentStudyClass> StudentStudyClasses { get; set; }

        // MANY-TO-MANY relation between Assignment and AssignmentSet
        public DbSet<AssignmentAssignmentSet> AssignmentAssignmentSets { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // -------------------------
            // Teacher → Assignment (1:M)
            // -------------------------
            modelBuilder.Entity<Assignment>()
                .HasOne(a => a.Teacher)
                .WithMany(t => t.Assignments)
                .HasForeignKey(a => a.TeacherId)
                .OnDelete(DeleteBehavior.NoAction);

            // -------------------------
            // Teacher → AssignmentSet (1:M)
            // -------------------------
            modelBuilder.Entity<AssignmentSet>()
                .HasOne(a => a.Teacher)
                .WithMany(t => t.AssignmentSets)
                .HasForeignKey(a => a.TeacherId)
                .OnDelete(DeleteBehavior.NoAction);

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
            // Student ↔ StudyClass (M:M)
            // -------------------------
            modelBuilder.Entity<StudentStudyClass>()
                .HasKey(ssc => new { ssc.StudentId, ssc.StudyClassId });

            modelBuilder.Entity<StudentStudyClass>()
                .HasOne(ssc => ssc.Student)
                .WithMany(s => s.StudentStudyClasses)
                .HasForeignKey(ssc => ssc.StudentId);

            modelBuilder.Entity<StudentStudyClass>()
                .HasOne(ssc => ssc.StudyClass)
                .WithMany(sc => sc.StudentStudyClasses)
                .HasForeignKey(ssc => ssc.StudyClassId);

            modelBuilder.Entity<StudyClass>()
                .HasOne(sc => sc.Teacher)
                .WithMany(t => t.StudyClasses)
                .HasForeignKey(sc => sc.TeacherId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Student>()
                .HasOne(s => s.CreatedByTeacher)
                .WithMany(t => t.CreatedStudents)
                .HasForeignKey(s => s.CreatedByTeacherId)
                .OnDelete(DeleteBehavior.NoAction);

            // -------------------------
            // Teacher -> AssignedAssignmentSet (1:M)
            // Student -> AssignedAssignmentSet (1:M)
            // -------------------------
            modelBuilder.Entity<AssignedAssignmentSet>()
                .HasOne(x => x.Teacher)
                .WithMany(t => t.AssignedAssignmentSets)
                .HasForeignKey(x => x.TeacherId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<AssignedAssignmentSet>()
                .HasOne(x => x.Student)
                .WithMany(s => s.AssignedAssignmentSets)
                .HasForeignKey(x => x.StudentId)
                .OnDelete(DeleteBehavior.NoAction);

            // -------------------------
            // AssignedAssignmentSet -> AssignedAssignment (1:M)
            // Assignment -> AssignedAssignment (1:M)
            // -------------------------
            modelBuilder.Entity<AssignedAssignment>()
                .HasOne(x => x.AssignedAssignmentSet)
                .WithMany(x => x.AssignedAssignments)
                .HasForeignKey(x => x.AssignedAssignmentSetId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<AssignedAssignment>()
                .HasOne(x => x.Assignment)
                .WithMany(a => a.AssignedAssignments)
                .HasForeignKey(x => x.AssignmentId)
                .OnDelete(DeleteBehavior.NoAction);

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
                    TeacherId = 1,
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
            // -------------------------
            // Seed StudyClass
            // -------------------------
            modelBuilder.Entity<StudyClass>().HasData(
                new StudyClass
                {
                    Id = 1,
                    Name = "Class A",
                    TeacherId = 1,
                    CreatedDate = new DateTime(2026, 4, 10)
                },
                new StudyClass
                {
                    Id = 2,
                    Name = "Class B",
                    TeacherId = 1,
                    CreatedDate = new DateTime(2026, 4, 10)
                }
            );

            // -------------------------
            // Seed MANY-TO-MANY Student ↔ StudyClass
            // -------------------------
            modelBuilder.Entity<StudentStudyClass>().HasData(
                new StudentStudyClass
                {
                    StudentId = 1,
                    StudyClassId = 1
                },
                new StudentStudyClass
                {
                    StudentId = 1,
                    StudyClassId = 2 // same student in multiple classes ✅
                },
                new StudentStudyClass
                {
                    StudentId = 2,
                    StudyClassId = 1 // multiple students in same class ✅
                }
            );
        }
    }
}