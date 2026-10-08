namespace BEMLPropertyManagement.Models
{
    public class Division
    {
        public int DivisionId { get; set; }

        public string DivisionName { get; set; } = string.Empty;

        public string? Description { get; set; }

        public bool IsActive { get; set; }
    }
}