using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Software_Engineering_Final_Project_Team_Primal_Animals.Models;

namespace Software_Engineering_Final_Project_Team_Primal_Animals.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // YOUR TABLES
        public DbSet<Patient> Patients { get; set; }
        public DbSet<PressureFrame> PressureFrames { get; set; }
        public DbSet<CommentThread> CommentThreads { get; set; }

        // ❌ REMOVE this — Identity automatically provides Users table
        // public DbSet<ApplicationUser> Users { get; set; }
    }
}
