using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KindergartenApp.Core.Entities;

public class Alert : BaseEntity
{
    [Required]
    public AlertType Type { get; set; }

    [Required]
    public AlertPriority Priority { get; set; }

    [Required]
    [MaxLength(500)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [MaxLength(2000)]
    public string Message { get; set; } = string.Empty;

    public DateTime? ExpiryDate { get; set; }

    public bool IsRead { get; set; } = false;

    public bool IsResolved { get; set; } = false;

    public DateTime? ResolvedAt { get; set; }

    // Reference to related entity (Child, Staff, etc.)
    public int? RelatedEntityId { get; set; }

    [MaxLength(50)]
    public string? RelatedEntityType { get; set; }

    // Navigation properties
    public int? ChildId { get; set; }
    public Child? Child { get; set; }

    public int? StaffId { get; set; }
    public Staff? Staff { get; set; }

    [NotMapped]
    public string TypeDisplay => Type.ToString();
    
    [NotMapped]
    public string PriorityDisplay => Priority.ToString();
    
    [NotMapped]
    public bool IsExpired => ExpiryDate.HasValue && ExpiryDate < DateTime.UtcNow;
}
