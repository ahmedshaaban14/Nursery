# Kindergarten Management System

A complete desktop application for managing kindergarten operations including children, attendance, payments, and reporting.

## Features

✅ **Children Management**
- Add, edit, delete children
- Track personal information
- Search and filter children
- Monthly fee management

✅ **Attendance Tracking**
- Daily attendance marking
- Present/Absent status
- Attendance history
- Child-wise attendance reports

✅ **Payment Management**
- Record child payments
- Track unpaid invoices
- Monthly revenue tracking
- Payment history per child

✅ **Dashboard**
- Real-time statistics
- Total children count
- Today's attendance
- Unpaid payments count
- Monthly income tracking
- Recent payment activities

✅ **Reports & Analytics**
- Unpaid children report (Excel)
- Monthly revenue report (Excel)
- Child attendance report (Excel)
- Date range filtering

✅ **Database & Backup**
- Local SQLite database
- Automatic database creation
- Manual backup functionality
- Automatic migrations

✅ **Security**
- Admin authentication
- Password hashing
- Session management
- Secure data storage

## Technology Stack

- **Framework**: .NET 8
- **UI**: WPF (Windows Presentation Foundation)
- **Pattern**: MVVM (Model-View-ViewModel)
- **Database**: SQLite
- **ORM**: Entity Framework Core
- **UI Theme**: Material Design In XAML
- **Reporting**: ClosedXML (Excel)
- **Architecture**: Clean Architecture

## Project Structure

```
KindergartenApp/
├── KindergartenApp.Core/          # Domain models and entities
├── KindergartenApp.Infrastructure/ # Database, repositories
├── KindergartenApp.Application/    # Business logic, services
└── KindergartenApp.UI/             # WPF user interface
```

## Getting Started

### Prerequisites

- Visual Studio 2022 or later
- .NET 8 SDK
- Windows 10/11

### Build & Run

1. **Open Solution**
   ```bash
   cd KindergartenApp
   ```

2. **Build Solution**
   - Open `KindergartenApp.sln` in Visual Studio
   - Build > Build Solution (Ctrl+Shift+B)

3. **Run Application**
   - Set `KindergartenApp.UI` as startup project
   - Press F5 or click Debug > Start Debugging
   - Or right-click the project and select "Set as Startup Project"

### First Login

- **Username**: admin
- **Password**: admin123

The system will automatically:
- Create SQLite database in `%AppData%\KindergartenApp\`
- Run migrations
- Seed admin account and sample data

## Database

Database file location: `C:\Users\[YourUsername]\AppData\Roaming\KindergartenApp\kindergarten.db`

### Backup Location

Backups are saved to: `C:\Users\[YourUsername]\AppData\Roaming\KindergartenApp\Backups\`

## Features Details

### Dashboard
- Real-time metrics
- Manual backup button
- Refresh dashboard data

### Children Management
- Full CRUD operations
- Search by name, parent, phone
- Age, gender, contact tracking
- Monthly fee configuration

### Attendance
- Today's attendance overview
- Mark present/absent
- View historical attendance
- Child attendance statistics

### Payments
- Record new payments
- Track unpaid invoices
- Monthly revenue summary
- Payment history per child

### Reports
- Export to Excel format
- Unpaid children analysis
- Monthly revenue reports
- Attendance reports with statistics

## Publishing as EXE

### Create Release Build

1. **Build as Release**
   ```
   Visual Studio > Build > Build Solution
   Configuration: Release | Platform: x64
   ```

2. **Publish Application**
   ```
   Right-click KindergartenApp.UI
   → Publish
   → Target: Folder
   → Location: Choose your folder
   → Finish
   ```

3. **Create Setup (Optional)**
   - Use NSIS, WIX, or Windows App Installer
   - Or distribute the published folder as-is

4. **Single EXE Distribution**
   - Use tools like ILMerge or .NET Native compilation
   - Or create a self-extracting installer

### System Requirements for Distribution

- Windows 10 or later
- .NET Desktop Runtime 8.0 or later
- Minimum 100 MB disk space

## Default Sample Data

The system includes sample data:
- 5 kindergarten children
- 3 months of attendance data
- Payment records
- Demo data for testing

Clear sample data anytime by deleting the database file.

## Development

### Adding New Features

1. **Add Entity** → KindergartenApp.Core/Entities/
2. **Add DbSet** → KindergartenApp.Infrastructure/Data/KindergartenDbContext.cs
3. **Create Migration**
   ```bash
   Add-Migration [MigrationName] -Project KindergartenApp.Infrastructure
   ```
4. **Create Repository** → KindergartenApp.Infrastructure/Repositories/
5. **Create Service** → KindergartenApp.Application/Services/
6. **Create ViewModel** → KindergartenApp.UI/ViewModels/
7. **Create View** → KindergartenApp.UI/Views/

### Code Standards

- Use async/await for all I/O operations
- Follow MVVM pattern strictly
- Use bindings, avoid code-behind
- Implement INotifyPropertyChanged
- Use DTOs for data transfer
- Hash passwords, never store plain text

## NuGet Packages

- **MaterialDesignThemes** - UI theming
- **MaterialDesignColors** - Color palette
- **Microsoft.EntityFrameworkCore** - ORM
- **Microsoft.EntityFrameworkCore.Sqlite** - SQLite provider
- **ClosedXML** - Excel report generation
- **Microsoft.Extensions.DependencyInjection** - Dependency injection

## Troubleshooting

### Database Issues
- Delete `kindergarten.db` to reset
- Clear `%AppData%\KindergartenApp\` folder
- Rebuild solution

### Migration Problems
- Remove latest migration: `Remove-Migration` (Package Manager Console)
- Update database: `Update-Database`

### Port Conflicts
- Application uses local SQLite, no port conflicts

## License

This application is provided as-is for educational and commercial use.

## Support

For issues, feature requests, or improvements:
1. Check existing solutions
2. Test with fresh database
3. Verify .NET 8 SDK installation
4. Check file permissions in AppData folder

---

**Version**: 1.0.0  
**Last Updated**: May 2026  
**Built with**: .NET 8 + WPF
