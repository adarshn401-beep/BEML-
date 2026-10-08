using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using BEMLPropertyManagement.Data;
using Microsoft.EntityFrameworkCore;

namespace BEMLPropertyManagement.Views;

public partial class AssetsWindow : Window
{
    private readonly string connectionString =
        "Server=localhost;Database=BEMLPropertyManagement;Trusted_Connection=True;TrustServerCertificate=True;";

    public AssetsWindow()
    {
        InitializeComponent();
        LoadAssets();
    }

    private AppDbContext CreateDbContext()
    {
        var options =
            new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlServer(connectionString)
                .Options;

        return new AppDbContext(options);
    }

    private void LoadAssets(string? searchText = null)
    {
        try
        {
            using var db = CreateDbContext();

            var query =
                from asset in db.Assets

                join assetType in db.AssetTypes
                    on asset.AssetTypeId equals assetType.AssetTypeId
                    into assetTypeGroup
                from assetType in assetTypeGroup.DefaultIfEmpty()

                join division in db.Divisions
                    on asset.DivisionId equals division.DivisionId
                    into divisionGroup
                from division in divisionGroup.DefaultIfEmpty()

                join vendor in db.Vendors
                    on asset.VendorId equals vendor.VendorId
                    into vendorGroup
                from vendor in vendorGroup.DefaultIfEmpty()

                select new
                {
                    asset.AssetId,
                    asset.ItemNumber,

                    AssetTypeName =
                        assetType != null
                            ? assetType.AssetTypeName
                            : "N/A",

                    asset.Brand,
                    asset.Model,
                    asset.SerialNumber,

                    DivisionName =
                        division != null
                            ? division.DivisionName
                            : "N/A",

                    VendorName =
                        vendor != null
                            ? vendor.VendorName
                            : "N/A",

                    asset.PurchaseDate,
                    asset.Status,
                    asset.AssetCondition
                };

            if (!string.IsNullOrWhiteSpace(searchText))
            {
                searchText = searchText.Trim();

                query = query.Where(a =>
                    a.ItemNumber.Contains(searchText));
            }

            AssetsDataGrid.ItemsSource = query
                .OrderBy(a => a.ItemNumber)
                .ToList();

            EditAssetButton.IsEnabled = false;
            DeleteAssetButton.IsEnabled = false;
            ViewDetailsButton.IsEnabled = false;
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                "Unable to load assets.\n\n" +
                ex.Message,
                "Database Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    private void SearchButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        LoadAssets(SearchTextBox.Text);
    }

    private void SearchTextBox_TextChanged(
        object sender,
        TextChangedEventArgs e)
    {
        if (IsInitialized)
        {
            LoadAssets(SearchTextBox.Text);
        }
    }

    private void RefreshButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        SearchTextBox.Clear();
        LoadAssets();
    }

    private void AddAssetButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        var addAssetWindow = new AddAssetWindow();

        bool? result = addAssetWindow.ShowDialog();

