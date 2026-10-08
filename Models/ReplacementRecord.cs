using System.ComponentModel.DataAnnotations;

namespace BEMLPropertyManagement.Models;

public class ReplacementRecord
{
    [Key]
    public int ReplacementId { get; set; }

    public int OldAssetId { get; set; }

    public int? NewAssetId { get; set; }

    public DateTime ReplacementDate { get; set; }

    public string ReplacementReason { get; set; } = string.Empty;

    public string? OldAssetCondition { get; set; }

    public string? OldAssetDisposalStatus { get; set; }

    public string? ApprovedBy { get; set; }

    public decimal? ReplacementCost { get; set; }

    public string? Remarks { get; set; }

    public DateTime CreatedDate { get; set; }
}