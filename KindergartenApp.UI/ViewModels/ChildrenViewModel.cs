using System.Collections.ObjectModel;
using System.Windows.Input;
using KindergartenApp.Application.DTOs;
using KindergartenApp.Application.Interfaces;
using KindergartenApp.UI.Infrastructure;

namespace KindergartenApp.UI.ViewModels;

public class ChildrenViewModel : ViewModelBase
{
    private readonly IChildService _childService;
    private readonly IClassroomService _classroomService;
    private readonly INotificationService _notificationService;
    private string _searchQuery = string.Empty;
    private ChildDto? _selectedChild;
    private bool _isLoading;
    private string _fullName = string.Empty;
    private int _age;
    private string _gender = "Male";
    private string _parentName = string.Empty;
    private string _phone = string.Empty;
    private string _address = string.Empty;
    private string _notes = string.Empty;
    private decimal _monthlyFee;
    private int? _classroomId;
    private bool _isEditMode;

    public ObservableCollection<ChildDto> Children { get; } = new();
    public ObservableCollection<ChildDto> FilteredChildren { get; } = new();
    public ObservableCollection<ClassroomDto> Classrooms { get; } = new();

    public string SearchQuery
    {
        get => _searchQuery;
        set
        {
            if (SetProperty(ref _searchQuery, value))
                FilterChildren();
        }
    }

    public ChildDto? SelectedChild
    {
        get => _selectedChild;
        set => SetProperty(ref _selectedChild, value);
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

    public int Age
    {
        get => _age;
        set => SetProperty(ref _age, value);
    }

    public string Gender
    {
        get => _gender;
        set => SetProperty(ref _gender, value);
    }

    public string ParentName
    {
        get => _parentName;
        set => SetProperty(ref _parentName, value);
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

    public string Notes
    {
        get => _notes;
        set => SetProperty(ref _notes, value);
    }

    public decimal MonthlyFee
    {
        get => _monthlyFee;
        set => SetProperty(ref _monthlyFee, value);
    }

    public int? ClassroomId
    {
        get => _classroomId;
        set => SetProperty(ref _classroomId, value);
    }

    public bool IsEditMode
    {
        get => _isEditMode;
        set => SetProperty(ref _isEditMode, value);
    }

    public ICommand LoadChildrenCommand { get; }
    public ICommand AddChildCommand { get; }
    public ICommand UpdateChildCommand { get; }
    public ICommand DeleteChildCommand { get; }
    public ICommand EditChildCommand { get; }
    public ICommand CancelEditCommand { get; }

    public ChildrenViewModel(IChildService childService, IClassroomService classroomService, INotificationService notificationService)
    {
        _childService = childService;
        _classroomService = classroomService;
        _notificationService = notificationService;

        LoadChildrenCommand = new AsyncRelayCommand(async _ => await LoadChildrenAsync());
        AddChildCommand = new AsyncRelayCommand(async _ => await AddChildAsync());
        UpdateChildCommand = new AsyncRelayCommand(async _ => await UpdateChildAsync());
        DeleteChildCommand = new AsyncRelayCommand(async param => await DeleteChildAsync(param), _ => SelectedChild != null);
        EditChildCommand = new RelayCommand(param => EditChild(param), _ => SelectedChild != null);
        CancelEditCommand = new RelayCommand(_ => CancelEdit());

        _ = LoadChildrenAsync();
        _ = LoadClassroomsAsync();
    }

    public async Task LoadChildrenAsync()
    {
        IsLoading = true;
        try
        {
            var children = await _childService.GetAllChildrenAsync();
            Children.Clear();
            foreach (var child in children)
                Children.Add(child);
            FilterChildren();
        }
        catch (Exception ex)
        {
            _notificationService.ShowError($"Failed to load children: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task AddChildAsync()
    {
        if (string.IsNullOrWhiteSpace(FullName))
        {
            _notificationService.ShowError("Please enter child's full name");
            return;
        }

        try
        {
            var dto = new ChildDto
            {
                FullName = FullName,
                Age = Age,
                Gender = Gender,
                ParentName = ParentName,
                Phone = Phone,
                Address = Address,
                Notes = Notes,
                MonthlyFee = MonthlyFee,
                ClassroomId = ClassroomId
            };

            await _childService.CreateChildAsync(dto);
            _notificationService.ShowSuccess("Child added successfully");
            ClearForm();
            await LoadChildrenAsync();
        }
        catch (Exception ex)
        {
            _notificationService.ShowError($"Failed to add child: {ex.Message}");
        }
    }

    private async Task UpdateChildAsync()
    {
        if (SelectedChild == null)
        {
            _notificationService.ShowError("Please select a child to update");
            return;
        }

        try
        {
            var dto = new ChildDto
            {
                Id = SelectedChild.Id,
                FullName = FullName,
                Age = Age,
                Gender = Gender,
                ParentName = ParentName,
                Phone = Phone,
                Address = Address,
                Notes = Notes,
                MonthlyFee = MonthlyFee,
                ClassroomId = ClassroomId
            };

            await _childService.UpdateChildAsync(dto);
            _notificationService.ShowSuccess("Child updated successfully");
            ClearForm();
            IsEditMode = false;
            await LoadChildrenAsync();
        }
        catch (Exception ex)
        {
            _notificationService.ShowError($"Failed to update child: {ex.Message}");
        }
    }

    private async Task DeleteChildAsync(object? param)
    {
        var childToDelete = param as ChildDto ?? SelectedChild;
        if (childToDelete == null)
            return;

        if (!_notificationService.ShowConfirmation($"Delete {childToDelete.FullName}?"))
            return;

        try
        {
            await _childService.DeleteChildAsync(childToDelete.Id);
            _notificationService.ShowSuccess("Child deleted successfully");
            await LoadChildrenAsync();
        }
        catch (Exception ex)
        {
            _notificationService.ShowError($"Failed to delete child: {ex.Message}");
        }
    }

    private void EditChild(object? param)
    {
        var childToEdit = param as ChildDto ?? SelectedChild;
        if (childToEdit == null)
            return;

        SelectedChild = childToEdit;
        IsEditMode = true;
        FullName = SelectedChild.FullName;
        Age = SelectedChild.Age;
        Gender = SelectedChild.Gender;
        ParentName = SelectedChild.ParentName;
        Phone = SelectedChild.Phone;
        Address = SelectedChild.Address;
        Notes = SelectedChild.Notes ?? string.Empty;
        MonthlyFee = SelectedChild.MonthlyFee;
        ClassroomId = SelectedChild.ClassroomId;
    }

    private void CancelEdit()
    {
        IsEditMode = false;
        ClearForm();
    }

    private void ClearForm()
    {
        FullName = string.Empty;
        Age = 0;
        Gender = "Male";
        ParentName = string.Empty;
        Phone = string.Empty;
        Address = string.Empty;
        Notes = string.Empty;
        MonthlyFee = 0;
        ClassroomId = null;
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

    private void FilterChildren()
    {
        FilteredChildren.Clear();
        var filtered = string.IsNullOrWhiteSpace(SearchQuery)
            ? Children
            : Children.Where(c => c.FullName.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase) ||
                                  c.ParentName.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase) ||
                                  c.Phone.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase));

        foreach (var child in filtered)
            FilteredChildren.Add(child);
    }
}
