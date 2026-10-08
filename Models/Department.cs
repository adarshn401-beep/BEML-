namespace BEMLPropertyManagement.Models;

public class Department
{
    public int DepartmentId { get; set; }

    public string DepartmentName { get; set; } = string.Empty;

    public int DivisionId { get; set; }
}