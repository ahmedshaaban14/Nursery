using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KindergartenApp.Application.DTOs;
using KindergartenApp.Application.Interfaces;
using KindergartenApp.Application.Utilities;
using KindergartenApp.Core.Entities;
using KindergartenApp.Core.Interfaces;

namespace KindergartenApp.Application.Services;

public class AuthenticationService : IAuthenticationService
{
    private readonly IRepository<User> _userRepository;

    public AuthenticationService(IRepository<User> userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<bool> AuthenticateAsync(string username, string password)
    {
        var users = await _userRepository.GetAllAsync();
        var user = users.FirstOrDefault(u => u.Username == username);

        if (user == null)
            return false;

        return PasswordHasher.Verify(password, user.PasswordHash);
    }

    public async Task<bool> InitializeAdminAsync()
    {
        var users = await _userRepository.GetAllAsync();
        var adminExists = users.FirstOrDefault(u => u.Username == "admin");

        if (adminExists != null)
            return true;

        var admin = new User
        {
            Username = "admin",
            PasswordHash = PasswordHasher.Hash("admin123"),
            Role = UserRole.Admin
        };

        await _userRepository.AddAsync(admin);
        await _userRepository.SaveChangesAsync();

        return true;
    }
}

public class ChildService : IChildService
{
    private readonly IRepository<Child> _childRepository;

    public ChildService(IRepository<Child> childRepository)
    {
        _childRepository = childRepository;
    }

    public async Task<IEnumerable<ChildDto>> GetAllChildrenAsync()
    {
        var children = await _childRepository.GetAllAsync();
        return children.Where(c => c.IsActive).Select(MapToDto);
    }

    public async Task<ChildDto?> GetChildAsync(int id)
    {
        var child = await _childRepository.GetByIdAsync(id);
        return child == null ? null : MapToDto(child);
    }

    public async Task<IEnumerable<ChildDto>> SearchChildrenAsync(string query)
    {
        var children = await _childRepository.GetAllAsync();

        return children
            .Where(c => c.FullName.Contains(query, StringComparison.OrdinalIgnoreCase))
            .Select(MapToDto);
    }

    public async Task<ChildDto> CreateChildAsync(ChildDto dto)
    {
        var child = new Child
        {
            FullName = dto.FullName,
            Age = dto.Age,
            Gender = dto.Gender,
            ParentName = dto.ParentName,
            Phone = dto.Phone,
            Address = dto.Address,
            Notes = dto.Notes,
            MonthlyFee = dto.MonthlyFee,
            ClassroomId = dto.ClassroomId,
            JoinDate = DateTime.UtcNow,
            IsActive = true
        };

        await _childRepository.AddAsync(child);
        await _childRepository.SaveChangesAsync();

        return MapToDto(child);
    }

    public async Task<ChildDto> UpdateChildAsync(ChildDto dto)
    {
        var child = await _childRepository.GetByIdAsync(dto.Id);

        if (child == null)
            throw new InvalidOperationException("Child not found");

        child.FullName = dto.FullName;
        child.Age = dto.Age;
        child.Gender = dto.Gender;
        child.ParentName = dto.ParentName;
        child.Phone = dto.Phone;
        child.Address = dto.Address;
        child.Notes = dto.Notes;
        child.MonthlyFee = dto.MonthlyFee;
        child.ClassroomId = dto.ClassroomId;
        child.UpdatedAt = DateTime.UtcNow;

        await _childRepository.UpdateAsync(child);
        await _childRepository.SaveChangesAsync();

        return MapToDto(child);
    }

    public async Task DeleteChildAsync(int id)
    {
        var child = await _childRepository.GetByIdAsync(id);

        if (child != null)
        {
            child.IsActive = false;
            child.UpdatedAt = DateTime.UtcNow;

            await _childRepository.UpdateAsync(child);
            await _childRepository.SaveChangesAsync();
        }
    }

    private static ChildDto MapToDto(Child child) => new()
    {
        Id = child.Id,
        FullName = child.FullName,
        Age = child.Age,
        Gender = child.Gender,
        ParentName = child.ParentName,
        Phone = child.Phone,
        Address = child.Address,
        Notes = child.Notes,
        MonthlyFee = child.MonthlyFee,
        ClassroomId = child.ClassroomId,
        ClassroomName = child.Classroom?.Name,
        JoinDate = child.JoinDate,
        IsActive = child.IsActive
    };
}