using System.Collections.ObjectModel;
using System.Windows.Input;
using KindergartenApp.Application.DTOs;
using KindergartenApp.Application.Interfaces;
using KindergartenApp.UI.Infrastructure;

namespace KindergartenApp.UI.ViewModels;

public class DashboardViewModel : ViewModelBase
{
    private readonly IDashboardService _dashboardService;
    private readonly IAttendanceService _attendanceService;
    private readonly IPaymentService _paymentService;
    private readonly IBackupService _backupService;
    private readonly INotificationService _notificationService;
    private DashboardDto _dashboardData = new();
    private bool _isLoading;
    private DateTime _selectedMonth = DateTime.Today;

    public DashboardDto DashboardData
    {
        get => _dashboardData;
        set => SetProperty(ref _dashboardData, value);
    }

    public bool IsLoading
    {
        get => _isLoading;
        set => SetProperty(ref _isLoading, value);
    }

    public DateTime SelectedMonth
    {
        get => _selectedMonth;
        set
        {
            if (SetProperty(ref _selectedMonth, value))
                _ = LoadAnalyticsAsync();
        }
    }

    public ObservableCollection<AttendanceDto> RecentAttendance { get; } = new();
    public ObservableCollection<PaymentDto> RecentPayments { get; } = new();
    public ObservableCollection<AlertDto> RecentAlerts { get; } = new();

    public int TotalChildren => DashboardData.TotalChildren;
    public int TotalStaff => DashboardData.TotalStaff;
    public int TotalClassrooms => DashboardData.TotalClassrooms;
    public decimal MonthlyRevenue { get; private set; }
    public decimal OutstandingBalance { get; private set; }
    public decimal AttendanceRate { get; private set; }

    public ICommand LoadDashboardCommand { get; }
    public ICommand BackupCommand { get; }
    public ICommand LoadAnalyticsCommand { get; }

    public DashboardViewModel(IDashboardService dashboardService, IAttendanceService attendanceService, IPaymentService paymentService, IBackupService backupService, INotificationService notificationService)
    {
        _dashboardService = dashboardService;
        _attendanceService = attendanceService;
        _paymentService = paymentService;
        _backupService = backupService;
        _notificationService = notificationService;

        LoadDashboardCommand = new AsyncRelayCommand(async _ => await LoadDashboardAsync());
        BackupCommand = new AsyncRelayCommand(async _ => await BackupDatabaseAsync());
        LoadAnalyticsCommand = new AsyncRelayCommand(async _ => await LoadAnalyticsAsync());

        _ = LoadDashboardAsync();
        _ = LoadAnalyticsAsync();
    }

    public async Task LoadDashboardAsync()
    {
        IsLoading = true;
        try
        {
            DashboardData = await _dashboardService.GetDashboardDataAsync();
            OnPropertyChanged(nameof(TotalChildren));
            OnPropertyChanged(nameof(TotalStaff));
            OnPropertyChanged(nameof(TotalClassrooms));
        }
        catch (Exception ex)
        {
            _notificationService.ShowError($"Failed to load dashboard: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    public async Task LoadAnalyticsAsync()
    {
        IsLoading = true;
        try
        {
            var monthStart = new DateTime(SelectedMonth.Year, SelectedMonth.Month, 1);
            var monthEnd = monthStart.AddMonths(1).AddDays(-1);

            MonthlyRevenue = await _paymentService.GetTotalRevenueAsync(monthStart, monthEnd);
            OutstandingBalance = await _paymentService.GetOutstandingBalanceAsync(0);
            
            var dailyStats = await _attendanceService.GetDailyAttendanceStatsAsync(monthStart, monthEnd);
            AttendanceRate = dailyStats.Count > 0 ? dailyStats.Values.Average() : 0;

            OnPropertyChanged(nameof(MonthlyRevenue));
            OnPropertyChanged(nameof(OutstandingBalance));
            OnPropertyChanged(nameof(AttendanceRate));
        }
        catch (Exception ex)
        {
            _notificationService.ShowError($"Failed to load analytics: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task BackupDatabaseAsync()
    {
        try
        {
            await _backupService.BackupDatabaseAsync();
            _notificationService.ShowSuccess($"Database backed up successfully\nLocation: {_backupService.GetBackupFolderPath()}");
        }
        catch (Exception ex)
        {
            _notificationService.ShowError($"Failed to backup database: {ex.Message}");
        }
    }
}