using KindergartenApp.Core.Entities;

namespace KindergartenApp.UI.Models;

public class CurrentUser
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public bool IsAuthenticated { get; set; }
    public UserRole Role { get; set; } = UserRole.Teacher;
    public DateTime? LastLoginAt { get; set; }
}

public class SessionManager
{
    public static CurrentUser CurrentUser { get; set; } = new();

    public static void Login(int userId, string username, string fullName, UserRole role)
    {
        CurrentUser = new CurrentUser
        {
            Id = userId,
            Username = username,
            FullName = fullName,
            IsAuthenticated = true,
            Role = role,
            LastLoginAt = DateTime.UtcNow
        };
    }

    public static void Logout()
    {
        CurrentUser = new CurrentUser();
    }

    public static bool IsInRole(UserRole role)
    {
        return CurrentUser.IsAuthenticated && CurrentUser.Role == role;
    }

    public static bool IsAdmin()
    {
        return IsInRole(UserRole.Admin);
    }

    public static bool CanManageUsers()
    {
        return IsInRole(UserRole.Admin);
    }

    public static bool CanManageStaff()
    {
        return IsInRole(UserRole.Admin);
    }

    public static bool CanManageClassrooms()
    {
        return IsInRole(UserRole.Admin) || IsInRole(UserRole.Teacher);
    }

    public static bool CanManagePayments()
    {
        return IsInRole(UserRole.Admin) || IsInRole(UserRole.Accountant);
    }

    public static bool CanManageAttendance()
    {
        return IsInRole(UserRole.Admin) || IsInRole(UserRole.Teacher) || IsInRole(UserRole.Receptionist);
    }

    public static bool CanViewReports()
    {
        return IsInRole(UserRole.Admin) || IsInRole(UserRole.Accountant);
    }

    public static bool CanManageChildren()
    {
        return IsInRole(UserRole.Admin) || IsInRole(UserRole.Teacher) || IsInRole(UserRole.Receptionist);
    }
}
