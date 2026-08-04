using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KindergartenApp.Core.Entities;

public class Attendance : BaseEntity
{
    [Required]
    public int ChildId { get; set; }

    [Required]
    public DateTime Date { get; set; }

    [Required]
    public bool IsPresent { get; set; }

    [MaxLength(500)]
    public string? Notes { get; set; }

    // Classroom and Staff tracking
    public int? ClassroomId { get; set; }
    public int? TeacherId { get; set; }

    public DateTime? CheckInTime { get; set; }
    public DateTime? CheckOutTime { get; set; }

    // Navigation properties
    public Child? Child { get; set; }
    public Classroom? Classroom { get; set; }
    public Staff? Teacher { get; set; }

    [NotMapped]
    public string StatusDisplay => IsPresent ? "Present" : "Absent";
    
    [NotMapped]
    public bool IsLate => CheckInTime.HasValue && CheckInTime.Value.TimeOfDay > TimeSpan.FromHours(9);
}

