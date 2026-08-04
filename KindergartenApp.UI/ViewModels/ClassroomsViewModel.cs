using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using KindergartenApp.Application.DTOs;
using KindergartenApp.Application.Interfaces;
using KindergartenApp.Core.Entities;
using KindergartenApp.UI.Infrastructure;
using KindergartenApp.UI.Models;

namespace KindergartenApp.UI.ViewModels;

public class ClassroomsViewModel : ViewModelBase
{
    private readonly IClassroomService _classroomService;
    private readonly IStaffService _staffService;
    private readonly INotificationService _notificationService;
    private string _searchQuery = string.Empty;
    private ClassroomDto? _selectedClassroom;
    private bool _isLoading;
    private string _name = string.Empty;
    private ClassroomStage _stage = ClassroomStage.Nursery;
    private int _capacity = 20;
    private int? _teacherId;
    private string _notes = string.Empty;
    private bool _isEditMode;

    public ObservableCollection<ClassroomDto> Classrooms { get; } = new();
    public ObservableCollection<ClassroomDto> FilteredClassrooms { get; } = new();
    public ObservableCollection<StaffDto> Teachers { get; } = new();

    public bool CanManageClassrooms => SessionManager.CanManageClassrooms();

    public string SearchQuery
    {
        get => _searchQuery;
        set
        {
            if (SetProperty(ref _searchQuery, value))
                FilterClassrooms();
        }
    }

    public ClassroomDto? SelectedClassroom
    {
        get => _selectedClassroom;
        set => SetProperty(ref _selectedClassroom, value);
    }

    public bool IsLoading
    {
        get => _isLoading;
        set => SetProperty(ref _isLoading, value);
    }

    public string Name
    {
        get => _name;
        set => SetProperty(ref _name, value);
    }

    public ClassroomStage Stage
    {
        get => _stage;
        set => SetProperty(ref _stage, value);
    }

    public int Capacity
    {
        get => _capacity;
        set => SetProperty(ref _capacity, value);
    }

    public int? TeacherId
    {
        get => _teacherId;
        set => SetProperty(ref _teacherId, value);
    }

    public string Notes
    {
        get => _notes;
        set => SetProperty(ref _notes, value);
    }

    public bool IsEditMode
    {
        get => _isEditMode;
        set => SetProperty(ref _isEditMode, value);
    }

    public ICommand LoadClassroomsCommand { get; }
    public ICommand AddClassroomCommand { get; }
    public ICommand UpdateClassroomCommand { get; }
    public ICommand DeleteClassroomCommand { get; }
    public ICommand EditClassroomCommand { get; }
    public ICommand CancelEditCommand { get; }

    public ClassroomsViewModel(IClassroomService classroomService, IStaffService staffService, INotificationService notificationService)
    {
        _classroomService = classroomService;
        _staffService = staffService;
        _notificationService = notificationService;

        LoadClassroomsCommand = new AsyncRelayCommand(async _ => await LoadClassroomsAsync());
        AddClassroomCommand = new AsyncRelayCommand(async _ => await AddClassroomAsync());
        UpdateClassroomCommand = new AsyncRelayCommand(async _ => await UpdateClassroomAsync());
        DeleteClassroomCommand = new AsyncRelayCommand(async _ => await DeleteClassroomAsync(), _ => SelectedClassroom != null);
        EditClassroomCommand = new RelayCommand(_ => EditClassroom(), _ => SelectedClassroom != null);
        CancelEditCommand = new RelayCommand(_ => CancelEdit());

        _ = LoadClassroomsAsync();
        _ = LoadTeachersAsync();
    }

    public async Task LoadClassroomsAsync()
    {
        IsLoading = true;
        try
        {
            var classrooms = await _classroomService.GetAllClassroomsAsync();
            Classrooms.Clear();
            foreach (var classroom in classrooms)
                Classrooms.Add(classroom);
            FilterClassrooms();
        }
        catch (Exception ex)
        {
            _notificationService.ShowError($"Failed to load classrooms: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task LoadTeachersAsync()
    {
        try
        {
            var teachers = await _staffService.GetStaffByRoleAsync(StaffRole.Teacher);
            Teachers.Clear();
            foreach (var teacher in teachers)
                Teachers.Add(teacher);
        }
        catch (Exception ex)
        {
            _notificationService.ShowError($"Failed to load teachers: {ex.Message}");
        }
    }

    private async Task AddClassroomAsync()
    {
        if (string.IsNullOrWhiteSpace(Name))
        {
            _notificationService.ShowError("Please enter classroom name");
            return;
        }

        if (Capacity <= 0)
        {
            _notificationService.ShowError("Capacity must be greater than 0");
            return;
        }

        try
        {
            var dto = new ClassroomDto
            {
                Name = Name,
                Stage = Stage,
                Capacity = Capacity,
                TeacherId = TeacherId,
                Notes = Notes
            };

            await _classroomService.CreateClassroomAsync(dto);
            _notificationService.ShowSuccess("Classroom added successfully");
            ClearForm();
            await LoadClassroomsAsync();
        }
        catch (Exception ex)
        {
            _notificationService.ShowError($"Failed to add classroom: {ex.Message}");
        }
    }

    private async Task UpdateClassroomAsync()
    {
        if (SelectedClassroom == null)
        {
            _notificationService.ShowError("Please select a classroom to update");
            return;
        }

        try
        {
            var dto = new ClassroomDto
            {
                Id = SelectedClassroom.Id,
                Name = Name,
                Stage = Stage,
                Capacity = Capacity,
                TeacherId = TeacherId,
                Notes = Notes
            };

            await _classroomService.UpdateClassroomAsync(dto);
            _notificationService.ShowSuccess("Classroom updated successfully");
            ClearForm();
            IsEditMode = false;
            await LoadClassroomsAsync();
        }
        catch (Exception ex)
        {
            _notificationService.ShowError($"Failed to update classroom: {ex.Message}");
        }
    }

    private async Task DeleteClassroomAsync()
    {
        if (SelectedClassroom == null)
            return;

        if (!_notificationService.ShowConfirmation($"Delete {SelectedClassroom.Name}?"))
            return;

        try
        {
            await _classroomService.DeleteClassroomAsync(SelectedClassroom.Id);
            _notificationService.ShowSuccess("Classroom deleted successfully");
            await LoadClassroomsAsync();
        }
        catch (Exception ex)
        {
            _notificationService.ShowError($"Failed to delete classroom: {ex.Message}");
        }
    }

    private void EditClassroom()
    {
        if (SelectedClassroom == null)
            return;

        IsEditMode = true;
        Name = SelectedClassroom.Name;
        Stage = SelectedClassroom.Stage;
        Capacity = SelectedClassroom.Capacity;
        TeacherId = SelectedClassroom.TeacherId;
        Notes = SelectedClassroom.Notes ?? string.Empty;
    }

    private void CancelEdit()
    {
        IsEditMode = false;
        ClearForm();
    }

    private void ClearForm()
    {
        Name = string.Empty;
        Stage = ClassroomStage.Nursery;
        Capacity = 20;
        TeacherId = null;
        Notes = string.Empty;
    }

    private void FilterClassrooms()
    {
        FilteredClassrooms.Clear();
        var filtered = string.IsNullOrWhiteSpace(SearchQuery)
            ? Classrooms
            : Classrooms.Where(c => c.Name.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase) ||
                                  c.Stage.ToString().Contains(SearchQuery, StringComparison.OrdinalIgnoreCase));

        foreach (var classroom in filtered)
            FilteredClassrooms.Add(classroom);
    }
}
