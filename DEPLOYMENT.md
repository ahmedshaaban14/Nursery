# Deployment Guide

## Prerequisites

- Visual Studio 2022 or later
- .NET 8 SDK
- Windows 10/11
- Approximately 500 MB free disk space

## Building the Project

### 1. Build Solution

```bash
# In Visual Studio:
Build → Build Solution (Ctrl+Shift+B)

# Or via command line:
cd KindergartenApp
dotnet build
```

### 2. Verify Build Success

- No errors in Error List
- All projects compile successfully
- NuGet packages downloaded

### 3. Run in Development

```bash
# F5 in Visual Studio
# Or via command line:
cd KindergartenApp.UI
dotnet run
```

---

## Publishing as Single EXE

### Method 1: Self-Contained Single File

```bash
cd KindergartenApp.UI

# Build as single file
dotnet publish -c Release -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true

# Output: bin/Release/net8.0-windows/publish/KindergartenApp.UI.exe
```

**Pros**: Single executable file, includes runtime
**Cons**: Larger file size (~120-150 MB)

### Method 2: Framework-Dependent Executable

```bash
cd KindergartenApp.UI

# Build executable (requires .NET 8 runtime)
dotnet publish -c Release -p:PublishSingleFile=false

# Output: bin/Release/net8.0-windows/publish/KindergartenApp.UI.exe
```

**Pros**: Smaller file size (~5-10 MB)
**Cons**: Requires .NET 8 runtime installed

### Method 3: Visual Studio Publish

1. **Right-click KindergartenApp.UI** → Publish
2. **Choose Profile**: Create new or select Folder
3. **Settings**:
   - Target location: `C:\KindergartenApp\published`
   - Configuration: Release
   - Target runtime: win-x64
4. **Publish**

---

## Creating Windows Installer

### Using NSIS (Advanced)

1. Install NSIS from https://nsis.sourceforge.io/
2. Create installer script:

```nsis
; installer.nsi
!include "MUI2.nsh"

Name "Kindergarten Management System"
OutFile "KindergartenApp-Installer.exe"
InstallDir "$PROGRAMFILES\KindergartenApp"

!insertmacro MUI_LANGUAGE "English"

Section "Install"
  SetOutPath "$INSTDIR"
  File /r "published\*.*"
  
  ; Create shortcuts
  SetOutPath "$SMPROGRAMS\Kindergarten App"
  CreateShortCut "$SMPROGRAMS\Kindergarten App\KindergartenApp.lnk" "$INSTDIR\KindergartenApp.UI.exe"
  CreateShortCut "$SMPROGRAMS\Kindergarten App\Uninstall.lnk" "$INSTDIR\uninstall.exe"
SectionEnd
```

3. Compile with: `makensis installer.nsi`

---

## Distributing the Application

### Package Contents

```
KindergartenApp/
├── KindergartenApp.UI.exe
├── README.md
├── QUICKSTART.md
└── (optional) .NET 8 Runtime Installer
```

### Distribution Options

1. **ZIP Archive**
   - Create folder with exe + docs
   - Compress to ZIP
   - Users extract and run

2. **Installer EXE**
   - Create NSIS/WIX installer
   - Handles installation path
   - Creates Start Menu shortcuts

3. **Microsoft Store**
   - Package as MSIX
   - Submit to Microsoft Store
   - Automatic updates

4. **Chocolatey Package**
   - Create Chocolatey package
   - Users install via: `choco install kindergarten-app`

---

## System Requirements

### Minimum
- **OS**: Windows 10 or later
- **RAM**: 2 GB
- **Disk**: 500 MB
- **.NET**: 8.0 Runtime (or included in single-file)
- **Architecture**: x64

### Recommended
- **OS**: Windows 11
- **RAM**: 4 GB
- **Disk**: 1 GB
- **.NET**: Latest .NET 8 LTS
- **Architecture**: x64

---

