using System;
using System.Windows;
using KindergartenApp.UI.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace KindergartenApp.UI.Views;

public partial class LoginWindow : Window
{
    private LoginViewModel? _vm;

    public LoginWindow()
    {
        InitializeComponent();
        DataContextChanged += (_, _) => AttachToViewModel();
    }

    protected override void OnInitialized(EventArgs e)
    {
        base.OnInitialized(e);

        PasswordBox.KeyDown += (s, ke) =>
        {
            if (ke.Key == System.Windows.Input.Key.Return)
            {
                LoginButton_Click(null, null);
                ke.Handled = true;
            }
        };

        AttachToViewModel();
    }

    private void AttachToViewModel()
    {
        if (_vm != null)
            _vm.LoginResult -= OnLoginResult;

        _vm = DataContext as LoginViewModel;
        if (_vm != null)
            _vm.LoginResult += OnLoginResult;
    }

    private void OnLoginResult(bool success)
    {
        if (!success)
            return;

        var mainWindow = App.Services.GetRequiredService<MainWindow>();
        mainWindow.Show();
        Close();
    }

    private void LoginButton_Click(object? sender, RoutedEventArgs? e)
    {
        if (DataContext is LoginViewModel vm)
        {
            var password = PasswordBox.Password;
            if (vm.LoginCommand.CanExecute(password))
            {
                vm.LoginCommand.Execute(password);
            }
        }
    }
}
