using System.ComponentModel.DataAnnotations;

namespace BEMLPropertyManagement.Models;

public class WarrantyRecord
{
    [Key]
    public int WarrantyId { get; set; }

    public int AssetId { get; set; }

    public string? WarrantyProvider { get; set; }

    public DateTime WarrantyStartDate { get; set; }

    public DateTime WarrantyEndDate { get; set; }

    public string? WarrantyType { get; set; }

    public bool IsExtendedWarranty { get; set; }

    public DateTime? ClaimDate { get; set; }

    public string? ClaimDescription { get; set; }

    public string? ClaimStatus { get; set; }

    public string? Remarks { get; set; }

    public DateTime CreatedDate { get; set; }
}