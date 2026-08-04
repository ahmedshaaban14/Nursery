using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KindergartenApp.Core.Entities;

public class Document : BaseEntity
{
    [Required]
    public DocumentType Type { get; set; }

    [Required]
    [MaxLength(255)]
    public string FileName { get; set; } = string.Empty;

    [Required]
    [MaxLength(500)]
    public string FilePath { get; set; } = string.Empty;

    [MaxLength(50)]
    public string? ContentType { get; set; }

    public long FileSize { get; set; }

    [MaxLength(500)]
    public string? Description { get; set; }

    // Navigation properties
    public int? ChildId { get; set; }
    public Child? Child { get; set; }

    [NotMapped]
    public string TypeDisplay => Type.ToString();
    
    [NotMapped]
    public string FileSizeDisplay => FileSize > 1024 * 1024 
        ? $"{FileSize / (1024 * 1024):F2} MB" 
        : $"{FileSize / 1024:F2} KB";
}
