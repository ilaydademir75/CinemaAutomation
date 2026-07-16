// AuditLog.cs
// Purpose: Represents an audit log entry used to track data changes in the system

namespace CinemaAutomation.Web.Data.Entities // Define the namespace for entity classes
{
    public class AuditLog // Entity class that stores audit log records
    {
        public int AuditLogId { get; set; } // Primary key of the audit log table
        public string EntityName { get; set; } = null!; // Name of the entity (table) where the change occurred
        public string ActionType { get; set; } = null!; // Type of action performed (INSERT, UPDATE, DELETE)
        public DateTime ActionDate { get; set; } // Date and time when the action occurred
        public int? UserId { get; set; } // Optional ID of the user who performed the action
        public string? KeyValue { get; set; } // Primary key value of the affected record
        public string? OldValue { get; set; } // Old value before the change (used for UPDATE or DELETE)
        public string? NewValue { get; set; } // New value after the change (used for INSERT or UPDATE)
    }
}

