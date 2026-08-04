using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace KindergartenApp.Core.Entities;

public class Payment : BaseEntity
{
    [Required]
    public int ChildId { get; set; }

    [Required]
    [Range(0, double.MaxValue)]
    public decimal Amount { get; set; }

    [Required]
    public DateTime Date { get; set; }

    [Required]
    [Range(1, 12)]
    public int Month { get; set; }

    [Required]
    [Range(2020, 2100)]
    public int Year { get; set; }

    public bool IsPaid { get; set; }

    [MaxLength(500)]
    public string? Notes { get; set; }

    // Advanced payment features
    public bool SupportsInstallments { get; set; } = false;

    [Range(0, double.MaxValue)]
    public decimal LateFee { get; set; } = 0;

    [Range(0, double.MaxValue)]
    public decimal Discount { get; set; } = 0;

    [MaxLength(255)]
    public string? ReceiptNumber { get; set; }

    public DateTime? DueDate { get; set; }

    [Required]
    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;

    // Navigation properties
    public Child? Child { get; set; }
    public ICollection<PaymentInstallment> Installments { get; set; } = new List<PaymentInstallment>();

    [NotMapped]
    public decimal TotalAmount => Amount + LateFee - Discount;
    
    [NotMapped]
    public decimal PaidAmount => Installments.Where(i => i.Status == PaymentStatus.Paid).Sum(i => i.Amount);
    
    [NotMapped]
    public decimal RemainingBalance => TotalAmount - PaidAmount;
    
    [NotMapped]
    public bool IsOverdue => Status == PaymentStatus.Pending && DueDate.HasValue && DueDate < DateTime.Today;
    
    [NotMapped]
    public string StatusDisplay => Status.ToString();
}