        if (result == true)
        {
            LoadAssets();
        }
    }

    // Enable buttons when an asset is selected
    private void AssetsDataGrid_SelectionChanged(
        object sender,
        SelectionChangedEventArgs e)
    {
        bool hasSelection =
            AssetsDataGrid.SelectedItem != null;

        EditAssetButton.IsEnabled = hasSelection;
        DeleteAssetButton.IsEnabled = hasSelection;
        ViewDetailsButton.IsEnabled = hasSelection;
    }

    // EDIT ASSET
    private void EditAssetButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        try
        {
            if (AssetsDataGrid.SelectedItem == null)
            {
                MessageBox.Show(
                    "Please select an asset first.",
                    "Asset Management",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                return;
            }

            dynamic selectedAsset =
                AssetsDataGrid.SelectedItem;

            int assetId = selectedAsset.AssetId;

            var editWindow =
                new EditAssetWindow(assetId);

            bool? result =
                editWindow.ShowDialog();

            if (result == true)
            {
                LoadAssets(SearchTextBox.Text);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                "Unable to open the Edit Asset window.\n\n" +
                ex.Message,
                "Asset Management",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    // DELETE ASSET
    private void DeleteAssetButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        try
        {
            if (AssetsDataGrid.SelectedItem == null)
            {
                MessageBox.Show(
                    "Please select an asset first.",
                    "Asset Management",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                return;
            }

            dynamic selectedAsset =
                AssetsDataGrid.SelectedItem;

            int assetId = selectedAsset.AssetId;
            string itemNumber =
                selectedAsset.ItemNumber;

            MessageBoxResult confirmation =
                MessageBox.Show(
                    "Are you sure you want to delete this asset?\n\n" +
                    $"Item Number: {itemNumber}\n" +
                    $"Asset ID: {assetId}\n\n" +
                    "This action cannot be undone.",
                    "Confirm Delete",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

            if (confirmation != MessageBoxResult.Yes)
            {
                return;
            }

            using var db = CreateDbContext();

            var asset = db.Assets
                .FirstOrDefault(a =>
                    a.AssetId == assetId);

            if (asset == null)
            {
                MessageBox.Show(
                    "The selected asset could not be found.",
                    "Delete Asset",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                LoadAssets(SearchTextBox.Text);
                return;
            }

            db.Assets.Remove(asset);

            db.SaveChanges();

            MessageBox.Show(
                $"Asset '{itemNumber}' was deleted successfully.",
                "Delete Asset",
                MessageBoxButton.OK,
                MessageBoxImage.Information);

            LoadAssets(SearchTextBox.Text);
        }
        catch (DbUpdateException ex)
        {
            MessageBox.Show(
                "The asset could not be deleted because it may be linked to other records.\n\n" +
                ex.InnerException?.Message ??
                ex.Message,
                "Delete Asset",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                "Unable to delete the asset.\n\n" +
                ex.Message,
                "Delete Asset",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    // VIEW COMPLETE ASSET INFORMATION
    private void ViewDetailsButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        try
        {
            if (AssetsDataGrid.SelectedItem == null)
            {
                MessageBox.Show(
                    "Please select an asset first.",
                    "Asset Management",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                return;
            }

            dynamic selectedAsset =
                AssetsDataGrid.SelectedItem;

            int assetId =
                selectedAsset.AssetId;

            using var db = CreateDbContext();

            var asset = db.Assets
                .FirstOrDefault(a =>
                    a.AssetId == assetId);

            if (asset == null)
            {
                MessageBox.Show(
                    "The selected asset could not be found.",
                    "Asset Information",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            string assetType =
                db.AssetTypes
                    .Where(x =>
                        x.AssetTypeId ==
                        asset.AssetTypeId)
                    .Select(x =>
                        x.AssetTypeName)
                    .FirstOrDefault() ?? "N/A";

            string division =
                db.Divisions
                    .Where(x =>
                        x.DivisionId ==
                        asset.DivisionId)
                    .Select(x =>
                        x.DivisionName)
                    .FirstOrDefault() ?? "N/A";

            string vendor =
                asset.VendorId.HasValue
                    ? db.Vendors
                        .Where(x =>
                            x.VendorId ==
                            asset.VendorId.Value)
                        .Select(x =>
                            x.VendorName)
                        .FirstOrDefault() ?? "N/A"
                    : "N/A";

            string department =
                asset.DepartmentId.HasValue
                    ? db.Departments
                        .Where(x =>
                            x.DepartmentId ==
                            asset.DepartmentId.Value)
                        .Select(x =>
                            x.DepartmentName)
                        .FirstOrDefault() ?? "N/A"
                    : "N/A";

            string location =
                asset.LocationId.HasValue
                    ? db.Locations
                        .Where(x =>
                            x.LocationId ==
                            asset.LocationId.Value)
                        .Select(x =>
                            x.LocationName)
                        .FirstOrDefault() ?? "N/A"
                    : "N/A";

            string employee =
                asset.EmployeeId.HasValue
                    ? db.Employees
                        .Where(x =>
                            x.EmployeeId ==
                            asset.EmployeeId.Value)
                        .Select(x =>
                            x.EmployeeName)
                        .FirstOrDefault() ?? "N/A"
                    : "N/A";

            string message =
                "ASSET DETAILS\n" +
                "==============================\n\n" +

                $"Asset ID: {asset.AssetId}\n" +
                $"Item Number: {asset.ItemNumber}\n" +
                $"Asset Type: {assetType}\n" +
                $"Brand: {asset.Brand ?? "N/A"}\n" +
                $"Model: {asset.Model ?? "N/A"}\n" +
                $"Serial Number: {asset.SerialNumber ?? "N/A"}\n\n" +

                $"Division: {division}\n" +
                $"Department: {department}\n" +
                $"Location: {location}\n" +
                $"Assigned Employee: {employee}\n" +
                $"Vendor: {vendor}\n\n" +

                $"Purchase Date: " +
                $"{(asset.PurchaseDate.HasValue
                    ? asset.PurchaseDate.Value.ToString("dd-MM-yyyy")
                    : "N/A")}\n" +

                $"Purchase Cost: " +
                $"{(asset.PurchaseCost.HasValue
                    ? "₹" + asset.PurchaseCost.Value.ToString("N2")
                    : "N/A")}\n\n" +

                $"Warranty Start: " +
                $"{(asset.WarrantyStartDate.HasValue
                    ? asset.WarrantyStartDate.Value.ToString("dd-MM-yyyy")
                    : "N/A")}\n" +

                $"Warranty End: " +
                $"{(asset.WarrantyEndDate.HasValue
                    ? asset.WarrantyEndDate.Value.ToString("dd-MM-yyyy")
                    : "N/A")}\n\n" +

                $"Expected Life: " +
                $"{(asset.ExpectedLifeYears.HasValue
                    ? asset.ExpectedLifeYears + " years"
                    : "N/A")}\n" +

                $"Expected Replacement: " +
                $"{(asset.ExpectedReplacementDate.HasValue
                    ? asset.ExpectedReplacementDate.Value.ToString("dd-MM-yyyy")
                    : "N/A")}\n\n" +

                $"Status: {asset.Status}\n" +
                $"Condition: {asset.AssetCondition}\n" +
                $"Remarks: {asset.Remarks ?? "N/A"}";

            MessageBox.Show(
                message,
                "Asset Details",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                "Unable to load asset details.\n\n" +
                ex.Message,
                "Database Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }
}