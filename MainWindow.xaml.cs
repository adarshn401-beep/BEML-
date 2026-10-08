using System;
using System.Linq;
using System.Windows;
using BEMLPropertyManagement.Data;
using BEMLPropertyManagement.Views;
using Microsoft.EntityFrameworkCore;

namespace BEMLPropertyManagement;

public partial class MainWindow : Window
{
    private readonly string connectionString =
        "Server=localhost;Database=BEMLPropertyManagement;Trusted_Connection=True;TrustServerCertificate=True;";

    public MainWindow()
    {
        InitializeComponent();
        LoadDashboardStatistics();
    }

    private AppDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlServer(connectionString)
            .Options;

        return new AppDbContext(options);
    }

    private void LoadDashboardStatistics()
    {
        try
        {
            using var db = CreateDbContext();

            int totalAssets = db.Assets.Count();
            int totalAssetTypes = db.AssetTypes.Count();
            int totalDepartments = db.Departments.Count();
            int totalEmployees = db.Employees.Count();

            DateTime today = DateTime.Today;
            DateTime expiringSoonDate = today.AddDays(30);

            int expiredWarranties = db.WarrantyRecords
                .Count(w => w.WarrantyEndDate < today);

            int expiringWarranties = db.WarrantyRecords
                .Count(w =>
                    w.WarrantyEndDate >= today &&
                    w.WarrantyEndDate <= expiringSoonDate);

            DateTime replacementSoonDate = today.AddDays(90);

            int assetsDueForReplacement = db.Assets
                .Count(a =>
                    a.ExpectedReplacementDate.HasValue &&
                    a.ExpectedReplacementDate.Value < today);

            int assetsApproachingReplacement = db.Assets
                .Count(a =>
                    a.ExpectedReplacementDate.HasValue &&
                    a.ExpectedReplacementDate.Value >= today &&
                    a.ExpectedReplacementDate.Value <= replacementSoonDate);

            TotalAssetsTextBlock.Text = totalAssets.ToString();
            AssetTypesTextBlock.Text = totalAssetTypes.ToString();
            DepartmentsTextBlock.Text = totalDepartments.ToString();
            EmployeesTextBlock.Text = totalEmployees.ToString();

            ExpiredWarrantiesTextBlock.Text =
                expiredWarranties.ToString();

            ExpiringWarrantiesTextBlock.Text =
                expiringWarranties.ToString();

            AssetsDueReplacementTextBlock.Text =
                assetsDueForReplacement.ToString();

            AssetsApproachingReplacementTextBlock.Text =
                assetsApproachingReplacement.ToString();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                "Unable to load dashboard statistics.\n\n" +
                ex.Message,
                "Database Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    private void AssetsButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        var assetsWindow = new AssetsWindow();
        assetsWindow.Show();
    }

    private void ServiceRecordsButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        var serviceRecordsWindow = new ServiceRecordsWindow();
        serviceRecordsWindow.Show();
    }

    private void ComplaintsButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        var complaintsWindow = new ComplaintsWindow();
        complaintsWindow.Show();
    }

    private void WarrantyButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        var warrantyWindow = new WarrantyWindow();
        warrantyWindow.Show();
    }

    private void ReplacementButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        var replacementWindow = new ReplacementWindow();
        replacementWindow.Show();
    }

    private void VendorsButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        var vendorsWindow = new VendorsWindow();
        vendorsWindow.Show();
    }

    private void EmployeesButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        var employeesWindow = new EmployeesWindow();
        employeesWindow.Show();
    }

    private void ReportsButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        var reportsWindow = new ReportsWindow();
        reportsWindow.Show();
    }

    private void ExpiredWarrantiesCard_Click(
        object sender,
        System.Windows.Input.MouseButtonEventArgs e)
    {
        try
        {
            using var db = CreateDbContext();

            DateTime today = DateTime.Today;

            var expiredWarranties = db.WarrantyRecords
                .Join(
                    db.Assets,
                    warranty => warranty.AssetId,
                    asset => asset.AssetId,
                    (warranty, asset) => new
                    {
                        Warranty = warranty,
                        Asset = asset
                    })
                .Where(x => x.Warranty.WarrantyEndDate < today)
                .OrderBy(x => x.Warranty.WarrantyEndDate)
                .ToList();

            if (expiredWarranties.Count == 0)
            {
                MessageBox.Show(
                    "There are currently no expired warranties.",
                    "Warranty Information",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                return;
            }

            string message = "Expired Warranties\n\n";

            foreach (var item in expiredWarranties)
            {
                message +=
                    $"Item Number: {item.Asset.ItemNumber}\n" +
                    $"Brand: {item.Asset.Brand ?? "N/A"}\n" +
                    $"Model: {item.Asset.Model ?? "N/A"}\n" +
                    $"Warranty Provider: {item.Warranty.WarrantyProvider}\n" +
                    $"Warranty Type: {item.Warranty.WarrantyType}\n" +
                    $"Warranty End Date: {item.Warranty.WarrantyEndDate:dd-MM-yyyy}\n" +
                    $"Claim Status: {item.Warranty.ClaimStatus}\n\n";
            }

            MessageBox.Show(
                message,
                "Expired Warranties",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                "Unable to load expired warranty information.\n\n" +
                ex.Message,
                "Database Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    private void ExpiringWarrantiesCard_Click(
        object sender,
        System.Windows.Input.MouseButtonEventArgs e)
    {
        try
        {
            using var db = CreateDbContext();

            DateTime today = DateTime.Today;
            DateTime expiringSoonDate = today.AddDays(30);

            var expiringWarranties = db.WarrantyRecords
                .Join(
                    db.Assets,
                    warranty => warranty.AssetId,
                    asset => asset.AssetId,
                    (warranty, asset) => new
                    {
                        Warranty = warranty,
                        Asset = asset
                    })
                .Where(x =>
                    x.Warranty.WarrantyEndDate >= today &&
                    x.Warranty.WarrantyEndDate <= expiringSoonDate)
                .OrderBy(x => x.Warranty.WarrantyEndDate)
                .ToList();

            if (expiringWarranties.Count == 0)
            {
                MessageBox.Show(
                    "There are currently no warranties expiring within the next 30 days.",
                    "Warranty Information",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                return;
            }

            string message = "Warranties Expiring Soon\n\n";

            foreach (var item in expiringWarranties)
            {
                message +=
                    $"Item Number: {item.Asset.ItemNumber}\n" +
                    $"Brand: {item.Asset.Brand ?? "N/A"}\n" +
                    $"Model: {item.Asset.Model ?? "N/A"}\n" +
                    $"Warranty Provider: {item.Warranty.WarrantyProvider}\n" +
                    $"Warranty Type: {item.Warranty.WarrantyType}\n" +
                    $"Warranty End Date: {item.Warranty.WarrantyEndDate:dd-MM-yyyy}\n" +
                    $"Claim Status: {item.Warranty.ClaimStatus}\n\n";
            }

            MessageBox.Show(
                message,
                "Warranties Expiring Soon",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                "Unable to load warranty information.\n\n" +
                ex.Message,
                "Database Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    private void AssetsDueReplacementCard_Click(
        object sender,
        System.Windows.Input.MouseButtonEventArgs e)
    {
        try
        {
            using var db = CreateDbContext();

            DateTime today = DateTime.Today;

            var assetsDue = db.Assets
                .Where(a =>
                    a.ExpectedReplacementDate.HasValue &&
                    a.ExpectedReplacementDate.Value < today)
                .OrderBy(a => a.ExpectedReplacementDate)
                .ToList();

            if (assetsDue.Count == 0)
            {
                MessageBox.Show(
                    "There are currently no assets due for replacement.",
                    "Replacement Information",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                return;
            }

            string message = "Assets Due for Replacement\n\n";

            foreach (var asset in assetsDue)
            {
                message +=
                    $"Item Number: {asset.ItemNumber}\n" +
                    $"Brand: {asset.Brand ?? "N/A"}\n" +
                    $"Model: {asset.Model ?? "N/A"}\n" +
                    $"Replacement Date: {asset.ExpectedReplacementDate:dd-MM-yyyy}\n" +
                    $"Condition: {asset.AssetCondition}\n" +
                    $"Status: {asset.Status}\n\n";
            }

            MessageBox.Show(
                message,
                "Assets Due for Replacement",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                "Unable to load replacement information.\n\n" +
                ex.Message,
                "Database Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    private void ReplacementDueSoonCard_Click(
        object sender,
        System.Windows.Input.MouseButtonEventArgs e)
    {
        try
        {
            using var db = CreateDbContext();

            DateTime today = DateTime.Today;
            DateTime replacementSoonDate = today.AddDays(90);

            var assetsDueSoon = db.Assets
                .Where(a =>
                    a.ExpectedReplacementDate.HasValue &&
                    a.ExpectedReplacementDate.Value >= today &&
                    a.ExpectedReplacementDate.Value <= replacementSoonDate)
                .OrderBy(a => a.ExpectedReplacementDate)
                .ToList();

            if (assetsDueSoon.Count == 0)
            {
                MessageBox.Show(
                    "There are currently no assets approaching replacement.",
                    "Replacement Information",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                return;
            }

            string message = "Assets Approaching Replacement\n\n";

            foreach (var asset in assetsDueSoon)
            {
                message +=
                    $"Item Number: {asset.ItemNumber}\n" +
                    $"Brand: {asset.Brand ?? "N/A"}\n" +
                    $"Model: {asset.Model ?? "N/A"}\n" +
                    $"Replacement Date: {asset.ExpectedReplacementDate:dd-MM-yyyy}\n" +
                    $"Condition: {asset.AssetCondition}\n" +
                    $"Status: {asset.Status}\n\n";
            }

            MessageBox.Show(
                message,
                "Replacement Due Soon",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                "Unable to load replacement information.\n\n" +
                ex.Message,
                "Database Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    private void LogoutButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        MessageBoxResult result = MessageBox.Show(
            "Are you sure you want to logout?",
            "Confirm Logout",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (result != MessageBoxResult.Yes)
        {
            return;
        }

        LoginWindow loginWindow = new LoginWindow();

        Application.Current.MainWindow = loginWindow;

        loginWindow.Show();

        Close();
    }
}