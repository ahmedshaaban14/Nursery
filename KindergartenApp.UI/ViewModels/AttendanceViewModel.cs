using System.Collections.ObjectModel;
using System.Windows.Input;
using KindergartenApp.Application.DTOs;
using KindergartenApp.Application.Interfaces;
using KindergartenApp.UI.Infrastructure;
using KindergartenApp.UI.Models;

namespace KindergartenApp.UI.ViewModels;

public class AttendanceViewModel : ViewModelBase
{
    private readonly IAttendanceService _attendanceService;
    private readonly IChildService _childService;
    private readonly IClassroomService _classroomService;
    private readonly INotificationService _notificationService;
    private bool _isLoading;
    private int _selectedChildId;
    private DateTime _selectedDate = DateTime.Today;
    private int? _selectedClassroomId;
    private string _searchQuery = string.Empty;

    public ObservableCollection<AttendanceDto> TodayAttendance { get; } = new();
    public ObservableCollection<AttendanceDto> FilteredAttendance { get; } = new();
    public ObservableCollection<ChildDto> Children { get; } = new();
    public ObservableCollection<AttendanceDto> ChildAttendanceHistory { get; } = new();
    public ObservableCollection<ClassroomDto> Classrooms { get; } = new();

    public bool CanManageAttendance => SessionManager.CanManageAttendance();
    public bool CanManageClassrooms => SessionManager.CanManageClassrooms();

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
                _ = LoadChildAttendanceHistoryAsync();
        }
    }

    public DateTime SelectedDate
    {
        get => _selectedDate;
        set
        {
            if (SetProperty(ref _selectedDate, value))
                _ = LoadTodayAttendanceAsync();
        }
    }

    public int? SelectedClassroomId
    {
        get => _selectedClassroomId;
        set
        {
            if (SetProperty(ref _selectedClassroomId, value))
                _ = LoadTodayAttendanceAsync();
        }
    }

    public string SearchQuery
    {
        get => _searchQuery;
        set
        {
            if (SetProperty(ref _searchQuery, value))
                FilterAttendance();
        }
    }

    public int TotalChildren => TodayAttendance.Count;
    public int PresentCount => TodayAttendance.Count(a => a.IsPresent);
    public int AbsentCount => TodayAttendance.Count(a => !a.IsPresent);
    public decimal AttendancePercentage => TotalChildren > 0 ? (decimal)PresentCount / TotalChildren * 100 : 0;

    public ICommand LoadTodayAttendanceCommand { get; }
    public ICommand MarkPresentCommand { get; }
    public ICommand MarkAbsentCommand { get; }
    public ICommand MarkAllPresentCommand { get; }
    public ICommand MarkAllAbsentCommand { get; }
    public ICommand LoadChildrenCommand { get; }
    public ICommand LoadClassroomsCommand { get; }

    public AttendanceViewModel(IAttendanceService attendanceService, IChildService childService, IClassroomService classroomService, INotificationService notificationService)
    {
        _attendanceService = attendanceService;
        _childService = childService;
        _classroomService = classroomService;
        _notificationService = notificationService;

        LoadTodayAttendanceCommand = new AsyncRelayCommand(async _ => await LoadTodayAttendanceAsync());
        MarkPresentCommand = new AsyncRelayCommand(async param => await MarkAttendanceAsync(param, true));
        MarkAbsentCommand = new AsyncRelayCommand(async param => await MarkAttendanceAsync(param, false));
        MarkAllPresentCommand = new AsyncRelayCommand(async _ => await MarkAllPresentAsync());
        MarkAllAbsentCommand = new AsyncRelayCommand(async _ => await MarkAllAbsentAsync());
        LoadChildrenCommand = new AsyncRelayCommand(async _ => await LoadChildrenAsync());
        LoadClassroomsCommand = new AsyncRelayCommand(async _ => await LoadClassroomsAsync());

        _ = LoadClassroomsAsync();
        _ = LoadChildrenAsync();
        _ = LoadTodayAttendanceAsync();
    }

    public async Task LoadTodayAttendanceAsync()
    {
        IsLoading = true;
        try
        {
            var attendances = await _attendanceService.GetTodayAttendanceAsync();
            TodayAttendance.Clear();
            
            foreach (var att in attendances)
                TodayAttendance.Add(att);
            
            FilterAttendance();
            OnPropertyChanged(nameof(TotalChildren));
            OnPropertyChanged(nameof(PresentCount));
            OnPropertyChanged(nameof(AbsentCount));
            OnPropertyChanged(nameof(AttendancePercentage));
        }
        catch (Exception ex)
        {
            _notificationService.ShowError($"Failed to load attendance: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task MarkAttendanceAsync(object? param, bool isPresent)
    {
        if (!(param is AttendanceDto attendance))
            return;

        try
        {
            await _attendanceService.MarkAttendanceAsync(attendance.ChildId, attendance.Date, isPresent);
            await LoadTodayAttendanceAsync();
        }
        catch (Exception ex)
        {
            _notificationService.ShowError($"Failed to mark attendance: {ex.Message}");
        }
    }

    private async Task MarkAllPresentAsync()
    {
        try
        {
            await _attendanceService.MarkAllPresentAsync(SelectedDate);
            _notificationService.ShowSuccess("All children marked as present");
            await LoadTodayAttendanceAsync();
        }
        catch (Exception ex)
        {
            _notificationService.ShowError($"Failed to mark all present: {ex.Message}");
        }
    }

    private async Task MarkAllAbsentAsync()
    {
        try
        {
            await _attendanceService.MarkAllAbsentAsync(SelectedDate);
            _notificationService.ShowSuccess("All children marked as absent");
            await LoadTodayAttendanceAsync();
        }
        catch (Exception ex)
        {
            _notificationService.ShowError($"Failed to mark all absent: {ex.Message}");
        }
    }

    private void FilterAttendance()
    {
        FilteredAttendance.Clear();
        var filtered = TodayAttendance.AsEnumerable();

        if (SelectedClassroomId.HasValue)
        {
            var classroomChildren = Children.Where(c => c.ClassroomId == SelectedClassroomId.Value).Select(c => c.Id);
            filtered = filtered.Where(a => classroomChildren.Contains(a.ChildId));
        }

        if (!string.IsNullOrWhiteSpace(SearchQuery))
        {
            filtered = filtered.Where(a => a.ChildName.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase));
        }

        foreach (var att in filtered)
            FilteredAttendance.Add(att);
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

    private async Task LoadClassroomsAsync()
    {
        try
        {
            var classrooms = await _classroomService.GetAllClassroomsAsync();
            Classrooms.Clear();
            foreach (var classroom in classrooms)
                Classrooms.Add(classroom);
        }
        catch (Exception ex)
        {
            _notificationService.ShowError($"Failed to load classrooms: {ex.Message}");
        }
    }

    private async Task LoadChildAttendanceHistoryAsync()
    {
        if (SelectedChildId == 0)
            return;

        try
        {
            var attendances = await _attendanceService.GetChildAttendanceAsync(SelectedChildId);
            ChildAttendanceHistory.Clear();
            foreach (var att in attendances)
                ChildAttendanceHistory.Add(att);
        }
        catch (Exception ex)
        {
            _notificationService.ShowError($"Failed to load attendance history: {ex.Message}");
        }
    }
}
