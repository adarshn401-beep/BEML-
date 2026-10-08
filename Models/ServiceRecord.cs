namespace BEMLPropertyManagement.Models;

public class ServiceRecord
{
    public int ServiceRecordId { get; set; }

    public int AssetId { get; set; }

    public DateTime ComplaintDate { get; set; }

    public string ProblemDescription { get; set; } = string.Empty;

    public DateTime? ServiceDate { get; set; }

    public string? ServiceProvider { get; set; }

    public string? WorkDescription { get; set; }

    public decimal? ServiceCost { get; set; }

    public string Status { get; set; } = "Open";

    public DateTime? NextServiceDate { get; set; }

    public string? Remarks { get; set; }

    public DateTime CreatedDate { get; set; }
}