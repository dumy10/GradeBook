using GradeBookAPI.Entities;
using Microsoft.EntityFrameworkCore;

namespace GradeBookAPI.Data
{
    public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
    {
        public DbSet<User> Users { get; set; }
        public DbSet<UserProfile> UserProfiles { get; set; }
        public DbSet<PasswordReset> PasswordResets { get; set; }
        public DbSet<Course> Courses { get; set; }
        public DbSet<Class> Classes { get; set; }
        public DbSet<ClassEnrollment> ClassEnrollments { get; set; }

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

            modelBuilder.Entity<Course>()
                .HasKey(c => c.CourseId);

            modelBuilder.Entity<Class>()
                .HasKey(c => c.ClassId);
            modelBuilder.Entity<Class>()
                .HasOne(c => c.Course)
                .WithMany(c => c.Classes)
                .HasForeignKey(c => c.CourseId);
            modelBuilder.Entity<Class>()
                .HasOne(c => c.Teacher)
                .WithMany()
                .HasForeignKey(c => c.TeacherId);

            modelBuilder.Entity<ClassEnrollment>()
                .HasKey(e => e.EnrollmentId);
            modelBuilder.Entity<ClassEnrollment>()
                .HasOne(e => e.Class)
                .WithMany(c => c.Enrollments)
                .HasForeignKey(e => e.ClassId);
            modelBuilder.Entity<ClassEnrollment>()
                .HasOne(e => e.Student)
                .WithMany()
                .HasForeignKey(e => e.StudentId);

            // Configure column names to match PostgreSQL snake_case convention
            ConfigureTables(modelBuilder);
        }

        private static void ConfigureTables(ModelBuilder modelBuilder)
        {
            // Get all the entity types
            var entityTypes = modelBuilder.Model.GetEntityTypes();
            foreach (var entityType in entityTypes)
            {
                // Get the table name and set it to snake_case
                var tableName = ToSnakeCase(entityType.GetTableName());
                entityType.SetTableName(tableName);

                // Get all the properties of the entity type
                foreach (var property in entityType.GetProperties())
                {
                    // Get the column name and set it to snake_case
                    var columnName = ToSnakeCase(property.Name);
                    property.SetColumnName(columnName);
                }
            }
        }

        private static string ToSnakeCase(string? input)
        {
            if (string.IsNullOrEmpty(input)) return string.Empty;

            var startUnderscores = string.Empty;
            for (int i = 0; i < input.Length && input[i] == '_'; i++)
            {
                startUnderscores += '_';
            }

            return startUnderscores + string.Concat(input.Select((x, i) => i > 0 && char.IsUpper(x) ? "_" + x : x.ToString())).ToLower();
        }
    }
}