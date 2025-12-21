namespace SMARTCAMPUS.EntityLayer.Enums
{
    public enum EnrollmentStatus
    {
        Pending = 0,    // Waiting for faculty approval
        Enrolled = 1,   // Approved and enrolled
        Dropped = 2,    // Dropped by student
        Completed = 3,  // Course completed
        Failed = 4,     // Course failed
        Withdrawn = 5,  // Administratively withdrawn
        Rejected = 6    // Rejected by faculty
    }
}
