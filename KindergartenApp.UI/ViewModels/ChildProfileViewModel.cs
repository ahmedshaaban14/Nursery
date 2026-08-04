using System.Collections.ObjectModel;
using System.Windows.Input;
using KindergartenApp.Application.DTOs;
using KindergartenApp.Application.Interfaces;
using KindergartenApp.UI.Infrastructure;

namespace KindergartenApp.UI.ViewModels;

public class ChildProfileViewModel : ViewModelBase
{
    private readonly IChildService _childService;
    private readonly IDocumentService _documentService;
    private readonly IAttendanceService _attendanceService;
    private readonly IPaymentService _paymentService;
    private readonly INotificationService _notificationService;
    private bool _isLoading;
    private ChildDto? _selectedChild;
    private byte[]? _photoData;
    private string _photoPath = string.Empty;

    public ObservableCollection<DocumentDto> Documents { get; } = new();
    public ObservableCollection<AttendanceDto> AttendanceHistory { get; } = new();
    public ObservableCollection<PaymentDto> PaymentHistory { get; } = new();
    public ObservableCollection<ChildDto> Children { get; } = new();

    public bool IsLoading
    {
        get => _isLoading;
        set => SetProperty(ref _isLoading, value);
    }

    public ChildDto? SelectedChild
    {
        get => _selectedChild;
        set
        {
            if (SetProperty(ref _selectedChild, value) && value != null)
                _ = LoadChildDataAsync(value.Id);
        }
    }

    public byte[]? PhotoData
    {
        get => _photoData;
        set => SetProperty(ref _photoData, value);
    }

    public string PhotoPath
    {
        get => _photoPath;
        set => SetProperty(ref _photoPath, value);
    }

    public decimal AttendancePercentage => AttendanceHistory.Count > 0 
        ? (decimal)AttendanceHistory.Count(a => a.IsPresent) / AttendanceHistory.Count * 100 
        : 0;

    public decimal TotalPaid => PaymentHistory.Where(p => p.IsPaid).Sum(p => p.Amount);
    public decimal TotalOutstanding => PaymentHistory.Where(p => !p.IsPaid).Sum(p => p.TotalAmount);

    public ICommand LoadChildrenCommand { get; }
    public ICommand UploadPhotoCommand { get; }
    public ICommand UploadDocumentCommand { get; }
    public ICommand DeleteDocumentCommand { get; }
    public ICommand DownloadDocumentCommand { get; }

    public ChildProfileViewModel(IChildService childService, IDocumentService documentService, IAttendanceService attendanceService, IPaymentService paymentService, INotificationService notificationService)
    {
        _childService = childService;
        _documentService = documentService;
        _attendanceService = attendanceService;
        _paymentService = paymentService;
        _notificationService = notificationService;

        LoadChildrenCommand = new AsyncRelayCommand(async _ => await LoadChildrenAsync());
        UploadPhotoCommand = new AsyncRelayCommand(async _ => await UploadPhotoAsync());
        UploadDocumentCommand = new AsyncRelayCommand(async _ => await UploadDocumentAsync());
        DeleteDocumentCommand = new AsyncRelayCommand(async param => await DeleteDocumentAsync(param));
        DownloadDocumentCommand = new AsyncRelayCommand(async param => await DownloadDocumentAsync(param));

        _ = LoadChildrenAsync();
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

    public async Task LoadChildDataAsync(int childId)
    {
        IsLoading = true;
        try
        {
            var documents = await _documentService.GetDocumentsByChildAsync(childId);
            Documents.Clear();
            foreach (var doc in documents)
                Documents.Add(doc);

            var attendances = await _attendanceService.GetChildAttendanceAsync(childId);
            AttendanceHistory.Clear();
            foreach (var att in attendances)
                AttendanceHistory.Add(att);

            var payments = await _paymentService.GetChildPaymentsAsync(childId);
            PaymentHistory.Clear();
            foreach (var pay in payments)
                PaymentHistory.Add(pay);

            OnPropertyChanged(nameof(AttendancePercentage));
            OnPropertyChanged(nameof(TotalPaid));
            OnPropertyChanged(nameof(TotalOutstanding));
        }
        catch (Exception ex)
        {
            _notificationService.ShowError($"Failed to load child data: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task UploadPhotoAsync()
    {
        if (SelectedChild == null)
        {
            _notificationService.ShowError("Please select a child first");
            return;
        }

        // In a real implementation, this would open a file dialog
        _notificationService.ShowSuccess("Photo upload functionality requires file dialog implementation");
    }

    private async Task UploadDocumentAsync()
    {
        if (SelectedChild == null)
        {
            _notificationService.ShowError("Please select a child first");
            return;
        }

        // In a real implementation, this would open a file dialog
        _notificationService.ShowSuccess("Document upload functionality requires file dialog implementation");
    }

    private async Task DeleteDocumentAsync(object? param)
    {
        if (!(param is DocumentDto document))
            return;

        if (!_notificationService.ShowConfirmation($"Delete document {document.FileName}?"))
            return;

        try
        {
            await _documentService.DeleteDocumentAsync(document.Id);
            _notificationService.ShowSuccess("Document deleted successfully");
            if (SelectedChild != null)
                await LoadChildDataAsync(SelectedChild.Id);
        }
        catch (Exception ex)
        {
            _notificationService.ShowError($"Failed to delete document: {ex.Message}");
        }
    }

    private async Task DownloadDocumentAsync(object? param)
    {
        if (!(param is DocumentDto document))
            return;

        try
        {
            var data = await _documentService.DownloadDocumentAsync(document.Id);
            if (data != null)
            {
                _notificationService.ShowSuccess($"Document downloaded: {document.FileName}");
            }
        }
        catch (Exception ex)
        {
            _notificationService.ShowError($"Failed to download document: {ex.Message}");
        }
    }
}