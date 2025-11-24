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

        // ✅ Only the REAL tables your system uses
        public DbSet<Patient> Patients { get; set; }
        public DbSet<SensorData> SensorData { get; set; }
        public DbSet<CommentThread> CommentThreads { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // ✅ Primary Keys
            builder.Entity<Patient>().HasKey(p => p.Patient_ID);
            builder.Entity<SensorData>().HasKey(s => s.Data_Id);
            builder.Entity<CommentThread>().HasKey(c => c.Comment_ID);

            // ✅ Patient ↔ IdentityUser (1-to-1)
            builder.Entity<Patient>()
                .HasOne(p => p.AppUser)
                .WithOne()
                .HasForeignKey<Patient>(p => p.AppUserId)
                .OnDelete(DeleteBehavior.Cascade);

            // ✅ Patient → SensorData (1-to-many)
            builder.Entity<SensorData>()
                .HasOne(s => s.Patient)
                .WithMany(p => p.SensorData)
                .HasForeignKey(s => s.Patient_ID);

            // ✅ Patient → Comments (1-to-many)
            builder.Entity<CommentThread>()
                .HasOne(c => c.Patient)
                .WithMany(p => p.Comments)
                .HasForeignKey(c => c.Patient_ID);
        }
    }
}
