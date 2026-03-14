using LMS_API.Models;
using Microsoft.EntityFrameworkCore;

namespace LMS_API.Data
{
    public class ApplicationDbContext(DbContextOptions options) : DbContext(options)
    {
        public DbSet<Teacher> Teacher { get; set; }
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
        }
    }
}
