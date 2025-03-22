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
        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<AssignmentType> AssignmentTypes { get; set; }
        public DbSet<Assignment> Assignments { get; set; }
        public DbSet<Grade> Grades { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure relationships
            modelBuilder.Entity<User>()
                .HasOne(u => u.Profile)
                .WithOne(p => p.User)
                .HasForeignKey<UserProfile>(p => p.UserId);
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Username)
                .IsUnique();
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();
            modelBuilder.Entity<User>()
                .Property(u => u.CreatedAt)
                .HasDefaultValueSql("now()");
            modelBuilder.Entity<User>()
                .Property(u => u.UpdatedAt)
                .HasDefaultValueSql("now()");
            modelBuilder.Entity<User>()
                .Property(u => u.IsActive)
                .HasDefaultValue(true);

            modelBuilder.Entity<PasswordReset>()
                .HasKey(p => p.ResetId);
            modelBuilder.Entity<PasswordReset>()
                .HasOne(p => p.User)
                .WithMany()
                .HasForeignKey(p => p.UserId);
            modelBuilder.Entity<PasswordReset>()
                .Property(p => p.CreatedAt)
                .HasDefaultValueSql("now()");
            modelBuilder.Entity<PasswordReset>()
                .Property(p => p.UsedAt)
                .HasDefaultValue(null);

            modelBuilder.Entity<Course>()
                .HasKey(c => c.CourseId);
            modelBuilder.Entity<Course>()
                .HasIndex(c => c.CourseCode)
                .IsUnique();
            modelBuilder.Entity<Course>()
                .Property(c => c.CreatedAt)
                .HasDefaultValueSql("now()");
            modelBuilder.Entity<Course>()
                .Property(c => c.UpdatedAt)
                .HasDefaultValueSql("now()");

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
            modelBuilder.Entity<Class>()
                .Property(c => c.CreatedAt)
                .HasDefaultValueSql("now()");
            modelBuilder.Entity<Class>()
                .Property(c => c.UpdatedAt)
                .HasDefaultValueSql("now()");

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
            modelBuilder.Entity<ClassEnrollment>()
                .Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()");
            modelBuilder.Entity<ClassEnrollment>()
                .Property(e => e.UpdatedAt)
                .HasDefaultValueSql("now()");

            modelBuilder.Entity<AuditLog>()
                .HasKey(a => a.LogId);
            modelBuilder.Entity<AuditLog>()
                .Property(a => a.Details)
                .HasColumnType("jsonb");
            modelBuilder.Entity<AuditLog>()
                .HasOne(a => a.User)
                .WithMany()
                .HasForeignKey(a => a.UserId);
            modelBuilder.Entity<AuditLog>()
                .Property(a => a.CreatedAt)
                .HasDefaultValueSql("now()");

            modelBuilder.Entity<AssignmentType>()
                .HasKey(t => t.TypeId);
            modelBuilder.Entity<AssignmentType>()
                .Property(t => t.CreatedAt)
                .HasDefaultValueSql("now()");
            modelBuilder.Entity<AssignmentType>()
                .Property(t => t.UpdatedAt)
                .HasDefaultValueSql("now()");

            modelBuilder.Entity<Assignment>()
                .HasKey(a => a.AssignmentId);
            modelBuilder.Entity<Assignment>()
                .HasOne(a => a.Class)
                .WithMany(c => c.Assignments)
                .HasForeignKey(a => a.ClassId);
            modelBuilder.Entity<Assignment>()
                .HasOne(a => a.AssignmentType)
                .WithMany()
                .HasForeignKey(a => a.TypeId);
            modelBuilder.Entity<Assignment>()
                .Property(a => a.CreatedAt)
                .HasDefaultValueSql("now()");
            modelBuilder.Entity<Assignment>()
                .Property(a => a.UpdatedAt)
                .HasDefaultValueSql("now()");
            modelBuilder.Entity<Assignment>()
                .Property(a => a.IsPublished)
                .HasDefaultValue(false);

            modelBuilder.Entity<Grade>()
                .HasKey(g => g.GradeId);
            modelBuilder.Entity<Grade>()
                .HasOne(g => g.Assignment)
                .WithMany()
                .HasForeignKey(g => g.AssignmentId);
            modelBuilder.Entity<Grade>()
                .HasOne(g => g.Student)
                .WithMany()
                .HasForeignKey(g => g.StudentId);
            modelBuilder.Entity<Grade>()
                .HasOne(g => g.Grader)
                .WithMany()
                .HasForeignKey(g => g.GradedBy);
            modelBuilder.Entity<Grade>()
                .Property(g => g.CreatedAt)
                .HasDefaultValueSql("now()");
            modelBuilder.Entity<Grade>()
                .Property(g => g.UpdatedAt)
                .HasDefaultValueSql("now()");

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