using System.Collections.ObjectModel;
using System.Windows.Input;
using KindergartenApp.Application.Interfaces;
using KindergartenApp.Application.DTOs;
using KindergartenApp.UI.Infrastructure;

namespace KindergartenApp.UI.ViewModels;

public class ReportsViewModel : ViewModelBase
{
    private readonly IReportService _reportService;
    private readonly IChildService _childService;
    private readonly INotificationService _notificationService;
    private bool _isLoading;
    private DateTime _fromDate = DateTime.Today.AddMonths(-1);
    private DateTime _toDate = DateTime.Today;
    private int _selectedMonth = DateTime.Today.Month;
    private int _selectedYear = DateTime.Today.Year;
    private int _selectedChildId;

    public ObservableCollection<ChildDto> Children { get; } = new();

    public bool IsLoading
    {
        get => _isLoading;
        set => SetProperty(ref _isLoading, value);
    }

    public DateTime FromDate
    {
        get => _fromDate;
        set => SetProperty(ref _fromDate, value);
    }

    public DateTime ToDate
    {
        get => _toDate;
        set => SetProperty(ref _toDate, value);
    }

    public int SelectedMonth
    {
        get => _selectedMonth;
        set => SetProperty(ref _selectedMonth, value);
    }

    public int SelectedYear
    {
        get => _selectedYear;
        set => SetProperty(ref _selectedYear, value);
    }

    public int SelectedChildId
    {
        get => _selectedChildId;
        set => SetProperty(ref _selectedChildId, value);
    }

    public ICommand GenerateUnpaidChildrenReportCommand { get; }
    public ICommand GenerateMonthlyRevenueReportCommand { get; }
    public ICommand GenerateAttendanceReportCommand { get; }
    public ICommand LoadChildrenCommand { get; }

    public ReportsViewModel(IReportService reportService, IChildService childService, INotificationService notificationService)
    {
        _reportService = reportService;
        _childService = childService;
        _notificationService = notificationService;

        GenerateUnpaidChildrenReportCommand = new AsyncRelayCommand(async _ => await GenerateUnpaidChildrenReportAsync());
        GenerateMonthlyRevenueReportCommand = new AsyncRelayCommand(async _ => await GenerateMonthlyRevenueReportAsync());
        GenerateAttendanceReportCommand = new AsyncRelayCommand(async _ => await GenerateAttendanceReportAsync());
        LoadChildrenCommand = new AsyncRelayCommand(async _ => await LoadChildrenAsync());

        _ = LoadChildrenAsync();
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

    private async Task GenerateUnpaidChildrenReportAsync()
    {
        IsLoading = true;
        try
        {
            var reportData = await _reportService.GenerateUnpaidChildrenReportAsync();
            _notificationService.ShowSuccess($"Unpaid children report generated ({reportData.Length} bytes)");
        }
        catch (Exception ex)
        {
            _notificationService.ShowError($"Failed to generate report: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task GenerateMonthlyRevenueReportAsync()
    {
        IsLoading = true;
        try
        {
            var reportData = await _reportService.GenerateMonthlyRevenueReportAsync(SelectedMonth, SelectedYear);
            _notificationService.ShowSuccess($"Monthly revenue report generated ({reportData.Length} bytes)");
        }
        catch (Exception ex)
        {
            _notificationService.ShowError($"Failed to generate report: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task GenerateAttendanceReportAsync()
    {
        if (SelectedChildId == 0)
        {
            _notificationService.ShowError("Please select a child first");
            return;
        }

        IsLoading = true;
        try
        {
            var reportData = await _reportService.GenerateAttendanceReportAsync(SelectedChildId, FromDate, ToDate);
            _notificationService.ShowSuccess($"Attendance report generated ({reportData.Length} bytes)");
        }
        catch (Exception ex)
        {
            _notificationService.ShowError($"Failed to generate report: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }
}