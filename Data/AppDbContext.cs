using BEMLPropertyManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace BEMLPropertyManagement.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Division> Divisions { get; set; }
    public DbSet<Asset> Assets { get; set; }
    public DbSet<AssetType> AssetTypes { get; set; }
    public DbSet<Vendor> Vendors { get; set; }
    public DbSet<Employee> Employees { get; set; }
    public DbSet<Department> Departments { get; set; }
    public DbSet<Location> Locations { get; set; }
    public DbSet<ServiceRecord> ServiceRecords { get; set; }
    public DbSet<Complaint> Complaints { get; set; }
    public DbSet<WarrantyRecord> WarrantyRecords { get; set; }
    public DbSet<ReplacementRecord> ReplacementRecords { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<AuditLog> AuditLogs { get; set; }
}