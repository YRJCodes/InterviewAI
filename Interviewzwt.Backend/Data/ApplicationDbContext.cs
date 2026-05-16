using Interviewzwt.Backend.Entities;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Interviewzwt.Backend.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<JobRole> JobRoles { get; set; }
        public DbSet<InterviewSession> InterviewSessions { get; set; }
        public DbSet<CustomJobDescription> CustomJobs { get; set; }
        public DbSet<PaymentOrder> PaymentOrders { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure JSON columns for lists if using MySQL 5.7+ or MariaDB 10.2+
            modelBuilder.Entity<JobRole>()
                .Property(e => e.Requirements)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null!),
                    v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions)null!) ?? new List<string>());

            modelBuilder.Entity<JobRole>()
                .Property(e => e.Skills)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null!),
                    v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions)null!) ?? new List<string>());

            modelBuilder.Entity<CustomJobDescription>()
                .Property(e => e.Requirements)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null!),
                    v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions)null!) ?? new List<string>());

            // Unique email
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            // Cascade delete user sessions and jobs
            modelBuilder.Entity<InterviewSession>()
                .HasOne(s => s.User)
                .WithMany()
                .HasForeignKey(s => s.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<CustomJobDescription>()
                .HasOne(j => j.User)
                .WithMany()
                .HasForeignKey(j => j.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
