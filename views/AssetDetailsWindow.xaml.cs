using System;
using System.Linq;
using System.Windows;
using BEMLPropertyManagement.Data;
using Microsoft.EntityFrameworkCore;

namespace BEMLPropertyManagement.Views;

public partial class AssetDetailsWindow : Window
{
    private readonly string connectionString =
        "Server=localhost;Database=BEMLPropertyManagement;Trusted_Connection=True;TrustServerCertificate=True;";

    private readonly int assetId;

    public AssetDetailsWindow(int selectedAssetId)
    {
        InitializeComponent();

        assetId = selectedAssetId;

        LoadAssetDetails();
    }

    private AppDbContext CreateDbContext()
    {
        var options =
            new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlServer(connectionString)
                .Options;

        return new AppDbContext(options);
    }

    private void LoadAssetDetails()
    {
        try
        {
            using var db = CreateDbContext();

            var asset = db.Assets
                .FirstOrDefault(a => a.AssetId == assetId);

            if (asset == null)
            {
                MessageBox.Show(
                    "The selected asset could not be found.",
                    "Asset Details",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                Close();
                return;
            }

            string assetType =
                db.AssetTypes
                    .Where(x => x.AssetTypeId == asset.AssetTypeId)
                    .Select(x => x.AssetTypeName)
                    .FirstOrDefault()
                ?? "N/A";

            string division =
                db.Divisions
                    .Where(x => x.DivisionId == asset.DivisionId)
                    .Select(x => x.DivisionName)
                    .FirstOrDefault()
                ?? "N/A";

            string department = "N/A";

            if (asset.DepartmentId.HasValue)
            {
                department =
                    db.Departments
                        .Where(x =>
                            x.DepartmentId ==
                            asset.DepartmentId.Value)
                        .Select(x => x.DepartmentName)
                        .FirstOrDefault()
                    ?? "N/A";
            }

            string location = "N/A";

            if (asset.LocationId.HasValue)
            {
                location =
                    db.Locations
                        .Where(x =>
                            x.LocationId ==
                            asset.LocationId.Value)
                        .Select(x => x.LocationName)
                        .FirstOrDefault()
                    ?? "N/A";
            }

            string employee = "N/A";

            if (asset.EmployeeId.HasValue)
            {
                employee =
                    db.Employees
                        .Where(x =>
                            x.EmployeeId ==
                            asset.EmployeeId.Value)
                        .Select(x => x.EmployeeName)
                        .FirstOrDefault()
                    ?? "N/A";
            }

            string vendor = "N/A";

            if (asset.VendorId.HasValue)
            {
                vendor =
                    db.Vendors
                        .Where(x =>
                            x.VendorId ==
                            asset.VendorId.Value)
                        .Select(x => x.VendorName)
                        .FirstOrDefault()
                    ?? "N/A";
            }

            // Asset information
            AssetIdTextBlock.Text =
                asset.AssetId.ToString();

            ItemNumberTextBlock.Text =
                asset.ItemNumber;

            AssetTypeTextBlock.Text =
                assetType;

            BrandTextBlock.Text =
                asset.Brand ?? "N/A";

            ModelTextBlock.Text =
                asset.Model ?? "N/A";

            SerialNumberTextBlock.Text =
                asset.SerialNumber ?? "N/A";

            StatusTextBlock.Text =
                asset.Status;

            ConditionTextBlock.Text =
                asset.AssetCondition;

            // Assignment information
            DivisionTextBlock.Text =
                division;

            DepartmentTextBlock.Text =
                department;

            LocationTextBlock.Text =
                location;

            EmployeeTextBlock.Text =
                employee;

            VendorTextBlock.Text =
                vendor;

            // Purchase information
            PurchaseDateTextBlock.Text =
                asset.PurchaseDate.HasValue
                    ? asset.PurchaseDate.Value.ToString("dd-MM-yyyy")
                    : "N/A";

            PurchaseCostTextBlock.Text =
                asset.PurchaseCost.HasValue
                    ? "₹" + asset.PurchaseCost.Value.ToString("N2")
                    : "N/A";

            // Warranty information
            WarrantyStartTextBlock.Text =
                asset.WarrantyStartDate.HasValue
                    ? asset.WarrantyStartDate.Value.ToString("dd-MM-yyyy")
                    : "N/A";

            WarrantyEndTextBlock.Text =
                asset.WarrantyEndDate.HasValue
                    ? asset.WarrantyEndDate.Value.ToString("dd-MM-yyyy")
                    : "N/A";

            // Replacement information
            ExpectedLifeTextBlock.Text =
                asset.ExpectedLifeYears.HasValue
                    ? asset.ExpectedLifeYears.Value + " years"
                    : "N/A";

            ReplacementDateTextBlock.Text =
                asset.ExpectedReplacementDate.HasValue
                    ? asset.ExpectedReplacementDate.Value.ToString("dd-MM-yyyy")
                    : "N/A";

            // Remarks
            RemarksTextBlock.Text =
                string.IsNullOrWhiteSpace(asset.Remarks)
                    ? "No remarks available."
                    : asset.Remarks;
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                "Unable to load asset details.\n\n" +
                ex.Message,
                "Database Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);

            Close();
        }
    }

    private void CloseButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        Close();
    }
}