## Installation Instructions for End Users

### For Self-Contained EXE

1. **Download** `KindergartenApp.UI.exe`
2. **Create Folder** `C:\KindergartenApp\`
3. **Place EXE** in the folder
4. **Double-click** to run
5. **Login** with admin/admin123

### For Framework-Dependent EXE

1. **Install .NET 8 Runtime**
   - Download from https://dotnet.microsoft.com/download
   - Run installer
   - Restart computer

2. **Download** `KindergartenApp.UI.exe`
3. **Place** in desired location
4. **Double-click** to run
5. **Login** with admin/admin123

---

## Database Initialization

### Automatic Setup (First Run)

When application starts:
1. Creates `%AppData%\KindergartenApp\` folder
2. Generates `kindergarten.db` file
3. Runs database migrations
4. Seeds sample data
5. Creates admin account

### Manual Database Reset

1. **Close Application**
2. **Navigate to**: `%AppData%\KindergartenApp\`
3. **Delete**: `kindergarten.db`
4. **Restart Application**
5. **Database recreates** with fresh sample data

---

## Updating the Application

### Method 1: Manual Update
1. Publish new version
2. Replace exe file
3. Restart application
4. Migrations run automatically

### Method 2: Version Management
1. Keep versions in separate folders
2. Create launcher to select version
3. Users can maintain multiple versions

---

## Troubleshooting Deployment

### Issue: "No .NET Runtime"
**Solution**: 
- Use self-contained single file (`PublishSingleFile=true`)
- Or provide .NET 8 runtime installer

### Issue: "Database Permission Denied"
**Solution**:
- Run as Administrator
- Check AppData folder permissions
- Move to different location (Documents folder)

### Issue: "Port Already in Use"
**Solution**:
- Application doesn't use ports (local SQLite)
- Shouldn't occur

### Issue: "Application Won't Start"
**Solution**:
- Check .NET 8 installation
- Run from Command Prompt to see errors
- Verify Windows 10+ OS

---

## Performance Optimization

### For Deployment

1. **Remove Debug Symbols**
   ```
   dotnet publish -c Release --no-self-contained
   ```

2. **Enable Trimming** (in .csproj)
   ```xml
   <TrimMode>link</TrimMode>
   <PublishTrimmed>true</PublishTrimmed>
   ```

3. **Enable ReadyToRun**
   ```
   dotnet publish -c Release -p:PublishReadyToRun=true
   ```

### Database Optimization

1. Regular backups
2. Index optimization
3. Query optimization
4. Archive old data periodically

---

## Uninstallation

### If Using Standalone Folder

1. Close application
2. Delete folder
3. Done

### If Using Installer

1. Control Panel → Programs → Programs and Features
2. Find "Kindergarten Management System"
3. Click Uninstall
4. Follow uninstaller

### Data Cleanup

1. Delete: `%AppData%\KindergartenApp\`
2. All application data removed

---

## Support & Maintenance

### Logs Location
Application doesn't create verbose logs by default.

To enable logging:
1. Add Serilog NuGet package
2. Configure in App.xaml.cs
3. Logs save to AppData folder

### Common Maintenance Tasks

**Monthly**
- Backup database
- Review payment records
- Archive old data

**Quarterly**
- Update .NET runtime
- Review application performance
- Archive attendance records

**Annually**
- Full system backup
- Database optimization
- Update documentation

---

## Version History

### v1.0.0 (Initial Release)
- Core features implemented
- Basic reporting
- Local database
- Admin authentication
- Attendance tracking
- Payment management

### Future Versions
- Multi-user support
- Role-based access
- Advanced reporting
- Bulk import/export
- Mobile app integration
- Cloud sync (optional)

---

## License & Terms

- Free for commercial use
- No licensing fees
- Local data storage only
- Regular backups recommended
- Support available (optional)

---

**For technical issues, refer to README.md and ARCHITECTURE.md**
