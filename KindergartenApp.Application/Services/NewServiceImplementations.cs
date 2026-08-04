using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KindergartenApp.Application.DTOs;
using KindergartenApp.Application.Interfaces;
using KindergartenApp.Core.Entities;
using KindergartenApp.Infrastructure.Repositories;

namespace KindergartenApp.Application.Services;

public class ClassroomService : IClassroomService
{
    private readonly IClassroomRepository _classroomRepository;
    private readonly IStaffRepository _staffRepository;
    private readonly IChildRepository _childRepository;

    public ClassroomService(IClassroomRepository classroomRepository, IStaffRepository staffRepository, IChildRepository childRepository)
    {
        _classroomRepository = classroomRepository;
        _staffRepository = staffRepository;
        _childRepository = childRepository;
    }

    public async Task<IEnumerable<ClassroomDto>> GetAllClassroomsAsync()
    {
        var classrooms = await _classroomRepository.GetAllAsync();
        var result = new List<ClassroomDto>();
        
        foreach (var classroom in classrooms)
        {
            var dto = await MapToDtoAsync(classroom);
            result.Add(dto);
        }
        
        return result;
    }

    public async Task<ClassroomDto?> GetClassroomAsync(int id)
    {
        var classroom = await _classroomRepository.GetByIdAsync(id);
        return classroom == null ? null : await MapToDtoAsync(classroom);
    }

    public async Task<IEnumerable<ClassroomDto>> GetClassroomsByStageAsync(ClassroomStage stage)
    {
        var classrooms = await _classroomRepository.GetClassroomsByStageAsync(stage);
        var result = new List<ClassroomDto>();
        
        foreach (var classroom in classrooms)
        {
            var dto = await MapToDtoAsync(classroom);
            result.Add(dto);
        }
        
        return result;
    }

    public async Task<ClassroomDto> CreateClassroomAsync(ClassroomDto dto)
    {
        var classroom = new Classroom
        {
            Name = dto.Name,
            Stage = dto.Stage,
            Capacity = dto.Capacity,
            TeacherId = dto.TeacherId,
            Notes = dto.Notes
        };

        await _classroomRepository.AddAsync(classroom);
        await _classroomRepository.SaveChangesAsync();

        return await MapToDtoAsync(classroom);
    }

    public async Task<ClassroomDto> UpdateClassroomAsync(ClassroomDto dto)
    {
        var classroom = await _classroomRepository.GetByIdAsync(dto.Id);

        if (classroom == null)
            throw new InvalidOperationException("Classroom not found");

        classroom.Name = dto.Name;
        classroom.Stage = dto.Stage;
        classroom.Capacity = dto.Capacity;
        classroom.TeacherId = dto.TeacherId;
        classroom.Notes = dto.Notes;
        classroom.UpdatedAt = DateTime.UtcNow;

        await _classroomRepository.UpdateAsync(classroom);
        await _classroomRepository.SaveChangesAsync();

        return await MapToDtoAsync(classroom);
    }

    public async Task DeleteClassroomAsync(int id)
    {
        var classroom = await _classroomRepository.GetByIdAsync(id);

        if (classroom != null)
        {
            await _classroomRepository.DeleteAsync(classroom);
            await _classroomRepository.SaveChangesAsync();
        }
    }

    public async Task AssignTeacherAsync(int classroomId, int teacherId)
    {
        var classroom = await _classroomRepository.GetByIdAsync(classroomId);
        var teacher = await _staffRepository.GetByIdAsync(teacherId);

        if (classroom == null)
            throw new InvalidOperationException("Classroom not found");

        if (teacher == null)
            throw new InvalidOperationException("Teacher not found");

        classroom.TeacherId = teacherId;
        classroom.UpdatedAt = DateTime.UtcNow;

        await _classroomRepository.UpdateAsync(classroom);
        await _classroomRepository.SaveChangesAsync();
    }

    public async Task AssignChildToClassroomAsync(int childId, int classroomId)
    {
        var child = await _childRepository.GetByIdAsync(childId);
        var classroom = await _classroomRepository.GetByIdAsync(classroomId);

        if (child == null)
            throw new InvalidOperationException("Child not found");

        if (classroom == null)
            throw new InvalidOperationException("Classroom not found");

        if (classroom.IsFull)
            throw new InvalidOperationException("Classroom is at full capacity");

        child.ClassroomId = classroomId;
        child.UpdatedAt = DateTime.UtcNow;

        await _childRepository.UpdateAsync(child);
        await _childRepository.SaveChangesAsync();
    }

