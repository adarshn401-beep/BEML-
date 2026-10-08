using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using BEMLPropertyManagement.Data;
using Microsoft.EntityFrameworkCore;

namespace BEMLPropertyManagement.Views;

public partial class WarrantyWindow : Window
{
    private readonly string connectionString =
        "Server=localhost;Database=BEMLPropertyManagement;Trusted_Connection=True;TrustServerCertificate=True;";

    public WarrantyWindow()
    {
        InitializeComponent();
        LoadWarranties();
    }

    private AppDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlServer(connectionString)
            .Options;

        return new AppDbContext(options);
    }

    private void LoadWarranties()
    {
        try
        {
            using var db = CreateDbContext();

            var warranties = (
                from warranty in db.WarrantyRecords
                join asset in db.Assets
                    on warranty.AssetId equals asset.AssetId
                select new
                {
                    warranty.WarrantyId,
                    asset.ItemNumber,
                    warranty.WarrantyProvider,
                    warranty.WarrantyStartDate,
                    warranty.WarrantyEndDate,
                    warranty.WarrantyType,
                    warranty.IsExtendedWarranty,
                    warranty.ClaimStatus
                }
            ).ToList();

            var displayWarranties = warranties.Select(warranty =>
                new WarrantyDisplay
                {
                    WarrantyId = warranty.WarrantyId,
                    ItemNumber = warranty.ItemNumber,
                    WarrantyProvider =
                        warranty.WarrantyProvider ?? "-",
                    WarrantyStartDate =
                        warranty.WarrantyStartDate.ToString("dd-MM-yyyy"),
                    WarrantyEndDate =
                        warranty.WarrantyEndDate.ToString("dd-MM-yyyy"),
                    WarrantyType =
                        warranty.WarrantyType ?? "-",
                    ExtendedWarranty =
                        warranty.IsExtendedWarranty ? "Yes" : "No",
                    ClaimStatus =
                        warranty.ClaimStatus ?? "-"
                }).ToList();

            WarrantyDataGrid.ItemsSource = displayWarranties;
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                "Unable to load warranty records.\n\n" +
                ex.Message,
                "Database Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    private void AddWarrantyButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        var addWarrantyWindow = new AddWarrantyWindow();

        addWarrantyWindow.ShowDialog();

        LoadWarranties();
    }

    private void ViewWarrantyButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        if (sender is not Button button)
            return;

        if (button.DataContext is not WarrantyDisplay selectedWarranty)
            return;

        MessageBox.Show(
            $"Warranty for:\n\n" +
            $"{selectedWarranty.ItemNumber}\n\n" +
            $"Provider: {selectedWarranty.WarrantyProvider}\n" +
            $"Start Date: {selectedWarranty.WarrantyStartDate}\n" +
            $"End Date: {selectedWarranty.WarrantyEndDate}\n" +
            $"Type: {selectedWarranty.WarrantyType}\n" +
            $"Extended Warranty: {selectedWarranty.ExtendedWarranty}\n" +
            $"Claim Status: {selectedWarranty.ClaimStatus}",
            "Warranty Details",
            MessageBoxButton.OK,
            MessageBoxImage.Information);
    }

    private void EditWarrantyButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        if (sender is not Button button)
            return;

        if (button.DataContext is not WarrantyDisplay selectedWarranty)
            return;

        var editWarrantyWindow =
            new EditWarrantyWindow(
                selectedWarranty.WarrantyId);

        editWarrantyWindow.ShowDialog();

        LoadWarranties();
    }

    private void DeleteWarrantyButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        if (sender is not Button button)
            return;

        if (button.DataContext is not WarrantyDisplay selectedWarranty)
            return;

        MessageBoxResult result = MessageBox.Show(
            $"Are you sure you want to delete the warranty for:\n\n" +
            $"{selectedWarranty.ItemNumber}?",
            "Confirm Delete",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (result != MessageBoxResult.Yes)
            return;

        try
        {
            using var db = CreateDbContext();

            var warranty = db.WarrantyRecords
                .FirstOrDefault(x =>
                    x.WarrantyId == selectedWarranty.WarrantyId);

            if (warranty == null)
            {
                MessageBox.Show(
                    "Warranty record not found.",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                LoadWarranties();
                return;
            }

            db.WarrantyRecords.Remove(warranty);
            db.SaveChanges();

            MessageBox.Show(
                "Warranty record deleted successfully!",
                "Success",
                MessageBoxButton.OK,
                MessageBoxImage.Information);

            LoadWarranties();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                "Unable to delete warranty record.\n\n" +
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
        SearchWarranties();
    }

    private void SearchTextBox_KeyDown(
        object sender,
        KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            SearchWarranties();
        }
    }

    private void SearchWarranties()
    {
        try
        {
            string searchText = SearchTextBox.Text.Trim();

            if (string.IsNullOrWhiteSpace(searchText) ||
                searchText == "Search warranty...")
            {
                LoadWarranties();
                return;
            }

            using var db = CreateDbContext();

            var warranties = (
                from warranty in db.WarrantyRecords
                join asset in db.Assets
                    on warranty.AssetId equals asset.AssetId
                where asset.ItemNumber.Contains(searchText)
                    || (warranty.WarrantyProvider != null &&
                        warranty.WarrantyProvider.Contains(searchText))
                    || (warranty.WarrantyType != null &&
                        warranty.WarrantyType.Contains(searchText))
                    || (warranty.ClaimStatus != null &&
                        warranty.ClaimStatus.Contains(searchText))
                select new
                {
                    warranty.WarrantyId,
                    asset.ItemNumber,
                    warranty.WarrantyProvider,
                    warranty.WarrantyStartDate,
                    warranty.WarrantyEndDate,
                    warranty.WarrantyType,
                    warranty.IsExtendedWarranty,
                    warranty.ClaimStatus
                }
            ).ToList();

            var displayWarranties = warranties.Select(warranty =>
                new WarrantyDisplay
                {
                    WarrantyId = warranty.WarrantyId,
                    ItemNumber = warranty.ItemNumber,
                    WarrantyProvider =
                        warranty.WarrantyProvider ?? "-",
                    WarrantyStartDate =
                        warranty.WarrantyStartDate.ToString("dd-MM-yyyy"),
                    WarrantyEndDate =
                        warranty.WarrantyEndDate.ToString("dd-MM-yyyy"),
                    WarrantyType =
                        warranty.WarrantyType ?? "-",
                    ExtendedWarranty =
                        warranty.IsExtendedWarranty ? "Yes" : "No",
                    ClaimStatus =
                        warranty.ClaimStatus ?? "-"
                }).ToList();

            WarrantyDataGrid.ItemsSource = displayWarranties;

            if (displayWarranties.Count == 0)
            {
                MessageBox.Show(
                    "No warranty records found.",
                    "Search",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                "Unable to search warranty records.\n\n" +
                ex.Message,
                "Database Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    private class WarrantyDisplay
    {
        public int WarrantyId { get; set; }

        public string ItemNumber { get; set; } = string.Empty;

        public string WarrantyProvider { get; set; } = string.Empty;

        public string WarrantyStartDate { get; set; } = string.Empty;

        public string WarrantyEndDate { get; set; } = string.Empty;

        public string WarrantyType { get; set; } = string.Empty;

        public string ExtendedWarranty { get; set; } = string.Empty;

        public string ClaimStatus { get; set; } = string.Empty;
    }
}