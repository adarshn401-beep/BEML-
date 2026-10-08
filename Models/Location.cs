namespace BEMLPropertyManagement.Models;

public class Location
{
    public int LocationId { get; set; }

    public string LocationName { get; set; } = string.Empty;

    public string? Building { get; set; }

    public string? Floor { get; set; }

    public string? Room { get; set; }

    public int DivisionId { get; set; }

    public int? DepartmentId { get; set; }

    public bool IsActive { get; set; }
}