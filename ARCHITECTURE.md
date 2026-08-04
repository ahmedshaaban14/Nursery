# Architecture Overview

## Project Structure

### 1. KindergartenApp.Core
**Purpose**: Domain models and entities  
**Contains**:
- `Entities/`: Domain objects
  - `BaseEntity`: Base class with Id, CreatedAt, UpdatedAt
  - `Child`: Kindergarten child profile
  - `Attendance`: Daily attendance records
  - `Payment`: Payment transactions
  - `User`: System users

**Responsibilities**:
- Define business entities
- Pure business logic models
- No dependencies on other projects

---

### 2. KindergartenApp.Infrastructure
**Purpose**: Data access and persistence layer  
**Contains**:
- `Data/`: Database configuration
  - `KindergartenDbContext`: EF Core DbContext
  - Entity configurations
  - Migration scripts

- `Repositories/`: Data access patterns
  - `Repository<T>`: Generic repository
  - Specific repositories for each entity
  - CRUD operations and queries

- `Persistence/`: Database helpers

**Responsibilities**:
- Database migrations
- Entity Framework configuration
- Repository implementations
- Database seeding

---

### 3. KindergartenApp.Application
**Purpose**: Business logic and service layer  
**Contains**:
- `DTOs/`: Data transfer objects
  - Simplified data contracts
  - Transfer between layers

- `Services/`: Business logic implementation
  - `AuthenticationService`: User authentication
  - `ChildService`: Child management
  - `AttendanceService`: Attendance operations
  - `PaymentService`: Payment handling
  - `DashboardService`: Dashboard metrics
  - `ReportService`: Report generation
  - `BackupService`: Database backup

- `Interfaces/`: Service contracts
- `Utilities/`: Helpers and utilities
  - `PasswordHasher`: Secure password hashing
  - `DatabaseSeeder`: Initial data setup

**Responsibilities**:
- Business logic implementation
- Service interfaces
- Data transformation (Entity ↔ DTO)
- Domain-specific calculations

---

### 4. KindergartenApp.UI
**Purpose**: User interface and presentation  
**Contains**:
- `Infrastructure/`: MVVM framework
  - `ViewModelBase`: Base class with INotifyPropertyChanged
  - `RelayCommand` & `AsyncRelayCommand`: Command implementation
  - `NotificationService`: User notifications

- `ViewModels/`: Presentation logic
  - `LoginViewModel`: Authentication
  - `DashboardViewModel`: Dashboard
  - `ChildrenViewModel`: Children management
  - `AttendanceViewModel`: Attendance
  - `PaymentsViewModel`: Payments
  - `ReportsViewModel`: Reports

- `Views/`: XAML user interfaces
  - `LoginWindow`: Login screen
  - `MainWindow`: Main application shell
  - `DashboardView`: Dashboard screen
  - `ChildrenView`: Children list
  - `AttendanceView`: Attendance tracking
  - `PaymentsView`: Payment management
  - `ReportsView`: Report generation

- `Converters/`: Value converters
  - Boolean to visibility
  - Date time formatting
  - Decimal formatting

- `Models/`: UI models
  - `SessionManager`: User session
  - `CurrentUser`: Current user info

**Responsibilities**:
- User interface
- User interactions
- Display logic
- Commands and events

---

## Data Flow

```
User Input (UI)
    ↓
ViewModel (Presentation Logic)
    ↓
Service (Business Logic)
    ↓
Repository (Data Access)
    ↓
Database (SQLite)
```

## Dependency Injection

```csharp
// Service Registration in App.xaml.cs
services.AddDbContext<KindergartenDbContext>();
services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
services.AddScoped<IChildService, ChildService>();
services.AddScoped<ChildrenViewModel>();
services.AddSingleton<INotificationService, NotificationService>();
```

## Key Design Patterns

### 1. MVVM (Model-View-ViewModel)
- Views: XAML UI
- ViewModels: Business logic, commands, bindings
- Models: Data structures (Entities, DTOs)
- Binding: Two-way data binding

### 2. Repository Pattern
- Generic `IRepository<T>` for CRUD
- Specific repositories for complex queries
- DbContext encapsulation

