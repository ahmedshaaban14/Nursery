using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KindergartenApp.Core.Entities;

public class Classroom : BaseEntity
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    public ClassroomStage Stage { get; set; }

    [Required]
    [Range(1, 50)]
    public int Capacity { get; set; }

    public int? TeacherId { get; set; }

    [MaxLength(500)]
    public string? Notes { get; set; }

    // Navigation properties
    public Staff? Teacher { get; set; }
    public ICollection<Child> Children { get; set; } = new List<Child>();

    [NotMapped]
    public int CurrentStudentCount => Children.Count;
    
    [NotMapped]
    public bool IsFull => CurrentStudentCount >= Capacity;
    
    [NotMapped]
    public double OccupancyPercentage => Capacity > 0 ? (double)CurrentStudentCount / Capacity * 100 : 0;
}
