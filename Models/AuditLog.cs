using System.ComponentModel.DataAnnotations;

namespace BEMLPropertyManagement.Models;

public class AuditLog
{
    [Key]
    public int AuditLogId { get; set; }

    public int? UserId { get; set; }

    public string Action { get; set; } = string.Empty;

    public string? TableName { get; set; }

    public int? RecordId { get; set; }

    public string? Description { get; set; }

    public DateTime ActionDate { get; set; }
}