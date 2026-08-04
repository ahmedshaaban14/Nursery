using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace KindergartenApp.Core.Entities;

public class Child : BaseEntity
{
    [Required]
    [MaxLength(100)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    public int Age { get; set; }

    public DateTime? DateOfBirth { get; set; }

    [Required]
    [MaxLength(20)]
    public string Gender { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string ParentName { get; set; } = string.Empty;

    [Required]
    [MaxLength(20)]
    [Phone]
    public string Phone { get; set; } = string.Empty;

    [MaxLength(200)]
    public string Address { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Notes { get; set; }

    [Required]
    [Range(0, double.MaxValue)]
    public decimal MonthlyFee { get; set; }

    [Required]
    public DateTime JoinDate { get; set; } = DateTime.UtcNow;

    public bool IsActive { get; set; } = true;

    // Classroom assignment
    public int? ClassroomId { get; set; }
    public Classroom? Classroom { get; set; }

    // Medical information
    [MaxLength(1000)]
    public string? MedicalNotes { get; set; }

    [MaxLength(500)]
    public string? Allergies { get; set; }

    // Emergency contact
    [MaxLength(100)]
    public string? EmergencyContactName { get; set; }

    [MaxLength(20)]
    [Phone]
    public string? EmergencyContactPhone { get; set; }

    // Photo
    [MaxLength(255)]
    public string? PhotoPath { get; set; }

    // Navigation properties
    public ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
    public ICollection<Document> Documents { get; set; } = new List<Document>();
    public ICollection<Alert> Alerts { get; set; } = new List<Alert>();

    [NotMapped]
    public int TotalPayments => Payments.Count;
    
    [NotMapped]
    public decimal TotalPaid => Payments.Where(p => p.IsPaid).Sum(p => p.Amount);
    
    [NotMapped]
    public decimal OutstandingBalance => (MonthlyFee * 12) - TotalPaid;
}