    private async Task<ClassroomDto> MapToDtoAsync(Classroom classroom)
    {
        var classroomWithChildren = await _classroomRepository.GetClassroomWithChildrenAsync(classroom.Id);
        var teacher = classroom.TeacherId.HasValue ? await _staffRepository.GetByIdAsync(classroom.TeacherId.Value) : null;

        return new ClassroomDto
        {
            Id = classroom.Id,
            Name = classroom.Name,
            Stage = classroom.Stage,
            Capacity = classroom.Capacity,
            TeacherId = classroom.TeacherId,
            TeacherName = teacher?.FullName,
            Notes = classroom.Notes,
            CurrentStudentCount = classroomWithChildren?.Children.Count ?? 0,
            IsFull = classroom.IsFull,
            OccupancyPercentage = classroom.OccupancyPercentage
        };
    }
}

public class StaffService : IStaffService
{
    private readonly IStaffRepository _staffRepository;
    private readonly IClassroomRepository _classroomRepository;

    public StaffService(IStaffRepository staffRepository, IClassroomRepository classroomRepository)
    {
        _staffRepository = staffRepository;
        _classroomRepository = classroomRepository;
    }

    public async Task<IEnumerable<StaffDto>> GetAllStaffAsync()
    {
        var staff = await _staffRepository.GetAllAsync();
        return staff.Select(MapToDto);
    }

    public async Task<StaffDto?> GetStaffAsync(int id)
    {
        var staff = await _staffRepository.GetByIdAsync(id);
        return staff == null ? null : MapToDto(staff);
    }

    public async Task<IEnumerable<StaffDto>> GetStaffByRoleAsync(StaffRole role)
    {
        var staff = await _staffRepository.GetStaffByRoleAsync(role);
        return staff.Select(MapToDto);
    }

    public async Task<IEnumerable<StaffDto>> GetActiveStaffAsync()
    {
        var staff = await _staffRepository.GetActiveStaffAsync();
        return staff.Select(MapToDto);
    }

    public async Task<StaffDto> CreateStaffAsync(StaffDto dto)
    {
        var staff = new Staff
        {
            FullName = dto.FullName,
            Role = dto.Role,
            Salary = dto.Salary,
            Phone = dto.Phone,
            Address = dto.Address,
            HireDate = dto.HireDate,
            Status = dto.Status,
            Notes = dto.Notes
        };

        await _staffRepository.AddAsync(staff);
        await _staffRepository.SaveChangesAsync();

        return MapToDto(staff);
    }

    public async Task<StaffDto> UpdateStaffAsync(StaffDto dto)
    {
        var staff = await _staffRepository.GetByIdAsync(dto.Id);

        if (staff == null)
            throw new InvalidOperationException("Staff not found");

        staff.FullName = dto.FullName;
        staff.Role = dto.Role;
        staff.Salary = dto.Salary;
        staff.Phone = dto.Phone;
        staff.Address = dto.Address;
        staff.HireDate = dto.HireDate;
        staff.Status = dto.Status;
        staff.Notes = dto.Notes;
        staff.UpdatedAt = DateTime.UtcNow;

        await _staffRepository.UpdateAsync(staff);
        await _staffRepository.SaveChangesAsync();

        return MapToDto(staff);
    }

    public async Task DeleteStaffAsync(int id)
    {
        var staff = await _staffRepository.GetByIdAsync(id);

        if (staff != null)
        {
            await _staffRepository.DeleteAsync(staff);
            await _staffRepository.SaveChangesAsync();
        }
    }

    public async Task MarkStaffAttendanceAsync(int staffId, DateTime date, bool isPresent)
    {
        // Staff attendance tracking would be implemented here
        // For now, this is a placeholder
        await Task.CompletedTask;
    }

