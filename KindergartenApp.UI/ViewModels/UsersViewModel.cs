using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using KindergartenApp.Application.DTOs;
using KindergartenApp.Application.Interfaces;
using KindergartenApp.Core.Entities;
using KindergartenApp.UI.Infrastructure;
using KindergartenApp.UI.Models;

namespace KindergartenApp.UI.ViewModels;

public class UsersViewModel : ViewModelBase
{
    private readonly IUserService _userService;
    private readonly IStaffService _staffService;
    private readonly INotificationService _notificationService;
    private string _searchQuery = string.Empty;
    private UserDto? _selectedUser;
    private bool _isLoading;
    private string _username = string.Empty;
    private string _email = string.Empty;
    private string _fullName = string.Empty;
    private UserRole _role = UserRole.Teacher;
    private bool _isActive = true;
    private int? _staffId;
    private string _password = string.Empty;
    private string _confirmPassword = string.Empty;
    private bool _isEditMode;

    public ObservableCollection<UserDto> Users { get; } = new();
    public ObservableCollection<UserDto> FilteredUsers { get; } = new();
    public ObservableCollection<StaffDto> Staff { get; } = new();

    public bool CanManageUsers => SessionManager.CanManageUsers();
    public bool IsAdmin => SessionManager.IsAdmin();

    public string SearchQuery
    {
        get => _searchQuery;
        set
        {
            if (SetProperty(ref _searchQuery, value))
                FilterUsers();
        }
    }

    public UserDto? SelectedUser
    {
        get => _selectedUser;
        set => SetProperty(ref _selectedUser, value);
    }

    public bool IsLoading
    {
        get => _isLoading;
        set => SetProperty(ref _isLoading, value);
    }

    public string Username
    {
        get => _username;
        set => SetProperty(ref _username, value);
    }

    public string Email
    {
        get => _email;
        set => SetProperty(ref _email, value);
    }

    public string FullName
    {
        get => _fullName;
        set => SetProperty(ref _fullName, value);
    }

    public UserRole Role
    {
        get => _role;
        set => SetProperty(ref _role, value);
    }

    public bool IsActive
    {
        get => _isActive;
        set => SetProperty(ref _isActive, value);
    }

    public int? StaffId
    {
        get => _staffId;
        set => SetProperty(ref _staffId, value);
    }

    public string Password
    {
        get => _password;
        set => SetProperty(ref _password, value);
    }

    public string ConfirmPassword
    {
        get => _confirmPassword;
        set => SetProperty(ref _confirmPassword, value);
    }

    public bool IsEditMode
    {
        get => _isEditMode;
        set => SetProperty(ref _isEditMode, value);
    }

    public ICommand LoadUsersCommand { get; }
    public ICommand AddUserCommand { get; }
    public ICommand UpdateUserCommand { get; }
    public ICommand DeleteUserCommand { get; }
    public ICommand EditUserCommand { get; }
    public ICommand CancelEditCommand { get; }

    public UsersViewModel(IUserService userService, IStaffService staffService, INotificationService notificationService)
    {
        _userService = userService;
        _staffService = staffService;
        _notificationService = notificationService;

        LoadUsersCommand = new AsyncRelayCommand(async _ => await LoadUsersAsync());
        AddUserCommand = new AsyncRelayCommand(async _ => await AddUserAsync());
        UpdateUserCommand = new AsyncRelayCommand(async _ => await UpdateUserAsync());
        DeleteUserCommand = new AsyncRelayCommand(async param => await DeleteUserAsync(param), _ => SelectedUser != null);
        EditUserCommand = new RelayCommand(param => EditUser(param), _ => SelectedUser != null);
        CancelEditCommand = new RelayCommand(_ => CancelEdit());

        _ = LoadUsersAsync();
        _ = LoadStaffAsync();
    }

