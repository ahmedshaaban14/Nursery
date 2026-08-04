using System.Windows.Input;
using KindergartenApp.Application.Interfaces;
using KindergartenApp.UI.Infrastructure;
using KindergartenApp.UI.Models;
using KindergartenApp.Core.Entities;

namespace KindergartenApp.UI.ViewModels;

public class LoginViewModel : ViewModelBase
{
    private readonly IAuthenticationService _authService;
    private readonly IUserService _userService;
    private readonly INotificationService _notificationService;
    private string _username = string.Empty;
    private string _password = string.Empty;
    private bool _isLoading;

    public string Username
    {
        get => _username;
        set => SetProperty(ref _username, value);
    }

    public string Password
    {
        get => _password;
        set => SetProperty(ref _password, value);
    }

    public bool IsLoading
    {
        get => _isLoading;
        set => SetProperty(ref _isLoading, value);
    }

    public ICommand LoginCommand { get; }
    public event Action<bool>? LoginResult;

    public LoginViewModel(IAuthenticationService authService, IUserService userService, INotificationService notificationService)
    {
        _authService = authService;
        _userService = userService;
        _notificationService = notificationService;
        LoginCommand = new AsyncRelayCommand(async param => await LoginAsync(param as string));
    }

    private async Task LoginAsync(string? password)
    {
        var pwd = password ?? Password;

        if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(pwd))
        {
            _notificationService.ShowError("Please enter username and password");
            return;
        }

        IsLoading = true;
        try
        {
            var authenticated = await _authService.AuthenticateAsync(Username, pwd);
            if (authenticated)
            {
                var user = await _userService.GetUserByUsernameAsync(Username);
                if (user != null)
                {
                    SessionManager.Login(user.Id, user.Username, user.FullName ?? string.Empty, user.Role);
                    await _userService.UpdateLastLoginAsync(user.Id);
                    LoginResult?.Invoke(true);
                }
                else
                {
                    _notificationService.ShowError("User not found");
                }
            }
            else
            {
                _notificationService.ShowError("Invalid username or password");
            }
        }
        catch (Exception ex)
        {
            _notificationService.ShowError($"Login failed: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }
}
