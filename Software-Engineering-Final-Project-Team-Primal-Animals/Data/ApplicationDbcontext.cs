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

        // ==============================
        // DOMAIN TABLES
        // ==============================
        public DbSet<Patient> Patients { get; set; }
        public DbSet<SensorData> SensorData { get; set; }
        public DbSet<CommentThread> CommentThreads { get; set; }

        // ==============================
        // ADMIN TABLES (SAFE ADDITIONS)
        // ==============================
        public DbSet<UserAccount> UserAccounts { get; set; }
        public DbSet<SystemSetting> SystemSettings { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // PATIENT PK
            builder.Entity<Patient>()
                .HasKey(p => p.Patient_ID);

            // SENSOR DATA PK
            builder.Entity<SensorData>()
                .HasKey(s => s.Data_Id);

            // COMMENTS PK
            builder.Entity<CommentThread>()
                .HasKey(c => c.Comment_ID);

            // Patient ↔ IdentityUser (1-to-1)
            builder.Entity<Patient>()
                .HasOne(p => p.AppUser)
                .WithOne()
                .HasForeignKey<Patient>(p => p.AppUserId)
                .HasPrincipalKey<ApplicationUser>(u => u.Id)
                .OnDelete(DeleteBehavior.Cascade);

            // Patient ↔ SensorData (1-to-many)
            builder.Entity<SensorData>()
                .HasOne(s => s.Patient)
                .WithMany(p => p.SensorData)
                .HasForeignKey(s => s.Patient_ID)
                .OnDelete(DeleteBehavior.Cascade);

            // Patient ↔ CommentThread (1-to-many)
            builder.Entity<CommentThread>()
                .HasOne(c => c.Patient)
                .WithMany(p => p.Comments)
                .HasForeignKey(c => c.Patient_ID)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