    public async Task LoadUsersAsync()
    {
        IsLoading = true;
        try
        {
            var users = await _userService.GetAllUsersAsync();
            Users.Clear();
            foreach (var user in users)
                Users.Add(user);
            FilterUsers();
        }
        catch (Exception ex)
        {
            _notificationService.ShowError($"Failed to load users: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task LoadStaffAsync()
    {
        try
        {
            var staff = await _staffService.GetAllStaffAsync();
            Staff.Clear();
            foreach (var s in staff)
                Staff.Add(s);
        }
        catch (Exception ex)
        {
            _notificationService.ShowError($"Failed to load staff: {ex.Message}");
        }
    }

    private async Task AddUserAsync()
    {
        if (string.IsNullOrWhiteSpace(Username))
        {
            _notificationService.ShowError("Please enter username");
            return;
        }

        if (string.IsNullOrWhiteSpace(Password))
        {
            _notificationService.ShowError("Please enter password");
            return;
        }

        if (Password != ConfirmPassword)
        {
            _notificationService.ShowError("Passwords do not match");
            return;
        }

        if (Password.Length < 6)
        {
            _notificationService.ShowError("Password must be at least 6 characters");
            return;
        }

        try
        {
            var dto = new UserDto
            {
                Username = Username,
                Email = Email,
                FullName = FullName,
                Role = Role,
                IsActive = IsActive,
                StaffId = StaffId
            };

            await _userService.CreateUserAsync(dto, Password);
            _notificationService.ShowSuccess("User added successfully");
            ClearForm();
            await LoadUsersAsync();
        }
        catch (Exception ex)
        {
            _notificationService.ShowError($"Failed to add user: {ex.Message}");
        }
    }

    private async Task UpdateUserAsync()
    {
        if (SelectedUser == null)
        {
            _notificationService.ShowError("Please select a user to update");
            return;
        }

        try
        {
            var dto = new UserDto
            {
                Id = SelectedUser.Id,
                Username = Username,
                Email = Email,
                FullName = FullName,
                Role = Role,
                IsActive = IsActive,
                StaffId = StaffId
            };

            await _userService.UpdateUserAsync(dto);
            _notificationService.ShowSuccess("User updated successfully");
            ClearForm();
            IsEditMode = false;
            await LoadUsersAsync();
        }
        catch (Exception ex)
        {
            _notificationService.ShowError($"Failed to update user: {ex.Message}");
        }
    }

    private async Task DeleteUserAsync(object? param)
    {
        var userToDelete = param as UserDto ?? SelectedUser;
        if (userToDelete == null)
            return;

        if (!_notificationService.ShowConfirmation($"Delete user {userToDelete.Username}?"))
            return;

        try
        {
            await _userService.DeleteUserAsync(userToDelete.Id);
            _notificationService.ShowSuccess("User deleted successfully");
            await LoadUsersAsync();
        }
        catch (Exception ex)
        {
            _notificationService.ShowError($"Failed to delete user: {ex.Message}");
        }
    }

    private void EditUser(object? param)
    {
        var userToEdit = param as UserDto ?? SelectedUser;
        if (userToEdit == null)
            return;

        SelectedUser = userToEdit;
        IsEditMode = true;
        Username = SelectedUser.Username;
        Email = SelectedUser.Email;
        FullName = SelectedUser.FullName;
        Role = SelectedUser.Role;
        IsActive = SelectedUser.IsActive;
        StaffId = SelectedUser.StaffId;
        Password = string.Empty;
        ConfirmPassword = string.Empty;
    }

    private void CancelEdit()
    {
        IsEditMode = false;
        ClearForm();
    }

    private void ClearForm()
    {
        Username = string.Empty;
        Email = string.Empty;
        FullName = string.Empty;
        Role = UserRole.Teacher;
        IsActive = true;
        StaffId = null;
        Password = string.Empty;
        ConfirmPassword = string.Empty;
    }

    private void FilterUsers()
    {
        FilteredUsers.Clear();
        var filtered = string.IsNullOrWhiteSpace(SearchQuery)
            ? Users
            : Users.Where(u => u.Username.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase) ||
                                  u.FullName.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase) ||
                                  u.Email.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase));

        foreach (var user in filtered)
            FilteredUsers.Add(user);
    }
}
