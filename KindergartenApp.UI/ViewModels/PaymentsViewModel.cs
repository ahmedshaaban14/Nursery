using System.Collections.ObjectModel;
using System.Windows.Input;
using KindergartenApp.Application.DTOs;
using KindergartenApp.Application.Interfaces;
using KindergartenApp.UI.Infrastructure;
using KindergartenApp.UI.Models;

namespace KindergartenApp.UI.ViewModels;

public class PaymentsViewModel : ViewModelBase
{
    private readonly IPaymentService _paymentService;
    private readonly IChildService _childService;
    private readonly INotificationService _notificationService;
    private bool _isLoading;
    private int _selectedChildId;
    private decimal _amount;
    private DateTime _paymentDate = DateTime.Now;
    private DateTime? _dueDate;
    private decimal _lateFee;
    private decimal _discount;
    private string _searchQuery = string.Empty;
    private bool _showOverdueOnly;

    public ObservableCollection<PaymentDto> UnpaidPayments { get; } = new();
    public ObservableCollection<PaymentDto> OverduePayments { get; } = new();
    public ObservableCollection<PaymentDto> AllPayments { get; } = new();
    public ObservableCollection<PaymentDto> FilteredPayments { get; } = new();
    public ObservableCollection<ChildDto> Children { get; } = new();
    public ObservableCollection<PaymentDto> ChildPayments { get; } = new();
    public ObservableCollection<PaymentInstallmentDto> Installments { get; } = new();

    public bool CanManagePayments => SessionManager.CanManagePayments();
    public bool CanViewReports => SessionManager.CanViewReports();

    public bool IsLoading
    {
        get => _isLoading;
        set => SetProperty(ref _isLoading, value);
    }

    public int SelectedChildId
    {
        get => _selectedChildId;
        set
        {
            if (SetProperty(ref _selectedChildId, value) && value > 0)
                _ = LoadChildPaymentsAsync();
        }
    }

    public decimal Amount
    {
        get => _amount;
        set => SetProperty(ref _amount, value);
    }

    public DateTime PaymentDate
    {
        get => _paymentDate;
        set => SetProperty(ref _paymentDate, value);
    }

    public DateTime? DueDate
    {
        get => _dueDate;
        set => SetProperty(ref _dueDate, value);
    }

    public decimal LateFee
    {
        get => _lateFee;
        set => SetProperty(ref _lateFee, value);
    }

    public decimal Discount
    {
        get => _discount;
        set => SetProperty(ref _discount, value);
    }

    public string SearchQuery
    {
        get => _searchQuery;
        set
        {
            if (SetProperty(ref _searchQuery, value))
                FilterPayments();
        }
    }

    public bool ShowOverdueOnly
    {
        get => _showOverdueOnly;
        set
        {
            if (SetProperty(ref _showOverdueOnly, value))
                FilterPayments();
        }
    }

    public decimal TotalUnpaid => UnpaidPayments.Sum(p => p.TotalAmount);
    public decimal TotalOverdue => OverduePayments.Sum(p => p.TotalAmount);
    public int UnpaidCount => UnpaidPayments.Count;
    public int OverdueCount => OverduePayments.Count;

    public ICommand LoadUnpaidPaymentsCommand { get; }
    public ICommand LoadOverduePaymentsCommand { get; }
    public ICommand LoadAllPaymentsCommand { get; }
    public ICommand LoadChildrenCommand { get; }
    public ICommand AddPaymentCommand { get; }
    public ICommand MarkPaidCommand { get; }
    public ICommand RecordPartialPaymentCommand { get; }
    public ICommand GenerateReceiptCommand { get; }
    public ICommand GenerateInvoiceCommand { get; }

    public PaymentsViewModel(IPaymentService paymentService, IChildService childService, INotificationService notificationService)
    {
        _paymentService = paymentService;
        _childService = childService;
        _notificationService = notificationService;

        LoadUnpaidPaymentsCommand = new AsyncRelayCommand(async _ => await LoadUnpaidPaymentsAsync());
        LoadOverduePaymentsCommand = new AsyncRelayCommand(async _ => await LoadOverduePaymentsAsync());
        LoadAllPaymentsCommand = new AsyncRelayCommand(async _ => await LoadAllPaymentsAsync());
        LoadChildrenCommand = new AsyncRelayCommand(async _ => await LoadChildrenAsync());
        AddPaymentCommand = new AsyncRelayCommand(async _ => await AddPaymentAsync());
        MarkPaidCommand = new AsyncRelayCommand(async param => await MarkPaymentAsync(param));
        RecordPartialPaymentCommand = new AsyncRelayCommand(async param => await RecordPartialPaymentAsync(param));
        GenerateReceiptCommand = new AsyncRelayCommand(async param => await GenerateReceiptAsync(param));
        GenerateInvoiceCommand = new AsyncRelayCommand(async _ => await GenerateMonthlyInvoiceAsync());

        _ = LoadChildrenAsync();
        _ = LoadUnpaidPaymentsAsync();
        _ = LoadOverduePaymentsAsync();
    }