    private static StaffDto MapToDto(Staff staff)
    {
        return new StaffDto
        {
            Id = staff.Id,
            FullName = staff.FullName,
            Role = staff.Role,
            Salary = staff.Salary,
            Phone = staff.Phone,
            Address = staff.Address,
            HireDate = staff.HireDate,
            Status = staff.Status,
            Notes = staff.Notes,
            YearsOfService = staff.YearsOfService,
            AssignedClassroomsCount = staff.Classrooms.Count
        };
    }
}

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IStaffRepository _staffRepository;

    public UserService(IUserRepository userRepository, IStaffRepository staffRepository)
    {
        _userRepository = userRepository;
        _staffRepository = staffRepository;
    }

    public async Task<IEnumerable<UserDto>> GetAllUsersAsync()
    {
        var users = await _userRepository.GetAllAsync();
        var result = new List<UserDto>();
        
        foreach (var user in users)
        {
            var dto = await MapToDtoAsync(user);
            result.Add(dto);
        }
        
        return result;
    }

    public async Task<UserDto?> GetUserAsync(int id)
    {
        var user = await _userRepository.GetByIdAsync(id);
        return user == null ? null : await MapToDtoAsync(user);
    }

    public async Task<UserDto?> GetUserByUsernameAsync(string username)
    {
        var user = await _userRepository.GetByUsernameAsync(username);
        return user == null ? null : await MapToDtoAsync(user);
    }

    public async Task<UserDto> CreateUserAsync(UserDto dto, string password)
    {
        var user = new User
        {
            Username = dto.Username,
            PasswordHash = KindergartenApp.Application.Utilities.PasswordHasher.Hash(password),
            Email = dto.Email,
            FullName = dto.FullName,
            Role = dto.Role,
            IsActive = true,
            StaffId = dto.StaffId
        };

        await _userRepository.AddAsync(user);
        await _userRepository.SaveChangesAsync();

        return await MapToDtoAsync(user);
    }

    public async Task<UserDto> UpdateUserAsync(UserDto dto)
    {
        var user = await _userRepository.GetByIdAsync(dto.Id);

        if (user == null)
            throw new InvalidOperationException("User not found");

        user.Username = dto.Username;
        user.Email = dto.Email;
        user.FullName = dto.FullName;
        user.Role = dto.Role;
        user.IsActive = dto.IsActive;
        user.StaffId = dto.StaffId;
        user.UpdatedAt = DateTime.UtcNow;

        await _userRepository.UpdateAsync(user);
        await _userRepository.SaveChangesAsync();

        return await MapToDtoAsync(user);
    }

    public async Task DeleteUserAsync(int id)
    {
        var user = await _userRepository.GetByIdAsync(id);

        if (user != null)
        {
            await _userRepository.DeleteAsync(user);
            await _userRepository.SaveChangesAsync();
        }
    }

    public async Task<bool> ChangePasswordAsync(int userId, string oldPassword, string newPassword)
    {
        var user = await _userRepository.GetByIdAsync(userId);

        if (user == null)
            return false;

        if (!KindergartenApp.Application.Utilities.PasswordHasher.Verify(oldPassword, user.PasswordHash))
            return false;

        user.PasswordHash = KindergartenApp.Application.Utilities.PasswordHasher.Hash(newPassword);
        user.UpdatedAt = DateTime.UtcNow;

        await _userRepository.UpdateAsync(user);
        await _userRepository.SaveChangesAsync();

        return true;
    }

    public async Task UpdateLastLoginAsync(int userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);

        if (user != null)
        {
            user.LastLoginAt = DateTime.UtcNow;
            user.UpdatedAt = DateTime.UtcNow;

            await _userRepository.UpdateAsync(user);
            await _userRepository.SaveChangesAsync();
        }
    }

    private async Task<UserDto> MapToDtoAsync(User user)
    {
        var staff = user.StaffId.HasValue ? await _staffRepository.GetByIdAsync(user.StaffId.Value) : null;

        return new UserDto
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            FullName = user.FullName,
            Role = user.Role,
            IsActive = user.IsActive,
            LastLoginAt = user.LastLoginAt,
            StaffId = user.StaffId,
            StaffName = staff?.FullName
        };
    }
}

public class AlertService : IAlertService
{
    private readonly IAlertRepository _alertRepository;
    private readonly IChildRepository _childRepository;
    private readonly IStaffRepository _staffRepository;

    public AlertService(IAlertRepository alertRepository, IChildRepository childRepository, IStaffRepository staffRepository)
    {
        _alertRepository = alertRepository;
        _childRepository = childRepository;
        _staffRepository = staffRepository;
    }

