using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.IO;
using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using KindergartenApp.Application.Interfaces;
using KindergartenApp.Application.Services;
using KindergartenApp.Application.Utilities;
using KindergartenApp.Core.Interfaces;
using KindergartenApp.Infrastructure.Data;
using KindergartenApp.Infrastructure.Repositories;
using KindergartenApp.UI.Infrastructure;
using KindergartenApp.UI.ViewModels;

namespace KindergartenApp.UI;

public partial class App : System.Windows.Application
{
    private static ServiceProvider? _serviceProvider;

    public static IServiceProvider Services
        => _serviceProvider ?? throw new InvalidOperationException("Service provider not initialized.");

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        DispatcherUnhandledException += (_, args) =>
        {
            MessageBox.Show(args.Exception.Message, "Unexpected error", MessageBoxButton.OK, MessageBoxImage.Error);
            args.Handled = true;
        };

        try
        {
            var services = new ServiceCollection();
            ConfigureServices(services);
            _serviceProvider = services.BuildServiceProvider();

            await InitializeDatabaseAsync();

            var loginVM = _serviceProvider.GetRequiredService<LoginViewModel>();
            var loginWindow = new Views.LoginWindow { DataContext = loginVM };
            MainWindow = loginWindow;
            loginWindow.Show();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Startup failed: {ex}");
            Shutdown(-1);
        }
    }

    private void ConfigureServices(ServiceCollection services)
    {
        var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var dbPath = Path.Combine(appDataPath, "KindergartenApp");
        Directory.CreateDirectory(dbPath);

        var connectionString = $"Data Source={Path.Combine(dbPath, "kindergarten.db")}";

        services.AddDbContext<KindergartenDbContext>(options =>
            options.UseSqlite(connectionString));

        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<IChildRepository, ChildRepository>();
        services.AddScoped<IAttendanceRepository, AttendanceRepository>();
        services.AddScoped<IPaymentRepository, PaymentRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IStaffRepository, StaffRepository>();
        services.AddScoped<IClassroomRepository, ClassroomRepository>();
        services.AddScoped<IDocumentRepository, DocumentRepository>();

        services.AddScoped<IAuthenticationService, AuthenticationService>();
        services.AddScoped<IChildService, ChildService>();
        services.AddScoped<IAttendanceService, AttendanceService>();
        services.AddScoped<IPaymentService, PaymentService>();
        services.AddScoped<IDashboardService, DashboardService>();
        services.AddScoped<IReportService, ReportService>();
        services.AddScoped<IBackupService, BackupService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IStaffService, StaffService>();
        services.AddScoped<IClassroomService, ClassroomService>();
        services.AddScoped<IDocumentService, DocumentService>();

        services.AddSingleton<INotificationService, NotificationService>();

        services.AddScoped<LoginViewModel>();
        services.AddScoped<DashboardViewModel>();
        services.AddScoped<ChildrenViewModel>();
        services.AddScoped<AttendanceViewModel>();
        services.AddScoped<PaymentsViewModel>();
        services.AddScoped<ReportsViewModel>();
        services.AddScoped<StaffViewModel>();
        services.AddScoped<ClassroomsViewModel>();
        services.AddScoped<UsersViewModel>();
        services.AddScoped<ChildProfileViewModel>();

        services.AddTransient<MainWindow>();
    }

    private async Task InitializeDatabaseAsync()
    {
        using var scope = _serviceProvider!.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<KindergartenDbContext>();

        try
        {
            await dbContext.Database.MigrateAsync();
            if (!await TableExistsAsync(dbContext, "Users"))
            {
                await dbContext.Database.EnsureDeletedAsync();
                await dbContext.Database.EnsureCreatedAsync();
            }
            await DatabaseSeeder.SeedAsync(dbContext);

            var authService = scope.ServiceProvider.GetRequiredService<IAuthenticationService>();
            await authService.InitializeAdminAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Database initialization failed: {ex.Message}");
        }
    }

    private static async Task<bool> TableExistsAsync(DbContext context, string tableName)
    {
        var connection = context.Database.GetDbConnection();

        if (connection.State != System.Data.ConnectionState.Open)
            await connection.OpenAsync();

        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT 1 FROM sqlite_master WHERE type='table' AND name=$name LIMIT 1;";

        var parameter = command.CreateParameter();
        parameter.ParameterName = "$name";
        parameter.Value = tableName;
        command.Parameters.Add(parameter);

        var result = await command.ExecuteScalarAsync();
        return result != null;
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _serviceProvider?.Dispose();
        base.OnExit(e);
    }

    public void NavButton_MouseEnter(object sender, MouseEventArgs e)
    {
        if (sender is Button btn)
            btn.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0x42, 0xa5, 0xf5));
    }

    public void NavButton_MouseLeave(object sender, MouseEventArgs e)
    {
        if (sender is Button btn)
            btn.Background = System.Windows.Media.Brushes.Transparent;
    }
}


