namespace BEMLPropertyManagement.Models;

public class Complaint
{
    public int ComplaintId { get; set; }

    public int AssetId { get; set; }

    public int? EmployeeId { get; set; }

    public DateTime ComplaintDate { get; set; }

    public string ProblemTitle { get; set; } = string.Empty;

    public string ProblemDescription { get; set; } = string.Empty;

    public string Priority { get; set; } = "Medium";

    public string Status { get; set; } = "Open";

    public DateTime? ResolvedDate { get; set; }

    public string? ResolutionNotes { get; set; }

    public DateTime CreatedDate { get; set; }
}