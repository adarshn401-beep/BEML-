using System.ComponentModel.DataAnnotations;

namespace BEMLPropertyManagement.Models;

public class User
{
    [Key]
    public int UserId { get; set; }

    public string Username { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;

    public string FullName { get; set; } = string.Empty;

    public string Role { get; set; } = "Viewer";

    public int? EmployeeId { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedDate { get; set; }

    public DateTime? LastLoginDate { get; set; }
}