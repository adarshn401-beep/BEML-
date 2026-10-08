namespace BEMLPropertyManagement.Models;

public class AssetType
{
    public int AssetTypeId { get; set; }

    public string AssetTypeName { get; set; } = string.Empty;

    public string? Description { get; set; }

    public bool IsActive { get; set; }
}