### 3. Service Pattern
- Business logic separation
- DTO transformation
- Cross-cutting concerns
- Dependency injection

### 4. Async/Await
- Responsive UI
- Non-blocking operations
- Async repositories and services

## Database Schema

### Children Table
```sql
CREATE TABLE Children (
  Id INTEGER PRIMARY KEY,
  FullName TEXT NOT NULL,
  Age INTEGER,
  Gender TEXT,
  ParentName TEXT,
  Phone TEXT,
  Address TEXT,
  MonthlyFee DECIMAL(10,2),
  JoinDate DATETIME,
  IsActive BOOLEAN,
  CreatedAt DATETIME,
  UpdatedAt DATETIME
)
```

### Attendance Table
```sql
CREATE TABLE Attendances (
  Id INTEGER PRIMARY KEY,
  ChildId INTEGER NOT NULL,
  Date DATETIME,
  IsPresent BOOLEAN,
  Notes TEXT,
  UNIQUE(ChildId, Date),
  FOREIGN KEY(ChildId) REFERENCES Children(Id)
)
```

### Payments Table
```sql
CREATE TABLE Payments (
  Id INTEGER PRIMARY KEY,
  ChildId INTEGER NOT NULL,
  Amount DECIMAL(10,2),
  Date DATETIME,
  Month INTEGER,
  Year INTEGER,
  IsPaid BOOLEAN,
  Notes TEXT,
  INDEX(ChildId, Month, Year),
  FOREIGN KEY(ChildId) REFERENCES Children(Id)
)
```

### Users Table
```sql
CREATE TABLE Users (
  Id INTEGER PRIMARY KEY,
  Username TEXT NOT NULL UNIQUE,
  PasswordHash TEXT NOT NULL,
  Email TEXT,
  IsAdmin BOOLEAN,
  CreatedAt DATETIME,
  UpdatedAt DATETIME
)
```

## Security Considerations

1. **Password Security**
   - SHA256 hashing
   - No plain text storage
   - Hash verification

2. **Data Privacy**
   - Local storage only
   - No cloud transmission
   - No online services

3. **Session Management**
   - Session-based authentication
   - Login/Logout functionality
   - User context tracking

## Performance Optimizations

1. **Database Queries**
   - Index on frequently queried columns
   - Eager loading with Include()
   - Query optimization

2. **UI Rendering**
   - Async operations prevent freezing
   - Virtual data grids (if implemented)
   - Minimal redrawing

3. **Memory Management**
   - Proper disposal of DbContext
   - Using statements for resources
   - Service provider cleanup

## Testing Considerations

### Unit Testing
- Services: Easy to test with mock repositories
- ViewModels: Test with mock services
- Repositories: Test with in-memory SQLite

### Integration Testing
- Full database with actual SQLite
- Service + Repository combinations
- ViewModel + Service combinations

### UI Testing
- XAML view compilation
- Command execution
- Data binding validation

## Extension Points

### Adding New Features

1. **New Entity**
   - Add to Core/Entities
   - Update DbContext
   - Create migration

2. **New Service**
   - Interface in Application/Interfaces
   - Implementation in Application/Services
   - Register in App.xaml.cs

3. **New View**
   - Create ViewModel in UI/ViewModels
   - Create View XAML in UI/Views
   - Add navigation in MainWindow

### Database Migration Example
```csharp
// Create migration
Add-Migration AddNewFeature -Project KindergartenApp.Infrastructure

// Applied automatically on startup via:
// await dbContext.Database.MigrateAsync();
```

## Configuration

### Connection String
Stored in App.xaml.cs:
```csharp
var connectionString = $"Data Source={Path.Combine(dbPath, "kindergarten.db")}";
```

### Default Admin
Username: `admin`
Password: `admin123`

### Database Location
`%AppData%\KindergartenApp\kindergarten.db`

### Backup Location
`%AppData%\KindergartenApp\Backups\`

---

This architecture ensures:
✅ Clean separation of concerns
✅ Testability and maintainability
✅ Scalability for future features
✅ Reusable components
✅ Professional code organization