    public async Task<IEnumerable<AlertDto>> GetAllAlertsAsync()
    {
        var alerts = await _alertRepository.GetAllAsync();
        return alerts.Select(MapToDto);
    }

    public async Task<IEnumerable<AlertDto>> GetActiveAlertsAsync()
    {
        var alerts = await _alertRepository.GetActiveAlertsAsync();
        return alerts.Select(MapToDto);
    }

    public async Task<IEnumerable<AlertDto>> GetAlertsByTypeAsync(AlertType type)
    {
        var alerts = await _alertRepository.GetAlertsByTypeAsync(type);
        return alerts.Select(MapToDto);
    }

    public async Task<IEnumerable<AlertDto>> GetAlertsForChildAsync(int childId)
    {
        var alerts = await _alertRepository.GetAlertsForChildAsync(childId);
        return alerts.Select(MapToDto);
    }

    public async Task<AlertDto> CreateAlertAsync(AlertDto dto)
    {
        var alert = new Alert
        {
            Type = dto.Type,
            Priority = dto.Priority,
            Title = dto.Title,
            Message = dto.Message,
            ExpiryDate = dto.ExpiryDate,
            IsRead = false,
            IsResolved = false,
            ChildId = dto.ChildId,
            StaffId = dto.StaffId
        };

        await _alertRepository.AddAsync(alert);
        await _alertRepository.SaveChangesAsync();

        return MapToDto(alert);
    }

    public async Task<AlertDto> UpdateAlertAsync(AlertDto dto)
    {
        var alert = await _alertRepository.GetByIdAsync(dto.Id);

        if (alert == null)
            throw new InvalidOperationException("Alert not found");

        alert.Type = dto.Type;
        alert.Priority = dto.Priority;
        alert.Title = dto.Title;
        alert.Message = dto.Message;
        alert.ExpiryDate = dto.ExpiryDate;
        alert.IsRead = dto.IsRead;
        alert.IsResolved = dto.IsResolved;
        alert.ResolvedAt = dto.ResolvedAt;
        alert.UpdatedAt = DateTime.UtcNow;

        await _alertRepository.UpdateAsync(alert);
        await _alertRepository.SaveChangesAsync();

        return MapToDto(alert);
    }

    public async Task DeleteAlertAsync(int id)
    {
        var alert = await _alertRepository.GetByIdAsync(id);

        if (alert != null)
        {
            await _alertRepository.DeleteAsync(alert);
            await _alertRepository.SaveChangesAsync();
        }
    }

    public async Task MarkAlertAsReadAsync(int id)
    {
        var alert = await _alertRepository.GetByIdAsync(id);

        if (alert != null)
        {
            alert.IsRead = true;
            alert.UpdatedAt = DateTime.UtcNow;

            await _alertRepository.UpdateAsync(alert);
            await _alertRepository.SaveChangesAsync();
        }
    }

    public async Task MarkAlertAsResolvedAsync(int id)
    {
        var alert = await _alertRepository.GetByIdAsync(id);

        if (alert != null)
        {
            alert.IsResolved = true;
            alert.ResolvedAt = DateTime.UtcNow;
            alert.UpdatedAt = DateTime.UtcNow;

            await _alertRepository.UpdateAsync(alert);
            await _alertRepository.SaveChangesAsync();
        }
    }

    public async Task CheckAndGenerateAlertsAsync()
    {
        // Check for unpaid fees, excessive absences, birthdays, etc.
        // This would be implemented based on business rules
        await Task.CompletedTask;
    }

    private static AlertDto MapToDto(Alert alert)
    {
        return new AlertDto
        {
            Id = alert.Id,
            Type = alert.Type,
            Priority = alert.Priority,
            Title = alert.Title,
            Message = alert.Message,
            ExpiryDate = alert.ExpiryDate,
            IsRead = alert.IsRead,
            IsResolved = alert.IsResolved,
            ResolvedAt = alert.ResolvedAt,
            ChildId = alert.ChildId,
            StaffId = alert.StaffId,
            IsExpired = alert.IsExpired
        };
    }
}

public class DocumentService : IDocumentService
{
    private readonly IDocumentRepository _documentRepository;
    private readonly IChildRepository _childRepository;

