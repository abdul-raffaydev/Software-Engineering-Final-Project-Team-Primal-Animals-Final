using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Software_Engineering_Final_Project_Team_Primal_Animals.Models;
using System.Collections.Generic;
namespace Software_Engineering_Final_Project_Team_Primal_Animals.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Patient> Patients { get; set; }
        public DbSet<PressureFrame> PressureFrames { get; set; }
        public DbSet<CommentThread> CommentThreads { get; set; }
    }
}