    public async Task LoadUnpaidPaymentsAsync()
    {
        IsLoading = true;
        try
        {
            var payments = await _paymentService.GetUnpaidPaymentsAsync();
            UnpaidPayments.Clear();
            foreach (var payment in payments)
                UnpaidPayments.Add(payment);
            
            OnPropertyChanged(nameof(TotalUnpaid));
            OnPropertyChanged(nameof(UnpaidCount));
            FilterPayments();
        }
        catch (Exception ex)
        {
            _notificationService.ShowError($"Failed to load unpaid payments: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    public async Task LoadOverduePaymentsAsync()
    {
        IsLoading = true;
        try
        {
            var payments = await _paymentService.GetOverduePaymentsAsync();
            OverduePayments.Clear();
            foreach (var payment in payments)
                OverduePayments.Add(payment);
            
            OnPropertyChanged(nameof(TotalOverdue));
            OnPropertyChanged(nameof(OverdueCount));
        }
        catch (Exception ex)
        {
            _notificationService.ShowError($"Failed to load overdue payments: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    public async Task LoadAllPaymentsAsync()
    {
        IsLoading = true;
        try
        {
            var allPayments = await _paymentService.GetUnpaidPaymentsAsync();
            var paidPayments = await _paymentService.GetPaymentHistoryAsync(0);
            
            AllPayments.Clear();
            foreach (var payment in allPayments.Concat(paidPayments))
                AllPayments.Add(payment);
            
            FilterPayments();
        }
        catch (Exception ex)
        {
            _notificationService.ShowError($"Failed to load payments: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void FilterPayments()
    {
        FilteredPayments.Clear();
        var source = ShowOverdueOnly ? OverduePayments : AllPayments;
        
        var filtered = source.AsEnumerable();
        if (!string.IsNullOrWhiteSpace(SearchQuery))
        {
            filtered = filtered.Where(p => p.ChildName.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase));
        }

        foreach (var payment in filtered)
            FilteredPayments.Add(payment);
    }

    private async Task LoadChildrenAsync()
    {
        try
        {
            var children = await _childService.GetAllChildrenAsync();
            Children.Clear();
            foreach (var child in children)
                Children.Add(child);
        }
        catch (Exception ex)
        {
            _notificationService.ShowError($"Failed to load children: {ex.Message}");
        }
    }

    private async Task LoadChildPaymentsAsync()
    {
        try
        {
            var payments = await _paymentService.GetChildPaymentsAsync(SelectedChildId);
            ChildPayments.Clear();
            foreach (var payment in payments)
                ChildPayments.Add(payment);
        }
        catch (Exception ex)
        {
            _notificationService.ShowError($"Failed to load child payments: {ex.Message}");
        }
    }

    private async Task AddPaymentAsync()
    {
        if (SelectedChildId == 0 || Amount <= 0)
        {
            _notificationService.ShowError("Please select a child and enter a valid amount");
            return;
        }

        try
        {
            var dto = new PaymentDto
            {
                ChildId = SelectedChildId,
                Amount = Amount,
                Date = PaymentDate,
                DueDate = DueDate,
                LateFee = LateFee,
                Discount = Discount,
                IsPaid = true
            };

            await _paymentService.CreatePaymentAsync(dto);
            _notificationService.ShowSuccess("Payment added successfully");
            Amount = 0;
            LateFee = 0;
            Discount = 0;
            DueDate = null;
            await LoadUnpaidPaymentsAsync();
            await LoadChildPaymentsAsync();
        }
        catch (Exception ex)
        {
            _notificationService.ShowError($"Failed to add payment: {ex.Message}");
        }
    }

    private async Task MarkPaymentAsync(object? param)
    {
        if (!(param is PaymentDto payment))
            return;

        try
        {
            await _paymentService.MarkPaymentAsync(payment.Id, true);
            _notificationService.ShowSuccess("Payment marked as paid");
            await LoadUnpaidPaymentsAsync();
        }
        catch (Exception ex)
        {
            _notificationService.ShowError($"Failed to mark payment: {ex.Message}");
        }
    }

    private async Task RecordPartialPaymentAsync(object? param)
    {
        if (!(param is PaymentDto payment))
            return;

        try
        {
            await _paymentService.RecordPartialPaymentAsync(payment.Id, Amount);
            _notificationService.ShowSuccess($"Partial payment of {Amount:C} recorded");
            Amount = 0;
            await LoadUnpaidPaymentsAsync();
        }
        catch (Exception ex)
        {
            _notificationService.ShowError($"Failed to record partial payment: {ex.Message}");
        }
    }

    private async Task GenerateReceiptAsync(object? param)
    {
        if (!(param is PaymentDto payment))
            return;

        try
        {
            var receipt = await _paymentService.GenerateReceiptAsync(payment.Id);
            _notificationService.ShowSuccess($"Receipt generated:\n{receipt}");
        }
        catch (Exception ex)
        {
            _notificationService.ShowError($"Failed to generate receipt: {ex.Message}");
        }
    }

    private async Task GenerateMonthlyInvoiceAsync()
    {
        try
        {
            var today = DateTime.Today;
            var invoice = await _paymentService.GenerateMonthlyInvoiceAsync(today.Month, today.Year);
            _notificationService.ShowSuccess($"Invoice generated for {invoice.Count} unpaid payments");
        }
        catch (Exception ex)
        {
            _notificationService.ShowError($"Failed to generate invoice: {ex.Message}");
        }
    }
}