    public DocumentService(IDocumentRepository documentRepository, IChildRepository childRepository)
    {
        _documentRepository = documentRepository;
        _childRepository = childRepository;
    }

    public async Task<IEnumerable<DocumentDto>> GetDocumentsByChildAsync(int childId)
    {
        var documents = await _documentRepository.GetDocumentsByChildAsync(childId);
        return documents.Select(MapToDto);
    }

    public async Task<DocumentDto?> GetDocumentAsync(int id)
    {
        var document = await _documentRepository.GetByIdAsync(id);
        return document == null ? null : MapToDto(document);
    }

    public async Task<DocumentDto> UploadDocumentAsync(DocumentDto dto, byte[] fileData)
    {
        var document = new Document
        {
            Type = dto.Type,
            FileName = dto.FileName,
            FilePath = dto.FilePath,
            ContentType = dto.ContentType,
            FileSize = fileData.Length,
            Description = dto.Description,
            ChildId = dto.ChildId
        };

        await _documentRepository.AddAsync(document);
        await _documentRepository.SaveChangesAsync();

        return MapToDto(document);
    }

    public async Task DeleteDocumentAsync(int id)
    {
        var document = await _documentRepository.GetByIdAsync(id);

        if (document != null)
        {
            await _documentRepository.DeleteAsync(document);
            await _documentRepository.SaveChangesAsync();
        }
    }

    public async Task<byte[]?> DownloadDocumentAsync(int id)
    {
        var document = await _documentRepository.GetByIdAsync(id);
        
        if (document == null)
            return null;

        // In a real implementation, this would read the file from disk
        // For now, return null as placeholder
        return null;
    }

    private static DocumentDto MapToDto(Document document)
    {
        return new DocumentDto
        {
            Id = document.Id,
            Type = document.Type,
            FileName = document.FileName,
            FilePath = document.FilePath,
            ContentType = document.ContentType,
            FileSize = document.FileSize,
            Description = document.Description,
            ChildId = document.ChildId,
            CreatedAt = document.CreatedAt,
            FileSizeDisplay = document.FileSizeDisplay
        };
    }
}

public class SearchService : ISearchService
{
    private readonly IChildRepository _childRepository;
    private readonly IStaffRepository _staffRepository;
    private readonly IClassroomRepository _classroomRepository;

    public SearchService(IChildRepository childRepository, IStaffRepository staffRepository, IClassroomRepository classroomRepository)
    {
        _childRepository = childRepository;
        _staffRepository = staffRepository;
        _classroomRepository = classroomRepository;
    }

    public async Task<SearchDto> GlobalSearchAsync(string query)
    {
        var children = await _childRepository.SearchAsync(query);
        var staff = await _staffRepository.GetAllAsync();
        var classrooms = await _classroomRepository.GetAllAsync();

        var filteredStaff = staff.Where(s => 
            s.FullName.Contains(query, StringComparison.OrdinalIgnoreCase) ||
            s.Phone.Contains(query, StringComparison.OrdinalIgnoreCase)
        );

        var filteredClassrooms = classrooms.Where(c =>
            c.Name.Contains(query, StringComparison.OrdinalIgnoreCase)
        );

        return new SearchDto
        {
            Query = query,
            Children = children.Select(c => new ChildSearchResultDto
            {
                Id = c.Id,
                FullName = c.FullName,
                ParentName = c.ParentName,
                Phone = c.Phone,
                ClassroomName = c.Classroom?.Name
            }).ToList(),
            Staff = filteredStaff.Select(s => new StaffSearchResultDto
            {
                Id = s.Id,
                FullName = s.FullName,
                Role = s.Role,
                Phone = s.Phone,
                Status = s.Status
            }).ToList(),
            Classrooms = filteredClassrooms.Select(c => new ClassroomSearchResultDto
            {
                Id = c.Id,
                Name = c.Name,
                Stage = c.Stage,
                TeacherName = c.Teacher?.FullName,
                Capacity = c.Capacity,
                CurrentCount = c.Children.Count
            }).ToList()
        };
    }
}

public class AnalyticsService : IAnalyticsService
{
    private readonly IChildRepository _childRepository;
    private readonly IAttendanceRepository _attendanceRepository;
    private readonly IPaymentRepository _paymentRepository;
    private readonly IClassroomRepository _classroomRepository;
    private readonly IStaffRepository _staffRepository;
    private readonly IAlertRepository _alertRepository;

