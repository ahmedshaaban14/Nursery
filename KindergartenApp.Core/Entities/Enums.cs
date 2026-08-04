namespace KindergartenApp.Core.Entities;

public enum UserRole
{
    Admin = 1,
    Teacher = 2,
    Accountant = 3,
    Receptionist = 4
}

public enum StaffRole
{
    Teacher = 1,
    Supervisor = 2,
    Accountant = 3,
    Driver = 4,
    Cleaner = 5,
    Nurse = 6
}

public enum StaffStatus
{
    Active = 1,
    OnLeave = 2,
    Inactive = 3,
    Terminated = 4
}

public enum AlertType
{
    UnpaidFees = 1,
    ExcessiveAbsences = 2,
    Birthday = 3,
    ClassroomCapacity = 4,
    PaymentReminder = 5,
    MedicalAlert = 6,
    General = 7
}

public enum AlertPriority
{
    Low = 1,
    Medium = 2,
    High = 3,
    Urgent = 4
}

public enum PaymentStatus
{
    Pending = 1,
    Partial = 2,
    Paid = 3,
    Overdue = 4,
    Cancelled = 5
}

public enum DocumentType
{
    BirthCertificate = 1,
    MedicalRecord = 2,
    VaccinationRecord = 3,
    ParentId = 4,
    Photo = 5,
    Other = 6
}

public enum ClassroomStage
{
    Nursery = 1,
    KG1 = 2,
    KG2 = 3,
    Preschool = 4
}
