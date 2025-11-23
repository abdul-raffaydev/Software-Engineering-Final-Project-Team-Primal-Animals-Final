using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
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

        public DbSet<AppUser> AppUsers { get; set; }
        public DbSet<Patient> Patients { get; set; }
        public DbSet<SensorData> SensorData { get; set; }
        public DbSet<CommentThread> CommentThreads { get; set; }
        public DbSet<UserAccount> UserAccounts { get; set; }


        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // AppUser PK (optional, EF will pick it up since [Key] is present)
            builder.Entity<AppUser>()
                .HasKey(u => u.AppUserId);

            // Patient PK (optional, EF will pick it up since [Key] is present)
            builder.Entity<Patient>()
                .HasKey(p => p.Patient_ID);

            // 1-to-1 AppUser ↔ Patient
            builder.Entity<Patient>()
                .HasOne(p => p.AppUser)
                .WithOne(u => u.Patient)
                .HasForeignKey<Patient>(p => p.AppUserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
