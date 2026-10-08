using System;
using System.Linq;
using System.Windows;
using BEMLPropertyManagement.Data;
using Microsoft.EntityFrameworkCore;

namespace BEMLPropertyManagement.Views;

public partial class ReportsWindow : Window
{
    private readonly string connectionString =
        "Server=localhost;Database=BEMLPropertyManagement;Trusted_Connection=True;TrustServerCertificate=True;";

    public ReportsWindow()
    {
        InitializeComponent();
    }

    private AppDbContext CreateDbContext()
    {
        var options =
            new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlServer(connectionString)
                .Options;

        return new AppDbContext(options);
    }

    // =========================
    // ASSET REPORT
    // =========================

    private void AssetReportButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        try
        {
            using var db = CreateDbContext();

            var report =
                (
                    from asset in db.Assets

                    join assetType in db.AssetTypes
                        on asset.AssetTypeId
                            equals assetType.AssetTypeId

                    join division in db.Divisions
                        on asset.DivisionId
                            equals division.DivisionId

                    join department in db.Departments
                        on asset.DepartmentId
                            equals department.DepartmentId
                        into departmentGroup

                    from department in
                        departmentGroup.DefaultIfEmpty()

                    join location in db.Locations
                        on asset.LocationId
                            equals location.LocationId
                        into locationGroup

                    from location in
                        locationGroup.DefaultIfEmpty()

                    join employee in db.Employees
                        on asset.EmployeeId
                            equals employee.EmployeeId
                        into employeeGroup

                    from employee in
                        employeeGroup.DefaultIfEmpty()

                    join vendor in db.Vendors
                        on asset.VendorId
                            equals vendor.VendorId
                        into vendorGroup

                    from vendor in
                        vendorGroup.DefaultIfEmpty()

                    select new AssetReportRow
                    {
                        ItemNumber =
                            asset.ItemNumber,

                        AssetType =
                            assetType.AssetTypeName,

                        Brand =
                            asset.Brand ?? "-",

                        Model =
                            asset.Model ?? "-",

                        SerialNumber =
                            asset.SerialNumber ?? "-",

                        Division =
                            division.DivisionName,

                        Department =
                            department != null
                                ? department.DepartmentName
                                : "-",

                        Location =
                            location != null
                                ? location.LocationName
                                : "-",

                        Employee =
                            employee != null
                                ? employee.EmployeeName
                                : "-",

                        Vendor =
                            vendor != null
                                ? vendor.VendorName
                                : "-",

                        PurchaseDate =
                            asset.PurchaseDate,

                        WarrantyEndDate =
                            asset.WarrantyEndDate,

                        Status =
                            asset.Status,

                        Condition =
                            asset.AssetCondition
                    }
                )
                .OrderBy(x => x.Division)
                .ThenBy(x => x.ItemNumber)
                .ToList();

            ReportDataGrid.ItemsSource =
                report;
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                "Unable to load asset report.\n\n" +
                ex.Message,
                "Report Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    // =========================
    // WARRANTY REPORT
    // =========================

    private void WarrantyReportButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        try
        {
            using var db = CreateDbContext();

            var report =
                (
                    from warranty in
                        db.WarrantyRecords

                    join asset in db.Assets
                        on warranty.AssetId
                            equals asset.AssetId

                    join assetType in db.AssetTypes
                        on asset.AssetTypeId
                            equals assetType.AssetTypeId

                    join division in db.Divisions
                        on asset.DivisionId
                            equals division.DivisionId

                    select new WarrantyReportRow
                    {
                        ItemNumber =
                            asset.ItemNumber,

                        AssetType =
                            assetType.AssetTypeName,

                        Brand =
                            asset.Brand ?? "-",

                        Model =
                            asset.Model ?? "-",

                        Division =
                            division.DivisionName,

                        WarrantyProvider =
                            warranty.WarrantyProvider ?? "-",

                        WarrantyStartDate =
                            warranty.WarrantyStartDate,

                        WarrantyEndDate =
                            warranty.WarrantyEndDate,

                        WarrantyType =
                            warranty.WarrantyType ?? "-",

                        ExtendedWarranty =
                            warranty.IsExtendedWarranty
                                ? "Yes"
                                : "No",

                        ClaimStatus =
                            warranty.ClaimStatus ?? "-"
                    }
                )
                .OrderBy(x => x.WarrantyEndDate)
                .ToList();

            ReportDataGrid.ItemsSource =
                report;

            if (report.Count == 0)
            {
                MessageBox.Show(
                    "No warranty records found.",
                    "Warranty Report",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                "Unable to load warranty report.\n\n" +
                ex.Message,
                "Report Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    // =========================
    // SERVICE REPORT
    // =========================

    private void ServiceReportButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        try
        {
            using var db = CreateDbContext();

            var report =
                (
                    from service in
                        db.ServiceRecords

                    join asset in db.Assets
                        on service.AssetId
                            equals asset.AssetId

                    join assetType in db.AssetTypes
                        on asset.AssetTypeId
                            equals assetType.AssetTypeId

                    join division in db.Divisions
                        on asset.DivisionId
                            equals division.DivisionId

                    select new ServiceReportRow
                    {
                        ItemNumber =
                            asset.ItemNumber,

                        AssetType =
                            assetType.AssetTypeName,

                        Division =
                            division.DivisionName,

                        ComplaintDate =
                            service.ComplaintDate,

                        ProblemDescription =
                            service.ProblemDescription ?? "-",

                        ServiceDate =
                            service.ServiceDate,

                        ServiceProvider =
                            service.ServiceProvider ?? "-",

                        WorkDescription =
                            service.WorkDescription ?? "-",

                        ServiceCost =
                            service.ServiceCost,

                        Status =
                            service.Status,

                        NextServiceDate =
                            service.NextServiceDate
                    }
                )
                .OrderByDescending(
                    x => x.ServiceDate)
                .ToList();

            ReportDataGrid.ItemsSource =
                report;

            if (report.Count == 0)
            {
                MessageBox.Show(
                    "No service records found.",
                    "Service Report",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                "Unable to load service report.\n\n" +
                ex.Message,
                "Report Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    // =========================
    // COMPLAINT REPORT
    // =========================

    private void ComplaintReportButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        try
        {
            using var db = CreateDbContext();

            var report =
                (
                    from complaint in
                        db.Complaints

                    join asset in db.Assets
                        on complaint.AssetId
                            equals asset.AssetId

                    join assetType in db.AssetTypes
                        on asset.AssetTypeId
                            equals assetType.AssetTypeId

                    join employee in db.Employees
                        on complaint.EmployeeId
                            equals employee.EmployeeId
                        into employeeGroup

                    from employee in
                        employeeGroup.DefaultIfEmpty()

                    select new ComplaintReportRow
                    {
                        ComplaintId =
                            complaint.ComplaintId,

                        ItemNumber =
                            asset.ItemNumber,

                        AssetType =
                            assetType.AssetTypeName,

                        Employee =
                            employee != null
                                ? employee.EmployeeName
                                : "-",

                        ComplaintDate =
                            complaint.ComplaintDate,

                        ProblemTitle =
                            complaint.ProblemTitle,

                        ProblemDescription =
                            complaint.ProblemDescription ?? "-",

                        Priority =
                            complaint.Priority,

                        Status =
                            complaint.Status,

                        ResolvedDate =
                            complaint.ResolvedDate,

                        ResolutionNotes =
                            complaint.ResolutionNotes ?? "-"
                    }
                )
                .OrderByDescending(
                    x => x.ComplaintDate)
                .ToList();

            ReportDataGrid.ItemsSource =
                report;

            if (report.Count == 0)
            {
                MessageBox.Show(
                    "No complaint records found.",
                    "Complaint Report",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                "Unable to load complaint report.\n\n" +
                ex.Message,
                "Report Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    // =========================
    // REPLACEMENT REPORT
    // =========================

    private void ReplacementReportButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        try
        {
            using var db = CreateDbContext();

            var report =
                (
                    from replacement in
                        db.ReplacementRecords

                    join oldAsset in db.Assets
                        on replacement.OldAssetId
                            equals oldAsset.AssetId

                    join newAsset in db.Assets
                        on replacement.NewAssetId
                            equals newAsset.AssetId
                        into newAssetGroup

                    from newAsset in
                        newAssetGroup.DefaultIfEmpty()

                    select new ReplacementReportRow
                    {
                        ReplacementId =
                            replacement.ReplacementId,

                        OldItemNumber =
                            oldAsset.ItemNumber,

                        NewItemNumber =
                            newAsset != null
                                ? newAsset.ItemNumber
                                : "-",

                        ReplacementDate =
                            replacement.ReplacementDate,

                        ReplacementReason =
                            replacement.ReplacementReason,

                        OldAssetCondition =
                            replacement.OldAssetCondition ?? "-",

                        DisposalStatus =
                            replacement.OldAssetDisposalStatus ?? "-",

                        ApprovedBy =
                            replacement.ApprovedBy ?? "-",

                        ReplacementCost =
                            replacement.ReplacementCost,

                        Remarks =
                            replacement.Remarks ?? "-"
                    }
                )
                .OrderByDescending(
                    x => x.ReplacementDate)
                .ToList();

            ReportDataGrid.ItemsSource =
                report;

            if (report.Count == 0)
            {
                MessageBox.Show(
                    "No replacement records found.",
                    "Replacement Report",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                "Unable to load replacement report.\n\n" +
                ex.Message,
                "Report Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    // =========================
    // ASSET REPORT CLASS
    // =========================

    private class AssetReportRow
    {
        public string ItemNumber { get; set; } =
            string.Empty;

        public string AssetType { get; set; } =
            string.Empty;

        public string Brand { get; set; } =
            string.Empty;

        public string Model { get; set; } =
            string.Empty;

        public string SerialNumber { get; set; } =
            string.Empty;

        public string Division { get; set; } =
            string.Empty;

        public string Department { get; set; } =
            string.Empty;

        public string Location { get; set; } =
            string.Empty;

        public string Employee { get; set; } =
            string.Empty;

        public string Vendor { get; set; } =
            string.Empty;

        public DateTime? PurchaseDate { get; set; }

        public DateTime? WarrantyEndDate { get; set; }

        public string Status { get; set; } =
            string.Empty;

        public string Condition { get; set; } =
            string.Empty;
    }

    // =========================
    // WARRANTY REPORT CLASS
    // =========================

    private class WarrantyReportRow
    {
        public string ItemNumber { get; set; } =
            string.Empty;

        public string AssetType { get; set; } =
            string.Empty;

        public string Brand { get; set; } =
            string.Empty;

        public string Model { get; set; } =
            string.Empty;

        public string Division { get; set; } =
            string.Empty;

        public string WarrantyProvider { get; set; } =
            string.Empty;

        public DateTime WarrantyStartDate { get; set; }

        public DateTime WarrantyEndDate { get; set; }

        public string WarrantyType { get; set; } =
            string.Empty;

        public string ExtendedWarranty { get; set; } =
            string.Empty;

        public string ClaimStatus { get; set; } =
            string.Empty;
    }

    // =========================
    // SERVICE REPORT CLASS
    // =========================

    private class ServiceReportRow
    {
        public string ItemNumber { get; set; } =
            string.Empty;

        public string AssetType { get; set; } =
            string.Empty;

        public string Division { get; set; } =
            string.Empty;

        public DateTime ComplaintDate { get; set; }

        public string ProblemDescription { get; set; } =
            string.Empty;

        public DateTime? ServiceDate { get; set; }

        public string ServiceProvider { get; set; } =
            string.Empty;

        public string WorkDescription { get; set; } =
            string.Empty;

        public decimal? ServiceCost { get; set; }

        public string Status { get; set; } =
            string.Empty;

        public DateTime? NextServiceDate { get; set; }
    }

    // =========================
    // COMPLAINT REPORT CLASS
    // =========================

    private class ComplaintReportRow
    {
        public int ComplaintId { get; set; }

        public string ItemNumber { get; set; } =
            string.Empty;

        public string AssetType { get; set; } =
            string.Empty;

        public string Employee { get; set; } =
            string.Empty;

        public DateTime ComplaintDate { get; set; }

        public string ProblemTitle { get; set; } =
            string.Empty;

        public string ProblemDescription { get; set; } =
            string.Empty;

        public string Priority { get; set; } =
            string.Empty;

        public string Status { get; set; } =
            string.Empty;

        public DateTime? ResolvedDate { get; set; }

        public string ResolutionNotes { get; set; } =
            string.Empty;
    }

    // =========================
    // REPLACEMENT REPORT CLASS
    // =========================

    private class ReplacementReportRow
    {
        public int ReplacementId { get; set; }

        public string OldItemNumber { get; set; } =
            string.Empty;

        public string NewItemNumber { get; set; } =
            string.Empty;

        public DateTime ReplacementDate { get; set; }

        public string ReplacementReason { get; set; } =
            string.Empty;

        public string OldAssetCondition { get; set; } =
            string.Empty;

        public string DisposalStatus { get; set; } =
            string.Empty;

        public string ApprovedBy { get; set; } =
            string.Empty;

        public decimal? ReplacementCost { get; set; }

        public string Remarks { get; set; } =
            string.Empty;
    }
}