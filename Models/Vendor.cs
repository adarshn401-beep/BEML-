namespace BEMLPropertyManagement.Models;

public class Vendor
{
    public int VendorId { get; set; }

    public string VendorName { get; set; } = string.Empty;

    public string? ContactPerson { get; set; }

    public string? Phone { get; set; }

    public string? Email { get; set; }

    public string? Address { get; set; }

    public bool IsActive { get; set; }
}