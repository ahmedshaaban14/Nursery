using System.Windows;
using System.Windows.Controls;
using KindergartenApp.UI.Views;
using KindergartenApp.UI.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace KindergartenApp.UI;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        ShowDashboard();
    }

    private void DashboardBtn_Click(object sender, RoutedEventArgs e) => ShowDashboard();
    private void ChildrenBtn_Click(object sender, RoutedEventArgs e) => ShowChildren();
    private void ClassroomsBtn_Click(object sender, RoutedEventArgs e) => ShowClassrooms();
    private void StaffBtn_Click(object sender, RoutedEventArgs e) => ShowStaff();
    private void UsersBtn_Click(object sender, RoutedEventArgs e) => ShowUsers();
    private void ChildProfileBtn_Click(object sender, RoutedEventArgs e) => ShowChildProfile();
    private void ReportsBtn_Click(object sender, RoutedEventArgs e) => ShowReports();
    private void AttendanceBtn_Click(object sender, RoutedEventArgs e) => ShowAttendance();
    private void PaymentsBtn_Click(object sender, RoutedEventArgs e) => ShowPayments();

    private void LogoutBtn_Click(object sender, RoutedEventArgs e)
    {
        var loginWindow = new Views.LoginWindow
        {
            DataContext = App.Services.GetRequiredService<LoginViewModel>()
        };
        loginWindow.Show();
        Close();
    }

    private void ShowDashboard()
    {
        ShowPage("Dashboard", () => new DashboardView
        {
            DataContext = App.Services.GetRequiredService<DashboardViewModel>()
        });
    }

    private void ShowReports()
    {
        ShowPage("Reports", () => new ReportsView
        {
            DataContext = App.Services.GetRequiredService<ReportsViewModel>()
        });
    }

    private void ShowChildProfile()
    {
        ShowPage("Child Profile", () => new ChildProfileView
        {
            DataContext = App.Services.GetRequiredService<ChildProfileViewModel>()
        });
    }

    private void ShowUsers()
    {
        ShowPage("Users", () => new UsersView
        {
            DataContext = App.Services.GetRequiredService<UsersViewModel>()
        });
    }

    private void ShowStaff()
    {
        ShowPage("Staff", () => new StaffView
        {
            DataContext = App.Services.GetRequiredService<StaffViewModel>()
        });
    }

    private void ShowClassrooms()
    {
        ShowPage("Classrooms", () => new ClassroomsView
        {
            DataContext = App.Services.GetRequiredService<ClassroomsViewModel>()
        });
    }

    private void ShowChildren()
    {
        ShowPage("Children", () => new ChildrenView
        {
            DataContext = App.Services.GetRequiredService<ChildrenViewModel>()
        });
    }

    private void ShowAttendance()
    {
        ShowPage("Attendance", () => new AttendanceView
        {
            DataContext = App.Services.GetRequiredService<AttendanceViewModel>()
        });
    }

    private void ShowPayments()
    {
        ShowPage("Payments", () => new PaymentsView
        {
            DataContext = App.Services.GetRequiredService<PaymentsViewModel>()
        });
    }

    private void ShowPage(string title, Func<UserControl> viewFactory)
    {
        try
        {
            PageTitleBlock.Text = title;
            ContentArea.Content = viewFactory();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Could not open {title}: {ex.Message}", "Navigation error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}