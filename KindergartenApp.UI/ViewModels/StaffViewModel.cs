using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using KindergartenApp.Application.DTOs;
using KindergartenApp.Application.Interfaces;
using KindergartenApp.Core.Entities;
using KindergartenApp.UI.Infrastructure;
using KindergartenApp.UI.Models;

namespace KindergartenApp.UI.ViewModels;

public class StaffViewModel : ViewModelBase
{
    private readonly IStaffService _staffService;
    private readonly INotificationService _notificationService;
    private string _searchQuery = string.Empty;
    private StaffDto? _selectedStaff;
    private bool _isLoading;
    private string _fullName = string.Empty;
    private StaffRole _role = StaffRole.Teacher;
    private decimal _salary;
    private string _phone = string.Empty;
    private string _address = string.Empty;
    private DateTime _hireDate = DateTime.Today;
    private StaffStatus _status = StaffStatus.Active;
    private string _notes = string.Empty;
    private bool _isEditMode;

    public ObservableCollection<StaffDto> Staff { get; } = new();
    public ObservableCollection<StaffDto> FilteredStaff { get; } = new();

    public bool CanManageStaff => SessionManager.CanManageStaff();

    public string SearchQuery
    {
        get => _searchQuery;
        set
        {
            if (SetProperty(ref _searchQuery, value))
                FilterStaff();
        }
    }

    public StaffDto? SelectedStaff
    {
        get => _selectedStaff;
        set => SetProperty(ref _selectedStaff, value);
    }

    public bool IsLoading
    {
        get => _isLoading;
        set => SetProperty(ref _isLoading, value);
    }

    public string FullName
    {
        get => _fullName;
        set => SetProperty(ref _fullName, value);
    }

    public StaffRole Role
    {
        get => _role;
        set => SetProperty(ref _role, value);
    }

    public decimal Salary
    {
        get => _salary;
        set => SetProperty(ref _salary, value);
    }

    public string Phone
    {
        get => _phone;
        set => SetProperty(ref _phone, value);
    }

    public string Address
    {
        get => _address;
        set => SetProperty(ref _address, value);
    }

    public DateTime HireDate
    {
        get => _hireDate;
        set => SetProperty(ref _hireDate, value);
    }

    public StaffStatus Status
    {
        get => _status;
        set => SetProperty(ref _status, value);
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

    public ICommand LoadStaffCommand { get; }
    public ICommand AddStaffCommand { get; }
    public ICommand UpdateStaffCommand { get; }
    public ICommand DeleteStaffCommand { get; }
    public ICommand EditStaffCommand { get; }
    public ICommand CancelEditCommand { get; }

    public StaffViewModel(IStaffService staffService, INotificationService notificationService)
    {
        _staffService = staffService;
        _notificationService = notificationService;

        LoadStaffCommand = new AsyncRelayCommand(async _ => await LoadStaffAsync());
        AddStaffCommand = new AsyncRelayCommand(async _ => await AddStaffAsync());
        UpdateStaffCommand = new AsyncRelayCommand(async _ => await UpdateStaffAsync());
        DeleteStaffCommand = new AsyncRelayCommand(async _ => await DeleteStaffAsync(), _ => SelectedStaff != null);
        EditStaffCommand = new RelayCommand(_ => EditStaff(), _ => SelectedStaff != null);
        CancelEditCommand = new RelayCommand(_ => CancelEdit());

        _ = LoadStaffAsync();
    }

    public async Task LoadStaffAsync()
    {
        IsLoading = true;
        try
        {
            var staff = await _staffService.GetAllStaffAsync();
            Staff.Clear();
            foreach (var s in staff)
                Staff.Add(s);
            FilterStaff();
        }
        catch (Exception ex)
        {
            _notificationService.ShowError($"Failed to load staff: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task AddStaffAsync()
    {
        if (string.IsNullOrWhiteSpace(FullName))
        {
            _notificationService.ShowError("Please enter full name");
            return;
        }

        if (string.IsNullOrWhiteSpace(Phone))
        {
            _notificationService.ShowError("Please enter phone number");
            return;
        }

        if (Salary <= 0)
        {
            _notificationService.ShowError("Salary must be greater than 0");
            return;
        }

        try
        {
            var dto = new StaffDto
            {
                FullName = FullName,
                Role = Role,
                Salary = Salary,
                Phone = Phone,
                Address = Address,
                HireDate = HireDate,
                Status = Status,
                Notes = Notes
            };

            await _staffService.CreateStaffAsync(dto);
            _notificationService.ShowSuccess("Staff member added successfully");
            ClearForm();
            await LoadStaffAsync();
        }
        catch (Exception ex)
        {
            _notificationService.ShowError($"Failed to add staff: {ex.Message}");
        }
    }

    private async Task UpdateStaffAsync()
    {
        if (SelectedStaff == null)
        {
            _notificationService.ShowError("Please select a staff member to update");
            return;
        }

        try
        {
            var dto = new StaffDto
            {
                Id = SelectedStaff.Id,
                FullName = FullName,
                Role = Role,
                Salary = Salary,
                Phone = Phone,
                Address = Address,
                HireDate = HireDate,
                Status = Status,
                Notes = Notes
            };

            await _staffService.UpdateStaffAsync(dto);
            _notificationService.ShowSuccess("Staff member updated successfully");
            ClearForm();
            IsEditMode = false;
            await LoadStaffAsync();
        }
        catch (Exception ex)
        {
            _notificationService.ShowError($"Failed to update staff: {ex.Message}");
        }
    }

    private async Task DeleteStaffAsync()
    {
        if (SelectedStaff == null)
            return;

        if (!_notificationService.ShowConfirmation($"Delete {SelectedStaff.FullName}?"))
            return;

        try
        {
            await _staffService.DeleteStaffAsync(SelectedStaff.Id);
            _notificationService.ShowSuccess("Staff member deleted successfully");
            await LoadStaffAsync();
        }
        catch (Exception ex)
        {
            _notificationService.ShowError($"Failed to delete staff: {ex.Message}");
        }
    }

    private void EditStaff()
    {
        if (SelectedStaff == null)
            return;

        IsEditMode = true;
        FullName = SelectedStaff.FullName;
        Role = SelectedStaff.Role;
        Salary = SelectedStaff.Salary;
        Phone = SelectedStaff.Phone;
        Address = SelectedStaff.Address;
        HireDate = SelectedStaff.HireDate;
        Status = SelectedStaff.Status;
        Notes = SelectedStaff.Notes ?? string.Empty;
    }

    private void CancelEdit()
    {
        IsEditMode = false;
        ClearForm();
    }

    private void ClearForm()
    {
        FullName = string.Empty;
        Role = StaffRole.Teacher;
        Salary = 0;
        Phone = string.Empty;
        Address = string.Empty;
        HireDate = DateTime.Today;
        Status = StaffStatus.Active;
        Notes = string.Empty;
    }

    private void FilterStaff()
    {
        FilteredStaff.Clear();
        var filtered = string.IsNullOrWhiteSpace(SearchQuery)
            ? Staff
            : Staff.Where(s => s.FullName.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase) ||
                                  s.Phone.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase) ||
                                  s.Role.ToString().Contains(SearchQuery, StringComparison.OrdinalIgnoreCase));

        foreach (var s in filtered)
            FilteredStaff.Add(s);
    }
}
