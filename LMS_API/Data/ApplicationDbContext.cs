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
        public DbSet<StudyClass> StudyClasses { get; set; }
        public DbSet<StudentStudyClass> StudentStudyClasses { get; set; }
        public DbSet<AssignmentAssignmentSet> AssignmentAssignmentSets { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Teacher → Assignment (1:M)
            modelBuilder.Entity<Assignment>()
                .HasOne(a => a.Teacher)
                .WithMany(t => t.Assignments)
                .HasForeignKey(a => a.TeacherId)
                .OnDelete(DeleteBehavior.NoAction);

            // Teacher → AssignmentSet (1:M)
            modelBuilder.Entity<AssignmentSet>()
                .HasOne(a => a.Teacher)
                .WithMany(t => t.AssignmentSets)
                .HasForeignKey(a => a.TeacherId)
                .OnDelete(DeleteBehavior.NoAction);

            // Assignment ↔ AssignmentSet (M:M)
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

            // Student ↔ StudyClass (M:M)
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

            // Teacher → StudyClass (1:M)
            modelBuilder.Entity<StudyClass>()
                .HasOne(sc => sc.Teacher)
                .WithMany(t => t.StudyClasses)
                .HasForeignKey(sc => sc.TeacherId)
                .OnDelete(DeleteBehavior.NoAction);

            // Teacher → Student (1:M, created-by)
            modelBuilder.Entity<Student>()
                .HasOne(s => s.CreatedByTeacher)
                .WithMany(t => t.CreatedStudents)
                .HasForeignKey(s => s.CreatedByTeacherId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
