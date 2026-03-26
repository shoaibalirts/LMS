using LMS_API.Models;
using Microsoft.EntityFrameworkCore;

namespace LMS_API.Data
{
    public class ApplicationDbContext(DbContextOptions options) : DbContext(options)
    {
        public DbSet<Teacher> Teacher { get; set; }
        public DbSet <Assignment> Assignments { get; set; }
        public DbSet<AssignmentSet> AssignmentSets { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
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
            modelBuilder.Entity<Assignment>().HasData(
                new Assignment
                {
                    Id = 1,
                    Points = 100,
                    Type = "Homework",
                    ClassLevel = "Grade 10",
                    Subject = "Mathematics",
                    CreatedDate = new DateTime(2026, 3, 25)
                }
            );

            modelBuilder.Entity<AssignmentSet>()
                        .HasOne(a => a.Teacher)
                        .WithMany(t => t.AssignmentSets)
                        .HasForeignKey(a => a.TeacherId);
                        
        }
    }
}
