namespace BEMLPropertyManagement.Models;

public class Asset
{
    public int AssetId { get; set; }

    public string ItemNumber { get; set; } = string.Empty;

    public int AssetTypeId { get; set; }

    public string? Brand { get; set; }

    public string? Model { get; set; }

    public string? SerialNumber { get; set; }

    public int DivisionId { get; set; }

    public int? DepartmentId { get; set; }

    public int? LocationId { get; set; }

    public int? EmployeeId { get; set; }

    public int? VendorId { get; set; }

    public DateTime? PurchaseDate { get; set; }

    public decimal? PurchaseCost { get; set; }

    public DateTime? WarrantyStartDate { get; set; }

    public DateTime? WarrantyEndDate { get; set; }

    public int? ExpectedLifeYears { get; set; }

    public DateTime? ExpectedReplacementDate { get; set; }

    public string Status { get; set; } = "Active";

    public string AssetCondition { get; set; } = "Good";

    public string? Remarks { get; set; }

    public DateTime CreatedDate { get; set; }

    public DateTime? UpdatedDate { get; set; }
}