    public AnalyticsService(IChildRepository childRepository, IAttendanceRepository attendanceRepository, 
        IPaymentRepository paymentRepository, IClassroomRepository classroomRepository, 
        IStaffRepository staffRepository, IAlertRepository alertRepository)
    {
        _childRepository = childRepository;
        _attendanceRepository = attendanceRepository;
        _paymentRepository = paymentRepository;
        _classroomRepository = classroomRepository;
        _staffRepository = staffRepository;
        _alertRepository = alertRepository;
    }

    public async Task<DashboardDto> GetEnhancedDashboardDataAsync()
    {
        var children = await _childRepository.GetAllAsync();
        var activeChildren = children.Where(c => c.IsActive).ToList();
        var attendances = await _attendanceRepository.GetAllAsync();
        var today = DateTime.Today;
        var todayAttendance = attendances.Where(a => a.Date.Date == today).ToList();
        var payments = await _paymentRepository.GetAllAsync();
        var unpaidPayments = payments.Where(p => !p.IsPaid).ToList();
        var classrooms = await _classroomRepository.GetAllAsync();
        var staff = await _staffRepository.GetAllAsync();
        var alerts = await _alertRepository.GetActiveAlertsAsync();

        var monthlyRevenue = await _paymentRepository.GetMonthlyRevenueAsync(DateTime.Today.Month, DateTime.Today.Year);
        var attendancePercentage = activeChildren.Count > 0 
            ? (decimal)todayAttendance.Count(a => a.IsPresent) / activeChildren.Count * 100 
            : 0;

        var classroomOccupancy = new List<ClassroomOccupancyDto>();
        foreach (var classroom in classrooms)
        {
            var classroomWithChildren = await _classroomRepository.GetClassroomWithChildrenAsync(classroom.Id);
            classroomOccupancy.Add(new ClassroomOccupancyDto
            {
                ClassroomName = classroom.Name,
                Capacity = classroom.Capacity,
                CurrentCount = classroomWithChildren?.Children.Count ?? 0,
                OccupancyPercentage = classroom.OccupancyPercentage
            });
        }

        return new DashboardDto
        {
            TotalChildren = activeChildren.Count,
            TodayAttendance = todayAttendance.Count(a => a.IsPresent),
            UnpaidPaymentsCount = unpaidPayments.Count,
            MonthlyIncome = monthlyRevenue,
            TotalClassrooms = classrooms.Count(),
            TotalStaff = staff.Count(),
            ActiveAlerts = alerts.Count(),
            AttendancePercentage = attendancePercentage,
            ClassroomOccupancy = classroomOccupancy
        };
    }

    public async Task<List<ClassroomOccupancyDto>> GetClassroomOccupancyAsync()
    {
        var classrooms = await _classroomRepository.GetAllAsync();
        var result = new List<ClassroomOccupancyDto>();

        foreach (var classroom in classrooms)
        {
            var classroomWithChildren = await _classroomRepository.GetClassroomWithChildrenAsync(classroom.Id);
            result.Add(new ClassroomOccupancyDto
            {
                ClassroomName = classroom.Name,
                Capacity = classroom.Capacity,
                CurrentCount = classroomWithChildren?.Children.Count ?? 0,
                OccupancyPercentage = classroom.OccupancyPercentage
            });
        }

        return result;
    }

    public async Task<decimal> GetAttendanceTrendAsync(int days)
    {
        var attendances = await _attendanceRepository.GetAllAsync();
        var startDate = DateTime.Today.AddDays(-days);
        var relevantAttendances = attendances.Where(a => a.Date >= startDate).ToList();
        
        if (relevantAttendances.Count == 0)
            return 0;

        return (decimal)relevantAttendances.Count(a => a.IsPresent) / relevantAttendances.Count * 100;
    }

    public async Task<decimal> GetRevenueTrendAsync(int months)
    {
        decimal totalRevenue = 0;
        
        for (int i = 0; i < months; i++)
        {
            var date = DateTime.Today.AddMonths(-i);
            totalRevenue += await _paymentRepository.GetMonthlyRevenueAsync(date.Month, date.Year);
        }

        return totalRevenue / months;
    }
}