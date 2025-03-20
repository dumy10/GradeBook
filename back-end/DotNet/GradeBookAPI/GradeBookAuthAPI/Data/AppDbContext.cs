using GradeBookAuthAPI.Entities;
using Microsoft.EntityFrameworkCore;

namespace GradeBookAuthAPI.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
        public DbSet<User> Users { get; set; }
        public DbSet<UserProfile> UserProfiles { get; set; }
        public DbSet<PasswordReset> PasswordResets { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure relationships
            modelBuilder.Entity<User>()
                .HasOne(u => u.Profile)
                .WithOne(p => p.User)
                .HasForeignKey<UserProfile>(p => p.UserId);

            modelBuilder.Entity<PasswordReset>()
                .HasKey(p => p.ResetId);

            // Configure column names to match PostgreSQL snake_case convention
            modelBuilder.Entity<User>().ToTable("users");
            modelBuilder.Entity<User>().Property(u => u.UserId).HasColumnName("user_id");
            modelBuilder.Entity<User>().Property(u => u.Username).HasColumnName("username");
            modelBuilder.Entity<User>().Property(u => u.Email).HasColumnName("email");
            modelBuilder.Entity<User>().Property(u => u.PasswordHash).HasColumnName("password_hash");
            modelBuilder.Entity<User>().Property(u => u.Salt).HasColumnName("salt");
            modelBuilder.Entity<User>().Property(u => u.Role).HasColumnName("role");
            modelBuilder.Entity<User>().Property(u => u.IsActive).HasColumnName("is_active");
            modelBuilder.Entity<User>().Property(u => u.LastLogin).HasColumnName("last_login");
            modelBuilder.Entity<User>().Property(u => u.CreatedAt).HasColumnName("created_at");
            modelBuilder.Entity<User>().Property(u => u.UpdatedAt).HasColumnName("updated_at");

            modelBuilder.Entity<UserProfile>().ToTable("user_profiles");
            modelBuilder.Entity<UserProfile>().Property(p => p.ProfileId).HasColumnName("profile_id");
            modelBuilder.Entity<UserProfile>().Property(p => p.UserId).HasColumnName("user_id");
            modelBuilder.Entity<UserProfile>().Property(p => p.FirstName).HasColumnName("first_name");
            modelBuilder.Entity<UserProfile>().Property(p => p.LastName).HasColumnName("last_name");
            modelBuilder.Entity<UserProfile>().Property(p => p.Phone).HasColumnName("phone");
            modelBuilder.Entity<UserProfile>().Property(p => p.Address).HasColumnName("address");
            modelBuilder.Entity<UserProfile>().Property(p => p.ProfilePicture).HasColumnName("profile_picture");
            modelBuilder.Entity<UserProfile>().Property(p => p.CreatedAt).HasColumnName("created_at");
            modelBuilder.Entity<UserProfile>().Property(p => p.UpdatedAt).HasColumnName("updated_at");

            modelBuilder.Entity<PasswordReset>().ToTable("password_resets");
            modelBuilder.Entity<PasswordReset>().Property(r => r.ResetId).HasColumnName("reset_id");
            modelBuilder.Entity<PasswordReset>().Property(r => r.UserId).HasColumnName("user_id");
            modelBuilder.Entity<PasswordReset>().Property(r => r.Token).HasColumnName("token");
            modelBuilder.Entity<PasswordReset>().Property(r => r.ExpiresAt).HasColumnName("expires_at");
            modelBuilder.Entity<PasswordReset>().Property(r => r.UsedAt).HasColumnName("used_at");
            modelBuilder.Entity<PasswordReset>().Property(r => r.CreatedAt).HasColumnName("created_at");
        }
    }
}
