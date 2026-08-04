using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KindergartenApp.Core.Entities;

public class PaymentInstallment : BaseEntity
{
    [Required]
    public int PaymentId { get; set; }

    [Required]
    [Range(1, 12)]
    public int InstallmentNumber { get; set; }

    [Required]
    [Range(0, double.MaxValue)]
    public decimal Amount { get; set; }

    [Required]
    public DateTime DueDate { get; set; }

    public DateTime? PaidDate { get; set; }

    [Required]
    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;

    [Range(0, double.MaxValue)]
    public decimal LateFee { get; set; } = 0;

    [Range(0, double.MaxValue)]
    public decimal Discount { get; set; } = 0;

    [MaxLength(500)]
    public string? Notes { get; set; }

    // Navigation properties
    public Payment Payment { get; set; } = null!;

    [NotMapped]
    public decimal TotalAmount => Amount + LateFee - Discount;
    
    [NotMapped]
    public bool IsOverdue => Status == PaymentStatus.Pending && DueDate < DateTime.Today;
    
    [NotMapped]
    public string StatusDisplay => Status.ToString();
}
