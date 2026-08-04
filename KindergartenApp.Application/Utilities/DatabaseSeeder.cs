using KindergartenApp.Core.Entities;
using KindergartenApp.Infrastructure.Data;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KindergartenApp.Application.Utilities;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(KindergartenDbContext context)
    {
        if (!context.Users.Any())
        {
            var adminUser = new User
            {
                Username = "admin",
                PasswordHash = PasswordHasher.Hash("admin123"),
                Email = "admin@kindergarten.local",
                Role = UserRole.Admin
            };

            context.Users.Add(adminUser);
            await context.SaveChangesAsync();
        }

        if (!context.Children.Any())
        {
            var children = new List<Child>
            {
                new() { FullName = "Ali Ahmed", Age = 4, Gender = "Male", ParentName = "Ahmed Hassan", Phone = "01012345678", Address = "Cairo", MonthlyFee = 500, JoinDate = DateTime.UtcNow.AddMonths(-6), IsActive = true },
                new() { FullName = "Fatima Mohamed", Age = 5, Gender = "Female", ParentName = "Mohamed Ali", Phone = "01023456789", Address = "Cairo", MonthlyFee = 500, JoinDate = DateTime.UtcNow.AddMonths(-4), IsActive = true },
                new() { FullName = "Sara Hassan", Age = 4, Gender = "Female", ParentName = "Hassan Ibrahim", Phone = "01034567890", Address = "Giza", MonthlyFee = 500, JoinDate = DateTime.UtcNow.AddMonths(-3), IsActive = true },
                new() { FullName = "Omar Khaled", Age = 5, Gender = "Male", ParentName = "Khaled Youssef", Phone = "01045678901", Address = "Cairo", MonthlyFee = 550, JoinDate = DateTime.UtcNow.AddMonths(-2), IsActive = true },
                new() { FullName = "Layla Saleh", Age = 4, Gender = "Female", ParentName = "Saleh Ramadan", Phone = "01056789012", Address = "Giza", MonthlyFee = 500, JoinDate = DateTime.UtcNow.AddMonths(-1), IsActive = true },
            };

            context.Children.AddRange(children);
            await context.SaveChangesAsync();
        }
        else
        {
            // Ensure older seeded/created rows defaulted to inactive.
            var inactive = context.Children.Where(c => !c.IsActive).ToList();
            if (inactive.Count > 0)
            {
                foreach (var child in inactive)
                    child.IsActive = true;
                await context.SaveChangesAsync();
            }
        }

        var savedChildren = context.Children.ToList();

        if (!context.Attendances.Any())
        {
            var rng = new Random();
            var attendances = new List<Attendance>();
            foreach (var child in savedChildren)
            {
                for (int i = 0; i < 20; i++)
                {
                    attendances.Add(new Attendance
                    {
                        ChildId = child.Id,
                        Date = DateTime.UtcNow.AddDays(-i),
                        IsPresent = rng.Next(0, 2) == 1,
                        Notes = null
                    });
                }
            }

            context.Attendances.AddRange(attendances);
            await context.SaveChangesAsync();
        }

        if (!context.Payments.Any())
        {
            var payments = new List<Payment>();
            var today = DateTime.UtcNow;
            foreach (var child in savedChildren)
            {
                for (int m = 0; m < 3; m++)
                {
                    var date = today.AddMonths(-m);
                    payments.Add(new Payment
                    {
                        ChildId = child.Id,
                        Amount = child.MonthlyFee,
                        Date = date,
                        Month = date.Month,
                        Year = date.Year,
                        IsPaid = m != 0,
                        Notes = null
                    });
                }
            }

            context.Payments.AddRange(payments);
            await context.SaveChangesAsync();
        }
    }
}
