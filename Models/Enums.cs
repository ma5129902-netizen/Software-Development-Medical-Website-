namespace HospitalSystem.Models
{
    public enum UserRole
    {
        Admin,
        Doctor,
        Patient
    }

    public enum AppointmentStatus
    {
        Pending,
        Confirmed,
        Completed,
        Cancelled
    }
}