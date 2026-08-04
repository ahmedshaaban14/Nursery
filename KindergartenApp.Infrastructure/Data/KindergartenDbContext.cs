using Microsoft.EntityFrameworkCore;
using KindergartenApp.Core.Entities;

namespace KindergartenApp.Infrastructure.Data;

public class KindergartenDbContext : DbContext
{
    public DbSet<User> Users { get; set; }
    public DbSet<Child> Children { get; set; }
    public DbSet<Attendance> Attendances { get; set; }
    public DbSet<Payment> Payments { get; set; }
    
    // New DbSets for additional modules
    public DbSet<Classroom> Classrooms { get; set; }
    public DbSet<Staff> Staff { get; set; }
    public DbSet<Alert> Alerts { get; set; }
    public DbSet<Document> Documents { get; set; }
    public DbSet<PaymentInstallment> PaymentInstallments { get; set; }

    public KindergartenDbContext(DbContextOptions<KindergartenDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Child>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FullName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Gender).IsRequired().HasMaxLength(20);
            entity.Property(e => e.ParentName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Phone).IsRequired().HasMaxLength(20);
            entity.Property(e => e.Address).HasMaxLength(200);
            entity.Property(e => e.MonthlyFee).HasPrecision(18, 2);
            
            // New fields
            entity.Property(e => e.MedicalNotes).HasMaxLength(1000);
            entity.Property(e => e.Allergies).HasMaxLength(500);
            entity.Property(e => e.EmergencyContactName).HasMaxLength(100);
            entity.Property(e => e.EmergencyContactPhone).HasMaxLength(20);
            entity.Property(e => e.PhotoPath).HasMaxLength(255);
            
            // Relationships
            entity.HasOne(e => e.Classroom)
                .WithMany(c => c.Children)
                .HasForeignKey(e => e.ClassroomId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<Attendance>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.Child)
                .WithMany(c => c.Attendances)
                .HasForeignKey(e => e.ChildId)
                .OnDelete(DeleteBehavior.Cascade);
            
            // New relationships
            entity.HasOne(e => e.Classroom)
                .WithMany()
                .HasForeignKey(e => e.ClassroomId)
                .OnDelete(DeleteBehavior.SetNull);
            
            entity.HasOne(e => e.Teacher)
                .WithMany(s => s.AttendanceRecords)
                .HasForeignKey(e => e.TeacherId)
                .OnDelete(DeleteBehavior.SetNull);
            
            entity.HasIndex(e => new { e.ChildId, e.Date }).IsUnique();
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.Child)
                .WithMany(c => c.Payments)
                .HasForeignKey(e => e.ChildId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.Property(e => e.Amount).HasPrecision(18, 2);
            
            // New fields
            entity.Property(e => e.LateFee).HasPrecision(18, 2);
            entity.Property(e => e.Discount).HasPrecision(18, 2);
            entity.Property(e => e.ReceiptNumber).HasMaxLength(255);
            
            // Relationship
            entity.HasMany(e => e.Installments)
                .WithOne(i => i.Payment)
                .HasForeignKey(i => i.PaymentId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasIndex(e => new { e.ChildId, e.Month, e.Year });
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Username).IsRequired().HasMaxLength(50);
            entity.Property(e => e.PasswordHash).IsRequired();
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.FullName).HasMaxLength(100);
            entity.HasIndex(e => e.Username).IsUnique();
            
            // New relationship
            entity.HasOne(e => e.Staff)
                .WithOne()
                .HasForeignKey<User>(e => e.StaffId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // New entity configurations

        modelBuilder.Entity<Classroom>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Notes).HasMaxLength(500);
            
            entity.HasOne(e => e.Teacher)
                .WithMany(s => s.Classrooms)
                .HasForeignKey(e => e.TeacherId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<Staff>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FullName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Phone).IsRequired().HasMaxLength(20);
            entity.Property(e => e.Address).HasMaxLength(200);
            entity.Property(e => e.Salary).HasPrecision(18, 2);
            entity.Property(e => e.Notes).HasMaxLength(500);
        });

        modelBuilder.Entity<Alert>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(500);
            entity.Property(e => e.Message).IsRequired().HasMaxLength(2000);
            entity.Property(e => e.RelatedEntityType).HasMaxLength(50);
            
            entity.HasOne(e => e.Child)
                .WithMany(c => c.Alerts)
                .HasForeignKey(e => e.ChildId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasOne(e => e.Staff)
                .WithMany()
                .HasForeignKey(e => e.StaffId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Document>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FileName).IsRequired().HasMaxLength(255);
            entity.Property(e => e.FilePath).IsRequired().HasMaxLength(500);
            entity.Property(e => e.ContentType).HasMaxLength(50);
            entity.Property(e => e.Description).HasMaxLength(500);
            
            entity.HasOne(e => e.Child)
                .WithMany(c => c.Documents)
                .HasForeignKey(e => e.ChildId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<PaymentInstallment>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Amount).HasPrecision(18, 2);
            entity.Property(e => e.LateFee).HasPrecision(18, 2);
            entity.Property(e => e.Discount).HasPrecision(18, 2);
            entity.Property(e => e.Notes).HasMaxLength(500);
        });
    }
}
