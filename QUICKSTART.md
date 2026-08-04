# Quick Start Guide

## Installation

1. **Extract Files**
   - Unzip KindergartenApp folder to desired location
   - Keep folder structure intact

2. **Open Project**
   - Double-click `KindergartenApp.sln`
   - Visual Studio will open automatically

3. **Build Project**
   - Visual Studio > Build > Build Solution
   - Wait for build to complete (may take 1-2 minutes on first build)

4. **Run Application**
   - Press F5 or click green "Start" button
   - Application window will open

5. **Login**
   - Username: `admin`
   - Password: `admin123`
   - Click "Login"

## First Steps

### 1. Review Dashboard
- Check statistics on Dashboard tab
- View today's attendance summary
- See recent payments

### 2. Add Children
- Go to "Children" tab
- Fill in the form on the left:
  - Full Name
  - Age
  - Gender
  - Parent Name
  - Phone
  - Address
  - Monthly Fee
- Click "Add" button
- View all children in the grid

### 3. Mark Attendance
- Go to "Attendance" tab
- Click "Today Attendance" button
- See all children listed
- Click "Mark Present" or "Mark Absent" for each child
- View attendance history in the second tab

### 4. Record Payments
- Go to "Payments" tab
- Select a child from dropdown
- Enter payment amount
- Click "Add Payment"
- View unpaid payments in first tab
- Click "Mark Paid" to update payment status

### 5. Generate Reports
- Go to "Reports" tab
- Click buttons to generate:
  - **Unpaid Children Report** - Shows who owes money
  - **Monthly Revenue Report** - Income tracking
  - **Attendance Report** - Select child & date range
- Reports save to Downloads folder as Excel files

### 6. Backup Database
- Click "Backup Database" button on Dashboard
- Backup creates .db file in Backups folder
- Done automatically, can also do manually anytime

## Daily Usage

### Morning Routine
1. Open application
2. Go to Attendance
3. Mark all children's attendance
4. Review Dashboard for stats

### End of Month
1. Go to Payments
2. Check Unpaid Payments list
3. Record all received payments
4. Generate Monthly Revenue Report
5. Backup database

### Weekly Reporting
1. Go to Reports
2. Select a child
3. Set date range (last week)
4. Generate Attendance Report
5. Review and print if needed

## Tips & Tricks

### Search Children
- Use search box on Children tab
- Search by name, parent, or phone
- Results update in real-time

### View Payment History
- Go to Payments tab
- Select child in dropdown
- View all their payments in second tab
- See paid/unpaid status

### Check Attendance History
- Go to Attendance tab
- Select child in dropdown
- View entire month's attendance
- See present/absent status

### Export Reports
- All reports are Excel files
- Open in Excel or Google Sheets
- Modify, print, or share as needed

## Keyboard Shortcuts

| Action | Shortcut |
|--------|----------|
| Save | Ctrl+S |
| Close Window | Alt+F4 |
| Refresh | F5 |
| Navigation | Alt+[Key] |

## Troubleshooting

### App Won't Start
- Ensure .NET 8 runtime is installed
- Check Windows 10 or later version
- Try running as Administrator

### Can't Login
- Check CAPS LOCK is off
- Default is: admin / admin123
- Never change unless you backup first

### Reports Not Generating
- Check Downloads folder exists
- Ensure sufficient disk space
- Try selecting different date range

### Database Issues
- Close app completely
- Delete kindergarten.db file
- Restart app (will recreate database)
- Re-add all children manually

## Default Data

System comes with sample data:
- 5 sample children
- 3 months of attendance records
- Payment history
- Revenue data

**To Start Fresh:**
1. Close application
2. Navigate to: `C:\Users\[YourName]\AppData\Roaming\KindergartenApp\`
3. Delete `kindergarten.db` file
4. Restart application
5. Database recreates with new seed data

## Data Privacy

- All data stored locally on your computer
- No cloud sync
- No internet connection required
- Complete privacy control
- Backup regularly for data safety

## Support Resources

### File Locations
- **Database**: %AppData%\KindergartenApp\kindergarten.db
- **Backups**: %AppData%\KindergartenApp\Backups\
- **Reports**: Your Downloads folder

### Common Tasks

**Export Data**
- All reports automatically export to Excel
- Use Excel to customize further

**Change Admin Password**
- Database stores hashed passwords
- Delete kindergarten.db to reset (loses all data)
- Or contact developer for password reset

**Add More Users**
- Requires database access
- Use database tool to add users table entries

---

**Need Help?** See README.md for more detailed information.
